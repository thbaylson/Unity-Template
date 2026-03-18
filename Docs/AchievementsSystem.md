# In-Game Achievements System Guide

## Purpose
This document describes the achievements architecture for the project.

It is intended for:
- new developers joining the project
- future maintainers
- LLM agents making safe, scoped changes

---

## Goals
- Keep the entire system local to the game with no third-party APIs.
- Keep achievement authoring mostly data-driven.
- Avoid coupling gameplay systems directly to `AchievementService`.
- Minimize updating existing code to implement new achievements.
- Evaluate only the achievements affected by a given gameplay event.
- Persist achievement progress through the save system.

---

## Current architecture summary
The achievements system uses a **hybrid event-driven and progression-based design**.

### High-level model
- **Gameplay systems** change progression data or raise gameplay events.
- **Reporter components** translate those gameplay events into generic achievement signals.
- **`AchievementSignalBus`** broadcasts those signals.
- **`AchievementService`** listens for signals and evaluates only the achievements mapped to those signals.
- **Achievement conditions** remain authored as `ScriptableObject` assets.
- **Save data** remains the source of truth for player progression and persisted achievement state.

### Design rule
Gameplay code should say **what happened**, not **which achievement system method to call**.

For example:
- good: gold system raises a gold-related event or updates progression data
- bad: gold system calls `AchievementService.RegisterGoldCollected(...)`

---

## Core runtime pieces

## 1) Achievement definition layer
Each achievement is authored as an `AchievementDefinition` `ScriptableObject`.

Expected fields:
- `string Id`
- `string DisplayName`
- `string Description`
- `string FlavorText`
- `Sprite Icon`
- `bool HideUntilUnlocked`
- `int DisplayOrder`
- `AchievementUnlockCondition UnlockCondition`

### Notes
- Definitions are loaded from `Resources/Achievements/`.
- Each definition must have a stable unique `Id`.
- The `UnlockCondition` is still asset-authored, but it no longer directly depends on scene objects or gameplay MonoBehaviours.

---

## 2) Achievement condition layer
All conditions inherit from `AchievementUnlockCondition`.

### Current responsibilities of a condition
A condition does two things:
1. declares which signal keys should trigger re-evaluation
2. evaluates itself against the current `AchievementEvaluationContext` and existing `AchievementProgressState`

### Current condition API shape
Conditions should expose:
- `IReadOnlyList<string> RelevantSignalKeys`
- `AchievementConditionEvaluationResult Evaluate(AchievementEvaluationContext evaluationContext, AchievementProgressState progressState)`

### Current built-in condition examples
- `TotalGoldCollectedAtLeastCondition`
- `TotalGoldOwnedAtLeastCondition`
- `SceneVisitedCondition`

### Important rule
Conditions should depend on **domain state**, not on concrete gameplay classes.

Prefer this:
- total gold collected
- current gold owned
- visited scenes

Avoid this:
- direct subscriptions inside the condition to `GoldCollector.OnGoldChanged`
- direct references from condition assets to scene-specific runtime objects

---

## 3) Evaluation context
`AchievementEvaluationContext` is the runtime snapshot passed into conditions.

### Current context contents
The refactored context should expose the progression state required by conditions, such as:
- `CurrentGoldOwned`
- `TotalGoldCollected`
- `VisitedSceneNames`
- `MostRecentScene`

### Why this matters
Conditions evaluate against a stable, explicit snapshot rather than reaching out into gameplay systems.
This keeps them deterministic and easier to test.

---

## 4) Signal layer
The signal layer is what removed the need for `RegisterGoldCollected` and similar methods.

### `AchievementSignalKeys`
A static class that defines canonical keys for achievement-related triggers.

Current examples:
- `progress.gold.collected`
- `progress.gold.owned.changed`
- `progress.scene.visited`

### `AchievementSignalBus`
A lightweight static event hub used to publish generic achievement signals.

### Why signals exist
Signals let gameplay and progression systems notify the achievements layer without knowing anything about specific achievement definitions.

---

## 5) Reporter layer
Reporter components subscribe to gameplay/progression events and publish achievement signals.

