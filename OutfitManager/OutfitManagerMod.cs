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
        public const float ApparelTotalStatWeight = 10;
        private const float HumanLeatherScoreBonus = 0.2f;
        private const float HumanLeatherScoreFactor = 0.2f;
        private const float HumanLeatherScorePenalty = 1.0f;
        private const float MaxInsulationScore = 2;
        private const float TaintedApparelScoreFactor = 0.2f;
        private const float TaintedApparelScorePenalty = 1.0f;

        private static readonly SimpleCurve HitPointsPercentScoreFactorCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(0.25f, 0.1f),
            new CurvePoint(0.5f, 0.25f),
            new CurvePoint(0.75f, 1f)
        };

        private static readonly SimpleCurve InsulationFactorCurve = new SimpleCurve
        {
            new CurvePoint(-20f, -1 * MaxInsulationScore),
            new CurvePoint(-10f, -0.75f * MaxInsulationScore),
            new CurvePoint(10f, 0.75f * MaxInsulationScore),
            new CurvePoint(20f, MaxInsulationScore)
        };

        private static readonly SimpleCurve InsulationTemperatureScoreFactorCurveNeed = new SimpleCurve
        {
            new CurvePoint(0f, 0f), new CurvePoint(30f, MaxInsulationScore)
        };

        internal static bool ShowApparelScores;

        static OutfitManagerMod()
        {
            HarmonyInstance.Create("rimworld.outfitmanager").PatchAll(Assembly.GetExecutingAssembly());
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
            Log.Message($"OutfitManager: ----- '{apparel.def.defName}' ({apparel.Label}) -----", true);
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
                    Log.Message(
                        $"OutfitManager: {statModifier.stat.defName} = {apparel.def.equippedStatOffsets.GetStatOffsetFromList(statModifier.stat)}",
                        true);
                }
            }
            Log.Message("OutfitManager: ------------------------------------------------------------", true);
            Log.Message($"OutfitManager: Calculating score of '{apparel.Label}' for pawn '{pawn.Name}'", true);
            #endif
            var statPriorities =
                StatPriorityHelper.CalculateStatPriorities(pawn, outfit.StatPriorities, outfit.AutoWorkPriorities);
            var score = ApparelScoreRawPriorities(apparel, statPriorities);
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
                score -= TaintedApparelScorePenalty;
                if (score > 0f)
                {
                    score *= TaintedApparelScoreFactor;
                }
            }
            if (apparel.Stuff == ThingDefOf.Human.race.leatherDef)
            {
                if (ThoughtUtility.CanGetThought(pawn, ThoughtDefOf.HumanLeatherApparelSad))
                {
                    #if DEBUG
                    Log.Message("OutfitManager: Penalizing human leather apparel", true);
                    #endif
                    score -= HumanLeatherScorePenalty;
                    if (score > 0f)
                    {
                        score *= HumanLeatherScoreFactor;
                    }
                }
                if (ThoughtUtility.CanGetThought(pawn, ThoughtDefOf.HumanLeatherApparelHappy))
                {
                    #if DEBUG
                    Log.Message("OutfitManager: Promoting human leather apparel", true);
                    #endif
                    score += HumanLeatherScoreBonus;
                }
            }
            #if DEBUG
            Log.Message($"OutfitManager: Total score of '{apparel.Label}' for pawn '{pawn.Name}' = {score}", true);
            Log.Message("OutfitManager: -----------------------------------------------------------------", true);
            #endif
            return score;
        }

        private static float ApparelScoreRawInsulation(Pawn pawn, Apparel apparel, ExtendedOutfit outfit,
            NeededWarmth neededWarmth)
        {
            #if DEBUG
            Log.Message(
                $"OutfitManager: Calculating scores for insulation (TargetTemperaturesOverride = {outfit.TargetTemperaturesOverride})",
                true);
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
                    return 0f;
                }
                var currentRange = pawn.ComfortableTemperatureRange();
                var candidateRange = currentRange;
                var targetRange = outfit.TargetTemperatures;
                var apparelOffset = OutfitStatHelper.GetInsulationStats(apparel);
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
                        var otherInsulationRange = OutfitStatHelper.GetInsulationStats(otherApparel);
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

        private static float ApparelScoreRawPriorities(Thing apparel, IEnumerable<StatPriority> statPriorities)
        {
            var statScores = new Dictionary<string, float>();
            foreach (var statPriority in statPriorities)
            {
                if (statScores.ContainsKey(statPriority.Stat.defName))
                {
                    continue;
                }
                var baseValue = apparel.def.statBases?.Find(modifier => modifier.stat == statPriority.Stat)?.value ?? 0;
                var statValue = apparel.GetStatValue(statPriority.Stat);
                var statOffset = apparel.def.equippedStatOffsets.GetStatOffsetFromList(statPriority.Stat);
                var totalValue = statValue + statOffset;
                var normalizedValue = OutfitStatHelper.NormalizeStatValue(statPriority.Stat, totalValue);
                var statScore = normalizedValue * statPriority.Weight;
                statScores.Add(statPriority.Stat.defName, statScore);
                #if DEBUG
                var statRange = OutfitStatHelper.StatRanges[statPriority.Stat.defName];
                Log.Message(
                    $"OutfitManager: Value of stat {statPriority.Stat.defName} ({statPriority.Weight}) [{statRange.min},{statRange.max}] = {statValue} = {baseValue} {(statOffset < 0 ? "-" : "+")} {statOffset} = {totalValue} ({normalizedValue} norm) ({statPriority.Stat.defaultBaseValue} def) ({statScore} score)",
                    true);
                #endif
            }
            var apparelScore = !statScores.Any() ? 0 : statScores.Sum(pair => pair.Value);
            #if DEBUG
            Log.Message($"OutfitManager: Stat score of {apparel.Label} = {apparelScore}", true);
            #endif
            return apparelScore;
        }
    }
}