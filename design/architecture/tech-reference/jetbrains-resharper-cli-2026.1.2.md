---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# JetBrains ReSharper Global Tools (`jb`) @ 2026.1.2

## Canonical source
- Official docs: https://www.jetbrains.com/help/resharper/InspectCode.html and https://www.jetbrains.com/help/resharper/CleanupCode.html
- Last verified: 2026-06-07

## API surface used in project
Package `jetbrains.resharper.globaltools` 2026.1.2 (confirmed latest as of 2026-05-19), invoked via the `jb` .NET global tool. Target commands (to be defined in `.asd/project/commands.yaml`):
- `jb-cleanup`: `jb cleanupcode Source\OutfitManager.slnx` — applies the solution code-cleanup profile across the solution.
- `jb-inspect`: `jb inspectcode Source\OutfitManager.slnx -o=".\TestResults\jb-inspect.sarif" --build` — runs ReSharper inspections across the solution.
  - `-o=<path>`: output file; here `.\TestResults\jb-inspect.sarif`.
  - `--build`: rebuild (restore + build) the solution before inspection so the code model and references are accurate.
- Output is SARIF (JSON) by default since version 2024.1; the `.sarif` extension matches.

## Version-specific notes
- 2026.1.2 is the confirmed latest ReSharper Command Line Tools release (2026-05-19).
- `inspectcode` defaults to SARIF JSON output; XML/HTML/text are available via `--format (-f)` but are not used here.
- `inspectcode` default minimum reported severity is Suggestion; it reports Error, Warning, Suggestion, Hint, and Info levels.
- `cleanupcode` default profile is "Built-in: Full Cleanup" (all cleanup tasks except file-header updates). Both tools respect `.DotSettings` and EditorConfig settings — the target ships `Source/OutfitManager.slnx.DotSettings` (the legacy repo currently has no `.slnx.DotSettings`; it is added with the migration).
- Both tools target the `.slnx` solution. The solution must be built (or `--build` passed) so binary references resolve; otherwise parts of the codebase may be skipped or mis-analyzed.

## Deprecations and breaking changes from prior version
- No prior in-project version to migrate from; 2026.1.2 is the established baseline.
- Historical note (informational): SARIF became the default `inspectcode` output in 2024.1; this project assumes SARIF output and parses it for severities.

## Project conventions
- Build/lint sequencing:
  - Run `jb-cleanup` *before* `dotnet build`, applying the solution code-cleanup profile.
  - Run `jb-inspect` *after* `dotnet format` lint, then verify `TestResults/jb-inspect.sarif` contains no `error` or `warning` severity entries. Presence of either is a failure.
- This is a verification gate layered on top of the compiler/analyzer zero-warning policy, not a replacement for it.

## Known issues and workarounds
- If the solution is not built first, `inspectcode`/`cleanupcode` may fail to resolve binary references and silently skip code — `--build` on `jb-inspect` mitigates this; ensure `jb-cleanup` runs after a successful build or restore.
- `jb` results depend on the `.DotSettings`/EditorConfig in the repo; changing those files changes inspection output. Keep `Source/OutfitManager.slnx.DotSettings` authoritative once added.
- The 2026.x CLI is newer than common reference baselines; confirm option names against the installed tool (`jb inspectcode --help`) if a flag behaves unexpectedly.
