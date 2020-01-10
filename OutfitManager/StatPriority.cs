using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace OutfitManager
{
    public class StatPriority : IExposable
    {
        private StatDef _stat;

        [SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
        public float Default;

        [SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
        public float Weight;

        public StatPriority(StatDef stat, float weight, float defaultWeight = float.NaN)
        {
            _stat = stat;
            Weight = weight;
            Default = defaultWeight;
        }

        [UsedImplicitly]
        public StatPriority()
        {
            // Used by ExposeData
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public bool IsDefault => Default == Weight;

        public bool IsManual => float.IsNaN(Default);
        public bool IsOverride => !IsManual && !IsDefault;

        public StatDef Stat => _stat;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref _stat, "Stat");
            Scribe_Values.Look(ref Weight, "Weight");
            Scribe_Values.Look(ref Default, "Default", float.NaN);
        }
    }
}