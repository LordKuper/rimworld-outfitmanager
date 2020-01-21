using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace OutfitManager
{
    internal static class OutfitStatHelper
    {
        private static readonly IEnumerable<StatCategoryDef> BlacklistedCategories = new List<StatCategoryDef>
        {
            StatCategoryDefOf.BasicsNonPawn, StatCategoryDefOf.Building, StatCategoryDefOf.StuffStatFactors
        };

        private static readonly IEnumerable<string> BlacklistedStats = new List<string>
        {
            StatDefOf.ComfyTemperatureMin.defName,
            StatDefOf.ComfyTemperatureMax.defName,
            StatDefOf.Insulation_Cold.defName,
            StatDefOf.Insulation_Heat.defName,
            StatDefOf.StuffEffectMultiplierInsulation_Cold.defName,
            StatDefOf.StuffEffectMultiplierInsulation_Heat.defName,
            StatDefOf.StuffEffectMultiplierArmor.defName
        };

        public static readonly Dictionary<string, FloatRange> StatRanges = new Dictionary<string, FloatRange>();

        internal static IEnumerable<StatDef> AllAvailableStats =>
            DefDatabase<StatDef>.AllDefs.Where(i =>
                !BlacklistedCategories.Contains(i.category) && !BlacklistedStats.Contains(i.defName)).ToList();

        private static FloatRange CalculateStatRange(StatDef stat)
        {
            var statRange = FloatRange.Zero;
            var apparelFilter = new ThingFilter();
            apparelFilter.SetAllow(ThingCategoryDefOf.Apparel, true);
            var apparels = ThingCategoryNodeDatabase.RootNode.catDef.DescendantThingDefs
                .Where(t => apparelFilter.Allows(t) && !apparelFilter.IsAlwaysDisallowedDueToSpecialFilters(t)).ToList()
                .Where(a => a.statBases != null && a.StatBaseDefined(stat) ||
                            a.equippedStatOffsets != null && a.equippedStatOffsets.Any(o => o.stat == stat)).ToList();
            if (apparels.Any())
            {
                foreach (var apparel in apparels)
                {
                    var statBase = apparel.statBases?.Find(sm => sm.stat == stat);
                    var baseStatValue = statBase?.value ?? stat.defaultBaseValue;
                    float statOffsetValue = 0;
                    var statOffset = apparel.equippedStatOffsets?.Find(sm => sm.stat == stat);
                    if (statOffset != null)
                    {
                        statOffsetValue = statOffset.value;
                    }
                    var totalStatValue = baseStatValue + statOffsetValue - stat.defaultBaseValue;
                    if (Math.Abs(statRange.min) < 0.0001 && Math.Abs(statRange.max) < 0.0001)
                    {
                        statRange.min = totalStatValue;
                        statRange.max = totalStatValue;
                    }
                    else
                    {
                        if (statRange.min > totalStatValue)
                        {
                            statRange.min = totalStatValue;
                        }
                        if (statRange.max < totalStatValue)
                        {
                            statRange.max = totalStatValue;
                        }
                    }
                }
            }
            else
            {
                statRange.min = stat.defaultBaseValue;
                statRange.max = stat.defaultBaseValue;
            }
            StatRanges.Add(stat.defName, statRange);
            return statRange;
        }

        public static FloatRange GetInsulationStats(Thing apparel)
        {
            var insulationCold = apparel.GetStatValue(StatDefOf.Insulation_Cold);
            var insulationHeat = apparel.GetStatValue(StatDefOf.Insulation_Heat);
            return new FloatRange(-insulationCold, insulationHeat);
        }

        public static StatDef GetStatDefByName([NotNull] string statName)
        {
            if (string.IsNullOrEmpty(statName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(statName));
            }
            return AllAvailableStats.FirstOrDefault(o =>
                statName.Equals(o.defName, StringComparison.OrdinalIgnoreCase));
        }

        public static float NormalizeStatValue(StatDef stat, float value)
        {
            var statRange = StatRanges.ContainsKey(stat.defName) ? StatRanges[stat.defName] : CalculateStatRange(stat);
            var valueDeviation = value - stat.defaultBaseValue;
            if (Math.Abs(statRange.min - statRange.max) < 0.0001)
            {
                statRange.min = valueDeviation;
                statRange.max = valueDeviation;
                return 0f;
            }
            if (statRange.min > valueDeviation)
            {
                statRange.min = valueDeviation;
            }
            if (statRange.max < valueDeviation)
            {
                statRange.max = valueDeviation;
            }
            if (Math.Abs(valueDeviation) < 0.0001)
            {
                return 0;
            }
            if (statRange.min < 0 && statRange.max < 0)
            {
                return -1 + (valueDeviation - statRange.min) / (statRange.max - statRange.min);
            }
            if (statRange.min < 0 && statRange.max > 0)
            {
                return -1 + 2 * ((valueDeviation - statRange.min) / (statRange.max - statRange.min));
            }
            return (valueDeviation - statRange.min) / (statRange.max - statRange.min);
        }
    }
}