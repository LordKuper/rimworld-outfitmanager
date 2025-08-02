using Verse;

namespace LordKuper.OutfitManager;

/// <summary>
///     Provides access to common resources used throughout the mod.
/// </summary>
public static class Resources
{
    /// <summary>
    ///     Contains string resources for the mod.
    /// </summary>
    public static class Strings
    {
        /// <summary>
        ///     Gets the translated mod title string.
        /// </summary>
        public static string ModTitle => $"{OutfitManagerMod.ModId}.{nameof(ModTitle)}".Translate();

        /// <summary>
        ///     Contains string resources related to mod settings.
        /// </summary>
        public static class Settings
        {
            /// <summary>
            ///     Contains general settings string resources.
            /// </summary>
            public static class General
            {
                /// <summary>
                ///     Gets the translated title for general settings.
                /// </summary>
                public static string Title =>
                    $"{OutfitManagerMod.ModId}.{nameof(General)}.{nameof(Title)}".Translate();

                /// <summary>
                ///     Gets the translated label for the work type score factor setting.
                /// </summary>
                public static string WorkTypeScoreFactorLabel =>
                    $"{OutfitManagerMod.ModId}.{nameof(General)}.{nameof(WorkTypeScoreFactorLabel)}".Translate();

                /// <summary>
                ///     Gets the translated tooltip for the work type score factor setting.
                /// </summary>
                public static string WorkTypeScoreFactorTooltip =>
                    $"{OutfitManagerMod.ModId}.{nameof(General)}.{nameof(WorkTypeScoreFactorTooltip)}".Translate();
            }

            /// <summary>
            ///     Contains string resources related to work types settings.
            /// </summary>
            public static class WorkTypes
            {
                public static string SelectWorkType =>
                    $"{OutfitManagerMod.ModId}.{nameof(WorkTypes)}.{nameof(SelectWorkType)}".Translate();

                /// <summary>
                ///     Gets the translated title for work types settings.
                /// </summary>
                public static string Title =>
                    $"{OutfitManagerMod.ModId}.{nameof(WorkTypes)}.{nameof(Title)}".Translate();
            }
        }
    }
}