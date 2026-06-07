---
responsibility:
  owns: task breakdown, dod, task status (checkboxes)
  excludes: requirements, design decisions, code, review findings
  delegates_to: design/ docs (requirements/design), reviews/ (findings)
---

# Plan

<!--
Format rules (parser-critical):
- Overview, Context, Definition of Done — prose only, NO checkboxes
- Checkboxes (- [ ]/- [x]) appear ONLY inside `### Task N:` sections
- Checkboxes in any non-task section break orchestrator task parsing
- A subtask deferred for a manual action stays `- [ ]` and is suffixed ` — BLOCKED: MS-N` (see manual-steps.md)
-->

## Overview

This plan implements sprint `001-full-audit-alignment`: bringing OutfitManager (OM) into full alignment with the modernized LordKuper sibling standard (EquipmentManager as the reference), without changing the mod's observable behaviour. It decomposes the seven PRD workstreams (W1 audit — already complete — through W7 rule conformance) into 10 implementation tasks across 4 sequenced phases:

- **Phase A — Structural + stack foundation (T1–T3):** restructure the production project, convert to SDK-style + `.slnx` + `PackageReference` + `Directory.Build.props`, and bump the dependency set (Harmony 2.4.2, Common 1.6, NetAnalyzers 9.0.0 pin). This phase ends at a **green build gate** that every later phase depends on.
- **Phase B — Nullable migration (T4):** flip `Nullable enable` project-wide and resolve all resulting NRT warnings. Gated behind the Phase A green build so the nullable diff is isolated from migration noise.
- **Phase C — Reuse, simplification, tests (T5–T8):** confirm reuse-over-duplication against Common, apply the two behaviour-neutral simplifications, stand up the test project (isolation + assembly resolver), and write initial coverage including the transpiler fail-soft characterization. The characterization tests are the safety net that proves the Phase C simplifications are behaviour-neutral.
- **Phase D — Conformance, save-compat, doc reconciliation (T9–T10):** enforce zero-warnings / language / logging conformance, verify save-key and behaviour stability end-to-end, flag any user-visible/save-affecting change per the published-mod constraint, and reconcile the deferred documentation-migration items.

All 33 acceptance criteria are mapped to tasks (see the coverage table at the end). Load-bearing sequencing is encoded as explicit task dependencies; the most important is that **Phase A's green build gate precedes the nullable flip (T4)**, which in turn precedes the simplification work whose neutrality is checked by the Phase C tests.

## Context

- [prd.html](design/prd.html) — 33 acceptance criteria across W1–W7 + end-to-end / published-mod constraint
- [adr.html](design/adr.html) — sprint design ADR set (drafts)
- [adr-0001-harmony-transpiler-integration.html](../../design/architecture/adr/adr-0001-harmony-transpiler-integration.html) — Harmony transpiler fail-soft with visible startup patch signal (reverse-engineered)
- [adr-0002-legacy-to-modern-stack-migration.html](../../design/architecture/adr/adr-0002-legacy-to-modern-stack-migration.html) — legacy → modern stack migration (original)
- [adr-0003-production-nullable-migration.html](../../design/architecture/adr/adr-0003-production-nullable-migration.html) — production nullable migration (original)
- [audit.md](audit.md) — verified baseline: OM is small (~640 LOC), already clean (0 TODO, 0 raw `Verse.Log.*`, no Common duplication); alignment is predominantly stack/project plumbing
- [stack.html](../../design/architecture/stack.html) — target modern stack (current-vs-target reconciliation is DM-1/DM-2, handled in T10)

## Definition of Done

The sprint is done when, and only when, all of the following hold:

- All 33 acceptance criteria (AC-1 … AC-33) are satisfied; each is covered by at least one completed task per the coverage table below. AC-1/AC-2/AC-3 (W1 audit) are already satisfied by the approved `audit.md` and are carried as verification-only.
- The solution builds end-to-end on the modern stack via `dotnet build` on the `.slnx`, producing **zero warnings** under `TreatWarningsAsErrors=true` with NetAnalyzers 9.0.0 (AC-28, AC-31).
- The full unit test suite is **green** (`dotnet test`), with isolation and assembly-resolver infrastructure in place and the required initial coverage present (AC-22 … AC-27).
- The mod's observable behaviour is preserved: apparel-pick outcomes are unchanged for a given pawn/apparel/settings configuration (characterization-verified), the sound-caching patterns are intact, and the Scribe save keys / serialized shape are stable (AC-20, AC-21, AC-32).
- Any user-visible or save-affecting change is explicitly flagged in the design ADR with its migration/communication implication (AC-33).
- Logging, language, and coding-convention conformance are confirmed held (AC-29, AC-30, AC-5).
- **All impl-review reviewers return green** at the final iteration.

