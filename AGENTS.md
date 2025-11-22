# Agent Guidelines

These instructions apply to the entire repository.

## Coding style
- Prefer descriptive names over abbreviations; avoid single-letter identifiers except in tightly scoped loops.
- Keep Unity `.meta` files in sync with their assets; commit them together.
- Document any new math-heavy calculations with inline comments. Avoid adding new comments around existing logic unless the change introduces new behavior.
- Do not wrap imports in try/catch blocks.

## Unity conventions
- Maintain `ScriptableObject` and scene references when moving assets; verify serialized references after refactors.
- Keep gameplay scripts under `Assets/Scripts/` and scenes under `Assets/Scenes/` unless intentionally reorganizing.
- When adding new input bindings, update `Assets/InputSystem_Actions.inputactions` and regenerate the C# class through the Unity Editor.

## Testing and validation
- Prefer adding Edit Mode tests for pure logic and Play Mode tests for scene interactions.
- When modifying gameplay, validate changes in Play Mode and, when feasible, add or update tests to cover new behavior.

## Documentation and PRs
- Update `README.md` or supplemental docs when adding new systems or dependencies.
- Provide concise summaries in pull requests, including any testing performed.

