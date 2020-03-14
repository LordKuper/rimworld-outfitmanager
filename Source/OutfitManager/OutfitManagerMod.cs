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

        private static readonly SimpleCurve HitPointsPercentScoreFactorCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(0.25f, 0.1f),
            new CurvePoint(0.5f, 0.25f),
            new CurvePoint(0.75f, 1f)
        };

        private static readonly SimpleCurve InsulationTemperatureScoreFactorCurveNeed = new SimpleCurve
        {
            new CurvePoint(0f, 0f), new CurvePoint(30f, MaxInsulationScore)
        };

        internal static bool ShowApparelScores;

        static OutfitManagerMod()
        {
            new Harmony("LordKuper.OutfitManager").PatchAll(Assembly.GetExecutingAssembly());
        }

        public static float ApparelScoreRaw([NotNull] Pawn pawn, [NotNull] Apparel apparel,
            NeededWarmth neededWarmth = NeededWarmth.Any)
        {
            if (pawn == null) { throw new ArgumentNullException(nameof(pawn)); }
            if (apparel == null) { throw new ArgumentNullException(nameof(apparel)); }
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
            score += ApparelScoreRawInsulation(apparel, neededWarmth);
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

        private static float ApparelScoreRawInsulation(Thing apparel, NeededWarmth neededWarmth)
        {
            #if DEBUG
            Log.Message("OutfitManager: Calculating scores for insulation", true);
            #endif
            float insulation;
            #if DEBUG
            Log.Message($"OutfitManager: Needed warmth = {Enum.GetName(typeof(NeededWarmth), neededWarmth)}", true);
            #endif
            float statValue;
            switch (neededWarmth)
            {
                case NeededWarmth.Warm:
                    statValue = apparel.GetStatValue(StatDefOf.Insulation_Cold);
                    insulation = InsulationTemperatureScoreFactorCurveNeed.Evaluate(statValue);
                    break;
                case NeededWarmth.Cool:
                    statValue = apparel.GetStatValue(StatDefOf.Insulation_Heat);
                    insulation = InsulationTemperatureScoreFactorCurveNeed.Evaluate(statValue);
                    break;
                case NeededWarmth.Any:
                default:
                    insulation = 0f;
                    break;
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
                if (statScores.ContainsKey(statPriority.Stat.defName)) { continue; }
                var baseValue = apparel.def.statBases?.Find(modifier => modifier.stat == statPriority.Stat)?.value ?? 0;
                var statValue = apparel.GetStatValue(statPriority.Stat);
                var statOffset = apparel.def.equippedStatOffsets.GetStatOffsetFromList(statPriority.Stat);
                var totalValue = statValue + statOffset;
                var normalizedValue = OutfitHelper.NormalizeStatValue(statPriority.Stat, totalValue);
                var statScore = normalizedValue * statPriority.Weight;
                statScores.Add(statPriority.Stat.defName, statScore);
                #if DEBUG
                var statRange = OutfitHelper.StatRanges[statPriority.Stat.defName];
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