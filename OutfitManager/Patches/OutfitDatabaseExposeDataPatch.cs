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
                        new StatPriority(StatDefOf.MoveSpeed, OutfitStatPriority.MinorPositive),
                        new StatPriority(StatDefOf.WorkSpeedGlobal, OutfitStatPriority.MajorPositive),
                        new StatPriority(StatDefOf.ArmorRating_Blunt, OutfitStatPriority.MinorPositive),
                        new StatPriority(StatDefOf.ArmorRating_Sharp, OutfitStatPriority.MinorPositive)
                    });
                    break;
                case "Worker":
                    newOutfit.AddStatPriorities(new List<StatPriority>
                    {
                        new StatPriority(StatDefOf.MoveSpeed, OutfitStatPriority.Neutral),
                        new StatPriority(StatDefOf.WorkSpeedGlobal, OutfitStatPriority.MinorPositive)
                    });
                    break;
                case "Soldier":
                    newOutfit.AddStatPriorities(new List<StatPriority>
                    {
                        new StatPriority(StatDefOf.ShootingAccuracyPawn, OutfitStatPriority.MajorPositive),
                        new StatPriority(StatDefOf.AccuracyShort, OutfitStatPriority.MinorPositive),
                        new StatPriority(StatDefOf.AccuracyMedium, OutfitStatPriority.MinorPositive),
                        new StatPriority(StatDefOf.AccuracyLong, OutfitStatPriority.MinorPositive),
                        new StatPriority(StatDefOf.MoveSpeed, OutfitStatPriority.MinorPositive),
                        new StatPriority(StatDefOf.ArmorRating_Blunt, OutfitStatPriority.Neutral),
                        new StatPriority(StatDefOf.ArmorRating_Sharp, OutfitStatPriority.MinorPositive),
                        new StatPriority(StatDefOf.MeleeDodgeChance, OutfitStatPriority.Neutral),
                        new StatPriority(StatDefOf.AimingDelayFactor, OutfitStatPriority.MajorNegative),
                        new StatPriority(StatDefOf.RangedWeapon_Cooldown, OutfitStatPriority.MajorNegative),
                        new StatPriority(StatDefOf.PainShockThreshold, OutfitStatPriority.MajorPositive)
                    });
                    break;
                case "Nudist":
                    newOutfit.AddStatPriorities(new List<StatPriority>
                    {
                        new StatPriority(StatDefOf.MoveSpeed, OutfitStatPriority.MinorPositive),
                        new StatPriority(StatDefOf.WorkSpeedGlobal, OutfitStatPriority.MajorPositive)
                    });
                    break;
            }
            return newOutfit;
        }
    }
}