### Task 1: Restructure production project to the EM reference layout
- [x] Move the production project from the flat `Source/` layout to `Source/OutfitManager/`, matching the EM reference folder organization <!-- owner: backend-dev | ac: AC-4 | adr: adr-0002 -->
- [x] Conform file, folder, and namespace organization to the modern sibling conventions per `custom-common-rules.md` <!-- owner: backend-dev | ac: AC-5 -->
- [x] Preserve the existing dense `///` XML doc-comment surface across all production files through the move <!-- owner: backend-dev | ac: AC-6 -->
- [x] Remove/replace `Source/Properties/AssemblyInfo.cs`; generate assembly attributes from MSBuild properties under the SDK project <!-- owner: backend-dev | ac: AC-7 | deps: T2 -->

### Task 2: Convert to SDK-style project, .slnx, PackageReference, and Directory.Build.props
- [x] Convert the non-SDK `OutfitManager.csproj` to an SDK-style project with no explicit `<Compile Include>` list <!-- owner: backend-dev | ac: AC-8 | adr: adr-0002 -->
- [x] Migrate `packages.config` dependencies to `PackageReference` and remove the `packages/` restore folder <!-- owner: backend-dev | ac: AC-9 | adr: adr-0002 -->
- [x] Convert the solution from VS `.sln` to `.slnx` at `Source/` <!-- owner: backend-dev | ac: AC-10 | adr: adr-0002 -->
- [x] Add `Source/Directory.Build.props` with shared properties and env-var-resolved reference HintPaths (`RIMWORLD_DIR` / `LORDKUPER_COMMON_DIR`); remove all machine-specific hardcoded paths <!-- owner: backend-dev | ac: AC-11 | adr: adr-0002 -->

### Task 3: Bump dependency set and establish the green build gate
- [x] Bump Harmony 2.3.6 → Lib.Harmony 2.4.2; pin LordKuper.Common at 1.6; pin NetAnalyzers at 9.0.0 (deliberate pin) <!-- owner: backend-dev | ac: AC-13 | adr: adr-0002 | deps: T2 -->
- [x] Verify the project builds via the .NET 10 SDK `dotnet build` path <!-- owner: backend-dev | ac: AC-14 | deps: T1, T2 -->
- [ ] Verify the `jb` (ReSharper CLI) cleanup/inspect flow runs against the migrated project <!-- owner: backend-dev | ac: AC-14 | deps: T2 --> — BLOCKED: MS-1
- [ ] Confirm the transpiler patch on `ApparelScoreRaw` still applies and emits its visible patch-applied startup signal after the dependency bump (per adr-0001) <!-- owner: backend-dev | ac: AC-14 | adr: adr-0001 | deps: T3 --> — BLOCKED: MS-2

### Task 4: Enable nullable and resolve NRT warnings (gated behind Phase A green build)
- [x] Enable `Nullable` project-wide <!-- owner: backend-dev | ac: AC-12 | adr: adr-0003 | deps: T3 -->
- [x] Replace the lone `[NotNull]` in `ApparelCache.cs` with real NRT and remove the JetBrains nullability attributes (keep the `[UsedImplicitly]` usages) <!-- owner: backend-dev | ac: AC-12 | adr: adr-0003 -->
- [x] Resolve every NRT warning surfaced by the flip so the build stays clean under zero-warnings <!-- owner: backend-dev | ac: AC-12, AC-28 | adr: adr-0003 | deps: T3 -->

### Task 5: Confirm reuse-over-duplication against LordKuper.Common
- [x] Audit OM's local surface against the LordKuper.Common public API; replace any code reimplementing Common functionality with calls into the shared library <!-- owner: backend-dev | ac: AC-15 -->
- [x] Record the reuse-audit result; where no local reimplementation exists (expected PASS for OM), document the confirmation rather than forcing a deletion <!-- owner: backend-dev | ac: AC-16 -->
- [x] Verify continued consumption of Common's public surface (`WorkTypeThingRule`, `ThingCache`, `WorkTypeThingRuleWidget`, `RimWorldTime`, `StatWeight`, `Common.UI`, `Common.Logger`) is contract-correct and public-surface-only (no forking) <!-- owner: backend-dev | ac: AC-17 -->

