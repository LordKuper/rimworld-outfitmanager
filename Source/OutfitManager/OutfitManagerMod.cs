using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
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
        private const float TemperatureRangeOffset = 5.0f;

        private static readonly SimpleCurve HitPointsPercentScoreFactorCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(0.25f, 0.1f),
            new CurvePoint(0.5f, 0.25f),
            new CurvePoint(0.75f, 1f)
        };

        private static readonly SimpleCurve InsulationScoreCurve = new SimpleCurve
        {
            new CurvePoint(-10f, -MaxInsulationScore),
            new CurvePoint(-5f, -0.6f * MaxInsulationScore),
            new CurvePoint(0f, 0),
            new CurvePoint(5f, 0.6f * MaxInsulationScore),
            new CurvePoint(10f, MaxInsulationScore)
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
            if (pawn.apparel.WornApparel.Contains(apparel)) { return 0f; }
            var currentRange = pawn.ComfortableTemperatureRange();
            var candidateRange = currentRange;
            var seasonalTemp = pawn.Map.mapTemperature.SeasonalTemp;
            var targetRange = new FloatRange(seasonalTemp - TemperatureRangeOffset,
                seasonalTemp + TemperatureRangeOffset);
            var apparelOffset = new FloatRange(-apparel.GetStatValue(StatDefOf.Insulation_Cold),
                apparel.GetStatValue(StatDefOf.Insulation_Heat));
            candidateRange.min += apparelOffset.min;
            candidateRange.max += apparelOffset.max;
            foreach (var wornApparel in pawn.apparel.WornApparel)
            {
                if (ApparelUtility.CanWearTogether(apparel.def, wornApparel.def, pawn.RaceProps.body)) { continue; }
                var wornInsulationRange = new FloatRange(-wornApparel.GetStatValue(StatDefOf.Insulation_Cold),
                    wornApparel.GetStatValue(StatDefOf.Insulation_Heat));
                candidateRange.min -= wornInsulationRange.min;
                candidateRange.max -= wornInsulationRange.max;
            }
            var insulationScore = 0f;
            var coldBenefit = candidateRange.min < currentRange.min
                ? currentRange.min <= targetRange.min
                    ? 0
                    :
                    candidateRange.min <= targetRange.min && currentRange.min > targetRange.min
                        ?
                        currentRange.min - targetRange.min
                        : currentRange.min - candidateRange.min
                :
                candidateRange.min <= targetRange.min
                    ? 0
                    :
                    currentRange.min <= targetRange.min && candidateRange.min > targetRange.min
                        ?
                        targetRange.min - candidateRange.min
                        : currentRange.min - candidateRange.min;
            insulationScore += InsulationScoreCurve.Evaluate(coldBenefit);
            var heatBenefit = candidateRange.max < currentRange.max
                ? currentRange.max < targetRange.max
                    ?
                    currentRange.max - candidateRange.max
                    : candidateRange.max < targetRange.max && currentRange.max >= targetRange.max
                        ? targetRange.max - candidateRange.max
                        : 0
                :
                candidateRange.max < targetRange.max
                    ? candidateRange.max - currentRange.max
                    :
                    currentRange.max < targetRange.max && candidateRange.max >= targetRange.max
                        ?
                        targetRange.max - currentRange.max
                        : 0;
            insulationScore += InsulationScoreCurve.Evaluate(heatBenefit);
            #if DEBUG
            Log.Message(
                $"OutfitManager: target range: {targetRange}, current range: {currentRange}, candidate range: {candidateRange}",
                true);
            Log.Message(
                $"OutfitManager: cold benefit = {coldBenefit}, heat benefit = {heatBenefit}), insulation score = {insulationScore}",
                true);
            #endif
            return insulationScore;
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
    }
}