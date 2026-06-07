[REVIEW-impl-quality]: APPROVE

# Review — quality

- **Phase**: impl-review
- **Iteration**: 1

## Scope verified

Focus areas confirmed clean:

- **C-3 dead-path removal (`ApparelCache.GetWorkTypeScore`)** — behaviour-equivalent. `Update` (ApparelCache.cs:72-86) eagerly seeds **every** `WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder` entry into `_workTypeScores`. `WorkTypeHelper.GetNormalizedWorkTypeWeights` (WorkTypeHelper.cs:21-40) only emits keys drawn from that same `WorkTypeDefsInPriorityOrder` source, so every weight key is guaranteed present before `GetWorkTypeScore` runs. The removed lazy `FirstOrDefault` lookup was genuinely dead; the simplified getter's miss-path (`TryGetValue ? cached : 0f`) returns the same `0f` the old miss-path produced. Verified.
- **C-4 init-flag reorder (`ApparelScoring.Initialize`)** — correct. `_isInitialized = true` now set AFTER `InitializeStatRanges()` (ApparelScoring.cs:91-96); a throw mid-seed leaves the flag clear so the next call retries from scratch. No reentrancy hazard: `InitializeStatRanges` does not recursively re-enter `ApparelScoring.Initialize`. (Note: `Settings.Initialize` sets its flag first — that is required there to break the `Initialize → InitializeWorkTypesSettings → WorkTypeRules getter → Initialize` recursion, so it is correct, not the same anti-pattern.)
- **Nullable migration** — clean. `_workTypeRules` declared `List<…>? = []` with `??=` restore in both `InitializeWorkTypesSettings` (Settings_WorkTypes.cs:102) and the `WorkTypeRules` getter (line 73), matching the Scribe-empty-load-null contract. `_selectedWorkTypeRule` legitimately `WorkTypeThingRule?` with guards. No `= null!` masking observed. Lone `[NotNull]` removed from `ApparelCache` (now plain `Apparel` param; base ctor null-guard preserved per ADR-0003). `[UsedImplicitly]` retained on `OutfitManagerMod`/`JobGiverPatch`/`Settings`. No JetBrains nullability attribute / NRT coexistence. `Logger` uses `Exception?`. Both csproj have `Nullable enable` + `TreatWarningsAsErrors`.
- **Transpiler fail-soft contract (`JobGiverPatch`)** — preserved. On pattern miss: `Logger.LogError(...)` + `return code` (original IL), never throws (JobGiverPatch.cs:38-44). Loop bound `i < code.Count - 4` safely indexes up to `code[i+4]` (no off-by-one). Anchor keyed on the `GetSpecialApparelScoreOffset` callvirt per ADR-0001. Harmony pinned 2.4.2.

No bugs, security issues, injection, secret leakage, resource leaks, or ADR contract drift found. `ConditionalWeakTable` weak-key cache and `Exception?`-carrying `Logger` are sound.

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | Source/OutfitManager.Tests/RimWorldAssemblyResolverFixture.cs:5,14 | Custom coding rule requires the RimWorld `[SetUpFixture]` to be **namespace-less (global)** so it runs before any RimWorld-typed type in any namespace loads. Here it sits inside `namespace LordKuper.OutfitManager.Tests`. It functions today because all test fixtures live under that namespace tree, but the global guarantee is lost if a fixture is ever added outside it. | Move `RimWorldAssemblyResolverFixture` out of the namespace (top-level/global) per the house standard, or document why namespace scope is sufficient. |
| 2 | low | Source/OutfitManager.Tests/ApparelScoringTests.cs:69-83 | Stale doc on `ApparelScoring_InitializeFlagIsSetImmediately`: the test name and inline comment ("sets `_isInitialized` only after full init completes" vs. "Document current flag-setting order (**before** InitializeStatRanges)") contradict each other, and the name "...IsSetImmediately" contradicts the C-4 fix (flag now set AFTER seeding). Misleading once the test is un-ignored. | Rename to reflect "set after init completes" and drop the "before InitializeStatRanges" comment so the characterization matches the shipped behaviour. |
| 3 | low | Source/OutfitManager/Settings_WorkTypes.cs:104-109 | In the dedup loop, when a rule is dropped (`WorkTypeDefName == null` or duplicate) the subsequent `foreach (var statWeight in rule.StatWeights) statWeight.Protected = false;` still executes on the just-removed rule — wasted work and slightly confusing control flow. Harmless (removed rule is discarded) but reads as an oversight. | `continue` after `_workTypeRules.Remove(rule)` so the stat-weight reset only runs for retained rules. |

## Verdict
APPROVE

All four flagged focus areas verified correct and behaviour-equivalent. The three findings are all `low` (doc/test-naming staleness and one cosmetic control-flow redundancy); none is a bug, security, or contract issue. The 13 live-context `[Ignore]` tests and no-UI scope are KNOWN/EXPECTED, not defects.

## Next action
Reviewer done (APPROVE). The three low findings are optional polish the dev may fold into a follow-up; none blocks DoD. Per severity floor, all are reported (iter 1 = low) but do not lower the verdict.

## Escalations (optional)
- None.
