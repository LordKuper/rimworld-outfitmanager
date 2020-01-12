using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OutfitManager
{
    internal static class OutfitStatPriority
    {
        public const float MajorNegative = -2f;
        public const float MajorPositive = 2f;
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

        public static Dictionary<StatDef, float> BaseSoldierStatPriorities
        {
            get
            {
                var priorities = new Dictionary<StatDef, float>();
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

        public static Dictionary<StatDef, float> BaseWorkerStatPriorities
        {
            get
            {
                var priorities = new Dictionary<StatDef, float>();
                ConfigureStatPriority(priorities, "MoveSpeed", MinorPositive);
                ConfigureStatPriority(priorities, "WorkSpeedGlobal", MediumPositive);
                ConfigureStatPriority(priorities, "ArmorRating_Blunt", NegligiblePositive);
                ConfigureStatPriority(priorities, "ArmorRating_Sharp", NegligiblePositive);
                ConfigureStatPriority(priorities, "CarryingCapacity", MicroPositive);
                ConfigureStatPriority(priorities, "CarryWeight", MicroPositive);
                ConfigureStatPriority(priorities, "CarryBulk", MicroPositive);
                ConfigureStatPriority(priorities, "Mass", NegligibleNegative);
                ConfigureStatPriority(priorities, "WornBulk", NegligibleNegative);
                return priorities;
            }
        }

        public static void ConfigureStatPriority(Dictionary<StatDef, float> priorities, string name, float weight)
        {
            var statDef = ExtendedOutfit.GetStatDefByName(name);
            if (statDef == null)
            {
                Log.Message($"OutfitManager: Could not find apparel stat named '{name}'");
                return;
            }
            if (priorities.ContainsKey(statDef))
            {
                priorities[statDef] = weight;
            }
            else
            {
                priorities.Add(statDef, weight);
            }
        }
    }
}