### Task 6: Behaviour-neutral simplification and optimization
- [x] Consolidate the redundant `Settings.WorkTypeRules.FirstOrDefault(...)` lookup duplicated across `ApparelCache.Update` and `ApparelCache.GetWorkTypeScore` (single path and/or index rules by defName) without changing observable behaviour <!-- owner: backend-dev | ac: AC-18 -->
- [x] Correct the `ApparelScoring.Initialize()` ordering fragility so a mid-init failure cannot leave `_isInitialized` set with partially-seeded ranges <!-- owner: backend-dev | ac: AC-19 -->
- [x] Confirm all W5 changes are behaviour-neutral against the characterization tests from T8 (apparel-pick outcomes unchanged for a given pawn/apparel/settings configuration) <!-- owner: backend-dev | ac: AC-20 | deps: T8 -->
- [x] Preserve the existing sound-caching patterns (`ConditionalWeakTable` weak keys, quadrum cache window); do not "optimize" them into a regression <!-- owner: backend-dev | ac: AC-21 -->

### Task 7: Stand up the unit test project with isolation and assembly-resolver infrastructure
- [x] Create the `Source/OutfitManager.Tests/` SDK-style project; ensure it builds and is included in the `.slnx` <!-- owner: test-engineer | ac: AC-22 | deps: T2 -->
- [x] Configure the test project on NUnit 4.x and FluentAssertions 7.x (license pinned to 7.x) <!-- owner: test-engineer | ac: AC-23 -->
- [x] Add static-state isolation infrastructure that snapshots/restores RimWorld/Verse and OM static state (`ApparelScoring._isInitialized`, the `ConditionalWeakTable`, static `Settings` fields) so state does not bleed between tests <!-- owner: test-engineer | ac: AC-24 -->
- [x] Add RimWorld assembly-resolver infrastructure so tests load against the game assemblies <!-- owner: test-engineer | ac: AC-25 | deps: T2 -->

### Task 8: Initial test coverage including transpiler fail-soft characterization
- [x] Cover `WorkTypeHelper.GetNormalizedWorkTypeWeights` including the `wpMin==wpMax`, empty-set, and single-work-type branches <!-- owner: test-engineer | ac: AC-26 | deps: T7 -->
- [x] Cover `ApparelCache.GetWorkTypesScore` weighted-sum and `ApparelScoring` factor multiplication <!-- owner: test-engineer | ac: AC-26 | deps: T7 -->
- [x] Characterization-test the transpiler fail-soft contract (pattern-not-found → returns original IL + logs, never throws) where the resolver fixture allows, or document it as integration-verified-in-game otherwise <!-- owner: test-engineer | ac: AC-27 | adr: adr-0001 | deps: T7 -->

### Task 9: Rule conformance — zero-warnings, language, logging
- [x] Enforce the zero-warnings policy: `TreatWarningsAsErrors=true` with a high warning level; confirm NetAnalyzers 9.0.0 reports 0 warnings on a clean build <!-- owner: backend-dev | ac: AC-28 | deps: T3, T4 -->
- [x] Confirm language policy holds: all docs in English; any new user-facing UI string lands in `Resources.Strings` (key from `ModId` + `nameof`) and the English Keyed XML in the same change <!-- owner: backend-dev | ac: AC-29 -->
- [x] Confirm logging conformance held: zero raw `Verse.Log.*` calls; all diagnostics route through the project `Logger` wrapping `Common.Logger` <!-- owner: backend-dev | ac: AC-30 -->

### Task 10: End-to-end build/test, save-compat, published-mod flagging, doc reconciliation
- [x] Verify the solution builds end-to-end on the modern stack (`dotnet build` on the `.slnx`) and all unit tests pass <!-- owner: backend-dev | ac: AC-31 | deps: T3, T4, T9, T8 -->
- [x] Verify save compatibility: the Scribe keys (`WorkTypeScoreFactor`, `WorkTypeRules`) and the serialized shape of persisted settings remain stable across the migration <!-- owner: backend-dev | ac: AC-32 -->
- [x] Flag any user-visible (settings/UI/labels) or save-affecting change in the design ADR with its migration/communication implication (`backward_compat=none`, no shim required) <!-- owner: backend-dev | ac: AC-33 | adr: adr-0002 -->
- [x] Reconcile the deferred documentation-migration items (DM-1/DM-2: stack.html current-vs-target relabel, concept/stack draft→approved status, csharp-net48 nullable note) now that the migration code exists <!-- owner: backend-dev | ac: AC-5 -->

## Risks

