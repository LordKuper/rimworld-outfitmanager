[REVIEW-impl-external]: CONCERNS

# External Impl Review — iter-01 (Codex CLI)

- Phase: impl-review
- Sprint: 001-full-audit-alignment (OutfitManager)
- Iteration: 1 — severity floor: **low+** (keep all)
- External reviewer: Codex CLI (codex-cli 0.130.0), available; invoked once, exit 0.
- Diff payload: `git diff master...HEAD` over `Source/` (production + tests), binary `.dll` excluded.

Codex returned `CONCERNS: 3`. All three findings validated against the diff, all valid, none in a nitpick category, all at or above the iter-1 floor. Severities below are the ASD-scale values Codex emitted directly (prompt constrained Codex to the ASD enum); mapping table: blocker/critical→critical, major→high, minor→medium, info/suggestion→low.

## Kept findings

| # | Sev | Location | Description | Fix | Codex source |
|---|-----|----------|-------------|-----|--------------|
| K1 | medium | `Source/OutfitManager.Tests/RimWorldAssemblyResolverFixture.cs:5` (the `namespace LordKuper.OutfitManager.Tests;` declaration) | The RimWorld assembly-resolver `[SetUpFixture]` is declared inside a namespace. The house rule requires a namespace-less (global) `[SetUpFixture]` so the `AppDomain.AssemblyResolve` handler is registered before any RimWorld-typed test loads. A namespaced `[SetUpFixture]` only scopes to its own namespace tree, narrowing the guarantee (AC-25). | Move `RimWorldAssemblyResolverFixture` to the global (file-less) namespace. | codex finding 1 |
| K2 | medium | `Source/OutfitManager.Tests/RimWorldAssemblyResolverFixture.cs:23` (`GetCustomAttribute<AssemblyMetadataAttribute>()`) | Reads a single `AssemblyMetadataAttribute`. SDK builds can emit multiple `AssemblyMetadata` attributes; `GetCustomAttribute<T>()` then throws/returns an arbitrary one, so the `"RimWorldManagedDir"` key check can spuriously fail at test startup depending on attribute order. | Use `GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault(a => a.Key == "RimWorldManagedDir")` (key-based lookup). | codex finding 2 |
| K3 | low | `Source/OutfitManager.Tests/ApparelScoringTests.cs:53` (also `:150`,`:167` and `WorkTypeHelperTests.cs` `TODO(sprint-001)` comments) | Test names/comments/`[Description]` cite internal rule/task ids (`C-3`, `C-4`, `MS-3`) in non-TODO comments — e.g. `Prove C-3 consolidation`, `[Description("CHARACTERIZATION: FirstOrDefault rule lookup consolidation is behaviour-neutral")]`. The self-contained-code rule forbids ADR/PRD/rule/sprint ids in code/comments; only forward-looking `TODO` may cite a sprint/issue. The bare `TODO(sprint-001): … (MS-3)` lines are permitted; the `C-3`/`C-4` characterization references are not. | Drop `C-3`/`C-4`/`MS-3` (in non-TODO text) from test names, comments, `[Description]`, and `[Ignore]` reasons; describe the behaviour directly. Keep forward-looking `TODO(sprint-001)` lines as-is. | codex finding 3 |

## Dropped (below floor)

None — iter-1 floor is low+; all findings qualify.

## Dropped (nitpick)

None — Codex emitted no wording/style/opinion-only items; the brace-style and blank-line churn throughout the diff (lint-managed) was correctly not reported.

## Reviewer note (not a Codex finding, recorded for the dispatching skill)

Worth a second internal-reviewer look: in `Source/OutfitManager/Settings_WorkTypes.cs` the `WorkTypeRules` getter hardens against a Scribe null-load with `_workTypeRules ??= []`, but `InitializeWorkTypesSettings()` dereferences `_workTypeRules` directly (`.ToList()`, `.Remove(...)`, `.Add(...)`) without the `??=` guard. This is null-safe only if `InitializeWorkTypesSettings()` is never reached with `_workTypeRules == null` (i.e. the field initializer `= []` always wins before any Scribe overwrite path runs Initialize). That ordering invariant holds in the current call graph, so this is informational rather than a finding — flagged only so the internal reviewer can confirm the invariant.

## Per-severity accounting

- critical: 0
- high: 0
- medium: 2 (K1, K2)
- low: 1 (K3)
- Total kept: 3 | Dropped (floor): 0 | Dropped (nitpick): 0

## Verdict

**CONCERNS** — 2 medium + 1 low, all in the new test infrastructure (`OutfitManager.Tests/`). No findings against the production code, the stack migration, the C-3/C-4 simplifications, the transpiler fail-soft contract, or Scribe save-shape stability. No blockers.

## Next action

Address K1 (global-namespace `[SetUpFixture]`) and K2 (key-based `AssemblyMetadata` lookup) — both are correctness/robustness issues in the test resolver. K3 is a low-severity rule-conformance cleanup of test comments. Re-review not required for K3 alone; the dispatching skill aggregates with internal reviewers.
