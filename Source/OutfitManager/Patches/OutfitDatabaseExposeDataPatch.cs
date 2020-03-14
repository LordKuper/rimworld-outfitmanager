using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace OutfitManager.Patches
{
    [HarmonyPatch(typeof(OutfitDatabase), nameof(OutfitDatabase.ExposeData))]
    [UsedImplicitly]
    public static class OutfitDatabaseExposeDataPatch
    {
        [UsedImplicitly]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static void Postfix(OutfitDatabase __instance)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars) { return; }
            OutfitHelper.GenerateDefaultOutfits(__instance);
        }
    }
}