### Current reporters
- `GoldAchievementSignalReporter`
- `SceneVisitAchievementSignalReporter`

### Reporter responsibilities
A reporter should:
- listen to gameplay-level delegates or engine callbacks
- publish one or more generic signal keys
- optionally update progression-related save data if that responsibility belongs there

A reporter should **not**:
- contain unlock logic
- know about specific achievement IDs
- loop through achievements directly

---

## 6) Achievement service
`AchievementService` is still the central runtime coordinator, but its role is narrower and cleaner than before.

### Current responsibilities
- load all `AchievementDefinition` assets
- build a dictionary of progress state by achievement id
- build an index of signal key -> achievement definitions
- listen to `AchievementSignalBus`
- create an `AchievementEvaluationContext` from current save data
- evaluate only the definitions relevant to the raised signal
- unlock achievements and persist timestamps/progress
- raise UI-facing events:
  - `AchievementUnlocked`
  - `AchievementsChanged`

### Responsibilities removed from the service
The service should **not** have one method per gameplay event family.

These old-style methods are no longer part of the target architecture:
- `RegisterGoldCollected(...)`
- `RegisterSceneVisited(...)`
- similar future `RegisterEnemyKilled(...)`, `RegisterDistanceTraveled(...)`, etc.

### Evaluation strategy
The service should:
- do a **full evaluation pass only during initialization/reset**
- do **targeted evaluations** for normal runtime signals

That means the service evaluates:
- gold-owned achievements when gold-owned changes
- total-gold-collected achievements when collected-gold changes
- scene-visit achievements when a scene visit signal is published

It should **not** evaluate every achievement every time the player does anything.

---

## 7) Persistence model
The system now relies on the project’s existing save architecture instead of introducing a separate achievement save file design.

### Current persistence source of truth
Achievement progress is stored in `PlayerSaveData` and accessed through `SaveService`.

### Relevant persisted data
Achievement-specific persisted data:
- `PlayerSaveData.Achievements`
  - list of `AchievementProgressState`

Progression data used by conditions:
- `PlayerSaveData.GoldAmount`
- `PlayerSaveData.TotalGoldCollected`
- `PlayerSaveData.VisitedSceneNames`

### `AchievementProgressState`
Expected fields:
- `string AchievementId`
- `bool IsUnlocked`
- `long UnlockedUnixTime`
- `int CurrentProgressValue`

### Dirty-state behavior
When progress changes or an achievement unlocks, the system should call `SaveService.MarkGameDirty()`.

---

## Current data flow examples

## Gold collected
### Flow
1. `GoldCollector` adds gold to player progression.
2. `GoldCollector` updates save-backed player values:
   - current gold owned
   - total gold collected
3. `GoldCollector` raises its gameplay delegates.
4. `GoldAchievementSignalReporter` listens to those delegates.
5. The reporter publishes:
   - `progress.gold.collected`
   - or `progress.gold.owned.changed`
6. `AchievementService` receives the signal.
7. `AchievementService` evaluates only the achievements indexed under that signal.
8. Progress and unlock state are saved and UI is notified.

### Architectural consequence
`GoldCollector` is no longer achievement-aware beyond exposing its own gameplay events.

---

## Scene visited
### Flow
1. A scene loads.
2. `SceneVisitAchievementSignalReporter` receives the scene callback.
3. The reporter records the scene in `PlayerSaveData.VisitedSceneNames` if needed.
4. The reporter publishes `progress.scene.visited`.
5. `AchievementService` evaluates only scene-related achievements.

### Architectural consequence
Scene-related achievement logic is routed through a generic signal, not a scene-specific service method.

---

## Initialization and bootstrap behavior
On startup or after save reset:
1. `AchievementService` loads all definitions.
2. `AchievementService` rebuilds progress state from `PlayerSaveData`.
3. `AchievementService` builds its signal index.
4. `AchievementService` performs one full evaluation pass to backfill progress for newly added achievements.
5. `AchievementService` raises `AchievementsChanged` so UI can refresh.

This one-time full scan is intentional and acceptable.

---

## File and folder layout

