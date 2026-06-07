---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-documentation]: CONCERNS

# Review — documentation

- **Phase**: impl-review
- **Iteration**: 1

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | `Source/OutfitManager.Tests/ApparelCacheTests.cs:7,18,30,42,54`; `ApparelScoringTests.cs:20,32,44`; `WorkTypeHelperTests.cs:117,130,143` | Self-contained-code rule violation (`custom-coding-rules.md` "Self-contained code"): test code references the ASD manual-step artifact id `MS-3` in `[Ignore("…(MS-3 …)")]` attribute reason strings and one plain `///` doc comment (`ApparelCacheTests.cs:7`). These are NOT forward-looking `TODO`/`FIXME` comments, so they fall outside the rule's only carve-out. The dev removed `AC-N` labels (confirmed: zero `AC-N` in source/tests) but the `MS-N` references — an ASD artifact of the same class — remain. Code must be self-sufficient without ASD docs. | Replace each `MS-3` citation with the *why* stated directly. `[Ignore]` reason → `[Ignore("Requires live RimWorld Pawn + workSettings context")]`; the `///` comment at `ApparelCacheTests.cs:7` → drop the `(MS-3)` reference and state the deferral reason directly. The `TODO(sprint-001): …` doc comments may keep the `sprint-001` sprint id (TODO carve-out permits a sprint/issue ref) but should drop the bare `MS-3` artifact id from the TODO text. |
| 2 | low | `design/architecture/tech-reference/lordkuper-common-1.6.md:52` and `:72` | Minor actuality drift on the `DoWidgetTab` description. The doc states OM "passes `null`" for the trailing `mapThings` parameter. The Common 1.6 signature declares it optional (`IReadOnlyList<Thing>? mapThings = null`), and OM's call site (`Settings_WorkTypes.cs:83-86`) **omits** the argument, relying on the default — it does not pass `null` explicitly. Functionally identical (value is null either way), but the doc's wording does not match the call site. The "gained two trailing parameters … consumed by OM" framing is also slightly off: only the first trailing param (`ref mapThingIconBoxScrollPosition`) is consumed/passed; the second is defaulted. | Reword to: OM passes its own `ref _workTypesMapThingIconBoxScrollPosition` and **relies on the default** for the optional `mapThings` (= `null`), rather than "passes `null`". |

## Verdict
CONCERNS: 2

## Next action
impl-review routes the sprint back to `impl` (fix mode). The Test Engineer / dev resolves finding #1 (strip `MS-3` ASD-artifact citations from test `[Ignore]` strings and the one non-TODO comment); the Architect resolves finding #2 (reword `lordkuper-common-1.6.md` `DoWidgetTab` note to match the actual default-argument call site). Sprint re-enters impl-review. Neither finding requires user escalation.

## Verification summary (passing checks, for traceability)

The DM-1 / DM-2 reconciliations and the core actuality claims were verified and are **correct**:

- **DM-1 — stack.html as-built**: `stack.html` now describes the modern stack as AS-BUILT (Migration history reframed as "history"; "Modern stack is as-built" constraint; `Nullable enable` / SDK-style / `.slnx` / `PackageReference` / Harmony 2.4.2 stated as current). Verified against `Source/OutfitManager/OutfitManager.csproj` (`<Nullable>enable</Nullable>`, `Lib.Harmony` 2.4.2 `PackageReference` `PrivateAssets=all`/`ExcludeAssets=runtime`, `TreatWarningsAsErrors=true`/`WarningLevel 9999`, `InternalsVisibleTo` Tests, NetAnalyzers 9.0.0), `Source/OutfitManager.slnx` present, `Source/OutfitManager.Tests/` present. No legacy/target framing remains stale.
- **DM-2 — status & nullable note**: `concept.html` and `stack.html` both `status=approved` (meta + badge). `csharp-net48.md` carries the "nullable enabled project-wide" note (as-built, both projects). Concept Pillars/Anti-Pillars/Constraints still hold — no behavioral change; transpiler/Common/1.6/CC-BY-NC-SA constraints match code.
- **Nullable / JetBrains attrs**: grep confirms **zero** `[NotNull]`/`[CanBeNull]`/`[ItemNotNull]` in `Source/`. The lone `[NotNull]` (formerly `ApparelCache.cs`) is gone; `ApparelCache` ctor param is now plain non-nullable `Apparel apparel`. Scribe-list field `_workTypeRules` declared nullable + `??=` restore (`Settings_WorkTypes.cs:24,73,102`) per the rule. Matches stack.html + csharp-net48.md.
- **DoWidgetTab signature actuality**: Common 1.6 source (`rimworld-common/.../WorkTypeThingRuleWidget.cs:243-249`) confirms the two trailing params (`ref Vector2 mapThingIconBoxScrollPosition`, `IReadOnlyList<Thing>? mapThings = null`); OM's call site consumes them. The signature claim is accurate apart from the "passes null" wording (finding #2).
- **Self-contained — AC-N**: confirmed removed — **zero** `AC-N`/`PRD`/`ADR`/`IMP-N`/`Task N` references in `Source/` (production or tests). Only the `MS-3` residue (finding #1) remains.
- **SSoT / provenance / shell**: concept + stack carry correct `provenance: reverse-engineered` + `source`, render the reverse-engineered badge (not omitted), and are wrapped in the HTML shell with all required placeholders filled (DOC_TYPE, SUBSYSTEM, STATUS, UPDATED_AT, RESPONSIBILITY, PROVENANCE, TITLE, STATS, TOC, CONTENT); no duplicated chrome, no bare fragments. tech-reference frontmatter responsibility blocks intact; no fact duplicated across home files (stack ↔ tech-reference link, not copy).
- **Traceability**: alignment sprint, no new PRD/AC; ADR corpus (adr-0001/0002/0003) maps the transpiler / stack-migration / nullable decisions to code — consistent with `audit.md` DM-5.

## Escalations
- none — both findings are dev/Architect autofix in impl fix-mode, no concept/contract/scope/abstraction change.
