---
responsibility:
  owns: append-only chronology of approved decisions across project lifetime
  excludes: sprint state, code review notes, custom rules
  delegates_to: .asd/sprints/ (sprint state), reviews/ (review notes), custom-common-rules.md / custom-design-rules.md / custom-coding-rules.md (rules)
---

# Decisions Log

Append-only. Never edited or removed. New entries appended below.

## Entry format

```markdown
## YYYY-MM-DD — <one-line summary>

- **Decision**: <what was decided>
- **Rationale**: <why>
- **Affected docs**: <links> (optional)
```

## Entries

<!-- entries appended below this line -->

## 2026-06-07 — ASD initialized for OutfitManager

- **Decision**: ASD workflow initialized (fresh, brownfield). Decomposition=disabled, diagram_tool=n/a, OS=windows. Config: chat=ru, docs=en, backward_compat=none, external_review=enabled, base_branch=master, auto_pr=true. Custom rules, commands, and config reused from sibling mods (rimworld-equipmentmanager / rimworld-workmanager), adapted to OutfitManager.
- **Rationale**: User requested reuse of sibling-project rules since the projects are analogous LordKuper RimWorld mods on the shared LordKuper.Common library. User chose the modernized sibling standard (.slnx / .NET 10 SDK / dotnet / jb / NUnit+FluentAssertions / Nullable / zero-warnings) as the target.
- **Affected docs**: `.asd/project/config.yaml`, `commands.yaml`, `custom-common-rules.md`, `custom-coding-rules.md`, `custom-design-rules.md`
- **Note**: OutfitManager is currently on the legacy stack (non-SDK csproj, packages.config, Harmony 2.3.6, no test project). commands.yaml and coding rules target the modern sibling stack; migration to it is pending project work.

## 2026-06-07 — Concept reverse-engineered from brownfield

