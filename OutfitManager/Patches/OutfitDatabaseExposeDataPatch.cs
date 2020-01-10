using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Harmony;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using StatDefOf = OutfitManager.DefOfs.StatDefOf;

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
                default:
                    newOutfit.AddStatPriorities(new List<StatPriority>
                    {
                        new StatPriority(StatDefOf.MoveSpeed, Priority.Desired),
                        new StatPriority(StatDefOf.WorkSpeedGlobal, Priority.Wanted),
                        new StatPriority(StatDefOf.ArmorRating_Blunt, Priority.Desired),
                        new StatPriority(StatDefOf.ArmorRating_Sharp, Priority.Desired)
                    });
                    break;
                case "Worker":
                    newOutfit.AddStatPriorities(new List<StatPriority>
                    {
                        new StatPriority(StatDefOf.MoveSpeed, Priority.Neutral),
                        new StatPriority(StatDefOf.WorkSpeedGlobal, Priority.Desired)
                    });
                    break;
                case "Soldier":
                    newOutfit.AddStatPriorities(new List<StatPriority>
                    {
                        new StatPriority(StatDefOf.ShootingAccuracyPawn, Priority.Wanted),
                        new StatPriority(StatDefOf.AccuracyShort, Priority.Desired),
                        new StatPriority(StatDefOf.AccuracyMedium, Priority.Desired),
                        new StatPriority(StatDefOf.AccuracyLong, Priority.Desired),
                        new StatPriority(StatDefOf.MoveSpeed, Priority.Desired),
                        new StatPriority(StatDefOf.ArmorRating_Blunt, Priority.Neutral),
                        new StatPriority(StatDefOf.ArmorRating_Sharp, Priority.Desired),
                        new StatPriority(StatDefOf.MeleeDodgeChance, Priority.Neutral),
                        new StatPriority(StatDefOf.AimingDelayFactor, Priority.Unwanted),
                        new StatPriority(StatDefOf.RangedWeapon_Cooldown, Priority.Unwanted),
                        new StatPriority(StatDefOf.PainShockThreshold, Priority.Wanted)
                    });
                    break;
                case "Nudist":
                    newOutfit.AddStatPriorities(new List<StatPriority>
                    {
                        new StatPriority(StatDefOf.MoveSpeed, Priority.Desired),
                        new StatPriority(StatDefOf.WorkSpeedGlobal, Priority.Wanted)
                    });
                    break;
            }
            return newOutfit;
        }
    }
}