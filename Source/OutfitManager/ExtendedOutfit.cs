using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace OutfitManager
{
    public class ExtendedOutfit : Outfit, IExposable
    {
        private List<StatPriority> _statPriorities = new List<StatPriority>();

        internal bool AutoWorkPriorities;

        internal bool PenalizeTaintedApparel = true;

        [UsedImplicitly]
        public ExtendedOutfit(int uniqueId, string label) : base(uniqueId, label)
        {
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public ExtendedOutfit([NotNull] Outfit outfit) : base(outfit.uniqueId, outfit.label)
        {
            if (outfit == null) { throw new ArgumentNullException(nameof(outfit)); }
            filter.CopyAllowancesFrom(outfit.filter);
        }

        public ExtendedOutfit()
        {
        }

        public IEnumerable<StatPriority> StatPriorities => _statPriorities;

        public IEnumerable<StatDef> UnassignedStats =>
            OutfitHelper.AllAvailableStats.Except(StatPriorities.Select(i => i.Stat));

        public new void ExposeData()
        {
            Scribe_Values.Look(ref uniqueId, "uniqueId");
            Scribe_Values.Look(ref label, "label");
            Scribe_Deep.Look(ref filter, "filter");
            Scribe_Values.Look(ref PenalizeTaintedApparel, "PenalizeTaintedApparel", true);
            Scribe_Collections.Look(ref _statPriorities, "statPriorities", LookMode.Deep);
            Scribe_Values.Look(ref AutoWorkPriorities, "AutoWorkPriorities");
        }

        public void AddStatPriority(StatDef stat)
        {
            _statPriorities.Add(new StatPriority(stat, 0));
        }

        public void RemoveStatPriority(StatDef def)
        {
            _statPriorities.RemoveAll(i => i.Stat == def);
        }

        public void SetDefaultStatPriorities(IEnumerable<StatPriority> defaultStatPriorities)
        {
            if (defaultStatPriorities == null) { return; }
            foreach (var statPriority in _statPriorities) { statPriority.Default = float.NaN; }
            foreach (var statPriority in defaultStatPriorities)
            {
                StatPriorityHelper.SetDefaultStatPriority(_statPriorities, statPriority.Stat, statPriority.Weight);
            }
        }
    }
}