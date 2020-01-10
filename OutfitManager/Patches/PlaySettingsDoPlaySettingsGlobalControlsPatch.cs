using Harmony;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace OutfitManager.Patches
{
    [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
    [UsedImplicitly]
    public static class PlaySettingsDoPlaySettingsGlobalControlsPatch
    {
        [UsedImplicitly]
        private static void Postfix(WidgetRow row, bool worldView)
        {
            if (worldView)
            {
                return;
            }
            row.ToggleableIcon(ref OutfitManagerMod.ShowApparelScores, ResourceBank.Textures.ShirtBasic,
                ResourceBank.Strings.OutfitShow, SoundDefOf.Mouseover_ButtonToggle);
        }
    }
}