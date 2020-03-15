using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace OutfitManager
{
    internal static class StatPriorityHelper
    {
        public static IEnumerable<StatPriority> CalculateStatPriorities(Pawn pawn,
            IEnumerable<StatPriority> statPriorities, bool autoWorkPriorities)
        {
            var originalStatPriorities = statPriorities.ToList();
            if (!originalStatPriorities.Any()) { return originalStatPriorities; }
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
                    var sourceStatPriority = normalizedStatPriorities.Find(o => o.Stat == workStatPriority.Stat);
                    if (sourceStatPriority == null)
                    {
                        normalizedStatPriorities.Add(new StatPriority(workStatPriority.Stat, workStatPriority.Weight));
                    }
                    else { sourceStatPriority.Weight = (sourceStatPriority.Weight + workStatPriority.Weight) / 2; }
                }
            }
            NormalizeStatPriorities(normalizedStatPriorities);
            #if DEBUG
            Log.Message("OutfitManager: Normalized stat priorities -----", true);
            foreach (var statPriority in normalizedStatPriorities)
            {
                var originalWeight = originalStatPriorities.Find(o => o.Stat == statPriority.Stat)?.Weight ?? 0;
                var workWeight = autoWorkPriorities
                    ? workStatPriorities.Find(o => o.Stat == statPriority.Stat)?.Weight ?? 0
                    : 0;
                Log.Message(
                    $"OutfitManager: {statPriority.Stat.defName} = {statPriority.Weight} ({originalWeight} original) ({workWeight} work)",
                    true);
            }
            Log.Message("OutfitManager: ------------------------------", true);
            #endif
            return normalizedStatPriorities;
        }

        private static void NormalizeStatPriorities([NotNull] ICollection<StatPriority> statPriorities)
        {
            if (statPriorities == null) { throw new ArgumentNullException(nameof(statPriorities)); }
            if (!statPriorities.Any()) { return; }
            var weightSum = statPriorities.Sum(priority => Math.Abs(priority.Weight));
            foreach (var statPriority in statPriorities)
            {
                statPriority.Weight *= OutfitManagerMod.ApparelTotalStatWeight / weightSum;
            }
        }

        public static void SetDefaultStatPriority(ICollection<StatPriority> priorities, StatDef stat,
            float defaultWeight)
        {
            var priority = priorities.FirstOrDefault(o => o.Stat == stat);
            if (priority != null)
            {
                if (Math.Abs(priority.Weight - priority.Default) < 0.0001) { priority.Weight = defaultWeight; }
                priority.Default = defaultWeight;
            }
            else { priorities.Add(new StatPriority(stat, defaultWeight, defaultWeight)); }
        }

        public static void SetDefaultStatPriority(ICollection<StatPriority> priorities, string name, float weight)
        {
            var stat = StatDef.Named(name);
            if (stat == null)
            {
                Log.Message($"OutfitManager: Could not find apparel stat named '{name}'");
                return;
            }
            SetDefaultStatPriority(priorities, stat, weight);
        }
    }
}