# BetterInputHandling

BetterInputHandling is the template input UX layer for active device detection, dynamic glyphs, contextual prompts, and in-game rebinding. The current implementation lives under `Assets/Scripts/BetterInputHandling` and is structured so it can be extracted into an embedded Unity package later.

## Runtime Pieces

- `BetterInputService`: persistent service for active device detection, action map switching, glyph lookup, contextual prompt state, and binding override persistence.
- `BetterInputSettings`: project asset that points at the input action asset, glyph sets, and controls exposed in the settings menu.
- `BetterInputGlyphSet`: data-driven mapping from normalized controls, such as `escape`, `buttonSouth`, or `leftShoulder`, to project-specific sprites and text fallbacks.
- `BetterInputPromptDetector`: player-side detector that finds nearby prompt providers and publishes the best prompt.
- `BetterInputPromptSource`: simple authoring component for test or basic interactable prompt providers.
- `BetterInputHudInstaller`: template integration that creates the temporary active-device debug text, bottom-left pause prompt, and context prompt under `UIService`'s HUD layer.

## Template Setup

Run `Tools > Better Input Handling > Apply Template Setup` after importing scripts or replacing glyph assets. The setup creates sample glyph sets from `Assets/Graphics/UI Sprites`, creates `Assets/Prefabs/BetterInputHandling/BetterInputService.prefab`, adds it to `Assets/Resources/Config/BootstrapConfig.asset`, repairs the UI input module to use the Starter Assets action asset, adds a prompt detector to `Assets/Prefabs/PlayerContainer.prefab`, and adds prompt test objects to `Assets/Scenes/FlatScene.unity`.

## Glyph Customization

The runtime does not hardcode the sample sprites. To replace glyph art in another project:

1. Import the project's glyph sprites.
2. Create or update `BetterInputGlyphSet` assets.
3. Map controls to sprites using normalized control keys.
4. Assign the glyph sets to `BetterInputSettings`.

Useful normalized keys include `escape`, `e`, `q`, `spacebar`, `leftShift`, `start`, `buttonSouth`, `buttonEast`, `buttonWest`, `buttonNorth`, `leftShoulder`, `rightShoulder`, `leftTrigger`, and `rightTrigger`.

## Prompt Authoring

For simple prompts, add `BetterInputPromptSource` to an object with a collider and set the prompt text, action reference, and priority. For gameplay-specific behavior, implement `IBetterInputPromptProvider` on a project script and keep execution logic in the gameplay layer.

## Controls Menu

`SettingsPopup` builds a tab bar at runtime. The tab bar is flanked by context switching glyphs: `Q` / `E` for keyboard and `LB` / `RB` for gamepad. The Controls tab lists actions from `BetterInputSettings.RemappableActions`, supports interactive rebinding, and persists overrides to `PlayerPrefs`.

## Validation Workflow

Playwright is part of the BetterInputHandling validation loop. After input, pause, settings, glyph, or prompt UI changes, run `npm.cmd run validate:webgl` for a fresh WebGL build or `npm.cmd run test:webgl` against an existing build. The suite drives the title settings entry point, audio-to-controls tab switching, Escape-opened pause menu mouse interaction, and pause-key behavior while settings owns focus. Review the screenshots and traces in `tests/playwright/test-results/` when investigating visual bugs.

Chromium Playwright cannot reliably emulate a physical controller START button for Unity WebGL. For controller-only regressions, keep the closest keyboard/gamepad-path surrogate in Playwright and document the remaining manual hardware check in the bug or PR notes.

## Package Extraction Notes

When extracting this into another project, move the portable scripts and assets into an embedded package, then replace or remove the template-specific pieces:

- Keep: service, settings, glyph sets, prompt interfaces, glyph views, rebinding logic.
- Replace: `BetterInputHudInstaller` if the target project has a different UI shell.
- Rewire: the persistent service bootstrap, player `PlayerInput` registration, settings screen entry point, and prompt detector placement.

## Steam Input

This implementation stays within Unity Input System device and binding abstractions. It does not add Steamworks or Steam Input API dependencies. That keeps Itch.io builds simple and avoids creating extra friction for a future Steam layer, but Steam-specific glyph/action-origin support would still need a dedicated integration later.
