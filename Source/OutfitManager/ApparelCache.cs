using System.Collections.Generic;
using System.Linq;
using LordKuper.Common;
using RimWorld;
using Verse;

namespace LordKuper.OutfitManager;

/// <summary>
///     Caches scores for an <see cref="Apparel" /> item based on work type rules.
/// </summary>
internal class ApparelCache : ThingCache
{
    /// <summary>
    ///     Stores cached scores for each work type definition name.
    /// </summary>
    private readonly Dictionary<string, float> _workTypeScores = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApparelCache" /> class.
    /// </summary>
    /// <param name="apparel">The apparel item to cache.</param>
    public ApparelCache(Apparel apparel) : base(apparel, RimWorldTime.HoursInQuadrum)
    {
    }

    /// <summary>
    ///     Gets the score for a specific work type definition name.
    ///     All valid callers go through <see cref="GetWorkTypesScore" />, which calls
    ///     <see cref="Update" /> first. Update eagerly seeds every work type in
    ///     <see cref="WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder" /> into
    ///     <see cref="_workTypeScores" />, so every key produced by
    ///     <see cref="WorkTypeHelper.GetNormalizedWorkTypeWeights" /> is already present by the
    ///     time this method runs. Reading from the already-populated dictionary is sufficient;
    ///     returning 0f for an absent key is the same result the old miss-path produced.
    /// </summary>
    /// <param name="workTypeDefName">The work type definition name.</param>
    /// <returns>
    ///     The cached score for the specified work type, or 0 if the key is not present.
    /// </returns>
    private float GetWorkTypeScore(string workTypeDefName)
    {
        var score = _workTypeScores.TryGetValue(workTypeDefName, out var cached) ? cached : 0f;
#if DEBUG
        Logger.LogMessage(
            $"Work type score for '{Thing.LabelCapNoCount}' ({Thing.def?.defName}) and work type '{workTypeDefName}' = {score:F2}");
#endif
        return score;
    }

    /// <summary>
    ///     Calculates the weighted sum of scores for multiple work types.
    /// </summary>
    /// <param name="workTypeWeights">A dictionary mapping work type definition names to their weights.</param>
    /// <param name="time">The current game time.</param>
    /// <returns>
    ///     The weighted sum of scores for the specified work types.
    /// </returns>
    public float GetWorkTypesScore(Dictionary<string, float> workTypeWeights, RimWorldTime time)
    {
        Update(time);
        return workTypeWeights.Sum(w => GetWorkTypeScore(w.Key) * w.Value);
    }

    /// <summary>
    ///     Updates the cached scores for all work types.
    /// </summary>
    /// <param name="time">The current game time.</param>
    /// <returns>
    ///     <c>true</c> if the cache was updated; otherwise, <c>false</c>.
    /// </returns>
    public override bool Update(RimWorldTime time)
    {
        if (!base.Update(time)) { return false; }
        _workTypeScores.Clear();
        foreach (var workTypeDef in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
        {
            var score = 0f;
            var workTypeRule =
                Settings.WorkTypeRules.FirstOrDefault(rule => rule.WorkTypeDefName == workTypeDef.defName);
            if (workTypeRule != null) { score += workTypeRule.GetThingScore(Thing); }
            _workTypeScores.Add(workTypeDef.defName, score);
        }
        return true;
    }
}