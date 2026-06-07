[REVIEW-design-documentation]: CONCERNS

# Review — documentation

- **Phase**: design-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | adr.html L7-8, L21-22 (doc-level provenance) vs L136, L148-149 (ADR-0001 badges) | Provenance is contradictory. The document-level shell metadata declares `provenance: original` / `<meta name="provenance" content="original">` / `<meta name="source" content="">`, but the rendered header badge and ADR-0001 `.meta` declare `reverse-engineered (ADR-0001, from Source/Patches/JobGiverPatch.cs + ApparelScoring.cs)` with a source. A tool parsing `<meta provenance>` and a human reading the badge get different answers — internal SSoT conflict on the provenance fact. In a multi-ADR sprint draft, provenance is mixed (ADR-0001 reverse-engineered, ADR-0002/0003 original) yet the shell carries a single doc-level `original`. Per `artifact-layout.md`, when provenance != original, `source` must be set; the doc-level `<meta source>` is empty while ADR-0001 has a source. | Make the provenance fact have one home. Either: set the doc-level shell provenance/source to reflect that the file is mixed (and keep the authoritative per-ADR provenance/source only in each `<article>` `.meta`, with doc-level `<meta provenance>` removed or marked `mixed`), or split ADR-0001 into its own reverse-engineered file. At minimum, reconcile `<meta provenance>`/`<meta source>` with the per-ADR badges so the two sources agree. |
| 2 | medium | adr.html L18, L135, L145, L200, L256 (all status=proposed) vs audit.md L261, L263 | Status drift between ADR draft and the audit recommendation it traces from. The audit explicitly recommends ADR-0001 `accepted` (reverse-engineered — documents already-shipped transpiler behaviour) and ADR-0003 `accepted`, with only ADR-0002 `proposed`. The draft sets all three to `proposed`. Labelling ADR-0001 (a decision reverse-engineered from production code, mod id 1984694952) as `proposed` mislabels a decision that is already in force, weakening traceability between audit findings and the recorded decision state. | Either align ADR-0001 (and ADR-0003 per audit) to `accepted`, or, if the team intends to re-ratify all three this sprint, record that intent so the divergence from the audit's recommended statuses is deliberate rather than an oversight. Update both the `<meta status>` and each article's `status-chip`/badge consistently. |
| 3 | low | adr.html L18, L139 (doc-level `status=proposed`) vs L256 (ADR-0003 article) | `<meta name="status" content="proposed">` and the stats strip (`3 decisions · proposed`) assert a single document status, but status is per-ADR in this multi-ADR draft and finding #2 may make them diverge (e.g. 0001/0003 accepted, 0002 proposed). A single doc-level `proposed` would then misrepresent the set. | If statuses diverge per finding #2, set the doc-level `<meta status>`/stats to reflect the design-phase wrapper state (`draft`/`in-review`) per `artifact-layout.md` `{{STATUS}}` rules and let the per-article chips carry the ADR-specific status, rather than asserting one ADR status doc-wide. |

## Verdict
CONCERNS: 3

## Next action
Creator (asd-architect) autofixes findings #1–#3 within the design-review loop, then iteration advances. All three are reconciliation/labelling fixes within the ADR draft (own artefact); none require user escalation. PRD draft is clean: SSoT respected (downstream stats/traceability link, not copy), responsibility frontmatter present and honoured, provenance `original` with badge correctly omitted, story↔AC traceability complete (US-1…US-7 all mapped, all 33 AC covered), AC→ADR mapping holds (AC-12/AC-33→ADR-0001/0003, AC-8…AC-14/AC-28/AC-31/AC-32→ADR-0002), custom-design-rules transpiler/fail-soft + data-driven + compat-flag requirements all satisfied (ADR-0001 captures fail-soft + compat risk; AC-33 + per-ADR published-mod flags present). Absence of ux-spec/requirements.html correctly treated as in-scope non-goals (no finding). HTML shell wrapping intact on both docs; no bare fragments, no duplicated chrome.

## Escalations (optional)
- none
