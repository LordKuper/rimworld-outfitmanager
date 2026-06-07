---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-simplification]: APPROVE

# Review — simplification

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | `Source/OutfitManager.Tests/*Tests.cs` (11 `[Ignore]` bodies) | Eleven empty-bodied `[Ignore]`d test methods stand in for live-game coverage (AC-26/AC-27 deferred to MS-3). Verges on "scaffolding in case we need it", but each carries a concrete coverage contract, an AC anchor, and a `TODO(sprint-001): MS-3` enable trigger — so they encode a real deferred-coverage obligation rather than dead speculation. `keep-as-is` at iter-1; if MS-3 in-game verification is dropped, these must be deleted, not left to rot. | None this iter. Track that each `[Ignore]` is wired to MS-3; on MS-3 cancellation, delete rather than keep. |

<!-- All checklist items below were evaluated and did NOT trip — recorded for the undroppable-findings audit trail: -->
<!-- interface w/ one impl: none. generic w/ one type: none. factory <3 classes: none. plugin/no-plugin: none. abstraction/no 2nd use: none. premature config flag: none. defensive-impossible: none (snapshot special-casing targets real fields). stdlib-wrapper helper: none. inheritance depth ≥3: max 1 (StateIsolationTestBase → 3 fixtures). framework-wrapping-framework: none. mock-of-mock: none (no mocks at all). comment-restates-code: none (GetWorkTypeScore doc is rationale, not restatement). dead code: none. -->

## Verdict
APPROVE

Lens-by-lens assessment (all `keep-as-is`):

- **`WorkTypeHelper.NormalizeWorkTypeWeights` seam** — `keep-as-is`. The `internal` method was carved out of `GetNormalizedWorkTypeWeights(Pawn)` to expose the pure normalization math (no `Pawn`, no `DefDatabase`) for the only branches that are unit-testable (`wpMin==wpMax`, empty-set, single, varying — AC-26). This is the **minimum** seam that makes the math testable, not an over-split: the public overload keeps its single responsibility, the `internal` method keeps its own, and 5 real `[Test]`s exercise it. No abstraction-without-2nd-use smell — the seam earns its weight via AC-26 directly.
- **Test isolation base (reflection snapshot/restore)** — `keep-as-is`. Mandated verbatim by AC-24 (snapshot/restore `ApparelScoring._isInitialized`, the `ConditionalWeakTable`, static `Settings` fields). It is a single `abstract` base with 3 concrete fixtures (not an interface, not generic, no factory). The `List<>`-clone and `ConditionalWeakTable`-sentinel special-casing maps 1:1 to real static fields that genuinely bleed between tests; it is contracted state handling, not defensive code for impossible cases. Did NOT balloon — no test DI container, no custom mock layer, no builder pattern. The design-review "hold test infra to minimum" warning was honored.
- **C-3 / C-4 simplifications** — confirmed net complexity reduction, not addition. C-3 collapses the duplicated `Settings.WorkTypeRules.FirstOrDefault(...)` lookup (`GetWorkTypeScore` now reads the already-seeded dict; absent-key→0f preserves the old miss-path result). C-4 moves `_isInitialized=true` after `InitializeStatRanges()` so a mid-init failure retries instead of returning early. Both remove a path or a fragility. Good.
- **`Directory.Build.props` / csproj migration** — standard SDK-style migration; nothing over-engineered.

No over-engineering checklist item trips. No `critical` findings. One `low` note recorded (above) per the iter-1 low floor.

## Next action
APPROVE — reviewer done for this lens. PM may proceed once all impl-review reviewers APPROVE the same iteration. No fix routed back to `impl` from this lens.

## Escalations (optional)
- None. No finding requires Complication Approval; no reviewer-proposed fix from this lens adds an abstraction, layer, or dependency.
