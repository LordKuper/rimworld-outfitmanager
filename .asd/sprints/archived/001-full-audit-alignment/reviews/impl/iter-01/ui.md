---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-ui]: APPROVE

# Review — UI

- **Phase**: impl-review
- **Iteration**: 01

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict

APPROVE

## Next action

No action required for UI. Settings window code correctly calls Common 1.6 UI widgets; no new UI surface or behaviour introduced; nullable migration applied safely with correct `??=` patterns for Scribe-populated fields.

## Notes

**Scope**: OM has no ux-spec and no design-system governance (explicit non-goal for this alignment sprint). UI review scope is structural — verify no new UI surface, verify Common UI widget calls are correct, verify nullable migration is safe.

**UI files verified**:
- `Source/OutfitManager/Settings.cs` — ModSettings subclass, tab management, no changes to structure
- `Source/OutfitManager/Settings_General.cs` — General settings tab with single WorkTypeScoreFactor slider; calls `Common.UI.Tabs.DoTab` and `Fields.DoLabeledFloatSlider` with correct signatures
- `Source/OutfitManager/Settings_WorkTypes.cs` — WorkTypes tab entirely delegated to `Common.UI.Widgets.WorkTypeThingRuleWidget.DoWidgetTab`; call includes Common 1.6 trailing `ref Vector2 mapThingIconBoxScrollPosition` parameter; passes `WorkTypesAvailableItems` (non-null) and implicitly null for map-things (OM has no map surface)
- `Source/OutfitManager/SettingsTabs.cs` — enum (General, WorkTypes); no changes
- `Source/OutfitManager/OutfitManagerMod.cs` — Mod subclass; DoSettingsWindowContents delegates to Settings.DoWindowContents; SettingsCategory returns localized title; no changes

**Nullable safety**:
- `Settings_WorkTypes.cs:16` — `_selectedWorkTypeRule` correctly declared nullable
- `Settings_WorkTypes.cs:24` — `_workTypeRules` correctly declared nullable with initialization `[]` and restored via `??=` pattern (line 73) per custom-coding-rules.md guidance for Scribe-populated collections
- All other Settings fields (floats, bool, enum, struct) non-nullable with appropriate defaults
- No unsafe null-forcing operators (`!`) in UI code paths

**Common widget call signatures verified against** `design/architecture/tech-reference/lordkuper-common-1.6.md`:
- `Common.UI.Tabs.DoTabs(rect, tabs)` ✓
- `Common.UI.Tabs.DoTab(rect, 0, null, contentHeight, ref scrollPos, contentDelegate, 0, null)` ✓ (line 48)
- `Fields.DoLabeledFloatSlider(rect, 0, null, label, tooltip, ref value, min, max, step, null, out _)` ✓ (line 69)
- `WorkTypeThingRuleWidget.DoWidgetTab(rect, ref contentHeight, ref scrollPos, 2, rules, selectedRule, onSelect, onUpdateAvailable, ref thingBoxScrollPos, availableItems, ref mapThingIconBoxScrollPosition)` ✓ (lines 83–86); call correctly includes new Common 1.6 mapThingIconBoxScrollPosition parameter

**No new UI**:
- File inventory unchanged: 10 production files in `Source/OutfitManager/` (OutfitManagerMod.cs, Settings.cs, SettingsTabs.cs, Settings_General.cs, Settings_WorkTypes.cs, Resources.cs, Logger.cs, ApparelCache.cs, ApparelScoring.cs, WorkTypeHelper.cs)
- No new UI component classes, no new IMGUI primitives, no new tab surfaces
- Settings window structure identical to pre-migration: General tab (slider) + WorkTypes tab (Common widget)

**Accessibility / design-system**: not in scope for this alignment sprint (OM has no ux-spec, no design-system); UI is IMGUI settings window only, no custom rendering or interactive state machines.
