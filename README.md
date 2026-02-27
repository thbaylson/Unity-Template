# Unity Template Project

This repository is a Unity 6 (6000.0.61f1) starter project with URP, Input System, Cinemachine, ProBuilder, and supporting tooling already wired in. It is intended to be forked to bootstrap new prototypes while keeping baseline systems consistent.

## Requirements
- **Unity Editor**: 6000.0.61f1 (see `ProjectSettings/ProjectVersion.txt`).
- **Render Pipeline**: Universal Render Pipeline (URP 17.0.4).
- **Platforms**: Windows and Web.

## Getting started
1. Install Unity 6000.0.61f1 via Unity Hub.
2. Clone or fork this repository.
3. Open the project in Unity Hub; the Editor will import packages defined in `Packages/manifest.json`.
4. Open one of the sample scenes:
   - `Assets/Scenes/FlatScene.unity` — minimal URP flat demo with collectibles.
   - `Assets/Scenes/BiggerScene.unity` — expanded third-person layout.
   - `Assets/Scenes/ProBuilderInterior.unity` — a simple interior layout using Pro Builder Shapes.
5. Enter Play Mode to verify input, collectibles, and UI.

## Gameplay scaffolding
- **Bootstrapper (`Assets/Scripts/Bootstrap/Bootstrapper.cs`)**: A runtime system that guarantees core game systems are created exactly once and are available no matter which scene a team member enters Play Mode from.
- **UIService (`Assets/Scripts/UIService.cs`)**: Singleton instance to manager the UI. A prefab of the same name contains the one and only canvas for the game.

### Wiring tips
- New UI elements should be saved as prefabs and wired up to the UIService. This requires them to have the UILayerAttachment component ('Assets/Scripts/UI/UILayerAttachment.cs') and be added to the bootstrap config ('Assets/Resources/Config/BootstrapConfig.asset').
- New services and managers should be added to the bootstrap config, more on that below.

## Package overview
Key dependencies from `Packages/manifest.json`:
- **Gameplay/UX**: Input System, Cinemachine, AI Navigation, ProBuilder, Timeline, TextMeshPro.
- **Graphics**: URP, Shader Graph, Light Transport, URP Config.

## Common tasks
- **Scene setup**: Add new scenes under `Assets/Scenes/` and include them in build settings.
- **Adding Services**: Add new services sparingly. New services should only be added if it is determined that it's required in all scenes. In such a case: add the service to the service locator, 'Assets/Services/Services.cs' as well as to the bootstrap config, 'Assets/Resources/Config/BootstrapConfig.asset'.
- **Adding Per-Scene Managers**: Create the new manager as a prefab. Regester the manager prefab with the bootstrap config.

## Notes for contributors and AI agents
- Keep `.meta` files in sync with assets; commit them together.
- Favor clear class and variable names over abbreviations.
- When adding math-heavy logic, document calculations for clarity.
- Use Play Mode to validate new interactions before committing.

## Credits
- Music by Abstraction: https://tallbeard.itch.io/music-loop-bundle 
