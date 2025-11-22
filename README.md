# Unity Template Project

This repository is a Unity 6 (6000.0.61f1) starter project with URP, Input System, Cinemachine, Addressables, and supporting tooling already wired in. It is intended to be forked to bootstrap new prototypes while keeping baseline systems consistent.

## Requirements
- **Unity Editor**: 6000.0.61f1 (see `ProjectSettings/ProjectVersion.txt`).
- **Render Pipeline**: Universal Render Pipeline (URP 17.0.4).
- **Platforms**: Default build target is standalone (set in the Editor as needed).

## Getting started
1. Install Unity 6000.0.61f1 via Unity Hub.
2. Clone or fork this repository.
3. Open the project in Unity Hub; the Editor will import packages defined in `Packages/manifest.json`.
4. Open one of the sample scenes:
   - `Assets/Scenes/FlatScene.unity` — minimal URP flat demo with collectibles.
   - `Assets/Scenes/ThirdPersonTempScene.unity` — third-person starter using Starter Assets.
   - `Assets/Scenes/ThirdPersonBiggerScene.unity` — expanded third-person layout.
5. Enter Play Mode to verify input, collectibles, and UI.

## Gameplay scaffolding
- **Collectable (`Assets/Scripts/Collectable.cs`)**: Coin or pickup that increments a collector on trigger, then despawns itself.
- **Collector (`Assets/Scripts/Collector.cs`)**: Maintains a running count of collected items and updates the UI via `UIManager`.
- **UIManager (`Assets/Scripts/UIManager.cs`)**: Singleton that prints the coin total to a TextMeshPro label.
- **Rotator (`Assets/Scripts/Rotator.cs`)**: Applies a per-axis rotation speed each frame (useful for spinning pickups).
- **Input actions**: Located at `Assets/InputSystem_Actions.inputactions`; bind gameplay input here.
- **URP settings**: `ProjectSettings/URPProjectSettings.asset` controls pipeline-level defaults; per-scene settings live in `Assets/Settings/`.

### Wiring tips
- Place `Collectable` on a trigger collider and set its **Amount** in the Inspector.
- Add `Collector` to the player object; ensure the collider that enters collectibles references this component.
- Add `UIManager` to a UI Canvas with a `TextMeshProUGUI` field assigned to **Amount Text**.
- Use `Rotator` on any object that should spin; set **Rotation Speed** per axis.

## Package overview
Key dependencies from `Packages/manifest.json`:
- **Gameplay/UX**: Input System, Cinemachine, AI Navigation, Addressables, ProBuilder, Timeline, TextMeshPro, PrimeTween (vendored at `Assets/Plugins/PrimeTween/internal/com.kyrylokuzyk.primetween.tgz`).
- **Graphics**: URP, Shader Graph, Light Transport, URP Config.
- **Testing**: NUnit extensions, Unity Test Framework, Performance Test Framework.

PrimeTween is bundled via a local tarball to keep the project self-contained. To upgrade it, replace the tarball and update the version line in `Packages/manifest.json`.

## Testing
- Edit Mode and Play Mode tests can be run with the Unity Test Runner.
- CLI example: `Unity -batchmode -nographics -quit -projectPath . -runTests -testResults Logs/EditModeResults.xml -testPlatform EditMode`.
- Performance tests use `com.unity.test-framework.performance`; add new tests under a `Tests` folder when creating scenes or scripts.

## Continuous integration
A GitHub Actions workflow (`.github/workflows/unity-tests.yml`) is provided to run Edit Mode and Play Mode tests in batch mode. It requires a `UNITY_LICENSE` secret configured in the repository to activate Unity in CI. The jobs are skipped automatically if the license is not present.

## Common tasks
- **Add inputs**: Open `Assets/InputSystem_Actions.inputactions`, edit actions and bindings, and regenerate C# code if needed.
- **Adjust URP**: Update assets in `Assets/Settings/` and global defaults in `ProjectSettings/URPProjectSettings.asset`.
- **Scene setup**: Add new scenes under `Assets/Scenes/` and include them in build settings.
- **Addressables**: If you add addressable assets, create groups via the Addressables window; settings will be stored under `Assets/AddressableAssetsData/` when created.

## Notes for contributors and AI agents
- Keep `.meta` files in sync with assets; commit them together.
- Favor clear class and variable names over abbreviations.
- When adding math-heavy logic, document calculations inline for clarity.
- Use Play Mode to validate new interactions before committing.

