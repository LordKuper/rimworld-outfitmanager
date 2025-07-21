using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
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
            var harmony = new Harmony(ModId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}