## Runtime scripts
Recommended structure:
- `Assets/Scripts/Achievements/`
  - `AchievementService.cs`
  - `AchievementDefinition.cs`
  - `AchievementEvaluationContext.cs`
  - `AchievementConditionEvaluationResult.cs`
  - `AchievementProgressState.cs`
  - `AchievementSignalBus.cs`
  - `AchievementSignalKeys.cs`
  - `GoldAchievementSignalReporter.cs`
  - `SceneVisitAchievementSignalReporter.cs`
  - `Conditions/`
    - `AchievementUnlockCondition.cs`
    - `SceneVisitedCondition.cs`
    - `TotalGoldCollectedAtLeastCondition.cs`
    - `TotalGoldOwnedAtLeastCondition.cs`

## Resources
- `Assets/Resources/Achievements/`
  - one `.asset` per achievement definition
- `Assets/Resources/Achievements/Conditions/`
  - reusable condition assets as needed

---

## Authoring workflow for a new achievement
For most new achievements, no changes to `AchievementService` should be required.

### Reuse an existing condition type
1. Duplicate an existing `AchievementDefinition` asset.
2. Assign a new stable `Id`.
3. Set display fields: name, description, flavor text, icon, display order.
4. Create or reuse a matching condition asset.
5. Configure the condition values.
6. Assign the condition to the definition.
7. Enter Play Mode and trigger the relevant gameplay action.
8. Verify unlock state and progress UI.

### Add a new condition family
If no existing condition type fits:
1. Create a new class inheriting `AchievementUnlockCondition`.
2. Define its `RelevantSignalKeys`.
3. Implement `Evaluate(...)` against `AchievementEvaluationContext` and `AchievementProgressState`.
4. Create assets for that condition as needed.
5. Create or reuse a signal reporter that publishes the needed signal.

### Important extension rule
When adding a new achievement family, prefer adding:
- a new signal key
- a new condition class
- a new reporter if needed

Do **not** add a new `RegisterX` method to `AchievementService` unless there is an unusually strong reason.

---

## Guidance for LLM agents and maintainers

## Safe changes
These are usually safe and localized:
- add a new `AchievementDefinition` asset
- add a new condition class
- add a new signal key
- add a new reporter component
- add a new field to `AchievementEvaluationContext`
- update UI to display progress or unlock state

## Changes that require extra care
- renaming achievement IDs
- changing how progress is persisted in `PlayerSaveData`
- moving resources out of `Resources/Achievements/`
- changing signal key strings after content already depends on them
- moving progression ownership out of the systems currently updating save data

## Architectural rules to preserve
- gameplay systems should not call achievement-specific service methods
- conditions should not subscribe directly to scene/gameplay delegates
- `AchievementService` should not become a god object full of gameplay-specific registration methods
- normal gameplay should trigger targeted evaluation, not full-list evaluation
- persisted progress should be derived from stable ids and save-backed progression state

---

## Risks and mitigations
- **Risk**: Duplicate IDs cause progress collisions.
  - **Mitigation**: Validate IDs during definition loading and warn loudly in development.
- **Risk**: A new achievement never updates because no signal is published.
  - **Mitigation**: Ensure every new condition family has a matching signal producer path.
- **Risk**: Conditions become tightly coupled to gameplay objects.
  - **Mitigation**: Route through context and signal keys instead of direct runtime references.
- **Risk**: Performance degrades as content grows.
  - **Mitigation**: Keep signal routing granular and evaluate only subscribed achievements.
- **Risk**: Save data drift causes incorrect evaluations.
  - **Mitigation**: Keep progression ownership clear and rebuild runtime context from save-backed data.

---

## Current acceptance criteria
- Achievement definitions are authored as assets.
- Achievement progress persists across game restarts through the existing save system.
- Unlocking an achievement updates UI state immediately through service events.
- Gold and scene achievements do not require gameplay systems to call achievement-specific registration methods.
- Runtime evaluation is signal-routed and targeted.
- No external service or network dependency is required.

---

## Recommended next step
A future improvement would be introducing a dedicated `PlayerProgressService` so gameplay systems stop writing directly to `SaveService.GameDataCache.Player`.

That is not required for the current architecture, but it would make ownership of progression data even clearer for achievements, analytics, and other meta-systems.
