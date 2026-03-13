# In-Game Achievements System Plan

## Goals
- Keep the entire system local to the game with no third-party APIs.
- Make adding new achievements data-driven and low effort.
- Let players browse achievements from a dedicated UI menu.
- Represent each achievement with icon, name, description, and flavor text.

## Proposed architecture

### 1) Achievement definition layer (authoring)
Use a `ScriptableObject` to define each achievement as content.

Suggested fields:
- `string Id` (stable unique key)
- `string DisplayName`
- `string Description`
- `string FlavorText`
- `Sprite Icon`
- `bool IsHiddenUntilUnlocked` (optional)
- `AchievementUnlockCondition UnlockCondition` (ScriptableObject reference)

Implementation notes:
- Store achievement definitions under `Assets/Resources/Achievements/`.
- Store achievement conditions under `Assets/Resources/Achievements/Conditions`.

### 2) Evaluation layer (runtime)
Use an abstract condition so new unlock logic can be added incrementally.

Core concepts:
- `AchievementUnlockCondition`
  - `AchievementConditionEvaluationResult Evaluate(AchievementEvaluationContext evaluationContext, AchievementProgressState progressState)`
- `AchievementUnlockCondition` assets
  - Examples: `TotalGoldOwnedAtLeastCondition`, `TotalGoldCollectedAtLeastCondition`, `SceneVisitedCondition`.

Why this helps:
- A new achievement often only needs a new definition asset that references an existing condition asset.
- Truly new behavior only requires adding one new condition ScriptableObject class.

### 3) Tracking and persistence layer
Create a central `AchievementService` responsible for:
- Loading all `AchievementDefinition` assets.
- Tracking unlock status and unlocked timestamps.
- Receiving game events and updating progress.
- Saving/loading progress from local storage.

Data models:
- `AchievementProgressState`
  - `string AchievementId`
  - `bool IsUnlocked`
  - `long UnlockedUnixTime`
  - `int CurrentProgressValue`
- `AchievementSaveData`
  - List/dictionary of progress states.
  - Save version for migration.

Persistence options:
- Start with JSON in `Application.persistentDataPath`.
- Keep a simple schema version to support future data migrations.

### 4) Event integration layer
Feed gameplay actions into `AchievementService` through a lightweight event hub.

Examples of events:
- `OnCollectiblePickedUp(int amount)`
- `OnEnemyDefeated(string enemyType)`
- `OnDistanceTraveledUpdated(float totalDistance)`
- `OnSceneEntered(string sceneName)`

Guidelines:
- Publish events from existing gameplay systems.
- Keep achievement logic out of gameplay classes.
- Batch evaluation where possible to avoid checking every achievement every frame.

## File and folder proposal
- `Assets/Scripts/Achievements/`
  - `AchievementService.cs`
  - `AchievementDefinition.cs`
  - `AchievementEvaluationContext.cs`
  - `AchievementConditionEvaluationResult.cs`
  - `AchievementProgressState.cs`
  - `Conditions/`
- `Assets/Resources/Achievements/`
  - One `.asset` per achievement definition.
- `Assets/Resources/Achievements/Conditions`
  - One `.asset` per achievement condition.

## Suggested implementation phases

### Phase 1: Core data and service
- Add `AchievementDefinition` and progress/save models.
- Build `AchievementService` with load/save and in-memory state.
- Add 2-3 basic condition types for smoke testing.

### Phase 2: Gameplay event hookups
- Add event dispatch points for existing core gameplay actions.
- Connect event handlers to update progress and evaluate unlocks.
- Add unlock notification event from service to UI.

### Phase 3: Achievements UI menu
- Build menu panel and list item prefab.
- Bind UI to service data (including icon, name, description, flavor text).
- Add locked state presentation.

### Phase 4: Content workflow hardening
- Add validation checks (duplicate ID detection, missing icon warnings).
- Create 10+ achievement assets to validate authoring speed.
- Verify save data migration path with schema version.

## Definition authoring workflow for new achievements
1. Duplicate an existing `AchievementDefinition` asset.
2. Assign new ID, name, description, flavor text, and icon.
3. Create a new unlock condition asset and set values on that asset.
4. Assign the new unlock condition to the new achievement definition.
5. Enter Play Mode and trigger relevant gameplay action.
6. Verify menu state.

This workflow should keep most future additions code-free.

## Risks and mitigations
- **Risk**: Duplicate IDs cause overwritten progress.
  - **Mitigation**: Validate all IDs on startup and fail loudly in development builds.
- **Risk**: Performance spikes from frequent full-list evaluation.
  - **Mitigation**: Evaluate only achievements subscribed to a given event type.
- **Risk**: Save schema changes break old progress.
  - **Mitigation**: Include versioned save data and explicit migration steps.

## Acceptance criteria
- At least 5 achievements can be authored using only assets.
- Achievements persist across game restarts.
- Player can open menu and see icon, name, description, flavor text, and unlock status.
- Unlocking an achievement updates menu state immediately.
- No external service or network dependency is required.
