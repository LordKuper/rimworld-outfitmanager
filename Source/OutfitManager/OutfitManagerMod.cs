using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
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
        private const float MaxInsulationScore = 1;
        private const float TaintedApparelScoreFactor = 0.2f;
        private const float TaintedApparelScorePenalty = 1.0f;
        private const float TemperatureRangeOffset = 15.0f;

        private static readonly SimpleCurve HitPointsPercentScoreFactorCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(0.25f, 0.1f),
            new CurvePoint(0.5f, 0.25f),
            new CurvePoint(0.75f, 1f)
        };

        private static readonly SimpleCurve InsulationFactorCurve = new SimpleCurve
        {
            new CurvePoint(-20f, -MaxInsulationScore),
            new CurvePoint(-10f, -0.6f * MaxInsulationScore),
            new CurvePoint(10f, 0.6f * MaxInsulationScore),
            new CurvePoint(20f, MaxInsulationScore)
        };

        internal static bool ShowApparelScores;

        static OutfitManagerMod()
        {
            new Harmony("LordKuper.OutfitManager").PatchAll(Assembly.GetExecutingAssembly());
        }

        public static float ApparelScoreRaw([NotNull] Pawn pawn, [NotNull] Apparel apparel)
        {
            if (pawn == null) { throw new ArgumentNullException(nameof(pawn)); }
            if (apparel == null) { throw new ArgumentNullException(nameof(apparel)); }
            if (!(pawn.outfits.CurrentOutfit is ExtendedOutfit outfit))
            {
                Log.ErrorOnce("OutfitManager: Not an ExtendedOutfit, something went wrong.", 399441);
                return 0f;
            }
            #if DEBUG
            Log.Message($"OutfitManager: ----- '{pawn.Name}' - '{apparel.def.defName}' ({apparel.Label}) -----", true);
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
            score += ApparelScoreRawInsulation(pawn, apparel);
            if (outfit.PenalizeTaintedApparel && apparel.WornByCorpse &&
                ThoughtUtility.CanGetThought(pawn, ThoughtDefOf.DeadMansApparel))
            {
                #if DEBUG
                Log.Message("OutfitManager: Penalizing tainted apparel", true);
                #endif
                score -= TaintedApparelScorePenalty;
                if (score > 0f) { score *= TaintedApparelScoreFactor; }
            }
            if (apparel.Stuff == ThingDefOf.Human.race.leatherDef)
            {
                if (ThoughtUtility.CanGetThought(pawn, ThoughtDefOf.HumanLeatherApparelSad))
                {
                    #if DEBUG
                    Log.Message("OutfitManager: Penalizing human leather apparel", true);
                    #endif
                    score -= HumanLeatherScorePenalty;
                    if (score > 0f) { score *= HumanLeatherScoreFactor; }
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

        private static float ApparelScoreRawInsulation(Pawn pawn, Apparel apparel)
        {
            #if DEBUG
            Log.Message("OutfitManager: Calculating scores for insulation", true);
            #endif
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
            if (pawn.apparel.WornApparel.Contains(apparel)) { return 0f; }
            var currentRange = pawn.ComfortableTemperatureRange();
            var candidateRange = currentRange;
            var seasonalTemp = pawn.Map.mapTemperature.SeasonalTemp;
            var targetRange = new FloatRange(seasonalTemp - TemperatureRangeOffset,
                seasonalTemp + TemperatureRangeOffset);
            var apparelOffset = GetInsulationStats(apparel);
            // effect of this piece of apparel
            candidateRange.min += apparelOffset.min;
            candidateRange.max += apparelOffset.max;
            foreach (var otherInsulationRange in from otherApparel in pawn.apparel.WornApparel
                where !ApparelUtility.CanWearTogether(apparel.def, otherApparel.def, pawn.RaceProps.body)
                select GetInsulationStats(otherApparel))
            {
                // effect of taking off any other apparel that is incompatible
                candidateRange.min -= otherInsulationRange.min;
                candidateRange.max -= otherInsulationRange.max;
            }
            // did we get any closer to our target range? (smaller distance is better, negative values are overkill).
            var currentDistance = new FloatRange(Mathf.Max(currentRange.min - targetRange.min, 0f),
                Mathf.Max(targetRange.max - currentRange.max, 0f));
            var candidateDistance = new FloatRange(Mathf.Max(candidateRange.min - targetRange.min, 0f),
                Mathf.Max(targetRange.max - candidateRange.max, 0f));
            // improvement in distances
            var insulation = InsulationFactorCurve.Evaluate(currentDistance.min - candidateDistance.min) +
                             InsulationFactorCurve.Evaluate(currentDistance.max - candidateDistance.max);
            #if DEBUG
            Log.Message(
                $"OutfitManager: {pawn.Name.ToStringShort} :: {apparel.LabelCap}\n" +
                $"\ttarget range: {targetRange}, current range: {currentRange}, candidate range {candidateRange}\n" +
                $"\tcurrent distance: {currentDistance}, candidate distance: {candidateDistance}\n" +
                $"\timprovement: {currentDistance.min - candidateDistance.min + (currentDistance.max - candidateDistance.max)}, insulation score: {insulation}\n",
                true);
            Log.Message($"OutfitManager: Insulation score = {insulation}", true);
            #endif
            return insulation;
        }

        private static float ApparelScoreRawPriorities(Thing apparel, IEnumerable<StatPriority> statPriorities)
        {
            var statScores = new Dictionary<StatDef, float>();
            foreach (var statPriority in statPriorities)
            {
                if (statScores.ContainsKey(statPriority.Stat)) { continue; }
                var baseValue = apparel.def.statBases?.Find(modifier => modifier.stat == statPriority.Stat)?.value ?? 0;
                var statValue = apparel.GetStatValue(statPriority.Stat);
                var statOffset = apparel.def.equippedStatOffsets.GetStatOffsetFromList(statPriority.Stat);
                var totalValue = statValue + statOffset;
                var normalizedValue = OutfitHelper.NormalizeStatValue(statPriority.Stat, totalValue);
                var statScore = normalizedValue * statPriority.Weight;
                statScores.Add(statPriority.Stat, statScore);
                #if DEBUG
                if (Math.Abs(statScore) > 0.0001)
                {
                    var statRange = OutfitHelper.StatRanges[statPriority.Stat];
                    Log.Message(
                        $"OutfitManager: Value of stat {statPriority.Stat} ({statPriority.Weight}) [{statRange.min},{statRange.max}] = {statValue} = {baseValue} {(statOffset < 0 ? "-" : "+")} {statOffset} = {totalValue} ({normalizedValue} norm) ({statPriority.Stat.defaultBaseValue} def) ({statScore} score)",
                        true);
                }
                #endif
            }
            var apparelScore = !statScores.Any() ? 0 : statScores.Sum(pair => pair.Value);
            #if DEBUG
            Log.Message($"OutfitManager: Stat score of {apparel.Label} = {apparelScore}", true);
            #endif
            return apparelScore;
        }

        private static FloatRange GetInsulationStats(Apparel apparel)
        {
            var insulationCold = apparel.GetStatValue(StatDefOf.Insulation_Cold);
            var insulationHeat = apparel.GetStatValue(StatDefOf.Insulation_Heat);
            return new FloatRange(-insulationCold, insulationHeat);
        }
    }
}