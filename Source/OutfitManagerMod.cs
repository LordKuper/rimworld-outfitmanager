using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace LordKuper.OutfitManager
{
    /// <summary>
    ///     The main mod class for the Outfit Manager mod.
    ///     Handles initialization and Harmony patching.
    /// </summary>
    [UsedImplicitly]
    public class OutfitManagerMod : Mod
    {
        /// <summary>
        ///     The unique identifier for the Outfit Manager mod.
        /// </summary>
        internal const string ModId = "LordKuper.OutfitManager";

        /// <summary>
        ///     Initializes a new instance of the <see cref="OutfitManagerMod" /> class.
        ///     Sets up logging and applies all Harmony patches for this assembly.
        /// </summary>
        /// <param name="content">The mod content pack.</param>
        public OutfitManagerMod(ModContentPack content) : base(content)
        {
            Logger.LogMessage($"Initializing (v.{Assembly.GetExecutingAssembly().GetName().Version})...");
            GetSettings<Settings>();
            var harmony = new Harmony(ModId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        ///     Draws the settings window contents for the mod.
        /// </summary>
        /// <param name="inRect">The rectangle area to draw the settings window in.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Settings.DoWindowContents(inRect);
        }

        /// <summary>
        ///     Gets the category name for the mod settings.
        /// </summary>
        /// <returns>The mod title string.</returns>
        public override string SettingsCategory()
        {
            return Resources.Strings.ModTitle;
        }
    }
}