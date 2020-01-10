using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Verse;

namespace OutfitManager
{
    public static class ResourceBank
    {
        [SuppressMessage("Design", "CA1034:Nested types should not be visible")]
        public static class Strings
        {
            public static readonly string AutoWorkPriorities = "AutoWorkPriorities".Translate();
            public static readonly string AutoWorkPrioritiesTooltip = "AutoWorkPrioritiesTooltip".Translate();
            public static readonly string None = "None".Translate();
            public static readonly string OutfitShow = "OutfitShow".Translate();
            public static readonly string PenalizeTaintedApparel = "PenalizeTaintedApparel".Translate();
            public static readonly string PenalizeTaintedApparelTooltip = "PenalizeTaintedApparelTooltip".Translate();
            public static readonly string PreferedStats = "PreferedStats".Translate();
            public static readonly string PreferedTemperature = "PreferedTemperature".Translate();
            public static readonly string StatPriorityAdd = "StatPriorityAdd".Translate();
            public static readonly string TemperatureRangeReset = "TemperatureRangeReset".Translate();

            public static string StatPriorityDelete(string labelCap)
            {
                return "StatPriorityDelete".Translate(labelCap);
            }

            public static string StatPriorityReset(string labelCap)
            {
                return "StatPriorityReset".Translate(labelCap);
            }
        }

        [StaticConstructorOnStartup]
        [SuppressMessage("Design", "CA1034:Nested types should not be visible")]
        public static class Textures
        {
            public static readonly Texture2D AddButton = ContentFinder<Texture2D>.Get("add");

            public static readonly Texture2D DeleteButton = ContentFinder<Texture2D>.Get("delete");

            public static readonly Texture2D RangeSlider = ContentFinder<Texture2D>.Get("UI/Widgets/RangeSlider");

            public static readonly Texture2D ResetButton = ContentFinder<Texture2D>.Get("reset");

            public static readonly Texture2D ShirtBasic =
                ContentFinder<Texture2D>.Get("Things/Pawn/Humanlike/Apparel/ShirtBasic/ShirtBasic");
        }
    }
}