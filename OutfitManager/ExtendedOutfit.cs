using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using StatDefOf = OutfitManager.DefOfs.StatDefOf;

namespace OutfitManager
{
    public class ExtendedOutfit : Outfit, IExposable
    {
        private static readonly IEnumerable<StatCategoryDef> BlacklistedCategories = new List<StatCategoryDef>
        {
            StatCategoryDefOf.BasicsNonPawn, StatCategoryDefOf.Building, StatCategoryDefOf.StuffStatFactors
        };

        private static readonly IEnumerable<StatDef> BlacklistedStats = new List<StatDef>
        {
            StatDefOf.ComfyTemperatureMin,
            StatDefOf.ComfyTemperatureMax,
            StatDefOf.Insulation_Cold,
            StatDefOf.Insulation_Heat,
            StatDefOf.StuffEffectMultiplierInsulation_Cold,
            StatDefOf.StuffEffectMultiplierInsulation_Heat,
            StatDefOf.StuffEffectMultiplierArmor
        };

        private List<StatPriority> _statPriorities = new List<StatPriority>();

        internal bool AutoWorkPriorities;

        internal bool PenalizeTaintedApparel = true;
        internal FloatRange TargetTemperatures = new FloatRange(-100, 100);
        internal bool TargetTemperaturesOverride;

        [UsedImplicitly]
        public ExtendedOutfit(int uniqueId, string label) : base(uniqueId, label) { }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public ExtendedOutfit([NotNull] Outfit outfit) : base(outfit.uniqueId, outfit.label)
        {
            if (outfit == null)
            {
                throw new ArgumentNullException(nameof(outfit));
            }
            filter.CopyAllowancesFrom(outfit.filter);
        }

        public ExtendedOutfit() { }

        internal static IEnumerable<StatDef> AllAvailableStats =>
            DefDatabase<StatDef>.AllDefs.Where(i => !BlacklistedCategories.Contains(i.category))
                .Except(BlacklistedStats).ToList();

        public IEnumerable<StatPriority> StatPriorities => _statPriorities;

        public IEnumerable<StatDef> UnassignedStats => AllAvailableStats.Except(StatPriorities.Select(i => i.Stat));

        public new void ExposeData()
        {
            Scribe_Values.Look(ref uniqueId, "uniqueId");
            Scribe_Values.Look(ref label, "label");
            Scribe_Deep.Look(ref filter, "filter");
            Scribe_Values.Look(ref TargetTemperaturesOverride, "targetTemperaturesOverride");
            Scribe_Values.Look(ref TargetTemperatures, "targetTemperatures");
            Scribe_Values.Look(ref PenalizeTaintedApparel, "PenaltyWornByCorpse", true);
            Scribe_Collections.Look(ref _statPriorities, "statPriorities", LookMode.Deep);
            Scribe_Values.Look(ref AutoWorkPriorities, "AutoWorkPriorities");
        }

        public void AddStatPriorities(IEnumerable<StatPriority> priorities)
        {
            _statPriorities.AddRange(priorities);
        }

        public void AddStatPriority(StatDef def, float priority, float defaultPriority = float.NaN)
        {
            _statPriorities.Insert(0, new StatPriority(def, priority, defaultPriority));
        }

        public void RemoveStatPriority(StatDef def)
        {
            _statPriorities.RemoveAll(i => i.Stat == def);
        }
    }
}