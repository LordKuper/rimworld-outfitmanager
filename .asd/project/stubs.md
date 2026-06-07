---
responsibility:
  owns: project-global registry of CURRENTLY OPEN todo stubs across all sprints
  excludes: code review issues, plan tasks, design todos, resolved stubs (deleted on resolution)
  delegates_to: reviews/ (code issues), plan.md (tasks), decisions-log (audit trail of resolutions)
---

# Stubs

Contains only OPEN stubs. Resolved stubs are deleted immediately. Migrated stubs are deleted from prior sprint and re-registered under the new sprint. Accepted-debt entries are kept; their Reason field MUST begin with `(accepted-debt)` so the pr-phase block exempts them.

Persists across sprint archival.

| Sprint | File:Line | Reason | Owner |
|---|---|---|---|
| 001-full-audit-alignment | Source/OutfitManager.Tests/WorkTypeHelperTests.cs:114 | (accepted-debt) `[Ignore]`d characterization test for `GetNormalizedWorkTypeWeights` null-workSettings branch; needs live RimWorld Pawn/workSettings context (same in-game limitation as MS-2/MS-3). Enable when in-game verification infra exists. | test-engineer |
| 001-full-audit-alignment | Source/OutfitManager.Tests/WorkTypeHelperTests.cs:127 | (accepted-debt) `[Ignore]`d characterization test for `GetNormalizedWorkTypeWeights` no-active-work-types branch; needs live RimWorld Pawn context. Enable when in-game verification infra exists. | test-engineer |
| 001-full-audit-alignment | Source/OutfitManager.Tests/WorkTypeHelperTests.cs:140 | (accepted-debt) `[Ignore]`d characterization test for `GetNormalizedWorkTypeWeights` no-rules-for-work-type branch; needs live RimWorld Pawn context. Enable when in-game verification infra exists. | test-engineer |
