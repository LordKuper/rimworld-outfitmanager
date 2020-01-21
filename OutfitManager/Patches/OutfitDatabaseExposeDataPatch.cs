using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Harmony;
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
        private static void Postfix(OutfitDatabase __instance, List<Outfit> ___outfits)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars)
            {
                return;
            }
            if (___outfits.Any(i => i is ExtendedOutfit))
            {
                return;
            }
            foreach (var outfit in ___outfits.ToList())
            {
                ___outfits.Remove(outfit);
                ___outfits.Add(ReplaceKnownVanillaOutfits(outfit));
            }
            OutfitDatabaseGenerateStartingOutfitsPatch.GenerateStartingOutfits(__instance, false);
        }

        private static Outfit ReplaceKnownVanillaOutfits(Outfit outfit)
        {
            var newOutfit = new ExtendedOutfit(outfit);
            switch (newOutfit.label)
            {
                case "Worker":
                case "Nudist":
                    newOutfit.AddStatPriorities(StatPriorityHelper.BaseWorkerStatPriorities);
                    break;
                case "Soldier":
                    newOutfit.AddStatPriorities(StatPriorityHelper.SoldierStatPriorities);
                    break;
                default:
                    newOutfit.AddStatPriorities(StatPriorityHelper.VanillaStatPriorities);
                    break;
            }
            return newOutfit;
        }
    }
}