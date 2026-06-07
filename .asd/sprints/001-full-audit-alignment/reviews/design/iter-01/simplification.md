---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-design-simplification]: APPROVE

# Review — simplification

- **Phase**: design-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | adr.html ADR-0001 (Published-mod flag para + R-C1 mitigation) | The "visible patch-applied/patch-failed startup signal" is *recommended* in ADR-0001 but is not bound to any AC and not in PRD scope. It is a sound, low-cost mitigation for silent transpiler disablement (R-C1), but as worded ("should be paired with… a visible patch-applied / patch-failed signal logged at startup") it floats outside the committed scope. Risk is only that an unbounded "signal" grows into a patch-self-check subsystem on a 640-LOC mod. | Keep it as a single startup `Logger` log line (patch applied vs pattern-not-found), reusing the existing fail-soft `Logger.LogError` path — no new self-check class, no new state, no settings surface. If it is to be a deliverable, attach it to an AC explicitly so scope is bounded; otherwise leave it as an ADR note. Category: **keep-as-is** (do not expand). |
| 2 | low | prd.html AC-24, AC-25 (W6 isolation/resolver infra) | `StateIsolationTestBase` + `RimWorldAssemblyResolverFixture` is genuine new infrastructure. It is justified (real static state: `_isInitialized`, `ConditionalWeakTable`, static `Settings`) and mirrors the proven EM pattern rather than inventing one, so it is NOT over-engineering at the design level. Noting only so impl-review holds it to the minimum: build only the snapshot/restore + resolve hooks the initial AC-26 tests actually exercise. | No design change. Category: **keep-as-is**. Watch at impl-review that no resolver/isolation feature is added that no test consumes (would trip "abstraction with no second use case"). |

## Verdict
APPROVE

## Next action
Reviewer done. No escalation, no creator fix required. The two low findings are advisory keep-as-is notes (build floor below severity floor=low is still reported per iter-1 rule, but neither blocks DoD nor requires a fix). Carry the AC-24/25 proportionality note forward into impl-review.

## Escalations (optional)
- none

## Notes (proportionality confirmation — not findings)

The design is proportionate to a ~640-LOC published mod doing a legacy→target alignment. Over-engineering checklist swept clean:

- **No new abstraction/interface/generic/factory/plugin.** ADR-0001 explicitly states "No new dependency or abstraction"; ADR-0002 is pure toolchain migration; ADR-0003 net-removes an attribute (the lone `[NotNull]`) and adds none.
- **No premature config flag.** The only tunable (`WorkTypeScoreFactor`) is pre-existing and UI-exposed; no new flag is introduced.
- **No gold-plating of the migration.** Out-of-scope list (non-goals) correctly excludes EM-only systems (StatDefs, loadout/weapon scoring, SimpleSidearms) and optional-mod integrations — no horizontal scaffolding for features OM lacks.
- **W4 reuse is a no-regression gate, not a delete campaign.** AC-16 documents the confirmed PASS rather than forcing a deletion, matching audit RU-1. Correct.
- **C-5 "do-not-fix" honoured.** W5 ACs (AC-18 C-3 redundant lookup, AC-19 C-4 init-flag order) touch only genuine issues; AC-21 explicitly guards the `ConditionalWeakTable`/quadrum window against being "optimized into a regression" — the design proactively refuses to optimize a non-problem.
- **AC-19 (C-4)** is a behaviour-neutral 2-line flag-ordering fix for a low-likelihood fragility — it reduces latent risk without adding complexity. Proportionate.
- **AC-27 (transpiler test)** is correctly hedged ("characterization-tested where the resolver allows, or documented as integration-verified") — no mandate to build heavy IL-test scaffolding.
- **ADR-0001 nullable/Harmony decisions reject the heavier alternatives** (wholesale method replacement, blanket `= null!`, incremental multi-sprint split) on KISS grounds with stated reasoning. Simplicity Default upheld.
