﻿using System;
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
    [HarmonyPatch(typeof(OutfitDatabase), "GenerateStartingOutfits")]
    public static class OutfitDatabaseGenerateStartingOutfitsPatch
    {
        private static void ConfigureOutfit(ExtendedOutfit outfit, Dictionary<StatDef, float> priorities)
        {
            #if DEBUG
            Log.Message($"OutfitManager: Configuring outfit {outfit.label}");
            #endif
            outfit.AddStatPriorities(priorities.Select(i => new StatPriority(i.Key, i.Value, i.Value)));
        }

        private static void ConfigureOutfitFiltered(ExtendedOutfit outfit, Dictionary<StatDef, float> priorities,
            Func<ThingDef, bool> filter)
        {
            outfit.filter.SetDisallowAll();
            outfit.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, false);
            foreach (var current in DefDatabase<ThingDef>.AllDefs.Where(filter))
            {
                outfit.filter.SetAllow(current, true);
            }
            ConfigureOutfit(outfit, priorities);
        }

        private static void ConfigureOutfitNudist(ExtendedOutfit outfit, Dictionary<StatDef, float> priorities)
        {
            var forbid = new[] {BodyPartGroupDefOf.Legs, BodyPartGroupDefOf.Torso};
            ConfigureOutfitFiltered(outfit, priorities,
                d => d.apparel?.bodyPartGroups.All(g => !forbid.Contains(g)) ?? false);
        }

        private static void ConfigureOutfitSoldier(ExtendedOutfit outfit, Dictionary<StatDef, float> priorities)
        {
            ConfigureOutfitTagged(outfit, priorities, "Soldier");
        }

        private static void ConfigureOutfitTagged(ExtendedOutfit outfit, Dictionary<StatDef, float> priorities,
            string tag)
        {
            ConfigureOutfitFiltered(outfit, priorities, d => d.apparel?.defaultOutfitTags?.Contains(tag) ?? false);
        }

        private static void ConfigureOutfitWorker(ExtendedOutfit outfit, Dictionary<StatDef, float> priorities)
        {
            ConfigureOutfitTagged(outfit, priorities, "Worker");
        }

        internal static void GenerateStartingOutfits(OutfitDatabase db, bool vanilla = true)
        {
            #if DEBUG
            Log.Message("OutfitManager: Generating starting outfits");
            Log.Message($"Vanilla: {vanilla}");
            #endif
            if (vanilla)
            {
                ConfigureOutfit(MakeOutfit(db, "Anything", true),
                    new Dictionary<StatDef, float>
                    {
                        {StatDefOf.MoveSpeed, Priority.Desired},
                        {StatDefOf.WorkSpeedGlobal, Priority.Wanted},
                        {StatDefOf.ArmorRating_Blunt, Priority.Desired},
                        {StatDefOf.ArmorRating_Sharp, Priority.Desired}
                    });
                ConfigureOutfitWorker(MakeOutfit(db, "Worker", true),
                    new Dictionary<StatDef, float>
                    {
                        {StatDefOf.MoveSpeed, Priority.Neutral}, {StatDefOf.WorkSpeedGlobal, Priority.Desired}
                    });
            }
            ConfigureOutfitWorker(MakeOutfit(db, "Doctor"),
                new Dictionary<StatDef, float>
                {
                    {StatDefOf.MedicalSurgerySuccessChance, Priority.Wanted},
                    {StatDef.Named("MedicalOperationSpeed"), Priority.Wanted},
                    {StatDefOf.MedicalTendQuality, Priority.Wanted},
                    {StatDefOf.MedicalTendSpeed, Priority.Desired},
                    {StatDefOf.WorkSpeedGlobal, Priority.Desired}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Warden"),
                new Dictionary<StatDef, float>
                {
                    {StatDefOf.NegotiationAbility, Priority.Wanted},
                    {StatDefOf.SocialImpact, Priority.Desired},
                    {StatDefOf.TradePriceImprovement, Priority.Wanted}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Handler"),
                new Dictionary<StatDef, float>
                {
                    {StatDefOf.TrainAnimalChance, Priority.Wanted},
                    {StatDefOf.TameAnimalChance, Priority.Wanted},
                    {StatDefOf.ArmorRating_Sharp, Priority.Neutral},
                    {StatDefOf.MeleeDodgeChance, Priority.Desired},
                    {StatDefOf.MeleeHitChance, Priority.Neutral},
                    {StatDefOf.MoveSpeed, Priority.Neutral},
                    {StatDefOf.MeleeDPS, Priority.Neutral},
                    {StatDefOf.AccuracyTouch, Priority.Neutral},
                    {StatDefOf.MeleeWeapon_CooldownMultiplier, Priority.Unwanted},
                    {StatDefOf.MeleeWeapon_DamageMultiplier, Priority.Neutral},
                    {StatDefOf.PainShockThreshold, Priority.Wanted},
                    {StatDefOf.AnimalGatherYield, Priority.Wanted},
                    {StatDefOf.AnimalGatherSpeed, Priority.Wanted}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Cook"),
                new Dictionary<StatDef, float>
                {
                    {StatDef.Named("DrugCookingSpeed"), Priority.Wanted},
                    {StatDef.Named("ButcheryFleshSpeed"), Priority.Wanted},
                    {StatDef.Named("ButcheryFleshEfficiency"), Priority.Wanted},
                    {StatDef.Named("CookSpeed"), Priority.Wanted},
                    {StatDefOf.FoodPoisonChance, Priority.Unwanted},
                    {StatDefOf.MoveSpeed, Priority.Desired},
                    {StatDefOf.WorkSpeedGlobal, Priority.Desired}
                });
            ConfigureOutfitSoldier(MakeOutfit(db, "Hunter"),
                new Dictionary<StatDef, float>
                {
                    {StatDefOf.ShootingAccuracyPawn, Priority.Wanted},
                    {StatDefOf.MoveSpeed, Priority.Desired},
                    {StatDefOf.AccuracyShort, Priority.Desired},
                    {StatDefOf.AccuracyMedium, Priority.Desired},
                    {StatDefOf.AccuracyLong, Priority.Desired},
                    {StatDefOf.MeleeDPS, Priority.Neutral},
                    {StatDefOf.MeleeHitChance, Priority.Neutral},
                    {StatDefOf.ArmorRating_Blunt, Priority.Neutral},
                    {StatDefOf.ArmorRating_Sharp, Priority.Neutral},
                    {StatDefOf.RangedWeapon_Cooldown, Priority.Unwanted},
                    {StatDefOf.AimingDelayFactor, Priority.Unwanted},
                    {StatDefOf.PainShockThreshold, Priority.Wanted}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Builder"),
                new Dictionary<StatDef, float>
                {
                    {StatDefOf.FixBrokenDownBuildingSuccessChance, Priority.Wanted},
                    {StatDefOf.ConstructionSpeed, Priority.Wanted},
                    {StatDefOf.ConstructSuccessChance, Priority.Wanted},
                    {StatDefOf.SmoothingSpeed, Priority.Wanted},
                    {StatDefOf.MoveSpeed, Priority.Neutral},
                    {StatDefOf.WorkSpeedGlobal, Priority.Neutral}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Grower"),
                new Dictionary<StatDef, float>
                {
                    {StatDefOf.PlantHarvestYield, Priority.Wanted},
                    {StatDefOf.PlantWorkSpeed, Priority.Wanted},
                    {StatDefOf.MoveSpeed, Priority.Neutral},
                    {StatDefOf.WorkSpeedGlobal, Priority.Desired}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Miner"),
                new Dictionary<StatDef, float>
                {
                    {StatDefOf.MiningYield, Priority.Wanted},
                    {StatDefOf.MiningSpeed, Priority.Wanted},
                    {StatDefOf.MoveSpeed, Priority.Neutral},
                    {StatDefOf.WorkSpeedGlobal, Priority.Desired}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Smith"),
                new Dictionary<StatDef, float>
                {
                    {StatDef.Named("SmithingSpeed"), Priority.Wanted}, {StatDefOf.WorkSpeedGlobal, Priority.Desired}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Tailor"),
                new Dictionary<StatDef, float>
                {
                    {StatDef.Named("TailoringSpeed"), Priority.Wanted},
                    {StatDefOf.WorkSpeedGlobal, Priority.Desired}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Artist"),
                new Dictionary<StatDef, float>
                {
                    {StatDef.Named("SculptingSpeed"), Priority.Wanted},
                    {StatDefOf.WorkSpeedGlobal, Priority.Desired}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Crafter"),
                new Dictionary<StatDef, float>
                {
                    {StatDef.Named("SmeltingSpeed"), Priority.Wanted},
                    {StatDef.Named("ButcheryMechanoidSpeed"), Priority.Wanted},
                    {StatDef.Named("ButcheryMechanoidEfficiency"), Priority.Wanted},
                    {StatDefOf.WorkSpeedGlobal, Priority.Wanted}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Hauler"),
                new Dictionary<StatDef, float>
                {
                    {StatDefOf.MoveSpeed, Priority.Wanted}, {StatDefOf.CarryingCapacity, Priority.Wanted}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Cleaner"),
                new Dictionary<StatDef, float>
                {
                    {StatDefOf.MoveSpeed, Priority.Wanted}, {StatDefOf.WorkSpeedGlobal, Priority.Wanted}
                });
            ConfigureOutfitWorker(MakeOutfit(db, "Researcher"),
                new Dictionary<StatDef, float>
                {
                    {StatDefOf.ResearchSpeed, Priority.Wanted}, {StatDefOf.WorkSpeedGlobal, Priority.Desired}
                });
            ConfigureOutfitSoldier(MakeOutfit(db, "Brawler"),
                new Dictionary<StatDef, float>
                {
                    {StatDefOf.MoveSpeed, Priority.Wanted},
                    {StatDefOf.AimingDelayFactor, Priority.Unwanted},
                    {StatDefOf.MeleeDPS, Priority.Wanted},
                    {StatDefOf.MeleeHitChance, Priority.Wanted},
                    {StatDefOf.MeleeDodgeChance, Priority.Wanted},
                    {StatDefOf.ArmorRating_Blunt, Priority.Neutral},
                    {StatDefOf.ArmorRating_Sharp, Priority.Desired},
                    {StatDefOf.AccuracyTouch, Priority.Wanted},
                    {StatDefOf.MeleeWeapon_DamageMultiplier, Priority.Wanted},
                    {StatDefOf.MeleeWeapon_CooldownMultiplier, Priority.Unwanted},
                    {StatDefOf.PainShockThreshold, Priority.Wanted}
                });
            if (vanilla)
            {
                ConfigureOutfitSoldier(MakeOutfit(db, "Soldier"),
                    new Dictionary<StatDef, float>
                    {
                        {StatDefOf.ShootingAccuracyPawn, Priority.Wanted},
                        {StatDefOf.AccuracyShort, Priority.Desired},
                        {StatDefOf.AccuracyMedium, Priority.Desired},
                        {StatDefOf.AccuracyLong, Priority.Desired},
                        {StatDefOf.MoveSpeed, Priority.Desired},
                        {StatDefOf.ArmorRating_Blunt, Priority.Neutral},
                        {StatDefOf.ArmorRating_Sharp, Priority.Desired},
                        {StatDefOf.MeleeDodgeChance, Priority.Neutral},
                        {StatDefOf.AimingDelayFactor, Priority.Unwanted},
                        {StatDefOf.RangedWeapon_Cooldown, Priority.Unwanted},
                        {StatDefOf.PainShockThreshold, Priority.Wanted}
                    });
                ConfigureOutfitNudist(MakeOutfit(db, "Nudist", true),
                    new Dictionary<StatDef, float>
                    {
                        {StatDefOf.MoveSpeed, Priority.Desired}, {StatDefOf.WorkSpeedGlobal, Priority.Wanted}
                    });
            }
        }

        private static ExtendedOutfit MakeOutfit(OutfitDatabase database, string name, bool autoWorkPriorities = false)
        {
            #if DEBUG
            Log.Message($"Outfit Manager: Creating outfit {name}");
            #endif
            if (!(database.MakeNewOutfit() is ExtendedOutfit outfit))
            {
                Log.Error("Outfit Manager: outfit is not of type ExtendedOutfit");
                return null;
            }
            outfit.label = ("Outfit" + name).Translate();
            outfit.AutoWorkPriorities = autoWorkPriorities;
            return outfit;
        }

        [UsedImplicitly]
        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("Code Quality", "IDE0051:Remove unused private members")]
        private static bool Prefix(OutfitDatabase __instance)
        {
            try
            {
                GenerateStartingOutfits(__instance);
            }
            catch (Exception e)
            {
                Log.Error("Can't generate outfits: " + e);
            }
            return false;
        }
    }
}