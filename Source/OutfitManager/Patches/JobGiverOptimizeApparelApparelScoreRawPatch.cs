using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace OutfitManager.Patches
{
    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), nameof(JobGiver_OptimizeApparel.ApparelScoreRaw))]
    [UsedImplicitly]
    public static class JobGiverOptimizeApparelApparelScoreRawPatch
    {
        [UsedImplicitly]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static bool Prefix(Pawn pawn, Apparel ap, out float __result, NeededWarmth ___neededWarmth)
        {
            if (pawn == null)
            {
                __result = float.NaN;
                return true;
            }
            __result = OutfitManagerMod.ApparelScoreRaw(pawn, ap, ___neededWarmth);
            return false;
        }
    }
}