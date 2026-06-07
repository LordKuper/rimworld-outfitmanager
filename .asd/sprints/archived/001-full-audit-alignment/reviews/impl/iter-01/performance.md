---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-performance]: APPROVE

# Review — performance

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings at or above floor (low) | — |

## Verdict
APPROVE

Behaviour-neutral migration sprint. Hot path (`ApparelScoring.GetPawnApparelWorkScore`, invoked per pawn per apparel via the `JobGiverPatch` transpiler) verified against all five required checks. No numeric latency/memory/throughput budgets are defined in `custom-coding-rules.md`, so no budget-compliance findings are raised; the perf-relevant rules (hot-path/transpiler stability, caching preservation) are met.

### Verification results

- **Weak-key cache + per-quadrum window preserved (AC-21)** — PASS. `ApparelScoring.cs:24` keeps `ConditionalWeakTable<Apparel, ApparelCache>` (weak keys → destroyed apparel GC'd, no leak). `ApparelCache.cs:23` passes `RimWorldTime.HoursInQuadrum` as the refresh interval; `ApparelCache.cs:74` gates the rebuild on `base.Update(time)`, so the per-quadrum window still short-circuits steady-state calls. Both caching mechanisms intact.
- **C-3 simplification did not regress caching** — PASS, net improvement. `GetWorkTypeScore` (`ApparelCache.cs:41-49`) is now a pure `_workTypeScores.TryGetValue` read; the old miss-path `Settings.WorkTypeRules.FirstOrDefault(...)` was removed. The eager `Update` seeding loop (`:75-83`) is retained and populates every work type in priority order, so the read path is fully covered. The redundant per-key rule lookup is gone from the steady-state read path — fewer ops, not more. Behaviour-neutral (documented by the `ApparelCache_FirstOrDefaultLookupPaths_AreFunctionallyEquivalent` characterization test).
- **GetWorkTypesScore / Update complexity unchanged** — PASS (improved). `Update` is O(workTypes × rules), gated per quadrum — this is the audit's C-5 informational do-not-fix item; left as-is, correctly not touched. `GetWorkTypesScore` (`ApparelCache.cs:59-63`) is now O(weights) dictionary reads plus the weighted sum; the C-3 change removed the worst-case O(weights × rules) miss-path. No complexity regression.
- **No new per-call allocations in the scoring loop** — PASS. The migration added no allocations. The pre-existing `.Sum(w => ...)` LINQ closure (`ApparelCache.cs:62`) and the dictionaries built in `WorkTypeHelper.GetNormalizedWorkTypeWeights` are baseline (per audit), not introduced this sprint, and so are not regressions. C-3 net-removed work from the path.
- **Transpiler injection point** — `JobGiverPatch.cs` is unchanged in shape: narrow 5-opcode anchor, fail-soft on no-match (`:38-44`). No new per-call work injected into vanilla `ApparelScoreRaw`.

### Anti-pattern scan (hot path)
- n+1 / repeated lookups: the only repeated linear lookup is the C-5 per-quadrum rule scan in `Update`, explicitly scoped as informational do-not-fix; not raised.
- sync IO / blocking on UI thread: none on the scoring path.
- unbounded allocation / copy-on-large-collection / deep clone: none. Weak-table cache bounds memory; `_workTypeScores` is cleared and re-seeded per quadrum, not grown.

## Next action
APPROVE — performance reviewer done for iteration 1. No fixes required from this lens.
