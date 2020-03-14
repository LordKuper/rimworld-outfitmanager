using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace OutfitManager.Patches
{
    [HarmonyPatch(typeof(OutfitDatabase), "GenerateStartingOutfits")]
    public static class OutfitDatabaseGenerateStartingOutfitsPatch
    {
        [UsedImplicitly]
        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("Code Quality", "IDE0051:Remove unused private members")]
        private static void Postfix(OutfitDatabase __instance)
        {
            try { OutfitHelper.GenerateDefaultOutfits(__instance); }
            catch (Exception e) { Log.Error("OutfitManager: Could not generate outfits - " + e); }
        }
    }
}