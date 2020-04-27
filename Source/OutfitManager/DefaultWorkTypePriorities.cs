using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OutfitManager
{
    internal static class DefaultWorkTypePriorities
    {
        private const float MajorNegative = -2f;
        private const float MajorPositive = 2f;
        internal const float MaxStatWeight = 2.5f;
        private const float MediumPositive = 1.0f;
        private const float MicroPositive = 0.5f;
        private const float MinorPositive = 0.75f;
        private const float NanoPositive = 0.25f;
        private const float NegligibleNegative = -0.1f;
        private const float NegligiblePositive = 0.1f;
        private const float Neutral = 0f;

        public static IEnumerable<StatPriority> ArtWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.GeneralLaborSpeed, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.WorkSpeedGlobal, MediumPositive);
                return priorities;
            }
        }

        private static IEnumerable<StatPriority> BaseCombatantStatPriorities
        {
            get
            {
                var priorities = new List<StatPriority>();
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MoveSpeed, MinorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ArmorRating_Blunt, MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ArmorRating_Sharp, MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.CarryingCapacity, NegligiblePositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.Mass, NegligibleNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.PainShockThreshold, MinorPositive);
                if (ModsConfig.ActiveModsInLoadOrder.Any(m =>
                    "CETeam.CombatExtended".Equals(m.PackageId, StringComparison.OrdinalIgnoreCase)))
                {
                    StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryWeight", NegligiblePositive);
                    StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryBulk", NegligiblePositive);
                    StatPriorityHelper.SetDefaultStatPriority(priorities, "WornBulk", NegligibleNegative);
                    StatPriorityHelper.SetDefaultStatPriority(priorities, "Suppressability", MinorPositive);
                }
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> BaseWorkerStatPriorities
        {
            get
            {
                var priorities = new List<StatPriority>();
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MoveSpeed, MinorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.WorkSpeedGlobal, MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.GeneralLaborSpeed, MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ArmorRating_Blunt, NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ArmorRating_Sharp, NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.CarryingCapacity, MicroPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.Mass, NegligibleNegative);
                if (ModsConfig.ActiveModsInLoadOrder.Any(m =>
                    "CETeam.CombatExtended".Equals(m.PackageId, StringComparison.OrdinalIgnoreCase)))
                {
                    StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryWeight", MicroPositive);
                    StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryBulk", MicroPositive);
                    StatPriorityHelper.SetDefaultStatPriority(priorities, "WornBulk", NegligibleNegative);
                }
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> BrawlerStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseCombatantStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AimingDelayFactor, MajorNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MeleeDPS, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MeleeHitChance, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MeleeDodgeChance, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AccuracyTouch, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MeleeWeapon_DamageMultiplier,
                    MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> CleaningWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MoveSpeed, MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> ConstructionWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.FixBrokenDownBuildingSuccessChance,
                    MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ConstructionSpeed, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ConstructSuccessChance, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.SmoothingSpeed, MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> CookingWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.FoodPoisonChance, MajorNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "DrugCookingSpeed", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ButcheryFleshSpeed", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ButcheryFleshEfficiency", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "CookSpeed", MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> CraftingWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.GeneralLaborSpeed, MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> DoctorWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MedicalSurgerySuccessChance,
                    MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MedicalTendQuality, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MedicalTendSpeed, MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MedicalOperationSpeed", MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> FirefighterWorkTypeStatPriorities
        {
            get
            {
                var priorities = new List<StatPriority>();
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MoveSpeed, NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ArmorRating_Blunt, NegligiblePositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ArmorRating_Sharp, NegligiblePositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.Mass, NegligibleNegative);
                if (ModsConfig.ActiveModsInLoadOrder.Any(m =>
                    "CETeam.CombatExtended".Equals(m.PackageId, StringComparison.OrdinalIgnoreCase)))
                {
                    StatPriorityHelper.SetDefaultStatPriority(priorities, "WornBulk", NegligibleNegative);
                }
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> GrowingWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.PlantHarvestYield, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.PlantWorkSpeed, MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> HandlingWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.TrainAnimalChance, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.TameAnimalChance, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AnimalGatherYield, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AnimalGatherSpeed, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MeleeDodgeChance, NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MeleeHitChance, NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MeleeDPS, NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AccuracyTouch, NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.PainShockThreshold, MicroPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> HaulingWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.CarryingCapacity, MajorPositive);
                if (ModsConfig.ActiveModsInLoadOrder.Any(m =>
                    "CETeam.CombatExtended".Equals(m.PackageId, StringComparison.OrdinalIgnoreCase)))
                {
                    StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryWeight", MajorPositive);
                }
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> HuntingWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.HuntingStealth, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ShootingAccuracyPawn, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AccuracyShort, MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AccuracyMedium, MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AccuracyLong, MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MeleeDPS, Neutral);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MeleeHitChance, Neutral);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.RangedWeapon_Cooldown, MajorNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AimingDelayFactor, MajorNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.PainShockThreshold, MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> MiningWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MiningYield, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MiningSpeed, MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> PlantCuttingWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.PlantHarvestYield, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.PlantWorkSpeed, MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> ResearchWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ResearchSpeed, MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> SmithingWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.GeneralLaborSpeed, MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> SoldierStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseCombatantStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ShootingAccuracyPawn, MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AccuracyShort, MinorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AccuracyMedium, MinorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AccuracyLong, MinorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MeleeDodgeChance, Neutral);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.AimingDelayFactor, MajorNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.RangedWeapon_Cooldown, MajorNegative);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> TailoringWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.GeneralLaborSpeed, MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> WardenWorkTypeStatPriorities
        {
            get
            {
                var priorities = new List<StatPriority>();
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.MoveSpeed, MicroPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ArmorRating_Blunt, NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.ArmorRating_Sharp, NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.Mass, NegligibleNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.NegotiationAbility, MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.TradePriceImprovement, MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, StatDefOf.SocialImpact, MicroPositive);
                if (ModsConfig.ActiveModsInLoadOrder.Any(m =>
                    "CETeam.CombatExtended".Equals(m.PackageId, StringComparison.OrdinalIgnoreCase)))
                {
                    StatPriorityHelper.SetDefaultStatPriority(priorities, "WornBulk", NegligibleNegative);
                }
                return priorities;
            }
        }
    }
}