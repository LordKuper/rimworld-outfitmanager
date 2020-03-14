using System.Collections.Generic;
using System.Linq;

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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "SculptingSpeed", MajorPositive);
                return priorities;
            }
        }

        private static IEnumerable<StatPriority> BaseCombatantStatPriorities
        {
            get
            {
                var priorities = new List<StatPriority>();
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MoveSpeed", MinorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ArmorRating_Blunt", MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ArmorRating_Sharp", MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryingCapacity", NegligiblePositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryWeight", NegligiblePositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryBulk", NegligiblePositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "Mass", NegligibleNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "WornBulk", NegligibleNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "Suppressability", MinorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "PainShockThreshold", MinorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> BaseWorkerStatPriorities
        {
            get
            {
                var priorities = new List<StatPriority>();
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MoveSpeed", MinorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "WorkSpeedGlobal", MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ArmorRating_Blunt", NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ArmorRating_Sharp", NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryingCapacity", MicroPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryWeight", MicroPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryBulk", MicroPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "Mass", NegligibleNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "WornBulk", NegligibleNegative);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AimingDelayFactor", MajorNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MeleeDPS", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MeleeHitChance", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MeleeDodgeChance", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AccuracyTouch", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MeleeWeapon_DamageMultiplier", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MoveSpeed", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "FixBrokenDownBuildingSuccessChance",
                    MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ConstructionSpeed", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ConstructSuccessChance", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "SmoothingSpeed", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "DrugCookingSpeed", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ButcheryFleshSpeed", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ButcheryFleshEfficiency", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "CookSpeed", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "FoodPoisonChance", MajorNegative);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "SmeltingSpeed", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ButcheryMechanoidSpeed", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ButcheryMechanoidEfficiency", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MedicalSurgerySuccessChance", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MedicalOperationSpeed", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MedicalTendQuality", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MedicalTendSpeed", MediumPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> FirefighterWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MoveSpeed", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "PlantHarvestYield", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "PlantWorkSpeed", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "TrainAnimalChance", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "TameAnimalChance", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MeleeDodgeChance", MinorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MeleeHitChance", NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MeleeDPS", NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AccuracyTouch", NanoPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MeleeWeapon_DamageMultiplier", Neutral);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "PainShockThreshold", MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AnimalGatherYield", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AnimalGatherSpeed", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryingCapacity", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "CarryWeight", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ShootingAccuracyPawn", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AccuracyShort", MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AccuracyMedium", MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AccuracyLong", MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MeleeDPS", Neutral);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MeleeHitChance", Neutral);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "RangedWeapon_Cooldown", MajorNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AimingDelayFactor", MajorNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "PainShockThreshold", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MiningYield", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MiningSpeed", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "PlantHarvestYield", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "PlantWorkSpeed", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ResearchSpeed", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "SmeltingSpeed", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "SmithingSpeed", MajorPositive);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "ShootingAccuracyPawn", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AccuracyShort", MinorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AccuracyMedium", MinorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AccuracyLong", MinorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "MeleeDodgeChance", Neutral);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "AimingDelayFactor", MajorNegative);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "RangedWeapon_Cooldown", MajorNegative);
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
                StatPriorityHelper.SetDefaultStatPriority(priorities, "TailoringSpeed", MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> WardenWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                StatPriorityHelper.SetDefaultStatPriority(priorities, "NegotiationAbility", MajorPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "SocialImpact", MediumPositive);
                StatPriorityHelper.SetDefaultStatPriority(priorities, "TradePriceImprovement", MajorPositive);
                return priorities;
            }
        }
    }
}