# Agent Guidelines

These instructions apply to the entire repository.

## Coding style
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

