using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OutfitManager
{
    internal static class OutfitHelper
    {
        private static readonly IEnumerable<StatCategoryDef> BlacklistedCategories = new List<StatCategoryDef>
        {
            StatCategoryDefOf.BasicsNonPawn, StatCategoryDefOf.Building, StatCategoryDefOf.StuffStatFactors
        };

        private static readonly IEnumerable<StatDef> BlacklistedStats = new List<StatDef>
        {
            StatDefOf.ComfyTemperatureMin,
            StatDefOf.ComfyTemperatureMax,
            StatDefOf.Insulation_Cold,
            StatDefOf.Insulation_Heat,
            StatDefOf.StuffEffectMultiplierInsulation_Cold,
            StatDefOf.StuffEffectMultiplierInsulation_Heat,
            StatDefOf.StuffEffectMultiplierArmor
        };

        public static readonly Dictionary<StatDef, FloatRange> StatRanges = new Dictionary<StatDef, FloatRange>();

        internal static IEnumerable<StatDef> AllAvailableStats =>
            DefDatabase<StatDef>.AllDefs.Where(i =>
                !BlacklistedCategories.Contains(i.category) && !BlacklistedStats.Contains(i)).ToList();

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
                    if (statOffset != null) { statOffsetValue = statOffset.value; }
                    var totalStatValue = baseStatValue + statOffsetValue - stat.defaultBaseValue;
                    if (Math.Abs(statRange.min) < 0.0001 && Math.Abs(statRange.max) < 0.0001)
                    {
                        statRange.min = totalStatValue;
                        statRange.max = totalStatValue;
                    }
                    else
                    {
                        if (statRange.min > totalStatValue) { statRange.min = totalStatValue; }
                        if (statRange.max < totalStatValue) { statRange.max = totalStatValue; }
                    }
                }
            }
            else
            {
                statRange.min = stat.defaultBaseValue;
                statRange.max = stat.defaultBaseValue;
            }
            StatRanges.Add(stat, statRange);
            return statRange;
        }

        private static void ConfigureApparelFilter(ExtendedOutfit outfit, Func<ThingDef, bool> filter)
        {
            outfit.filter.SetDisallowAll();
            outfit.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, false);
            foreach (var current in DefDatabase<ThingDef>.AllDefs.Where(filter))
            {
                outfit.filter.SetAllow(current, true);
            }
        }

        private static void ConfigureStatPriorities(DefaultOutfits outfitName, ExtendedOutfit outfit)
        {
            switch (outfitName)
            {
                case DefaultOutfits.Anything:
                case DefaultOutfits.Worker:
                case DefaultOutfits.Nudist:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.BaseWorkerStatPriorities);
                    break;
                case DefaultOutfits.Doctor:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.DoctorWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Warden:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.WardenWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Handler:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.HandlingWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Cook:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.CookingWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Hunter:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.HuntingWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Builder:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.ConstructionWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Grower:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.GrowingWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Miner:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.MiningWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Smith:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.SmithingWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Tailor:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.TailoringWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Artist:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.ArtWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Crafter:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.CraftingWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Hauler:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.HaulingWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Cleaner:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.CleaningWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Researcher:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.ResearchWorkTypeStatPriorities);
                    break;
                case DefaultOutfits.Brawler:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.BrawlerStatPriorities);
                    break;
                case DefaultOutfits.Soldier:
                    outfit.SetDefaultStatPriorities(DefaultWorkTypePriorities.SoldierStatPriorities);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(message: "Unexpected default outfit", null);
            }
        }

        public static void GenerateDefaultOutfits(OutfitDatabase db)
        {
            #if DEBUG
            Log.Message("OutfitManager: Discovered outfit stats -----");
            foreach (var stat in AllAvailableStats) { Log.Message($"OutfitManager: {stat.defName} - {stat.LabelCap}"); }
            Log.Message("OutfitManager: -----------------------------");
            Log.Message("OutfitManager: Generating starting outfits");
            #endif
            foreach (DefaultOutfits outfitName in Enum.GetValues(typeof(DefaultOutfits)))
            {
                var outfitLabel = ("Outfit" + outfitName).Translate();
                if (!(db.AllOutfits.FirstOrDefault(o => o.label.Equals(outfitLabel, StringComparison.OrdinalIgnoreCase))
                    is ExtendedOutfit outfit))
                {
                    outfit = MakeOutfit(db, outfitName.ToString());
                    switch (outfitName)
                    {
                        case DefaultOutfits.Worker:
                            outfit.AutoWorkPriorities = true;
                            ConfigureApparelFilter(outfit,
                                d => d.apparel?.defaultOutfitTags?.Contains("Worker") ?? false);
                            break;
                        case DefaultOutfits.Anything:
                            outfit.AutoWorkPriorities = true;
                            break;
                        case DefaultOutfits.Nudist:
                            outfit.AutoWorkPriorities = true;
                            ConfigureApparelFilter(outfit,
                                d => d.apparel?.bodyPartGroups.All(g =>
                                         !new[] {BodyPartGroupDefOf.Legs, BodyPartGroupDefOf.Torso}.Contains(g)) ??
                                     false);
                            break;
                        default:
                            outfit.AutoWorkPriorities = false;
                            break;
                    }
                }
                ConfigureStatPriorities(outfitName, outfit);
            }
        }

        private static ExtendedOutfit MakeOutfit(OutfitDatabase database, string name)
        {
            #if DEBUG
            Log.Message($"Outfit Manager: Creating outfit {name}");
            #endif
            if (!(database.MakeNewOutfit() is ExtendedOutfit outfit))
            {
                Log.Error("Outfit Manager: outfit is not of type ExtendedOutfit");
                return null;
            }
            outfit.label = ("Outfit" + name).Translate();
            return outfit;
        }

        public static float NormalizeStatValue(StatDef stat, float value)
        {
            var statRange = StatRanges.ContainsKey(stat) ? StatRanges[stat] : CalculateStatRange(stat);
            var valueDeviation = value - stat.defaultBaseValue;
            if (Math.Abs(statRange.min - statRange.max) < 0.0001)
            {
                statRange.min = valueDeviation;
                statRange.max = valueDeviation;
                return 0f;
            }
            if (statRange.min > valueDeviation) { statRange.min = valueDeviation; }
            if (statRange.max < valueDeviation) { statRange.max = valueDeviation; }
            if (Math.Abs(valueDeviation) < 0.0001) { return 0; }
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