using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OutfitManager
{
    internal static class OutfitStatPriority
    {
        public const float MajorNegative = -5f;
        public const float MajorPositive = 5f;
        public const float MediumNegative = -2.5f;
        public const float MediumPositive = 2.5f;
        public const float MicroNegative = -0.5f;
        public const float MicroPositive = 0.5f;
        public const float MinorNegative = -1f;
        public const float MinorPositive = 1f;
        public const float Neutral = 0f;

        public static Dictionary<StatDef, float> BaseSoldierStatPriorities
        {
            get
            {
                var priorities = new Dictionary<StatDef, float>();
                ConfigureStatPriority(priorities, "MoveSpeed", MinorPositive);
                ConfigureStatPriority(priorities, "ArmorRating_Blunt", MajorPositive);
                ConfigureStatPriority(priorities, "ArmorRating_Sharp", MajorPositive);
                ConfigureStatPriority(priorities, "CarryingCapacity", MicroPositive);
                ConfigureStatPriority(priorities, "CarryWeight", MicroPositive);
                ConfigureStatPriority(priorities, "CarryBulk", MicroPositive);
                ConfigureStatPriority(priorities, "Mass", MicroNegative);
                ConfigureStatPriority(priorities, "WornBulk", MicroNegative);
                ConfigureStatPriority(priorities, "Suppressability", MinorPositive);
                ConfigureStatPriority(priorities, "PainShockThreshold", MediumPositive);
                return priorities;
            }
        }

        public static Dictionary<StatDef, float> BaseWorkerStatPriorities
        {
            get
            {
                var priorities = new Dictionary<StatDef, float>();
                ConfigureStatPriority(priorities, "MoveSpeed", MediumPositive);
                ConfigureStatPriority(priorities, "WorkSpeedGlobal", MajorPositive);
                ConfigureStatPriority(priorities, "ArmorRating_Blunt", MicroPositive);
                ConfigureStatPriority(priorities, "ArmorRating_Sharp", MicroPositive);
                ConfigureStatPriority(priorities, "CarryingCapacity", MinorPositive);
                ConfigureStatPriority(priorities, "CarryWeight", MinorPositive);
                ConfigureStatPriority(priorities, "CarryBulk", MinorPositive);
                ConfigureStatPriority(priorities, "Mass", MicroNegative);
                ConfigureStatPriority(priorities, "WornBulk", MicroNegative);
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