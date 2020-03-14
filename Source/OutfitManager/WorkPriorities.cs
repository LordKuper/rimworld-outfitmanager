using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using RimWorld.Planet;
using Verse;

namespace OutfitManager
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class WorkPriorities : WorldComponent
    {
        private static List<WorktypePriorities> _worktypePriorities;

        public WorkPriorities(World world) : base(world)
        {
            Log.Message("WorldComponent created!");
        }

        [SuppressMessage("Style", "IDE0010:Add missing cases")]
        private static List<StatPriority> DefaultPriorities(string worktype)
        {
            var stats = new List<StatPriority>();
            switch (worktype)
            {
                case "Art":
                    stats.AddRange(DefaultWorkTypePriorities.ArtWorkTypeStatPriorities);
                    break;
                case "BasicWorker":
                    stats.AddRange(DefaultWorkTypePriorities.BaseWorkerStatPriorities);
                    break;
                case "Cleaning":
                    stats.AddRange(DefaultWorkTypePriorities.CleaningWorkTypeStatPriorities);
                    break;
                case "Cooking":
                    stats.AddRange(DefaultWorkTypePriorities.CookingWorkTypeStatPriorities);
                    break;
                case "Construction":
                    stats.AddRange(DefaultWorkTypePriorities.ConstructionWorkTypeStatPriorities);
                    break;
                case "Crafting":
                    stats.AddRange(DefaultWorkTypePriorities.CraftingWorkTypeStatPriorities);
                    break;
                case "Doctor":
                    stats.AddRange(DefaultWorkTypePriorities.DoctorWorkTypeStatPriorities);
                    break;
                case "Firefighter":
                    stats.AddRange(DefaultWorkTypePriorities.FirefighterWorkTypeStatPriorities);
                    break;
                case "Growing":
                    stats.AddRange(DefaultWorkTypePriorities.GrowingWorkTypeStatPriorities);
                    break;
                case "Handling":
                    stats.AddRange(DefaultWorkTypePriorities.HandlingWorkTypeStatPriorities);
                    break;
                case "Hauling":
                    stats.AddRange(DefaultWorkTypePriorities.HaulingWorkTypeStatPriorities);
                    break;
                case "Hunting":
                    stats.AddRange(DefaultWorkTypePriorities.HuntingWorkTypeStatPriorities);
                    break;
                case "Mining":
                    stats.AddRange(DefaultWorkTypePriorities.MiningWorkTypeStatPriorities);
                    break;
                case "PlantCutting":
                    stats.AddRange(DefaultWorkTypePriorities.PlantCuttingWorkTypeStatPriorities);
                    break;
                case "Research":
                    stats.AddRange(DefaultWorkTypePriorities.ResearchWorkTypeStatPriorities);
                    break;
                case "Smithing":
                    stats.AddRange(DefaultWorkTypePriorities.SmithingWorkTypeStatPriorities);
                    break;
                case "Tailoring":
                    stats.AddRange(DefaultWorkTypePriorities.TailoringWorkTypeStatPriorities);
                    break;
                case "Warden":
                    stats.AddRange(DefaultWorkTypePriorities.WardenWorkTypeStatPriorities);
                    break;
            }
            return stats;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _worktypePriorities, "worktypePriorities", LookMode.Deep);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (!_worktypePriorities.NullOrEmpty()) { return; }
            _worktypePriorities = new List<WorktypePriorities>();
            foreach (var worktype in DefDatabase<WorkTypeDef>.AllDefsListForReading)
            {
                _worktypePriorities.Add(new WorktypePriorities(worktype, DefaultPriorities(worktype.defName)));
            }
        }

        private static IEnumerable<StatPriority> GetWorkTypeStatPriorities([NotNull] WorkTypeDef worktype)
        {
            if (worktype == null) { throw new ArgumentNullException(nameof(worktype)); }
            var worktypePriorities = _worktypePriorities.Find(wp => wp.Worktype == worktype);
            if (worktypePriorities == null)
            {
                worktypePriorities = new WorktypePriorities(worktype, DefaultPriorities(worktype.defName));
                _worktypePriorities.Add(worktypePriorities);
            }
            return worktypePriorities.Priorities.Select(o => new StatPriority(o.Stat, o.Weight, o.Weight));
        }

        public static IEnumerable<StatPriority> GetWorkTypeStatPrioritiesForPawn([NotNull] Pawn pawn)
        {
            if (pawn == null) { throw new ArgumentNullException(nameof(pawn)); }
            var statPriorities = new List<StatPriority>();
            var workTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            var workTypePriorities = new Dictionary<string, int>();
            var workTypeWeights = new Dictionary<string, IEnumerable<StatPriority>>();
            foreach (var workType in workTypes)
            {
                var priority = pawn.workSettings?.GetPriority(workType) ?? 0;
                if (priority <= 0) { continue; }
                workTypePriorities.Add(workType.defName, priority);
                workTypeWeights.Add(workType.defName, GetWorkTypeStatPriorities(workType));
            }
            if (!workTypePriorities.Any()) { return new List<StatPriority>(); }
            var priorityRange =
                new IntRange(workTypePriorities.Min(s => s.Value), workTypePriorities.Max(s => s.Value));
            #if DEBUG
            Log.Message(
                $"OutfitManager: Work priorities for pawn '{pawn.Name}' [{priorityRange.min};{priorityRange.max}] -----",
                true);
            Log.Message("OutfitManager: ------------------------------", true);
            #endif
            foreach (var workTypePriority in workTypePriorities)
            {
                var normalizedWorkPriority = priorityRange.min == priorityRange.max
                    ? 1f
                    : 1f - (float) (workTypePriority.Value - priorityRange.min) /
                    (priorityRange.max + 1 - priorityRange.min);
                #if DEBUG
                Log.Message(
                    $"OutfitManager: {workTypePriority.Key} = {workTypePriority.Value} ({normalizedWorkPriority} norm)",
                    true);
                #endif
                foreach (var workTypeStatPriority in workTypeWeights[workTypePriority.Key])
                {
                    var statPriority = statPriorities.Find(o =>
                        o.Stat.defName.Equals(workTypeStatPriority.Stat.defName, StringComparison.OrdinalIgnoreCase));
                    if (statPriority == null)
                    {
                        statPriority = new StatPriority(workTypeStatPriority.Stat,
                            workTypeStatPriority.Weight * normalizedWorkPriority);
                        statPriorities.Add(statPriority);
                    }
                    else { statPriority.Weight += workTypeStatPriority.Weight * normalizedWorkPriority; }
                }
            }
            #if DEBUG
            Log.Message("OutfitManager: ---------------------------------------------", true);
            #endif
            return statPriorities;
        }
    }
}