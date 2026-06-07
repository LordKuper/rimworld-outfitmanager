---
responsibility:
  owns: sprint scope, goal, top-level acceptance criteria
  excludes: task breakdown, design decisions, code, audit findings
  delegates_to: plan.md (tasks), design/ docs (decisions), audit.md (audit)
---

# Sprint 001-full-audit-alignment

## Goal

Bring OutfitManager (OM) into full structural, technical, and quality alignment with the modernized LordKuper sibling-mod standard already established by EquipmentManager (EM), executed as a single end-to-end sprint. OM is presently on the legacy stack (non-SDK `.csproj`, `packages.config`, Harmony 2.3.6, no test project). This sprint audits the codebase against the EM reference, restructures each project, migrates the entire build/test stack to the modern target (`.slnx` / .NET 10 SDK / `dotnet` / `jb` / Nullable / zero-warnings), eliminates duplication in favour of LordKuper.Common reuse, simplifies and optimizes hot paths, stands up the unit test project, and verifies conformance to the project rule set. This sprint mirrors EM sprint 001 in shape and intent, adapted to OM's narrower feature surface.

The seven workstreams below are delivered together in this one sprint (no split).

### Workstream 1 — Audit
Full audit of the existing OM codebase, documentation, and build configuration against the modern sibling standard (EM as the reference). Produce the findings that seed every subsequent workstream: divergences in structure, stack, duplication, performance, and rule conformance.

### Workstream 2 — Per-project restructure
Restructure each project in the solution to match the modern sibling layout (folder organization, file/namespace alignment, project-level conventions), per project, to the EM reference shape.

### Workstream 3 — Stack migration
Migrate the full build/test stack from legacy to the modern target:
- non-SDK `.csproj` → SDK-style projects; `packages.config` → `PackageReference`
- solution → `.slnx`
- .NET 10 SDK / `dotnet` CLI build path; `jb` (ReSharper CLI) integration
- Nullable enable; zero-warnings policy
- Harmony 2.3.6 → Lib.Harmony 2.4.2; LordKuper.Common 1.6; NetAnalyzers 9.0.0 (deliberate pin)
per the stack defined in `design/architecture/stack.html`.

### Workstream 4 — Reuse over duplication
Identify code in OM that duplicates functionality already provided by LordKuper.Common (or otherwise reusable), and replace local copies with calls into the shared library. Reuse over re-implementation.

### Workstream 5 — Optimization & simplification
Simplify over-complex code paths and optimize performance-sensitive areas (e.g. apparel/cache hot paths), reducing complexity and allocation where it does not change observable behaviour.

### Workstream 6 — Unit test project (confirmed in-scope)
Stand up the OM unit test project in THIS sprint:
- NUnit 4.x test framework
- FluentAssertions 7.x (license pin to 7.x)
- static-state isolation infrastructure (so tests touching RimWorld/Verse static state do not bleed between tests)
- RimWorld assembly-resolver infrastructure (so tests can load against the game assemblies)
- initial test coverage for the migrated/refactored code.

### Workstream 7 — Rule conformance
Verify and bring the project into conformance with the project rule set (`.asd/rules/*`, custom common/coding/design rules), including the zero-warnings policy, language policy (docs in English), and coding conventions.

## Acceptance

- An audit of OM against the modern sibling standard is produced and approved (W1).
- Each project in the solution is restructured to the modern sibling layout (W2).
- The full stack is migrated: `.slnx`, SDK-style projects, `PackageReference`, .NET 10 SDK / `dotnet`, `jb`, Nullable enable, zero-warnings, Harmony 2.4.2, LordKuper.Common 1.6, NetAnalyzers 9.0.0 (W3).
- Functionality duplicating LordKuper.Common is removed in favour of shared-library reuse (W4).
- Identified complexity and hot-path inefficiencies are simplified/optimized without changing observable behaviour (W5).
- The unit test project exists and builds, using NUnit 4.x + FluentAssertions 7.x, with static-state-isolation and RimWorld assembly-resolver infrastructure, and carries initial coverage (W6).
- The project conforms to the project rule set, including zero-warnings and language policy (W7).
- The build succeeds end-to-end on the modern stack and tests pass.
- **Published-mod constraint**: although project `backward_compat=none`, OM is a published mod with an existing player base and existing saves. Any change that is user-visible (settings/UI/labels) or that could break existing saves MUST be explicitly flagged in the design ADR, with the migration/communication implication called out, even though no automated backward-compatibility shim is required.

## Out of scope

- **EM-only systems that OM does not have**: StatDefs, loadout/weapon scoring, and SimpleSidearms integration. These exist in EM but OM has no equivalent feature; this sprint does NOT add them to OM.
- **Workflow infrastructure**: `.asd/` and `.claude/` directories (rules, templates, agents, skills, hooks, config) are not modified by this sprint's product work.
- **LordKuper.Common itself**: the upstream shared library is consumed, not modified. Any gap found in Common is reused-around or noted, not patched here.

## Sprint decisions

- **Single-sprint scope**: restructure + stack migration + audit + dedup + optimization + tests + rule conformance are all delivered in this ONE sprint. No split into multiple sprints (clarifying Q2, approved 2026-06-07).
- **Test project included**: the unit test project is delivered in THIS sprint (clarifying Q1, approved 2026-06-07), not deferred.
- This sprint mirrors EM sprint 001 in structure and intent, adapted to OM's feature surface.
