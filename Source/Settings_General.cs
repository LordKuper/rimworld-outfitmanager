using LordKuper.Common.UI;
using UnityEngine;
using Verse;

namespace LordKuper.OutfitManager;

/// <summary>
///     Provides general settings for the Outfit Manager mod.
/// </summary>
public partial class Settings
{
    /// <summary>
    ///     The maximum allowed value for the work type score factor.
    /// </summary>
    private const float WorkTypeScoreFactorMax = 5f;

    /// <summary>
    ///     The minimum allowed value for the work type score factor.
    /// </summary>
    private const float WorkTypeScoreFactorMin = 0f;

    /// <summary>
    ///     Stores the height of the general tab content for scroll calculations.
    /// </summary>
    private static float _generalContentHeight;

    /// <summary>
    ///     The score factor used for work type evaluation.
    /// </summary>
    private static float _workTypeScoreFactor = 2f;

    /// <summary>
    ///     Gets the score factor used for work type evaluation.
    ///     Value is clamped between <see cref="WorkTypeScoreFactorMin" /> and <see cref="WorkTypeScoreFactorMax" />.
    /// </summary>
    public static float WorkTypeScoreFactor
    {
        get => _workTypeScoreFactor;
        private set => _workTypeScoreFactor = value;
    }

    /// <summary>
    ///     Draws the general settings tab UI.
    /// </summary>
    /// <param name="rect">The rectangle area to draw the tab in.</param>
    private static void DoGeneralTab(Rect rect)
    {
        Common.UI.Tabs.DoTab(rect, 0, null, _generalContentHeight, ref _scrollPosition, DoGeneralTabContent, 0, null);
    }

    /// <summary>
    ///     Draws the content of the general settings tab.
    /// </summary>
    /// <param name="rect">The rectangle area to draw the content in.</param>
    private static void DoGeneralTabContent(Rect rect)
    {
        var y = 0f;
        y += DoWorkTypeScoreFactorField(rect);
        if (Event.current.type == EventType.Layout) { _generalContentHeight = y; }
    }

    /// <summary>
    ///     Draws the slider field for the work type score factor setting.
    /// </summary>
    /// <param name="rect">The rectangle area to draw the slider in.</param>
    /// <returns>The height used by the slider field.</returns>
    private static float DoWorkTypeScoreFactorField(Rect rect)
    {
        return Fields.DoLabeledFloatSlider(rect, 0, null, Resources.Strings.Settings.General.WorkTypeScoreFactorLabel,
            Resources.Strings.Settings.General.WorkTypeScoreFactorTooltip, ref _workTypeScoreFactor,
            WorkTypeScoreFactorMin, WorkTypeScoreFactorMax, 0.1f, out _);
    }

    /// <summary>
    ///     Exposes the general settings data for saving and loading.
    /// </summary>
    private static void ExposeGeneralData()
    {
        Scribe_Values.Look(ref _workTypeScoreFactor, nameof(WorkTypeScoreFactor), 2f);
    }

    /// <summary>
    ///     Initializes general settings, clamping values to their allowed ranges.
    /// </summary>
    private static void InitializeGeneralSettings()
    {
        WorkTypeScoreFactor = Mathf.Clamp(WorkTypeScoreFactor, WorkTypeScoreFactorMin, WorkTypeScoreFactorMax);
    }
}