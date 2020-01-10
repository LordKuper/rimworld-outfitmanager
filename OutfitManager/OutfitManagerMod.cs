using System;
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
            new CurvePoint(-20f, -3f), new CurvePoint(-10f, -2f), new CurvePoint(10f, 2f), new CurvePoint(20f, 3f)
        };

        private static readonly SimpleCurve InsulationTemperatureScoreFactorCurveNeed = new SimpleCurve
        {
            new CurvePoint(0f, 1f), new CurvePoint(30f, 4f)
        };

        internal static bool ShowApparelScores;

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
                Log.Message($"OutfitManager: Score of stat {stat.label}[{stat.weight}] = {stat.value}", true);
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
            Log.Message($"OutfitManager: Calculating score of {apparel.Label} for pawn {pawn.Name}", true);
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
            Log.Message($"OutfitManager: Total score of {apparel.Label} for pawn {pawn.Name} = {score}", true);
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
                    insulation = 1f;
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
                sp.Stat.label,
                weight = sp.Weight,
                value = apparel.def.equippedStatOffsets.GetStatOffsetFromList(sp.Stat) +
                        apparel.GetStatValue(sp.Stat),
                defaultValue = sp.Stat.defaultBaseValue
            }).ToList();
            var averageScore = stats.Average(sp =>
                (Math.Abs(sp.defaultValue) < 0.001f ? sp.value : (sp.value - sp.defaultValue) / sp.defaultValue) *
                Mathf.Pow(sp.weight, 3));
            #if DEBUG
            foreach (var stat in stats)
            {
                Log.Message($"OutfitManager: Score of stat {stat.label} = {stat.value}", true);
            }
            Log.Message($"OutfitManager: Average stat score of {apparel.Label} = {averageScore}", true);
            #endif
            return averageScore;
        }

        private static FloatRange GetInsulationStats(Thing apparel)
        {
            var insulationCold = apparel.GetStatValue(StatDefOf.Insulation_Cold);
            var insulationHeat = apparel.GetStatValue(StatDefOf.Insulation_Heat);
            return new FloatRange(-insulationCold, insulationHeat);
        }
    }
}