[REVIEW-design-external]: CONCERNS

# External Review Report

- **Phase**: design-review
- **Iteration**: 1
- **Severity floor (this iter)**: low (keep all)
- **External tool**: Codex CLI (codex-cli 0.130.0), invoked per `system.os=windows`
- **Artifacts reviewed**: `design/prd.html` (33 AC), `design/adr.html` (3 ADRs, all proposed)

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | adr.html : ADR-0001 (Published-mod flag) | ADR-0001 recommends pairing the fail-soft transpiler with a "visible patch-applied / patch-failed startup signal." This is in tension with the PRD non-goal "no UX/UI change" and "no new user-facing UI string is introduced" (and AC-29's keying obligation) — unless "visible" means Logger/dev-console output only rather than a player-facing UI surface. As worded it is ambiguous whether a new user-facing string is implied. | Clarify in ADR-0001 that the patch-applied/failed signal is non-user-facing diagnostic output routed through the project `Logger` (conformant with AC-30, no new Keyed string, no UI change) — OR, if it is genuinely user-facing, explicitly flag it under AC-33 with the `Resources.Strings` + Keyed XML obligation (AC-29) and the published-mod communication implication. (Codex finding: severity=medium.) |

## Dropped findings (below severity floor)

None. Iteration-1 floor is `low`; all findings are kept.

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| — | — | — | — | — |

## Dropped findings (nitpick)

None reported by Codex.

| # | Location | Description | Drop reason |
|---|---|---|---|
| — | — | — | — |

## Severity mapping applied

Codex → ASD: `medium` → `medium` (1 finding). No `blocker/critical` (→critical), `major` (→high), or `info/suggestion` (→low) findings were returned.

## Verdict
CONCERNS: 1

Codex returned `CONCERNS: 1`. One `medium` finding survives the iteration-1 floor (low). The finding is an internal-consistency catch between ADR-0001 and the PRD UX non-goal — autofixable by the responsible creator (asd-architect) without escalation (a one-line clarification that the patch signal is Logger-only, or an explicit AC-33 flag if user-facing). It does not require a concept/PRD-contract change or new abstraction, so it does not trip the escalation triggers in `review-policy.md`.

## Next action
asd-architect autofixes ADR-0001 within the design-review loop — clarify the patch-applied/failed signal as non-user-facing `Logger` output (or flag it under AC-33/AC-29 if user-facing) — then design-review advances to iteration 2. External Review re-runs fresh on iter-2 with the medium floor.
