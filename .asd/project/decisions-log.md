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
