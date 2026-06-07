---
responsibility:
  owns: project-owner custom rules read during design and design-review phases
  excludes: universal rules, code/test rules
  delegates_to: custom-common-rules.md (all phases), custom-coding-rules.md (impl/impl-review)
---

# Custom Design Rules

Inherited from the `LordKuper.Common` parent library and adapted to OutfitManager.

## Modding & patchability

- Harmony-patchable: prefer small methods, stable public entry points, predictable side effects.
- Don't seal mod extension points without strong reason.
- No static constructors with heavy side effects.
- OutfitManager's core integration surface is a **transpiler** on `JobGiver_OptimizeApparel.ApparelScoreRaw` (`Source/Patches/JobGiverPatch.cs`) that injects `ApparelScoring.GetPawnApparelWorkScore` into vanilla apparel scoring. Transpilers are brittle against RimWorld version changes and other apparel-scoring mods. Design changes touching apparel scoring, the patch target, or the injected IL MUST call out version/compat risk in the ADR and keep the fail-soft behaviour (log + return original IL when the pattern is not matched).

## Data-driven over hardcoded

- Stat / balance / scoring weights and tuning values come from RimWorld `Def`s or mod settings (`Settings`, `Settings_General`, `Settings_WorkTypes`), never hardcoded literals in code. ADRs/PRDs introducing new tunables MUST specify the Def/settings surface, not literal constants.
- Work-type score mapping is configured via the mod settings UI (`SettingsTabs`, `WorkTypeHelper`); new tunables should follow that settings pattern rather than ad-hoc constants.

## Determinism

- Apparel scoring, work-type score evaluation, filtering, and caching logic (`ApparelScoring`, `ApparelCache`): same inputs → same outputs.
- No time- or order-dependent behavior in core logic unless explicitly required.

## Compatibility

- OutfitManager modifies vanilla apparel scoring, an area many mods touch. Any other mod that patches `JobGiver_OptimizeApparel.ApparelScoreRaw` or alters apparel-score math is a potential conflict. Design changes touching the scoring path MUST consider transpiler conflicts and patch ordering, and call out impact in the ADR.
