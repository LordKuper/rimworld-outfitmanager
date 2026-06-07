---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# LordKuper.Common @ 1.6 (upstream shared library; local sibling repo)

## Canonical source
- Local upstream repo: `..\..\rimworld-common` (sibling working copy; the authoritative source). Steam Workshop id 3531352422.
- Last verified: 2026-06-07 (re-verified against the as-built post-migration call sites)
- **HIGH knowledge-gap risk**: a private/community library with **no LLM training coverage**. Every member below was confirmed by grepping Outfit Manager's source (`Source/*.cs`); do NOT invent additional API or copy EquipmentManager's (much larger, weapon/loadout-centric) Common surface. To verify signatures, read the sibling repo or decompile `..\..\rimworld-common\1.6\Assemblies\LordKuper.Common.dll`. **Never assume a member exists because EM uses it — OM consumes a different, smaller subset.**

## Reference nature (compile-only, upstream contract)
- Referenced in `Source/OutfitManager/OutfitManager.csproj` by the `HintPath` `$(LordKuperCommonAssembliesDir)\LordKuper.Common.dll`, `Private=False` (compile-only; the runtime copy comes from the installed Common mod). `$(LordKuperCommonAssembliesDir)` resolves from `Source/Directory.Build.props` via the `LORDKUPER_COMMON_DIR` env var (falling back to the `..\..\rimworld-common` sibling). The pre-sprint-001 legacy csproj used a hardcoded relative `HintPath`.
- Declared a runtime `modDependency` AND `loadAfter` in `About/About.xml` (`packageId` `LordKuper.Common`, workshop 3531352422). The dependency is **unconditional** — there is no `modDependenciesByVersion` (OM supports only 1.6).
- **Upstream integration contract — do not fork or reimplement.** Consume only the public surface; if a needed capability is missing, the change belongs upstream in `rimworld-common`, not here.

## API surface used in project (confirmed from OM source — grep of `using LordKuper.Common*` and `Common.` usages)

Namespaces imported by OM: `LordKuper.Common`, `LordKuper.Common.UI`, `LordKuper.Common.UI.Widgets`.

- **`LordKuper.Common.Logger`** — static logging keyed by mod id. Wrapped locally by `OutfitManager.Logger` (`Logger.cs`), always passing `OutfitManagerMod.ModId`:
  - `Logger.LogError(modId, message, exception)`
  - `Logger.LogMessage(modId, message)`
  - `Logger.LogWarning(modId, message, exception)`
- **`LordKuper.Common.RimWorldTime`** — in-game time type threaded through OM's cache/score APIs (`ApparelCache.cs`, `ApparelScoring.cs`):
  - `RimWorldTime.HoursInQuadrum` — constant, passed as the cache refresh interval: `base(apparel, RimWorldTime.HoursInQuadrum)`.
  - `RimWorldTime.GetHomeTime()` — static; current colony/home time, passed into scoring: `GetHomeTime()` in `ApparelScoring.GetPawnApparelWorkScore`.
  - The `RimWorldTime` type itself is used as a parameter type (`GetWorkTypesScore(Dictionary<string,float>, RimWorldTime)`, `Update(RimWorldTime)`).
- **`LordKuper.Common.ThingCache`** — base class OM's `ApparelCache` extends (`ApparelCache : ThingCache`):
  - protected ctor `ThingCache(Thing thing, <interval>)` — OM calls `base(apparel, RimWorldTime.HoursInQuadrum)`.
  - `virtual bool Update(RimWorldTime time)` — OM overrides it (`public override bool Update(RimWorldTime time)`) and calls `base.Update(time)` to decide whether the cache is stale.
  - `Thing` property — the cached thing, read by OM as `Thing` / `Thing.def` / `Thing.LabelCapNoCount`.
