---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# FluentAssertions @ 7.2.2

## Canonical source
- Official docs: https://fluentassertions.com/
- Last verified: 2026-06-07

## API surface used in project
OM has no test project yet; this describes the target test setup. FluentAssertions will be imported globally via `<Using Include="FluentAssertions" />` in `Source/OutfitManager.Tests/OutfitManager.Tests.csproj` (no per-file `using FluentAssertions;` directive). The `.Should()` fluent assertion style is to be the project's only assertion mechanism. Expected patterns:
- `value.Should().Be(expected)` for value/equality assertions. Example targets in OM: `Settings.WorkTypeScoreFactor.Should().Be(2f)` (default), normalized weight sums (`WorkTypeHelper.GetNormalizedWorkTypeWeights` returns weights summing to 1f), clamp bounds.
- `.Should()` is an extension method chained from the subject; assertions read as intention-revealing English and produce descriptive failure messages.

## Version-specific notes
- 7.2.2 is the pinned version for this project.
- 7.x is the last fully open-source line and "will remain fully open-source indefinitely and receive bugfixes and other important corrections" per the official site.

## Deprecations and breaking changes from prior version
- N/A within the 7.x line for this project's intended usage (`.Should().Be(...)` is stable). The critical concern is the forward boundary at 8.x — see Known issues.

## Project conventions
- All test assertions use FluentAssertions `.Should()`. Do NOT use NUnit `Assert.*` or `NUnit.Framework.Legacy.ClassicAssert.*`. This is why the NUnit 3->4 assert migration is a non-issue here (see `nunit-4.6.1` reference).
- Imported once globally as `<Using Include="FluentAssertions" />`; do not add redundant per-file `using` directives.
- Test naming + assertion pairing follows `Method_Scenario_ExpectedOutcome` test names with a small number of `.Should()` assertions per test.

## Known issues and workarounds
- LICENSING — DO NOT UPGRADE PAST 7.x WITHOUT A LICENSE DECISION. FluentAssertions 8.x+ (latest confirmed 8.7 as of 2026-06-07) is dual-licensed: free for open-source / non-commercial use, but commercial use requires a paid license from Xceed. The 7.x line stays open-source. This project is pinned to 7.2.2 deliberately. Any bump to 8.x is a licensing decision, not a routine dependency update, and must be escalated for approval (Complication / license review) before adoption. Risk is LOW for current usage, HIGH if upgraded unaware.
- Tooling that auto-bumps dependencies (e.g. package-update PRs) must be prevented from silently moving FluentAssertions to 8.x. Treat the `7.2.2` pin as a hard constraint.
