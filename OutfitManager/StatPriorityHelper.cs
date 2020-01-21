using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace OutfitManager
{
    internal static class StatPriorityHelper
    {
        public const float MajorNegative = -2f;
        public const float MajorPositive = 2f;
        public const float MaxStatWeight = 2.5f;
        public const float MediumNegative = -1.0f;
        public const float MediumPositive = 1.0f;
        public const float MicroNegative = -0.5f;
        public const float MicroPositive = 0.5f;
        public const float MinorNegative = -0.75f;
        public const float MinorPositive = 0.75f;
        public const float NanoNegative = -0.25f;
        public const float NanoPositive = 0.25f;
        public const float NegligibleNegative = -0.1f;
        public const float NegligiblePositive = 0.1f;
        public const float Neutral = 0f;

        public static IEnumerable<StatPriority> ArtWorkTypeStatPriorities
        {
            get
            {
                var priorities =
                    new List<StatPriority>(
                        BaseWorkerStatPriorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight)));
                ConfigureStatPriority(priorities, "SculptingSpeed", MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> BaseCombatantStatPriorities
        {
            get
            {
                var priorities = new List<StatPriority>();
                ConfigureStatPriority(priorities, "MoveSpeed", MinorPositive);
                ConfigureStatPriority(priorities, "ArmorRating_Blunt", MediumPositive);
                ConfigureStatPriority(priorities, "ArmorRating_Sharp", MediumPositive);
                ConfigureStatPriority(priorities, "CarryingCapacity", NegligiblePositive);
                ConfigureStatPriority(priorities, "CarryWeight", NegligiblePositive);
                ConfigureStatPriority(priorities, "CarryBulk", NegligiblePositive);
                ConfigureStatPriority(priorities, "Mass", NegligibleNegative);
                ConfigureStatPriority(priorities, "WornBulk", NegligibleNegative);
                ConfigureStatPriority(priorities, "Suppressability", MinorPositive);
                ConfigureStatPriority(priorities, "PainShockThreshold", MinorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> BaseWorkerStatPriorities
        {
            get
            {
                var priorities = new List<StatPriority>();
                ConfigureStatPriority(priorities, "MoveSpeed", MinorPositive);
                ConfigureStatPriority(priorities, "WorkSpeedGlobal", MediumPositive);
                ConfigureStatPriority(priorities, "ArmorRating_Blunt", NanoPositive);
                ConfigureStatPriority(priorities, "ArmorRating_Sharp", NanoPositive);
                ConfigureStatPriority(priorities, "CarryingCapacity", MicroPositive);
                ConfigureStatPriority(priorities, "CarryWeight", MicroPositive);
                ConfigureStatPriority(priorities, "CarryBulk", MicroPositive);
                ConfigureStatPriority(priorities, "Mass", NegligibleNegative);
                ConfigureStatPriority(priorities, "WornBulk", NegligibleNegative);
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
                ConfigureStatPriority(priorities, "AimingDelayFactor", MajorNegative);
                ConfigureStatPriority(priorities, "MeleeDPS", MajorPositive);
                ConfigureStatPriority(priorities, "MeleeHitChance", MajorPositive);
                ConfigureStatPriority(priorities, "MeleeDodgeChance", MajorPositive);
                ConfigureStatPriority(priorities, "AccuracyTouch", MajorPositive);
                ConfigureStatPriority(priorities, "MeleeWeapon_DamageMultiplier", MajorPositive);
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
                ConfigureStatPriority(priorities, "MoveSpeed", MajorPositive);
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
                ConfigureStatPriority(priorities, "FixBrokenDownBuildingSuccessChance", MajorPositive);
                ConfigureStatPriority(priorities, "ConstructionSpeed", MajorPositive);
                ConfigureStatPriority(priorities, "ConstructSuccessChance", MajorPositive);
                ConfigureStatPriority(priorities, "SmoothingSpeed", MajorPositive);
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
                ConfigureStatPriority(priorities, "DrugCookingSpeed", MajorPositive);
                ConfigureStatPriority(priorities, "ButcheryFleshSpeed", MajorPositive);
                ConfigureStatPriority(priorities, "ButcheryFleshEfficiency", MajorPositive);
                ConfigureStatPriority(priorities, "CookSpeed", MajorPositive);
                ConfigureStatPriority(priorities, "FoodPoisonChance", MajorNegative);
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
                ConfigureStatPriority(priorities, "SmeltingSpeed", MajorPositive);
                ConfigureStatPriority(priorities, "ButcheryMechanoidSpeed", MajorPositive);
                ConfigureStatPriority(priorities, "ButcheryMechanoidEfficiency", MajorPositive);
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
                ConfigureStatPriority(priorities, "MedicalSurgerySuccessChance", MajorPositive);
                ConfigureStatPriority(priorities, "MedicalOperationSpeed", MajorPositive);
                ConfigureStatPriority(priorities, "MedicalTendQuality", MajorPositive);
                ConfigureStatPriority(priorities, "MedicalTendSpeed", MediumPositive);
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
                ConfigureStatPriority(priorities, "MoveSpeed", MajorPositive);
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
                ConfigureStatPriority(priorities, "PlantHarvestYield", MajorPositive);
                ConfigureStatPriority(priorities, "PlantWorkSpeed", MajorPositive);
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
                ConfigureStatPriority(priorities, "TrainAnimalChance", MajorPositive);
                ConfigureStatPriority(priorities, "TameAnimalChance", MajorPositive);
                ConfigureStatPriority(priorities, "MeleeDodgeChance", MinorPositive);
                ConfigureStatPriority(priorities, "MeleeHitChance", NanoPositive);
                ConfigureStatPriority(priorities, "MeleeDPS", NanoPositive);
                ConfigureStatPriority(priorities, "AccuracyTouch", NanoPositive);
                ConfigureStatPriority(priorities, "MeleeWeapon_DamageMultiplier", Neutral);
                ConfigureStatPriority(priorities, "PainShockThreshold", MediumPositive);
                ConfigureStatPriority(priorities, "AnimalGatherYield", MajorPositive);
                ConfigureStatPriority(priorities, "AnimalGatherSpeed", MajorPositive);
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
                ConfigureStatPriority(priorities, "CarryingCapacity", MajorPositive);
                ConfigureStatPriority(priorities, "CarryWeight", MajorPositive);
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
                ConfigureStatPriority(priorities, "ShootingAccuracyPawn", MajorPositive);
                ConfigureStatPriority(priorities, "AccuracyShort", MediumPositive);
                ConfigureStatPriority(priorities, "AccuracyMedium", MediumPositive);
                ConfigureStatPriority(priorities, "AccuracyLong", MediumPositive);
                ConfigureStatPriority(priorities, "MeleeDPS", Neutral);
                ConfigureStatPriority(priorities, "MeleeHitChance", Neutral);
                ConfigureStatPriority(priorities, "RangedWeapon_Cooldown", MajorNegative);
                ConfigureStatPriority(priorities, "AimingDelayFactor", MajorNegative);
                ConfigureStatPriority(priorities, "PainShockThreshold", MajorPositive);
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
                ConfigureStatPriority(priorities, "MiningYield", MajorPositive);
                ConfigureStatPriority(priorities, "MiningSpeed", MajorPositive);
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
                ConfigureStatPriority(priorities, "PlantHarvestYield", MajorPositive);
                ConfigureStatPriority(priorities, "PlantWorkSpeed", MajorPositive);
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
                ConfigureStatPriority(priorities, "ResearchSpeed", MajorPositive);
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
                ConfigureStatPriority(priorities, "SmeltingSpeed", MajorPositive);
                ConfigureStatPriority(priorities, "SmithingSpeed", MajorPositive);
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
                ConfigureStatPriority(priorities, "ShootingAccuracyPawn", MajorPositive);
                ConfigureStatPriority(priorities, "AccuracyShort", MinorPositive);
                ConfigureStatPriority(priorities, "AccuracyMedium", MinorPositive);
                ConfigureStatPriority(priorities, "AccuracyLong", MinorPositive);
                ConfigureStatPriority(priorities, "MeleeDodgeChance", Neutral);
                ConfigureStatPriority(priorities, "AimingDelayFactor", MajorNegative);
                ConfigureStatPriority(priorities, "RangedWeapon_Cooldown", MajorNegative);
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
                ConfigureStatPriority(priorities, "TailoringSpeed", MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> VanillaStatPriorities
        {
            get
            {
                var priorities = new List<StatPriority>();
                ConfigureStatPriority(priorities, "ArmorRating_Blunt", MediumPositive);
                ConfigureStatPriority(priorities, "ArmorRating_Sharp", MediumPositive);
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
                ConfigureStatPriority(priorities, "NegotiationAbility", MajorPositive);
                ConfigureStatPriority(priorities, "SocialImpact", MediumPositive);
                ConfigureStatPriority(priorities, "TradePriceImprovement", MajorPositive);
                return priorities;
            }
        }

        public static IEnumerable<StatPriority> CalculateStatPriorities(Pawn pawn,
            IEnumerable<StatPriority> statPriorities, bool autoWorkPriorities)
        {
            var originalStatPriorities = statPriorities.ToList();
            if (!originalStatPriorities.Any())
            {
                return originalStatPriorities;
            }
            #if DEBUG
            Log.Message($"OutfitManager: Normalizing stat priorities (autoWorkPriorities = {autoWorkPriorities})",
                true);
            #endif
            var normalizedStatPriorities = originalStatPriorities
                .Select(statPriority => new StatPriority(statPriority.Stat, statPriority.Weight)).ToList();
            List<StatPriority> workStatPriorities = null;
            if (autoWorkPriorities)
            {
                workStatPriorities = WorkPriorities.GetWorkTypeStatPrioritiesForPawn(pawn).ToList();
                foreach (var workStatPriority in workStatPriorities)
                {
                    var sourceStatPriority = normalizedStatPriorities.Find(o =>
                        o.Stat.defName.Equals(workStatPriority.Stat.defName, StringComparison.OrdinalIgnoreCase));
                    if (sourceStatPriority == null)
                    {
                        normalizedStatPriorities.Add(new StatPriority(workStatPriority.Stat, workStatPriority.Weight));
                    }
                    else
                    {
                        sourceStatPriority.Weight += workStatPriority.Weight;
                    }
                }
            }
            NormalizeStatPriorities(normalizedStatPriorities);
            #if DEBUG
            Log.Message("OutfitManager: Normalized stat priorities -----", true);
            foreach (var statPriority in normalizedStatPriorities)
            {
                var originalWeight =
                    originalStatPriorities.Find(o => o.Stat.defName == statPriority.Stat.defName)?.Weight ?? 0;
                var workWeight = autoWorkPriorities
                    ? workStatPriorities.Find(o => o.Stat.defName == statPriority.Stat.defName)?.Weight ?? 0
                    : 0;
                Log.Message(
                    $"OutfitManager: {statPriority.Stat.defName} = {statPriority.Weight} ({originalWeight} original) ({workWeight} work)",
                    true);
            }
            Log.Message("OutfitManager: ------------------------------", true);
            #endif
            return normalizedStatPriorities;
        }

        private static void ConfigureStatPriority(ICollection<StatPriority> priorities, string name, float weight)
        {
            var statDef = OutfitStatHelper.GetStatDefByName(name);
            if (statDef == null)
            {
                Log.Message($"OutfitManager: Could not find apparel stat named '{name}'");
                return;
            }
            var priority =
                priorities.FirstOrDefault(o => o.Stat.defName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (priority != null)
            {
                priority.Weight = weight;
                priority.Default = weight;
            }
            else
            {
                priorities.Add(new StatPriority(name, weight, weight));
            }
        }

        private static void NormalizeStatPriorities([NotNull] ICollection<StatPriority> statPriorities)
        {
            if (statPriorities == null)
            {
                throw new ArgumentNullException(nameof(statPriorities));
            }
            if (!statPriorities.Any())
            {
                return;
            }
            var weightSum = statPriorities.Sum(priority => Math.Abs(priority.Weight));
            foreach (var statPriority in statPriorities)
            {
                statPriority.Weight *= OutfitManagerMod.ApparelTotalStatWeight / weightSum;
            }
        }
    }
}