- **Decision**: Project concept formed via /asd-concept variant D (brownfield extraction). asd-ba scanned README, Source/*.cs, About.xml, git, and sibling concepts, then produced design/product/concept.html (provenance: reverse-engineered). Required sections Vision / Target users / Value proposition plus optional Pillars, Core Identity, Unique Hook, Anti-Pillars, Constraints. User revision applied: removed the LordKuper mod-suite segment from Target users. Success metrics section omitted (no measurable targets in evidence).
- **Rationale**: Existing published RimWorld mod; concept extracted from code/docs rather than authored greenfield.
- **Affected docs**: `design/product/concept.html`

## 2026-06-07 — Tech stack defined (reverse-engineered target, reused from sibling EM)

- **Decision**: Tech stack authored via /asd-stack variant C. stack.html written describing the TARGET sibling standard (post-migration), reused/adapted from rimworld-equipmentmanager: C#/net48 (Nullable enable target), RimWorld 1.6 + UnityEngine modules, Lib.Harmony 2.4.2, LordKuper.Common 1.6, dotnet SDK 10.0.300, NetAnalyzers 9.0.0 (deliberate pin — kept over 10.0.300), jb ReSharper CLI 2026.1.2, NUnit 4.6.1 + NUnit3TestAdapter 6.2.0 + Microsoft.NET.Test.Sdk 18.6.0 + FluentAssertions 7.2.2 (license pin to 7.x). SimpleSidearms/CombatExtended/VFE dropped (OM integrates no other mods — confirmed by source grep + About.xml). 12 tech-reference docs written; LordKuper.Common reference built from a grep of OM's actual (narrow) Common API surface, not copied from EM. All versions WebFetch-verified 2026-06-07.
- **Rationale**: At init the user chose the modernized sibling standard as OM's target; OM is currently legacy and will migrate. EM is the closest analog (same net48 target).
- **Risk summary**: Overall HIGH knowledge-gap risk, driven solely by the private upstream LordKuper.Common 1.6 (outside training data — API must be source-verified, never invented). NetAnalyzers 9.0.0 and FluentAssertions 7.2.2 intentionally held below latest (CA-breakage avoidance / 8.x commercial license). All other pins are current latest stable.
- **Affected docs**: `design/architecture/stack.html`, `design/architecture/tech-reference/*.md` (12 files)

## 2026-06-07 — Sprint 001-full-audit-alignment scope approved

- **Decision**: Scope phase approved for sprint `001-full-audit-alignment`. The sprint brings OutfitManager into full alignment with the modernized LordKuper sibling standard (EquipmentManager as reference), mirroring EM sprint 001 adapted to OM's narrower feature surface. Delivered as a SINGLE sprint (clarifying Q2 — no split) covering seven workstreams: (1) audit, (2) per-project restructure, (3) stack migration to the modern target (`.slnx` / SDK-style / `PackageReference` / .NET 10 SDK / `dotnet` / `jb` / Nullable / zero-warnings / Harmony 2.4.2 / Common 1.6 / NetAnalyzers 9.0.0), (4) reuse-over-duplication against LordKuper.Common, (5) optimization & simplification, (6) unit test project, (7) rule conformance. The unit test project is INCLUDED in this sprint (clarifying Q1 — NUnit 4.x + FluentAssertions 7.x with static-state-isolation and RimWorld assembly-resolver infrastructure), not deferred. Published-mod constraint recorded: despite `backward_compat=none`, user-visible and save-breaking changes must be flagged in the design ADR. Out-of-scope noted: EM-only systems OM lacks (StatDefs, loadout/weapon scoring, SimpleSidearms), workflow infra (`.asd/`, `.claude/`), and LordKuper.Common itself.
- **Rationale**: OM is the published analog of EM on the shared LordKuper.Common library and is currently on the legacy stack; the user chose to align it to the modern sibling standard in one end-to-end pass. Test infrastructure and migration are tightly coupled, so bundling them in one sprint avoids churn.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/sprint.md`, `.asd/sprints/001-full-audit-alignment/state.json`

## 2026-06-07 — Sprint 001-full-audit-alignment audit phase approved

- **Decision**: Audit phase approved for sprint `001-full-audit-alignment`. Merged `audit.md` (asd-ba docs scan + asd-architect code scan) accepted by user. Key findings: OutfitManager is small (~640 LOC across 12 files) and already clean — 0 TODO markers, 0 raw `Verse.Log.*` calls, 1 nullable attribute, and no LordKuper.Common duplication. Alignment is therefore predominantly stack/project plumbing, not code rework. Gaps identified: non-SDK `.csproj` + `packages.config`, no `.slnx` / `Directory.Build.props`, hardcoded `HintPath`s (no env-var resolution), Harmony 2.3.6 (target 2.4.2), `Nullable` off, no `TreatWarningsAsErrors`, and no test project. Top risks: transpiler IL fragility on `ApparelScoreRaw` (needs a visible patch-applied / patch-failed signal so a silent IL mismatch is detectable) and published-mod save/behaviour compatibility. Three ADRs recommended for the design phase: (1) transpiler integration with fail-soft behaviour, (2) stack migration to the modern sibling target, (3) nullable migration.
- **Rationale**: Establishes the verified baseline before design. The audit confirms the bulk of the sprint is migration plumbing rather than logic changes, which de-risks the code workstreams and concentrates risk on the transpiler patch and save-compat surface — both flagged for explicit ADR treatment.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/audit.md`, `.asd/sprints/001-full-audit-alignment/state.json`

## 2026-06-07 — Sprint 001-full-audit-alignment design phase drafts approved

- **Decision**: Design phase drafts approved for sprint `001-full-audit-alignment`. Produced two artefacts: `prd.html` (33 acceptance criteria spanning the 7 workstreams, provenance: original) and `adr.html` (3 ADRs, all status=proposed): ADR-0001 Harmony transpiler fail-soft with a visible startup patch signal (reverse-engineered from the existing `ApparelScoreRaw` transpiler), ADR-0002 legacy→modern stack migration (original), ADR-0003 production nullable migration (original). `ux-spec` and `design-system` were intentionally SKIPPED this sprint — this is an alignment/migration sprint with no UI change, so UX is a non-goal and design-system docs are deferred to a future UI sprint. `c4-full/` was skipped because `subsystem_decomposition=disabled`.
- **Rationale**: The audit confirmed the sprint is predominantly stack/project plumbing with no UI surface change, so PRD + ADRs fully cover the design surface. The transpiler ADR was reverse-engineered from existing code; the migration and nullable ADRs are original decisions for this sprint. Decomposition being disabled removes the need for C4 artefacts.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/design/prd.html`, `.asd/sprints/001-full-audit-alignment/design/adr.html`, `.asd/sprints/001-full-audit-alignment/state.json`

## 2026-06-07 — design-review iter 02: APPROVE

- **Decision**: Design drafts (`prd.html`, `adr.html`) passed design-review after one autofix iteration. iter-01 returned CONCERNS on documentation and external dimensions (UI and simplification APPROVE: no UI surface, design proportionate); the concerns centred on ADR provenance reconciliation and ADR-0001 patch-signal clarification, plus confirmation that the all-`proposed` ADR statuses were a deliberate user decision. asd-architect autofixed `adr.html` in place. iter-02 returned all-APPROVE (documentation + external re-reviewed; UI + simplification carried forward APPROVE as their dimensions were unchanged). DoD met — all dimensions APPROVE at the current state.
- **Rationale**: Closes the design-review phase. The single CONCERNS→APPROVE iteration resolved provenance/clarity gaps without altering the design intent, confirming the drafts are ready for promotion.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/design/adr.html`, `.asd/sprints/001-full-audit-alignment/state.json`

## 2026-06-07 — Sprint 001-full-audit-alignment design-promote: 3 ADRs promoted

- **Decision**: design-promote phase finalized (user-confirmed). 3 ADRs promoted from sprint draft `design/adr.html` to persistent `design/architecture/adr/`: `adr-0001-harmony-transpiler-integration.html` (provenance: reverse-engineered), `adr-0002-legacy-to-modern-stack-migration.html` (original), `adr-0003-production-nullable-migration.html` (original); all status=proposed (statuses move to accepted at impl, per sprint decision). Flat promotion — `subsystem_decomposition=disabled`, so no per-subsystem split. Product and UX promotion were no-ops: this is an alignment sprint with no standing requirements and no `ux-spec`. DM-1/DM-2 doc reconciliations DEFERRED to impl/pr (stack.html current-vs-target relabel, concept/stack draft→approved status, csharp-net48 nullable note) because they are coupled to migration code not yet written.
- **Rationale**: Promotes the approved, design-reviewed ADRs into the persistent architecture record while keeping doc reconciliations that depend on as-yet-unwritten migration code deferred to the phase where that code lands, avoiding premature relabels that would diverge from reality.
- **Affected docs**: `design/architecture/adr/adr-0001-harmony-transpiler-integration.html`, `design/architecture/adr/adr-0002-legacy-to-modern-stack-migration.html`, `design/architecture/adr/adr-0003-production-nullable-migration.html`, `.asd/sprints/001-full-audit-alignment/state.json`

## 2026-06-07 — Plan approved for sprint 001-full-audit-alignment

- **Decision**: Plan phase approved (as-is) for sprint `001-full-audit-alignment`. `plan.md` decomposes the seven PRD workstreams into 10 tasks across 4 phases: Phase A — structural + stack foundation (T1 restructure to EM layout, T2 SDK-style/.slnx/PackageReference/Directory.Build.props, T3 dependency bump + green build gate); Phase B — nullable migration (T4); Phase C — reuse confirmation, behaviour-neutral simplification, test project, and initial/transpiler-fail-soft coverage (T5–T8); Phase D — rule conformance + end-to-end build/test + save-compat + published-mod flagging + deferred doc reconciliation (T9–T10). All 33 acceptance criteria (AC-1 … AC-33) are mapped via the AC→Task coverage table (W1 audit ACs carried as verification-only since `audit.md` is approved). Load-bearing sequencing is encoded as explicit task dependencies, the key one being that **Phase A's green build gate precedes the nullable flip (T4)**, which precedes the simplification work whose neutrality is checked by the Phase C characterization tests. No related open stubs. Owners: backend-dev (T1–T6, T9, T10) and test-engineer (T7, T8). DoD = all 33 AC covered + zero-warnings green build (`TreatWarningsAsErrors`, NetAnalyzers 9.0.0) + green test suite + all impl-review reviewers green.
- **Rationale**: The audit established that OM is small and already clean, so the plan concentrates effort on stack/project plumbing and isolates the highest-risk steps (transpiler patch verification, nullable flip, behaviour neutrality) behind explicit gates and a characterization-test safety net. Phasing the nullable flip behind a green Phase A build keeps its diff isolated from migration noise.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/plan.md`, `.asd/sprints/001-full-audit-alignment/state.json`

## 2026-06-07 — Impl assessment approved for sprint 001-full-audit-alignment

- **Decision**: Impl assessment approved (INITIAL mode) for sprint `001-full-audit-alignment`. All 10 plan tasks complete. Build green (0 warnings / 0 errors); test suite: 5 passing, 13 ignored (game-context, in-game verification), 0 failed. 30 of 33 acceptance criteria closed by code; AC-14, AC-20, and AC-27 deferred for final closure to in-game manual steps MS-2/MS-3. Delivered: legacy→modern stack migration, production nullable migration, Harmony transpiler fail-soft with a visible startup patch signal, behaviour-neutral C-3/C-4 simplifications, a stood-up unit test project, reuse-over-duplication PASS against LordKuper.Common, and deferred doc reconciliation (concept + stack promoted draft→approved). `jb cleanupcode` applied and `jb inspectcode` clean (0 error / 0 warning). Manual steps MS-2 (in-game transpiler patch verification) and MS-3 (in-game behaviour-neutral apparel-pick verification) remain PENDING for the user to perform in-game; AC-14/20/27 final closure tracks them.
- **Rationale**: The audit established OM was small and already clean, so the impl concentrated on stack/project plumbing with behaviour neutrality protected by characterization tests. The three open ACs depend on observing the live game (transpiler IL match and apparel-pick parity), which cannot be verified autonomously — hence the two PENDING manual steps and deferred final closure.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/state.json`, `.asd/sprints/001-full-audit-alignment/manual-steps.md`

## 2026-06-07 — impl-review iter 01: 6 APPROVE + 2 CONCERNS (documentation, external) -> impl fix

- **Decision**: impl-review iteration 01 for sprint `001-full-audit-alignment` returned 6 APPROVE (quality, implementation, testing, performance, simplification, ui) + 2 CONCERNS (documentation, external). All findings are low/medium severity and confined to the new test infrastructure plus one tech-reference wording fix; no production-logic regressions. Findings: (M) `RimWorldAssemblyResolverFixture` `[SetUpFixture]` is namespaced — should be global/namespace-less so the assembly resolver applies suite-wide; (M) the single `AssemblyMetadataAttribute` read is fragile under SDK multi-attribute emission — use `GetCustomAttributes` + match on `Key`; (L) test `[Ignore]` reasons/comments cite ASD ids MS-3/C-3/C-4 (self-contained-code violation — strip workflow ids from shipped test source); (L) redundant stat-weight reset on a removed rule; (L) tech-ref `lordkuper-common-1.6.md` `DoWidgetTab` "passes null" wording is inaccurate (the call omits the optional arg). Routed to impl fix mode: `review_fixes_pending="iter-01"`, phase set to `impl-review` ahead of the impl⇄impl-review back-step.
- **Rationale**: Two dimensions raised CONCERNS rather than APPROVE, so DoD is not yet met. The concerns are non-blocking polish on test-harness robustness and documentation accuracy, all autonomously fixable, so the standard impl-fix cycle resolves them without user escalation.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/state.json`, `.asd/sprints/001-full-audit-alignment/reviews/`

## 2026-06-07 — impl fix for iter-01: findings resolved

- **Decision**: impl fix mode for sprint `001-full-audit-alignment` resolved all impl-review iter-01 findings. Devs applied: `RimWorldAssemblyResolverFixture` made global (namespace-less `[SetUpFixture]`) so the assembly resolver applies suite-wide, and the `AssemblyMetadata` read made robust via `GetCustomAttributes` + `Key` match (the two medium findings); ASD-id references (MS-3 / C-3 / C-4) stripped from test `[Ignore]` reasons and comments so the test source is self-contained; the redundant stat-weight reset on the removed rule deleted; and the tech-reference `lordkuper-common-1.6.md` `DoWidgetTab` wording corrected. Build green (0 warnings / 0 errors); test suite 5 passing / 13 ignored / 0 failed. `review_fixes_pending` cleared (null); phase held at `impl-review` for re-review (impl.iteration stays 1, the re-review bumps it to 2).
- **Rationale**: All iter-01 concerns were non-blocking, autonomously fixable polish on test-harness robustness and documentation accuracy; the impl-fix cycle resolved them with a green build and green test suite, returning the sprint to impl-review for the verifying re-review pass.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/state.json`, test infrastructure sources, `design/architecture/tech-reference/lordkuper-common-1.6.md`

## 2026-06-07 — impl-review iter 02: APPROVE — DoD met

- **Decision**: Code and tests passed impl-review for sprint `001-full-audit-alignment` after one fix iteration — DoD met. iter-01 returned CONCERNS on the documentation and external dimensions (the other 6 dimensions APPROVE), centred on test-infra robustness (global namespace-less assembly-resolver `[SetUpFixture]`, robust `AssemblyMetadata` read via `GetCustomAttributes` + `Key` match), self-contained-code (ASD ids MS-3/C-3/C-4 stripped from test comments/`[Ignore]` reasons), a redundant stat-weight reset on a removed rule, and a tech-reference `DoWidgetTab` wording fix — all resolved in the impl-fix cycle. iter-02 re-reviewed documentation + external as APPROVE; the other 6 dimensions carried APPROVE from iter-01 (their code was unchanged or only strengthened by the iter-01 test-infra fixes). All 8 dimensions APPROVE at the current state → DoD met.
- **Rationale**: Closes the impl-review phase. The single CONCERNS→APPROVE iteration resolved non-blocking test-harness robustness and documentation-accuracy polish without touching production logic, confirming the implementation is ready for the pr phase.
- **Note**: Final closure of AC-14 / AC-20 / AC-27 still depends on the in-game manual steps MS-2 (transpiler patch applied + visible startup signal) and MS-3 (behaviour-neutral apparel-pick verification), which the user performs in-game before/around PR.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/state.json`, `.asd/sprints/001-full-audit-alignment/reviews/`
