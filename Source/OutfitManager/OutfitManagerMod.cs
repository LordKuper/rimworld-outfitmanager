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
        private const float AllowedRoyaltyApparelScoreFactor = 10f;
        public const float ApparelTotalStatWeight = 10;
        private const float HumanLeatherScoreBonus = 0.2f;
        private const float HumanLeatherScoreFactor = 0.2f;
        private const float HumanLeatherScorePenalty = 1.0f;
        private const float IncorrectGenderApparelScoreFactor = 0.01f;
        private const float LowQualityApparelScoreFactor = 0.25f;
        private const float MaxInsulationScore = 2;
        private const float RequiredRoyaltyApparelScoreFactor = 25f;
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
            ApplyHitPointsScoring(apparel, ref score);
            ApplySpecialScoring(apparel, ref score);
            ApplyInsulationScoring(pawn, apparel, ref score);
            ApplyTaintedScoring(pawn, apparel, outfit, ref score);
            ApplyHumanLeatherScoring(pawn, apparel, ref score);
            ApplyGenderScoring(pawn, apparel, ref score);
            ApplyRoyalTitleScoring(pawn, apparel, ref score);
            #if DEBUG
            Log.Message($"OutfitManager: Total score of '{apparel.Label}' for pawn '{pawn.Name}' = {score}", true);
            Log.Message("OutfitManager: -----------------------------------------------------------------", true);
            #endif
            return score;
        }

        private static float ApparelScoreRawPriorities(Thing apparel, IEnumerable<StatPriority> statPriorities)
        {
            var statScores = new Dictionary<StatDef, float>();
            foreach (var statPriority in statPriorities)
            {
                if (statScores.ContainsKey(statPriority.Stat)) { continue; }
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
                        $"OutfitManager: Value of stat {statPriority.Stat} ({statPriority.Weight}) [{statRange.min},{statRange.max}] = {totalValue} ({normalizedValue} norm) ({statPriority.Stat.defaultBaseValue} def) ({statScore} score)",
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

        private static void ApplyGenderScoring(Pawn pawn, Thing apparel, ref float score)
        {
            if (!apparel.def.apparel.CorrectGenderForWearing(pawn.gender))
            {
                score *= IncorrectGenderApparelScoreFactor;
            }
        }

        private static void ApplyHitPointsScoring(Thing apparel, ref float score)
        {
            if (!apparel.def.useHitPoints) { return; }
            var hitPointsScoreCoefficient =
                HitPointsPercentScoreFactorCurve.Evaluate((float) apparel.HitPoints / apparel.MaxHitPoints);
            #if DEBUG
                Log.Message($"OutfitManager: Hit point score coefficient = {hitPointsScoreCoefficient}", true);
            #endif
            score *= hitPointsScoreCoefficient;
        }

        private static void ApplyHumanLeatherScoring(Pawn pawn, Thing apparel, ref float score)
        {
            if (apparel.Stuff != ThingDefOf.Human.race.leatherDef) { return; }
            if (ThoughtUtility.CanGetThought_NewTemp(pawn, ThoughtDefOf.HumanLeatherApparelSad))
            {
                #if DEBUG
                    Log.Message("OutfitManager: Penalizing human leather apparel", true);
                #endif
                score -= HumanLeatherScorePenalty;
                if (score > 0f) { score *= HumanLeatherScoreFactor; }
            }
            if (ThoughtUtility.CanGetThought_NewTemp(pawn, ThoughtDefOf.HumanLeatherApparelHappy))
            {
                #if DEBUG
                    Log.Message("OutfitManager: Promoting human leather apparel", true);
                #endif
                score += HumanLeatherScoreBonus;
            }
        }

        private static void ApplyInsulationScoring(Pawn pawn, Apparel apparel, ref float score)
        {
            #if DEBUG
            Log.Message("OutfitManager: Calculating scores for insulation", true);
            #endif
            if (pawn.apparel.WornApparel.Contains(apparel)) { score += 0f; }
            else
            {
                var currentRange = pawn.ComfortableTemperatureRange();
                var candidateRange = currentRange;
                var seasonalTemp = pawn.Map.mapTemperature.SeasonalTemp;
                var targetRange = new FloatRange(seasonalTemp - TemperatureRangeOffset,
                    seasonalTemp + TemperatureRangeOffset);
                var apparelOffset = new FloatRange(-apparel.GetStatValue(StatDefOf.Insulation_Cold),
                    apparel.GetStatValue(StatDefOf.Insulation_Heat));
                candidateRange.min += apparelOffset.min;
                candidateRange.max += apparelOffset.max;
                foreach (var wornApparel in pawn.apparel.WornApparel.Where(wornApparel =>
                    !ApparelUtility.CanWearTogether(apparel.def, wornApparel.def, pawn.RaceProps.body)))
                {
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
                        candidateRange.max - currentRange.max
                        : candidateRange.max < targetRange.max && currentRange.max >= targetRange.max
                            ? candidateRange.max - targetRange.max
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
                score += insulationScore;
            }
        }

        private static void ApplyRoyalTitleScoring(Pawn pawn, Thing apparel, ref float score)
        {
            if (pawn.royalty == null || pawn.royalty.AllTitlesInEffectForReading.Count <= 0) { return; }
            var allowedApparels = new HashSet<ThingDef>();
            var requiredApparels = new HashSet<ThingDef>();
            var bodyPartGroups = new HashSet<BodyPartGroupDef>();
            var qualityCategory = QualityCategory.Awful;
            foreach (var royalTitle in pawn.royalty.AllTitlesInEffectForReading)
            {
                if (royalTitle.def.requiredApparel != null)
                {
                    foreach (var requirement in royalTitle.def.requiredApparel)
                    {
                        allowedApparels.AddRange(requirement.AllAllowedApparelForPawn(pawn, includeWorn: true));
                        requiredApparels.AddRange(requirement.AllRequiredApparelForPawn(pawn, includeWorn: true));
                        bodyPartGroups.AddRange(requirement.bodyPartGroupsMatchAny);
                    }
                }
                if (royalTitle.def.requiredMinimumApparelQuality > qualityCategory)
                {
                    qualityCategory = royalTitle.def.requiredMinimumApparelQuality;
                }
            }
            if (apparel.TryGetQuality(out var qc) && qc < qualityCategory) { score *= LowQualityApparelScoreFactor; }
            if (apparel.def.apparel.bodyPartGroups.Any(bp => bodyPartGroups.Contains(bp)))
            {
                foreach (var item in requiredApparels) { allowedApparels.Remove(item); }
                if (allowedApparels.Contains(apparel.def)) { score *= AllowedRoyaltyApparelScoreFactor; }
                if (requiredApparels.Contains(apparel.def)) { score *= RequiredRoyaltyApparelScoreFactor; }
            }
        }

        private static void ApplySpecialScoring(Apparel apparel, ref float score)
        {
            var specialApparelScoreOffset = apparel.GetSpecialApparelScoreOffset();
            #if DEBUG
            Log.Message($"OutfitManager: Special apparel score offset = {specialApparelScoreOffset}", true);
            #endif
            score += specialApparelScoreOffset;
        }

        private static void ApplyTaintedScoring(Pawn pawn, Apparel apparel, ExtendedOutfit outfit, ref float score)
        {
            if (!outfit.PenalizeTaintedApparel || !apparel.WornByCorpse ||
                !ThoughtUtility.CanGetThought_NewTemp(pawn, ThoughtDefOf.DeadMansApparel)) { return; }
            #if DEBUG
                Log.Message("OutfitManager: Penalizing tainted apparel", true);
            #endif
            score -= TaintedApparelScorePenalty;
            if (score > 0f) { score *= TaintedApparelScoreFactor; }
        }
    }
}