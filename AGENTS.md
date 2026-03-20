# Project rules

## Environment
- This is a Unity project targeting Windows first.
- Prefer Windows-native commands and PowerShell examples.
- Do not assume WSL paths.

## Change safety
- Prefer minimal, high-confidence diffs.
- Do not edit Packages/ProjectSettings unless the task requires it.
- Preserve .meta files and GUIDs.
- Avoid reserializing scenes/prefabs unless necessary.
- Call out any manual Unity Editor steps clearly.

## Code conventions
- Prefer small focused classes and minimal public surface area.
- Match existing naming and asmdef organization.
- Do not add new dependencies without asking.
- Prefer descriptive names over abbreviations; avoid single-letter identifiers except in tightly scoped loops.
- Keep Unity `.meta` files in sync with their assets; commit them together.
- Include descriptive summaries of added classes.
- Code comments should be on their own line and placed above the relevant code.
- Document any new math-heavy calculations with comments. 
- Avoid adding new comments around existing logic unless the change introduces new behavior.

## Unity conventions
- Maintain `ScriptableObject` and scene references when moving assets; verify serialized references after refactors.
- Keep gameplay scripts under `Assets/Scripts/` and scenes under `Assets/Scenes/`.

## Documentation and PRs
- Update `README.md` or supplemental docs when adding new systems or dependencies.
- Write commit messages in past tense. Include punctuation at the end of commit messages.
- Provide concise summaries in pull requests, including any testing performed.

## Testing
- Prefer EditMode or pure C# tests first.
- Use PlayMode tests only when runtime behavior is required.
- After changing gameplay code, run the relevant test action if available.
- If no automated test exists, propose one.

## Output expectations
- Summarize files changed, risks, and validation performed.
- Flag anything that still requires manual editor verification.

