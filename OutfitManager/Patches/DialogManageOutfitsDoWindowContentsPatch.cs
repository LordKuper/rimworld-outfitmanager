using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Harmony;
using JetBrains.Annotations;
using Multiplayer.API;
using OutfitManager.Widgets;
using RimWorld;
using UnityEngine;
using Verse;

namespace OutfitManager.Patches
{
    [HarmonyPatch(typeof(Dialog_ManageOutfits), nameof(Dialog_ManageOutfits.DoWindowContents))]
    [UsedImplicitly]
    public static class DialogManageOutfitsDoWindowContentsPatch
    {
        private const float MarginBottom = 55f;

        private const float MarginLeft = 320f;
        private const float MarginRight = 10f;
        private const float MarginTop = 10f;
        private const float MarginVertical = 10f;

        private const float MaxValue = 2.5f;

        private static Vector2 _scrollPosition = Vector2.zero;

        private static readonly FloatRange MinMaxTemperatureRange = new FloatRange(-100, 100);

        private static Color AssignmentColor(StatPriority statPriority)
        {
            return statPriority.IsManual ? Color.white :
                statPriority.IsDefault ? Color.grey :
                statPriority.IsOverride ? new Color(0.75f, 0.69f, 0.33f) : Color.cyan;
        }

        private static void DrawApparelStats(ExtendedOutfit selectedOutfit, Vector2 cur, Rect canvas)
        {
            // header
            var statsHeaderRect = new Rect(cur.x, cur.y, canvas.width, 30f);
            cur.y += 30f;
            Text.Anchor = TextAnchor.LowerLeft;
            Text.Font = GameFont.Small;
            Verse.Widgets.Label(statsHeaderRect, ResourceBank.Strings.PreferedStats);
            Text.Anchor = TextAnchor.UpperLeft;

            // add button
            var addStatRect = new Rect(statsHeaderRect.xMax - 16f, statsHeaderRect.yMin + MarginVertical, 16f, 16f);
            if (Verse.Widgets.ButtonImage(addStatRect, ResourceBank.Textures.AddButton))
            {
                var options = new List<FloatMenuOption>();
                foreach (var def in selectedOutfit.UnassignedStats.OrderBy(i => i.label)
                    .ThenBy(i => i.category.displayOrder))
                {
                    var option = new FloatMenuOption(def.LabelCap,
                        delegate { selectedOutfit.AddStatPriority(def, 0f); });
                    options.Add(option);
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            TooltipHandler.TipRegion(addStatRect, ResourceBank.Strings.StatPriorityAdd);

            // line
            GUI.color = Color.grey;
            Verse.Widgets.DrawLineHorizontal(cur.x, cur.y, canvas.width);
            GUI.color = Color.white;

            // some padding
            cur.y += MarginVertical;
            var stats = selectedOutfit.StatPriorities.ToList();

            // main content in scrolling view
            var contentRect = new Rect(cur.x, cur.y, canvas.width, canvas.height - cur.y);
            var viewRect = new Rect(contentRect) {height = 30f * stats.Count};
            if (viewRect.height > contentRect.height)
            {
                viewRect.width -= 20f;
            }
            Verse.Widgets.BeginScrollView(contentRect, ref _scrollPosition, viewRect);
            GUI.BeginGroup(viewRect);
            cur = Vector2.zero;

            // none label
            if (stats.Count > 0)
            {
                // legend kind of thingy.
                var legendRect = new Rect(cur.x + (viewRect.width - 24) / 2, cur.y, (viewRect.width - 24) / 2, 20f);
                Text.Font = GameFont.Tiny;
                GUI.color = Color.grey;
                Text.Anchor = TextAnchor.LowerLeft;
                Verse.Widgets.Label(legendRect, "-" + MaxValue.ToString("N1", CultureInfo.InvariantCulture));
                Text.Anchor = TextAnchor.LowerRight;
                Verse.Widgets.Label(legendRect, MaxValue.ToString("N1", CultureInfo.InvariantCulture));
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                cur.y += 15f;

                // statPriority weight sliders
                foreach (var stat in stats)
                {
                    DrawStatRow(selectedOutfit, stat, ref cur, viewRect.width);
                }
            }
            else
            {
                var noneLabel = new Rect(cur.x, cur.y, viewRect.width, 30f);
                GUI.color = Color.grey;
                Text.Anchor = TextAnchor.MiddleCenter;
                Verse.Widgets.Label(noneLabel, ResourceBank.Strings.None);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
                cur.y += 30f;
            }
            GUI.EndGroup();
            Verse.Widgets.EndScrollView();
        }

        private static void DrawAutoWorkPrioritiesToggle(ExtendedOutfit outfit, ref Vector2 pos, Rect canvas)
        {
            var rect = new Rect(pos.x, pos.y, canvas.width, 30f);
            Verse.Widgets.CheckboxLabeled(rect, ResourceBank.Strings.AutoWorkPriorities, ref outfit.AutoWorkPriorities);
            TooltipHandler.TipRegion(rect, ResourceBank.Strings.AutoWorkPrioritiesTooltip);
            pos.y += rect.height;
        }

        private static void DrawDeadmanToogle(ExtendedOutfit selectedOutfit, ref Vector2 cur, Rect canvas)
        {
            var rect = new Rect(cur.x, cur.y, canvas.width, 30f);
            Verse.Widgets.CheckboxLabeled(rect, ResourceBank.Strings.PenalizeTaintedApparel,
                ref selectedOutfit.PenalizeTaintedApparel);
            TooltipHandler.TipRegion(rect, ResourceBank.Strings.PenalizeTaintedApparelTooltip);
            cur.y += rect.height;
        }

        private static void DrawStatRow(ExtendedOutfit selectedOutfit, StatPriority statPriority, ref Vector2 cur,
            float width)
        {
            // set up rects
            var labelRect = new Rect(cur.x, cur.y, (width - 24) / 2f, 30f);
            var sliderRect = new Rect(labelRect.xMax + 4f, cur.y + 5f, labelRect.width, 25f);
            var buttonRect = new Rect(sliderRect.xMax + 4f, cur.y + 3f, 16f, 16f);

            // draw label
            Text.Font = Text.CalcHeight(statPriority.Stat.LabelCap, labelRect.width) > labelRect.height
                ? GameFont.Tiny
                : GameFont.Small;
            GUI.color = AssignmentColor(statPriority);
            Verse.Widgets.Label(labelRect, statPriority.Stat.LabelCap);
            Text.Font = GameFont.Small;

            // draw button
            // if manually added, delete the priority
            var buttonTooltip = string.Empty;
            if (statPriority.IsManual)
            {
                buttonTooltip = ResourceBank.Strings.StatPriorityDelete(statPriority.Stat.LabelCap);
                if (Verse.Widgets.ButtonImage(buttonRect, ResourceBank.Textures.DeleteButton))
                {
                    selectedOutfit.RemoveStatPriority(statPriority.Stat);
                }
            }
            // if overridden auto assignment, reset to auto
            else if (statPriority.IsOverride)
            {
                buttonTooltip = ResourceBank.Strings.StatPriorityReset(statPriority.Stat.LabelCap);
                if (Verse.Widgets.ButtonImage(buttonRect, ResourceBank.Textures.ResetButton))
                {
                    statPriority.Weight = statPriority.Default;
                    if (MP.IsInMultiplayer)
                    {
                        ExtendedOutfitProxy.SetStatPriority(selectedOutfit.uniqueId, statPriority.Stat,
                            statPriority.Default);
                    }
                }
            }

            // draw line behind slider
            GUI.color = new Color(.3f, .3f, .3f);
            for (var y = (int) cur.y; y < cur.y + 30; y += 5)
            {
                Verse.Widgets.DrawLineVertical((sliderRect.xMin + sliderRect.xMax) / 2f, y, 3f);
            }

            // draw slider
            GUI.color = AssignmentColor(statPriority);
            var weight = GUI.HorizontalSlider(sliderRect, statPriority.Weight, -MaxValue, MaxValue);
            if (Mathf.Abs(weight - statPriority.Weight) > 1e-4)
            {
                statPriority.Weight = weight;
                if (MP.IsInMultiplayer)
                {
                    ExtendedOutfitProxy.SetStatPriority(selectedOutfit.uniqueId, statPriority.Stat, weight);
                }
            }
            GUI.color = Color.white;

            // tooltips
            TooltipHandler.TipRegion(labelRect, statPriority.Stat.LabelCap + "\n\n" + statPriority.Stat.description);
            if (!string.IsNullOrEmpty(buttonTooltip))
            {
                TooltipHandler.TipRegion(buttonRect, buttonTooltip);
            }
            TooltipHandler.TipRegion(sliderRect, statPriority.Weight.ToStringByStyle(ToStringStyle.FloatTwo));

            // advance row
            cur.y += 30f;
        }

        private static void DrawTemperatureStats(ExtendedOutfit selectedOutfit, ref Vector2 cur, Rect canvas)
        {
            // header
            var tempHeaderRect = new Rect(cur.x, cur.y, canvas.width, 30f);
            cur.y += 30f;
            Text.Anchor = TextAnchor.LowerLeft;
            Verse.Widgets.Label(tempHeaderRect, ResourceBank.Strings.PreferedTemperature);
            Text.Anchor = TextAnchor.UpperLeft;

            // line
            GUI.color = Color.grey;
            Verse.Widgets.DrawLineHorizontal(cur.x, cur.y, canvas.width);
            GUI.color = Color.white;

            // some padding
            cur.y += MarginVertical;

            // temperature slider
            var sliderRect = new Rect(cur.x, cur.y, canvas.width - 20f, 40f);
            var tempResetRect = new Rect(sliderRect.xMax + 4f, cur.y + MarginVertical, 16f, 16f);
            cur.y += 40f; // includes padding
            FloatRange targetTemps;
            if (selectedOutfit.TargetTemperaturesOverride)
            {
                targetTemps = selectedOutfit.TargetTemperatures;
                GUI.color = Color.white;
            }
            else
            {
                targetTemps = MinMaxTemperatureRange;
                GUI.color = Color.grey;
            }
            var minMaxTemps = MinMaxTemperatureRange;
            FloatRangeWidget.FloatRange(sliderRect, 123123123, ref targetTemps, minMaxTemps, ToStringStyle.Temperature);
            GUI.color = Color.white;
            if (Math.Abs(targetTemps.min - selectedOutfit.TargetTemperatures.min) > 1e-4 ||
                Math.Abs(targetTemps.max - selectedOutfit.TargetTemperatures.max) > 1e-4)
            {
                selectedOutfit.TargetTemperatures = targetTemps;
                selectedOutfit.TargetTemperaturesOverride = true;
            }
            if (selectedOutfit.TargetTemperaturesOverride)
            {
                if (Verse.Widgets.ButtonImage(tempResetRect, ResourceBank.Textures.ResetButton))
                {
                    selectedOutfit.TargetTemperaturesOverride = false;
                    selectedOutfit.TargetTemperatures = MinMaxTemperatureRange;
                }
                TooltipHandler.TipRegion(tempResetRect, ResourceBank.Strings.TemperatureRangeReset);
            }
        }

        [UsedImplicitly]
        private static void Postfix(Rect inRect, Outfit ___selOutfitInt)
        {
            if (!(___selOutfitInt is ExtendedOutfit selectedOutfit))
            {
                return;
            }
            var canvas = new Rect(MarginLeft, Dialog_ManageOutfits.TopAreaHeight + MarginTop,
                inRect.xMax - MarginLeft - MarginRight,
                inRect.yMax - Dialog_ManageOutfits.TopAreaHeight - MarginTop - MarginBottom);
            GUI.BeginGroup(canvas);
            var cur = Vector2.zero;
            if (MP.IsInMultiplayer)
            {
                MP.WatchBegin();
                ExtendedOutfitProxy.Watch(ref selectedOutfit);
            }
            DrawDeadmanToogle(selectedOutfit, ref cur, canvas);
            DrawAutoWorkPrioritiesToggle(selectedOutfit, ref cur, canvas);
            DrawTemperatureStats(selectedOutfit, ref cur, canvas);
            cur.y += MarginVertical;
            DrawApparelStats(selectedOutfit, cur, canvas);
            if (MP.IsInMultiplayer)
            {
                MP.WatchEnd();
            }
            else if (GUI.changed)
            {
                var affected =
                    Find.CurrentMap.mapPawns.FreeColonists.Where(i => i.outfits.CurrentOutfit == selectedOutfit);
                foreach (var pawn in affected)
                {
                    pawn.mindState?.Notify_OutfitChanged();
                }
            }
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}