- **`LordKuper.Common.WorkTypeThingRule`** — the work-type scoring rule type; OM holds a `List<WorkTypeThingRule>` as its persisted settings (`Settings_WorkTypes.cs`). Members OM consumes:
  - ctor `new WorkTypeThingRule(string workTypeDefName)`.
  - `WorkTypeDefName` (string) — matched against `WorkTypeDef.defName`.
  - `Label` (string) — used in debug logging.
  - `StatWeights` — enumerable of stat-weight entries; each entry exposes `StatDefName` (string), `StatDef`, `Weight` (float), and a settable `Protected` (bool).
  - `GetThingScore(Thing)` — score a thing against the rule (`ApparelCache.GetWorkTypeScore`, `Update`).
  - `GetThingDefScore(ThingDef)` — score a def (used to seed stat ranges in `ApparelScoring.InitializeStatRanges`).
  - `GetGloballyAvailableItems()` — returns the apparel/things the rule applies to (`Settings_WorkTypes.UpdateWorkTypesAvailableItems`).
  - `SetStatWeight(StatDef, float weight)` — add/update a stat weight (default-rule merge in `InitializeWorkTypesSettings`).
  - `DefaultRules` (static) — seeds default work-type rules (merged into OM's rule list on init).
- **`LordKuper.Common.UI.Tabs`** — settings tab helpers (referenced as `Common.UI.Tabs`):
  - `Tabs.DoTabs(rect, tabs)` → returns the active tab `Rect` (`Settings.DoWindowContents`).
  - `Tabs.DoTab(rect, 0, null, contentHeight, ref scrollPos, contentDelegate, 0, null)` — draws a scrollable tab body (`Settings_General.DoGeneralTab`).
- **`LordKuper.Common.UI.Fields`** (`Fields` via `using LordKuper.Common.UI;`):
  - `Fields.DoLabeledFloatSlider(rect, 0, null, label, tooltip, ref value, min, max, step, null, out _)` — labeled float slider used for the Work Type Score Factor field (`Settings_General.DoWorkTypeScoreFactorField`).
- **`LordKuper.Common.UI.Widgets.WorkTypeThingRuleWidget`**:
  - `WorkTypeThingRuleWidget.DoWidgetTab(rect, ref contentHeight, ref scrollPos, 2, rules, selectedRule, onSelect, onUpdateAvailable, ref thingBoxScrollPos, availableItems, ref mapThingIconBoxScrollPosition, mapThings)` — drives the entire Work Types editor tab (`Settings_WorkTypes.DoWorkTypesTab`). **As of Common 1.6 the signature gained two trailing parameters** consumed by OM: `ref Vector2 mapThingIconBoxScrollPosition` (OM passes its own `_workTypesMapThingIconBoxScrollPosition`, which stays at the origin since OM has no map things) and `IReadOnlyList<Thing>? mapThings` (OM passes `null` — it has no map-thing surface). Verify the exact parameter list against the sibling source before changing the call site.

> Scope note: OM does **NOT** use `StatHelper`, `StatRanges`, `StatCategory`, `CustomStats`, `Filters.Limits` / `StatLimit`, `Cache.TimedCache`, `WorkTypeStatMap`, `SkillStatMap`, `UI.Windows`, `ThingIconBox`, `Labels`, `Buttons`, `Sections`, `Layout`, or `ScrollView`. Those appear in EquipmentManager's Common surface but are absent from OM's source. Do not assume them here.

## Version-specific notes
- The mod pins the 1.6 Common build (`\1.6\Assemblies`). The runtime dependency is unconditional in About.xml (OM supports only game 1.6), so the Common contract is a 1.6 expectation throughout.
- OM's persisted settings embed Common types: `Scribe_Collections.Look(ref _workTypeRules, "WorkTypeRules", LookMode.Deep)` saves a `List<WorkTypeThingRule>`. A breaking change to `WorkTypeThingRule`'s scribe shape upstream would affect existing OM saves.

## Deprecations and breaking changes from prior version
- Common's public surface is the contract; a breaking change upstream surfaces here as a compile error (or, for `WorkTypeThingRule`, a save-compat issue). There is no in-repo legacy Common version to migrate from.

## Project conventions
- Treat Common as read-only upstream: consume `Logger`, `RimWorldTime`, `ThingCache`, `WorkTypeThingRule`, `UI.Tabs`, `UI.Fields`, `UI.Widgets.WorkTypeThingRuleWidget` as-is. Do not reimplement what Common already provides.
- Local `OutfitManager.Logger` thinly wraps `LordKuper.Common.Logger`, always passing `OutfitManagerMod.ModId`.
- OM's only cache (`ApparelCache`) derives from `ThingCache` and takes a `RimWorldTime` in `Update`; never roll a bespoke time/cache mechanism.
- All settings UI is drawn through Common's `UI.Tabs` / `UI.Fields` / `UI.Widgets.WorkTypeThingRuleWidget`; the rule editor is entirely delegated to `WorkTypeThingRuleWidget.DoWidgetTab`.

## Known issues and workarounds
- No training coverage / private API → confirm every member against the sibling repo before relying on it; this doc lists only grep-confirmed usage from OM's source.
- Resolution depends on the sibling checkout being present at `..\..\rimworld-common` (or an override env var once the migration parameterizes the path) → build fails fast if the path is missing.
- `WorkTypeThingRuleWidget.DoWidgetTab` and `Tabs.DoTab`/`DoTabs` signatures are wide (many positional args, some passed as literal `0`/`null` in OM); verify the exact parameter list against the sibling source before changing any call site. Note Common 1.6's `DoWidgetTab` carries two trailing params beyond the older form (`ref Vector2 mapThingIconBoxScrollPosition`, `IReadOnlyList<Thing>? mapThings`) — OM passes its own origin-pinned scroll position and `null` since it has no map-thing surface.
