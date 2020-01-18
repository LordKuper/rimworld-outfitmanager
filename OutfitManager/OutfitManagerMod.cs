using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace OutfitManager
{
    [StaticConstructorOnStartup]
    public static class OutfitManagerMod
    {
        private static readonly SimpleCurve HitPointsPercentScoreFactorCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(0.2f, 0.2f),
            new CurvePoint(0.22f, 0.6f),
            new CurvePoint(0.5f, 0.6f),
            new CurvePoint(0.52f, 1f)
        };

        private static readonly SimpleCurve InsulationFactorCurve = new SimpleCurve
        {
            new CurvePoint(-20f, -1f),
            new CurvePoint(-10f, -0.5f),
            new CurvePoint(10f, 0.5f),
            new CurvePoint(20f, 1f)
        };

        private static readonly SimpleCurve InsulationTemperatureScoreFactorCurveNeed = new SimpleCurve
        {
            new CurvePoint(0f, 0f), new CurvePoint(30f, 1f)
        };

        internal static bool ShowApparelScores;
        private static readonly Dictionary<string, FloatRange> StatRanges = new Dictionary<string, FloatRange>();

        static OutfitManagerMod()
        {
            HarmonyInstance.Create("rimworld.outfitmanager").PatchAll(Assembly.GetExecutingAssembly());
        }

        private static float ApparelScoreAutoWorkPriorities(Pawn pawn, Thing apparel)
        {
            #if DEBUG
            Log.Message("OutfitManager: Calculating scores for auto work priorities", true);
            #endif
            var stats = WorkPriorities.WorktypeStatPriorities(pawn).Select(sp => new
            {
                sp.Stat.label,
                weight = sp.Weight,
                value = (apparel.def.equippedStatOffsets.GetStatOffsetFromList(sp.Stat) +
                         apparel.GetStatValue(sp.Stat) - sp.Stat.defaultBaseValue) * sp.Weight
            }).ToList();
            var statSum = stats.Sum(stat => stat.value);
            #if DEBUG
            foreach (var stat in stats)
            {
                Log.Message($"OutfitManager: Score of stat {stat.label} [{stat.weight}] = {stat.value}", true);
            }
            Log.Message($"OutfitManager: Sum of scores for auto work priorities = {statSum}", true);
            #endif
            return statSum; // NOTE: weights were already normalized to sum to 1.
        }

        public static float ApparelScoreRaw([NotNull] Pawn pawn, [NotNull] Apparel apparel,
            NeededWarmth neededWarmth = NeededWarmth.Any)
        {
            if (pawn == null)
            {
                throw new ArgumentNullException(nameof(pawn));
            }
            if (apparel == null)
            {
                throw new ArgumentNullException(nameof(apparel));
            }
            if (!(pawn.outfits.CurrentOutfit is ExtendedOutfit outfit))
            {
                Log.ErrorOnce("OutfitManager: Not an ExtendedOutfit, something went wrong.", 399441);
                return 0f;
            }
            #if DEBUG
            Log.Message($"OutfitManager: StatBases of '{apparel.def.defName}' ({apparel.Label}) -----", true);
            if (apparel.def.statBases != null)
            {
                foreach (var statBase in apparel.def.statBases)
                {
                    Log.Message(
                        $"OutfitManager: {statBase.stat.defName} = {statBase.value} (default = {statBase.stat.defaultBaseValue})",
                        true);
                }
            }
            Log.Message("OutfitManager: ------------------------------------------------------------", true);
            Log.Message($"OutfitManager: EquippedStatOffsets of '{apparel.def.defName}' ({apparel.Label}) -----", true);
            if (apparel.def.equippedStatOffsets != null)
            {
                foreach (var statModifier in apparel.def.equippedStatOffsets)
                {
                    Log.Message($"OutfitManager: {statModifier.stat.defName} = {statModifier.value}", true);
                }
            }
            Log.Message("OutfitManager: ------------------------------------------------------------", true);
            Log.Message($"OutfitManager: Calculating score of '{apparel.Label}' for pawn '{pawn.Name}'", true);
            #endif
            var score = ApparelScoreRawPriorities(apparel, outfit);
            if (outfit.AutoWorkPriorities)
            {
                score += ApparelScoreAutoWorkPriorities(pawn, apparel);
            }
            if (apparel.def.useHitPoints)
            {
                var x = (float) apparel.HitPoints / apparel.MaxHitPoints;
                var hitPointsScoreCoefficient = HitPointsPercentScoreFactorCurve.Evaluate(x);
                #if DEBUG
                Log.Message($"OutfitManager: Hit point score coefficient = {hitPointsScoreCoefficient}", true);
                #endif
                score *= hitPointsScoreCoefficient;
            }
            var specialApparelScoreOffset = apparel.GetSpecialApparelScoreOffset();
            #if DEBUG
            Log.Message($"OutfitManager: Special apparel score offset = {specialApparelScoreOffset}", true);
            #endif
            score += specialApparelScoreOffset;
            score += ApparelScoreRawInsulation(pawn, apparel, outfit, neededWarmth);
            if (outfit.PenalizeTaintedApparel && apparel.WornByCorpse &&
                ThoughtUtility.CanGetThought(pawn, ThoughtDefOf.DeadMansApparel))
            {
                #if DEBUG
                Log.Message("OutfitManager: Penalizing tainted apparel", true);
                #endif
                score -= 0.5f;
                if (score > 0f)
                {
                    score *= 0.1f;
                }
            }
            if (apparel.Stuff == ThingDefOf.Human.race.leatherDef)
            {
                if (ThoughtUtility.CanGetThought(pawn, ThoughtDefOf.HumanLeatherApparelSad))
                {
                    #if DEBUG
                    Log.Message("OutfitManager: Penalizing human leather apparel", true);
                    #endif
                    score -= 0.5f;
                    if (score > 0f)
                    {
                        score *= 0.1f;
                    }
                }
                if (ThoughtUtility.CanGetThought(pawn, ThoughtDefOf.HumanLeatherApparelHappy))
                {
                    #if DEBUG
                    Log.Message("OutfitManager: Promoting human leather apparel", true);
                    #endif
                    score += 0.12f;
                }
            }
            #if DEBUG
            Log.Message($"OutfitManager: Total score of '{apparel.Label}' for pawn '{pawn.Name}' = {score}", true);
            #endif
            return score;
        }

        private static float ApparelScoreRawInsulation(Pawn pawn, Apparel apparel, ExtendedOutfit outfit,
            NeededWarmth neededWarmth)
        {
            #if DEBUG
            Log.Message("OutfitManager: Calculating scores for insulation", true);
            Log.Message($"OutfitManager: TargetTemperaturesOverride = {outfit.TargetTemperaturesOverride}", true);
            #endif
            float insulation;
            if (outfit.TargetTemperaturesOverride)
            {
                // NOTE: We can't rely on the vanilla check for taking off gear for temperature, because
                // we need to consider all the wardrobe changes taken together; each individual change may
                // note push us over the thresholds, but several changes together may.
                // Return 1 for temperature offsets here, we'll look at the effects of any gear we have to 
                // take off below.
                // NOTE: This is still suboptimal, because we're still only considering one piece of apparel
                // to wear at each time. A better solution would be reducing the problem to a series of linear
                // equations, and then solving that system. 
                // I'm not sure that's feasible at all; first off for simple computational reasons: the linear
                // system to solve would be fairly massive, optimizing for dozens of pawns and hundreds of pieces 
                // of gear simultaneously. Second, many of the stat functions aren't actually linear, and would
                // have to be made to be linear.
                if (pawn.apparel.WornApparel.Contains(apparel))
                {
                    return 1f;
                }
                var currentRange = pawn.ComfortableTemperatureRange();
                var candidateRange = currentRange;
                var targetRange = outfit.TargetTemperatures;
                var apparelOffset = GetInsulationStats(apparel);
                #if DEBUG
                Log.Message($"OutfitManager: Pawn comfortable temperature = [{currentRange.min},{currentRange.max}]",
                    true);
                Log.Message($"OutfitManager: Outfit target temperatures = [{targetRange.min},{targetRange.max}]", true);
                Log.Message($"OutfitManager: Apparel temperature offsets = [{apparelOffset.min},{apparelOffset.max}]",
                    true);
                #endif
                // effect of this piece of apparel
                candidateRange.min += apparelOffset.min;
                candidateRange.max += apparelOffset.max;
                #if DEBUG
                Log.Message(
                    $"OutfitManager: Apparel candidate temperatures = [{candidateRange.min},{candidateRange.max}]",
                    true);
                #endif
                foreach (var otherApparel in pawn.apparel.WornApparel)
                    // effect of taking off any other apparel that is incompatible
                {
                    if (!ApparelUtility.CanWearTogether(apparel.def, otherApparel.def, pawn.RaceProps.body))
                    {
                        var otherInsulationRange = GetInsulationStats(otherApparel);
                        candidateRange.min -= otherInsulationRange.min;
                        candidateRange.max -= otherInsulationRange.max;
                    }
                }
                // did we get any closer to our target range? (smaller distance is better, negative values are overkill).
                var currentDistance = new FloatRange(Mathf.Max(currentRange.min - targetRange.min, 0f),
                    Mathf.Max(targetRange.max - currentRange.max, 0f));
                var candidateDistance = new FloatRange(Mathf.Max(candidateRange.min - targetRange.min, 0f),
                    Mathf.Max(targetRange.max - candidateRange.max, 0f));

                // improvement in distances
                insulation = InsulationFactorCurve.Evaluate(currentDistance.min - candidateDistance.min) +
                             InsulationFactorCurve.Evaluate(currentDistance.max - candidateDistance.max);
            }
            else
            {
                #if DEBUG
                Log.Message($"OutfitManager: Needed warmth = {Enum.GetName(typeof(NeededWarmth), neededWarmth)}", true);
                #endif
                float statValue;
                if (neededWarmth == NeededWarmth.Warm)
                {
                    statValue = apparel.GetStatValue(StatDefOf.Insulation_Cold);
                    insulation = InsulationTemperatureScoreFactorCurveNeed.Evaluate(statValue);
                }
                else if (neededWarmth == NeededWarmth.Cool)
                {
                    statValue = apparel.GetStatValue(StatDefOf.Insulation_Heat);
                    insulation = InsulationTemperatureScoreFactorCurveNeed.Evaluate(statValue);
                }
                else
                {
                    insulation = 0f;
                }
            }
            #if DEBUG
            Log.Message($"OutfitManager: Insulation score = {insulation}", true);
            #endif
            return insulation;
        }

        private static float ApparelScoreRawPriorities(Thing apparel, ExtendedOutfit outfit)
        {
            if (!outfit.StatPriorities.Any())
            {
                return 0f;
            }
            var stats = outfit.StatPriorities.Select(sp => new
            {
                name = sp.Stat.defName,
                weight = sp.Weight,
                defaultValue = sp.Stat.defaultBaseValue,
                baseValue = apparel.GetStatValue(sp.Stat),
                offset = apparel.def.equippedStatOffsets.GetStatOffsetFromList(sp.Stat),
                offsettedValue =
                    apparel.GetStatValue(sp.Stat) + apparel.def.equippedStatOffsets.GetStatOffsetFromList(sp.Stat),
                normalizedValue =
                    NormalizeStatValue(sp.Stat,
                        apparel.GetStatValue(sp.Stat) +
                        apparel.def.equippedStatOffsets.GetStatOffsetFromList(sp.Stat)),
                score = NormalizeStatValue(sp.Stat,
                            apparel.GetStatValue(sp.Stat) +
                            apparel.def.equippedStatOffsets.GetStatOffsetFromList(sp.Stat)) * sp.Weight
            }).ToList();
            var score = stats.Sum(sp => sp.score);
            #if DEBUG
            foreach (var stat in stats)
            {
                var statRange = StatRanges[stat.name];
                Log.Message(
                    $"OutfitManager: Value of stat {stat.name} ({stat.weight}) [{statRange.min},{statRange.max}] = {stat.offsettedValue} ({stat.normalizedValue} norm) ({stat.defaultValue} def) ({stat.score} score)",
                    true);
            }
            Log.Message($"OutfitManager: Stat score of {apparel.Label} = {score}", true);
            #endif
            return score;
        }

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
                    var statBase = apparel.statBases?.FirstOrDefault(sm => sm.stat == stat);
                    var baseStatValue = statBase?.value ?? stat.defaultBaseValue;
                    float statOffsetValue = 0;
                    var statOffset = apparel.equippedStatOffsets?.FirstOrDefault(sm => sm.stat == stat);
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

        private static FloatRange GetInsulationStats(Thing apparel)
        {
            var insulationCold = apparel.GetStatValue(StatDefOf.Insulation_Cold);
            var insulationHeat = apparel.GetStatValue(StatDefOf.Insulation_Heat);
            return new FloatRange(-insulationCold, insulationHeat);
        }

        private static float NormalizeStatValue(StatDef stat, float value)
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