---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# RimWorld game API (Assembly-CSharp) @ 1.6 (game-bound)

## Canonical source
- Official docs: https://rimworldwiki.com/wiki/Modding (modding wiki); decompiled `Assembly-CSharp.dll` is the authoritative API surface
- Last verified: 2026-06-07
- Note: the wiki is not reliably machine-fetchable; API facts below are extracted from OM's actual usages, not from the wiki. Decompile the RimWorld `Managed\Assembly-CSharp.dll` to confirm signatures.

## Reference nature (game-bound, not a package)
- `Assembly-CSharp.dll` is RimWorld itself. There is **no NuGet package and no semantic version**; the "version" tracks the game build (1.6). It is referenced by `HintPath` from RimWorld's `Managed` directory (legacy csproj: `..\..\..\Games\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed\Assembly-CSharp.dll`), `Private=False` (never copied to output — the game already loads it).
- OM's `About/About.xml` declares `supportedVersions` **1.6 only** (single `<li>1.6</li>`). Source targets `net48` to match RimWorld's Mono runtime. Build output lands in `1.6/Assemblies/` (the csproj `OutputPath` for both Debug and Release is `..\1.6\Assemblies\`).
- This API is **not reliably present in LLM training data** and changes between game versions without published changelogs. Treat any recalled signature as unverified until checked against the decompiled assembly.

## API surface used in project (confirmed from source)
- `Verse.Mod` / `Verse.ModContentPack`: mod entry point. `OutfitManagerMod : Mod` ctor receives `ModContentPack`, calls `GetSettings<Settings>()`, constructs `new Harmony(ModId)`, and runs `PatchAll`. (`OutfitManagerMod.cs`)
- `Verse.ModSettings`: `Settings : ModSettings` holds all persisted mod settings; overrides `ExposeData()`. Retrieved via `Mod.GetSettings<Settings>()`.
- Settings window: `OutfitManagerMod.DoSettingsWindowContents(Rect)` and `SettingsCategory()` overrides drive the in-game mod settings UI (General + Work Types tabs). Rendering uses `Verse.TabRecord` and IMGUI via `Verse.Widgets`.
- **Harmony transpiler target — `RimWorld.JobGiver_OptimizeApparel.ApparelScoreRaw`**: OM's single patch (`Patches/JobGiverPatch.cs`) transpiles this method to add the OM work-type score into the vanilla apparel score. This is the core integration point.
- `RimWorld.Apparel.GetSpecialApparelScoreOffset`: used by the transpiler purely as the **IL anchor** — `AccessTools.Method(typeof(Apparel), nameof(Apparel.GetSpecialApparelScoreOffset))` locates the call site in `ApparelScoreRaw` where OM's score is injected.
- `Verse.Apparel` / `Verse.Pawn`: the patched scoring runs per `(Pawn, Apparel)`; `ApparelScoring.GetPawnApparelWorkScore(pawn, apparel)` is the injected method. `apparel.def`, `apparel.LabelCapNoCount`, `pawn.Name`, `pawn.LabelShort` are read in scoring/diagnostics.
- `Verse.Pawn.workSettings` (`Pawn_WorkSettings`): `WorkIsActive(WorkTypeDef)` and `GetPriority(WorkTypeDef)` drive `WorkTypeHelper.GetNormalizedWorkTypeWeights` — OM's work-type weighting is built from the pawn's actual work priorities.
- Def system:
  - `RimWorld.WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder`: enumerated to build per-work-type apparel scores and to seed default work-type rules.
  - `Verse.DefDatabase<ThingDef>.AllDefs` filtered by `ThingDef.IsApparel`: `ApparelScoring.InitializeStatRanges` iterates all apparel defs to seed stat ranges.
  - `Verse.WorkTypeDef.defName`, `Verse.ThingDef`, `RimWorld.StatDef` (via the Common `WorkTypeThingRule.StatWeights`).
- Save/load (Scribe): `Settings.ExposeData()` overrides `ModSettings.ExposeData`; uses `Scribe_Values.Look(ref _workTypeScoreFactor, "WorkTypeScoreFactor", 2f)` and `Scribe_Collections.Look(ref _workTypeRules, "WorkTypeRules", LookMode.Deep)`. `LookMode.Deep` persists the `WorkTypeThingRule` list (a Common type) inside OM's settings.

## Version-specific notes
- Targets game **1.6 only** (unlike EM, which supports 1.3–1.6). The Common runtime dependency is unconditional in About.xml (no `modDependenciesByVersion`), consistent with a single supported version.
- Mono/.NET Framework 4.8 runtime: no `Span<T>`-heavy or modern-BCL-only APIs at runtime even though `LangVersion latest` is the target (collection expressions like `[]` are compiler features, fine on net48).

## Deprecations and breaking changes from prior version
- Cross-version Def renames and method-signature changes occur between game versions without published notes; verify the patched target (`JobGiver_OptimizeApparel.ApparelScoreRaw`) and the IL anchor (`Apparel.GetSpecialApparelScoreOffset`) against the target game build before bumping the supported version.
- The transpiler relies on a specific IL shape in `ApparelScoreRaw`; if RimWorld re-orders that method the anchor scan fails and OM falls back to vanilla scoring (fail-soft, see `lib-harmony-2.4.2.md`).

## Project conventions
- Never hardcode the mod id; use `OutfitManagerMod.ModId` (`"LordKuper.OutfitManager"`). The local `Logger` wrapper always passes it.
- The only game-state mutation OM performs is the single transpiler on `ApparelScoreRaw`; all OM logic is pure scoring + settings, with no GameComponent/MapComponent/PawnColumn machinery (a much smaller game-surface than EM).
- All save/load goes through `Settings.ExposeData` + `Scribe_*`; the persisted `WorkTypeRules` list is a `LordKuper.Common.WorkTypeThingRule` collection saved `LookMode.Deep`.

## Known issues and workarounds
- API surface not in training data and wiki not machine-fetchable → confirm signatures by decompiling the local `Assembly-CSharp.dll`; do not trust recalled API.
- Static game state and the static `Settings`/`ApparelScoring` design make unit testing fragile → the future test project must isolate static state and register a RimWorld `AssemblyResolve` handler (see `nunit-4.6.1.md`).
- The transpiler is the single point of failure for OM's value proposition: if it does not apply, apparel is scored exactly as vanilla and OM silently has no effect. Confirm the `"Applying JobGiver patch."` log line after any game update.