- **Transpiler IL fragility (R-C5, adr-0001):** the `ApparelScoreRaw` transpiler can silently mismatch after the Harmony bump. Mitigated by T3's explicit patch-applied verification and T8's fail-soft characterization test.
- **Nullable flip blast radius:** flipping `Nullable enable` (T4) can surface many warnings. Mitigated by gating it behind the Phase A green build so the diff is isolated; bounded by OM's small (~640 LOC) surface.
- **Build-machine dependency resolution:** a green `dotnet build` and in-game patch verification depend on RimWorld game assemblies and `rimworld-common` being resolvable via env vars. Captured as a build-environment precondition (T2/T3); may require a manual-steps entry if assemblies are unavailable to the agent.
- **Published-mod save/behaviour compatibility (US-5):** any silent save-shape or behaviour change would break existing player colonies. Mitigated by T6 behaviour-neutrality checks, T10 save-key verification, and the AC-33 ADR flagging gate.
- **License pin drift:** FluentAssertions must stay on 7.x (8.x is commercially licensed) and NetAnalyzers on 9.0.0 (CA-breakage avoidance). Encoded as explicit pins in T3/T7.

## Dependencies

- Task 4 depends on Task 3 (nullable flip is gated behind the Phase A green build gate — load-bearing).
- Task 3 depends on Task 1 and Task 2 (build verification needs the restructured, SDK-style project).
- Task 1's AssemblyInfo subtask depends on Task 2 (MSBuild-generated attributes require the SDK project).
- Task 6's behaviour-neutrality check depends on Task 8 (characterization tests are the neutrality oracle).
- Task 7 depends on Task 2 (test project must join the `.slnx`; resolver targets the migrated references).
- Task 8 depends on Task 7 (coverage needs the project + isolation/resolver infra).
- Task 9 depends on Task 3 and Task 4 (zero-warnings gate needs the migrated, nullable-clean build).
- Task 10 depends on Task 3, Task 4, Task 8, and Task 9 (end-to-end gate aggregates build, tests, and conformance).

## Out of scope

- EM-only systems OM does not have — StatDefs, loadout/weapon scoring, SimpleSidearms integration are NOT added.
- Optional-mod integrations — no SimpleSidearms / Combat Extended / VFE compatibility (OM is integration-free by design).
- UX / UI change — no UX spec produced and no UI change; OM's settings window (General + Work Types tabs) is unchanged.
- Workflow infrastructure — `.asd/` and `.claude/` (rules, templates, agents, skills, hooks, config) are not modified by this sprint's product work.
- LordKuper.Common itself — consumed, not modified; any gap is reused-around or noted, not patched here.
- Persistent requirements doc — no `design/product/requirements.html`; no new user-facing behaviour is introduced.

## AC → Task coverage

| AC | Workstream | Task(s) |
|---|---|---|
| AC-1 | W1 Audit | (W1 complete — `audit.md`; verified in DoD) |
| AC-2 | W1 Audit | (W1 complete — `audit.md`; verified in DoD) |
| AC-3 | W1 Audit | (W1 complete — `audit.md`; verified in DoD) |
| AC-4 | W2 Restructure | T1 |
| AC-5 | W2 Restructure / W7 | T1, T10 |
| AC-6 | W2 Restructure | T1 |
| AC-7 | W2 Restructure | T1 |
| AC-8 | W3 Stack | T2 |
| AC-9 | W3 Stack | T2 |
| AC-10 | W3 Stack | T2 |
| AC-11 | W3 Stack | T2 |
| AC-12 | W3 Stack (nullable) | T4 |
| AC-13 | W3 Stack | T3 |
| AC-14 | W3 Stack | T3 |
| AC-15 | W4 Reuse | T5 |
| AC-16 | W4 Reuse | T5 |
| AC-17 | W4 Reuse | T5 |
| AC-18 | W5 Optimize | T6 |
| AC-19 | W5 Optimize | T6 |
| AC-20 | W5 Optimize | T6 (verified via T8) |
| AC-21 | W5 Optimize | T6 |
| AC-22 | W6 Tests | T7 |
| AC-23 | W6 Tests | T7 |
| AC-24 | W6 Tests | T7 |
| AC-25 | W6 Tests | T7 |
| AC-26 | W6 Tests | T8 |
| AC-27 | W6 Tests | T8 |
| AC-28 | W7 Conformance | T4, T9 |
| AC-29 | W7 Conformance | T9 |
| AC-30 | W7 Conformance | T9 |
| AC-31 | End-to-end | T10 |
| AC-32 | Published-mod | T10 |
| AC-33 | Published-mod | T10 |
