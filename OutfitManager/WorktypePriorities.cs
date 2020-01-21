using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Verse;

namespace OutfitManager
{
    public class WorktypePriorities : IExposable
    {
        [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public List<StatPriority> Priorities = new List<StatPriority>();

        [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public WorkTypeDef Worktype;

        [UsedImplicitly]
        public WorktypePriorities()
        {
            // used by ExposeData
        }

        public WorktypePriorities(WorkTypeDef worktype, List<StatPriority> priorities)
        {
            Worktype = worktype;
            Priorities = priorities;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref Worktype, "worktype");
            Scribe_Collections.Look(ref Priorities, "statPriorities", LookMode.Deep);
        }
    }
}