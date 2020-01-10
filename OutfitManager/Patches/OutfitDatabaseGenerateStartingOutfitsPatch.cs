using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Harmony;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace OutfitManager.Patches
{
    [HarmonyPatch(typeof(OutfitDatabase), "GenerateStartingOutfits")]
    public static class OutfitDatabaseGenerateStartingOutfitsPatch
    {
        private static void ConfigureAnythingOutfit(OutfitDatabase db)
        {
            ConfigureOutfit(MakeOutfit(db, "Anything", true),
                new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities));
        }

        private static void ConfigureArtisanOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "SmithingSpeed", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "TailoringSpeed", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "SmeltingSpeed", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "ButcheryMechanoidSpeed",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "ButcheryMechanoidEfficiency",
                OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Artisan"), priorities);
        }

        private static void ConfigureArtistOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "SculptingSpeed", OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Artist"), priorities);
        }

        private static void ConfigureBrawlerOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseSoldierStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AimingDelayFactor", OutfitStatPriority.MajorNegative);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeDPS", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeHitChance", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeDodgeChance", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AccuracyTouch", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeWeapon_DamageMultiplier",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeWeapon_CooldownMultiplier",
                OutfitStatPriority.MajorNegative);
            ConfigureOutfitSoldier(MakeOutfit(db, "Brawler"), priorities);
        }

        private static void ConfigureBuilderOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "FixBrokenDownBuildingSuccessChance",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "ConstructionSpeed", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "ConstructSuccessChance",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "SmoothingSpeed", OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Builder"), priorities);
        }

        private static void ConfigureCleanerOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MoveSpeed", OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Cleaner"), priorities);
        }

        private static void ConfigureCookOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "DrugCookingSpeed", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "ButcheryFleshSpeed",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "ButcheryFleshEfficiency",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "CookSpeed", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "FoodPoisonChance", OutfitStatPriority.MajorNegative);
            ConfigureOutfit(MakeOutfit(db, "Cook"), priorities);
        }

        private static void ConfigureCrafterOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "SmeltingSpeed", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "ButcheryMechanoidSpeed",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "ButcheryMechanoidEfficiency",
                OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Crafter"), priorities);
        }

        private static void ConfigureDoctorOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MedicalSurgerySuccessChance",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MedicalOperationSpeed",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MedicalTendQuality",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MedicalTendSpeed", OutfitStatPriority.MediumPositive);
            ConfigureOutfit(MakeOutfit(db, "Doctor"), priorities);
        }

        private static void ConfigureGrowerOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "PlantHarvestYield", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "PlantWorkSpeed", OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Grower"), priorities);
        }

        private static void ConfigureHandlerOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "TrainAnimalChance", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "TameAnimalChance", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeDodgeChance", OutfitStatPriority.MinorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeHitChance", OutfitStatPriority.Neutral);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeDPS", OutfitStatPriority.Neutral);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AccuracyTouch", OutfitStatPriority.Neutral);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeWeapon_CooldownMultiplier",
                OutfitStatPriority.MinorNegative);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeWeapon_DamageMultiplier",
                OutfitStatPriority.Neutral);
            OutfitStatPriority.ConfigureStatPriority(priorities, "PainShockThreshold",
                OutfitStatPriority.MediumPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AnimalGatherYield", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AnimalGatherSpeed", OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Handler"), priorities);
        }

        private static void ConfigureHaulerOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "CarryingCapacity", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "CarryWeight", OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Hauler"), priorities);
        }

        private static void ConfigureHunterOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "ShootingAccuracyPawn",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AccuracyShort", OutfitStatPriority.MediumPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AccuracyMedium", OutfitStatPriority.MediumPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AccuracyLong", OutfitStatPriority.MediumPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeDPS", OutfitStatPriority.Neutral);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeHitChance", OutfitStatPriority.Neutral);
            OutfitStatPriority.ConfigureStatPriority(priorities, "RangedWeapon_Cooldown",
                OutfitStatPriority.MajorNegative);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AimingDelayFactor", OutfitStatPriority.MajorNegative);
            OutfitStatPriority.ConfigureStatPriority(priorities, "PainShockThreshold",
                OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Hunter"), priorities);
        }

        private static void ConfigureMinerOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MiningYield", OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MiningSpeed", OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Miner"), priorities);
        }

        private static void ConfigureNudistOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>();
            OutfitStatPriority.ConfigureStatPriority(priorities, "MoveSpeed", OutfitStatPriority.MediumPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "WorkSpeedGlobal", OutfitStatPriority.MajorPositive);
            ConfigureOutfitNudist(MakeOutfit(db, "Nudist", true), priorities);
        }

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
            //ConfigureOutfitTagged(outfit, priorities, "Soldier");
            ConfigureOutfit(outfit, priorities);
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

        private static void ConfigureResearcherOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "ResearchSpeed", OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Researcher"), priorities);
        }

        private static void ConfigureSmithOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "SmithingSpeed", OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Smith"), priorities);
        }

        private static void ConfigureSoldierOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseSoldierStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "ShootingAccuracyPawn",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AccuracyShort", OutfitStatPriority.MinorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AccuracyMedium", OutfitStatPriority.MinorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AccuracyLong", OutfitStatPriority.MinorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "MeleeDodgeChance", OutfitStatPriority.Neutral);
            OutfitStatPriority.ConfigureStatPriority(priorities, "AimingDelayFactor", OutfitStatPriority.MajorNegative);
            OutfitStatPriority.ConfigureStatPriority(priorities, "RangedWeapon_Cooldown",
                OutfitStatPriority.MajorNegative);
            ConfigureOutfitSoldier(MakeOutfit(db, "Soldier"), priorities);
        }

        private static void ConfigureTailorOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "TailoringSpeed", OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Tailor"), priorities);
        }

        private static void ConfigureWardenOutfit(OutfitDatabase db)
        {
            var priorities = new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities);
            OutfitStatPriority.ConfigureStatPriority(priorities, "NegotiationAbility",
                OutfitStatPriority.MajorPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "SocialImpact", OutfitStatPriority.MediumPositive);
            OutfitStatPriority.ConfigureStatPriority(priorities, "TradePriceImprovement",
                OutfitStatPriority.MajorPositive);
            ConfigureOutfit(MakeOutfit(db, "Warden"), priorities);
        }

        private static void ConfigureWorkerOutfit(OutfitDatabase db)
        {
            ConfigureOutfitWorker(MakeOutfit(db, "Worker", true),
                new Dictionary<StatDef, float>(OutfitStatPriority.BaseWorkerStatPriorities));
        }

        internal static void GenerateStartingOutfits(OutfitDatabase db, bool vanilla = true)
        {
            #if DEBUG
            Log.Message("OutfitManager: Discovered stats");
            foreach (var stat in ExtendedOutfit.AllAvailableStats)
            {
                Log.Message($"OutfitManager: {stat.defName} - {stat.LabelCap}");
            }
            Log.Message("OutfitManager: Generating starting outfits");
            Log.Message($"Vanilla: {vanilla}");
            #endif
            if (vanilla)
            {
                ConfigureAnythingOutfit(db);
                ConfigureWorkerOutfit(db);
            }
            ConfigureDoctorOutfit(db);
            ConfigureWardenOutfit(db);
            ConfigureHandlerOutfit(db);
            ConfigureCookOutfit(db);
            ConfigureHunterOutfit(db);
            ConfigureBuilderOutfit(db);
            ConfigureGrowerOutfit(db);
            ConfigureMinerOutfit(db);
            ConfigureSmithOutfit(db);
            ConfigureTailorOutfit(db);
            ConfigureArtistOutfit(db);
            ConfigureCrafterOutfit(db);
            ConfigureHaulerOutfit(db);
            ConfigureCleanerOutfit(db);
            ConfigureResearcherOutfit(db);
            ConfigureBrawlerOutfit(db);
            if (vanilla)
            {
                ConfigureSoldierOutfit(db);
                ConfigureNudistOutfit(db);
            }
            ConfigureArtisanOutfit(db);
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
                Log.Error("OutfitManager: Could not generate outfits - " + e);
            }
            return false;
        }
    }
}