using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace OutfitManager.Patches
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.DrawGUIOverlay))]
    [UsedImplicitly]
    public static class ThingDrawGuiOverlayPatch
    {
        [UsedImplicitly]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static void Postfix(Thing __instance)
        {
            if (!OutfitManagerMod.ShowApparelScores) { return; }
            if (Find.CameraDriver.CurrentZoom != CameraZoomRange.Closest) { return; }
            if (!(Find.Selector.SingleSelectedThing is Pawn pawn) || !pawn.IsColonistPlayerControlled) { return; }
            if (!(__instance is Apparel apparel)) { return; }
            if (!(pawn.outfits.CurrentOutfit is ExtendedOutfit outfit)) { return; }
            if (!outfit.filter.Allows(apparel)) { return; }
            var wornApparelScores = pawn.apparel.WornApparel
                .Select(wornApparel => OutfitManagerMod.ApparelScoreRaw(pawn, wornApparel)).ToList();
            var score = JobGiver_OptimizeApparel.ApparelScoreGain_NewTmp(pawn, apparel, wornApparelScores);
            if (!(Math.Abs(score) > 0.01f)) { return; }
            var pos = GenMapUI.LabelDrawPosFor(apparel, 0f);
            GenMapUI.DrawThingLabel(pos, score.ToString("F1", CultureInfo.InvariantCulture),
                BeautyDrawer.BeautyColor(score, 3f));
        }
    }
}