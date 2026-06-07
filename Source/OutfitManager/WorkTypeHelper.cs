using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace LordKuper.OutfitManager;

/// <summary>
///     Provides helper methods for calculating work type weights for pawns.
/// </summary>
public static class WorkTypeHelper
{
    /// <summary>
    ///     Calculates normalized weights for each active work type of the specified pawn.
    ///     The weights are based on the pawn's work priorities, normalized so that their sum is 1.
    /// </summary>
    /// <param name="pawn">The pawn whose work type weights are to be calculated.</param>
    /// <returns>
    ///     A dictionary mapping work type def names to their normalized weights.
    /// </returns>
    public static Dictionary<string, float> GetNormalizedWorkTypeWeights(Pawn pawn)
    {
        if (pawn.workSettings == null) { return new Dictionary<string, float>(); }
        var workTypePriorities = new Dictionary<string, int>();
        foreach (var workType in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.Where(wt =>
                     pawn.workSettings.WorkIsActive(wt)))
        {
            var rule = Settings.WorkTypeRules.FirstOrDefault(r =>
                string.Equals(r.WorkTypeDefName, workType.defName, StringComparison.OrdinalIgnoreCase));
            if (rule == null || !rule.StatWeights.Any()) { continue; }
            workTypePriorities[workType.defName] = pawn.workSettings.GetPriority(workType);
        }
        var normalizedWorkTypeWeights = NormalizeWorkTypeWeights(workTypePriorities);
#if DEBUG
            Logger.LogMessage(
                $"Normalized work type weights for {pawn.LabelShort}: {string.Join(", ", normalizedWorkTypeWeights.Select(w => $"{w.Key}={w.Value:F2}"))}");
#endif
        return normalizedWorkTypeWeights;
    }

    /// <summary>
    ///     Converts a raw work-type priority map into normalized weights that sum to 1.
    ///     Priority values follow RimWorld's convention: a lower number means higher priority.
    ///     The formula inverts priorities relative to the observed range so that a higher-priority
    ///     work type receives a greater weight. When all priorities are equal (range is zero),
    ///     weights are distributed uniformly.
    /// </summary>
    /// <param name="workTypePriorities">
    ///     Map of work type def names to their raw priority values. An empty map produces an empty result.
    /// </param>
    /// <returns>
    ///     A dictionary mapping each work type def name to its normalized weight (sum of values is 1).
    /// </returns>
    internal static Dictionary<string, float> NormalizeWorkTypeWeights(
        IReadOnlyDictionary<string, int> workTypePriorities)
    {
        if (workTypePriorities.Count == 0) { return new Dictionary<string, float>(); }
        var wpMin = workTypePriorities.Min(wp => wp.Value);
        var wpMax = workTypePriorities.Max(wp => wp.Value);
        int wpRange;
        if (wpMin == wpMax) { wpRange = 0; }
        else
        {
            wpMax++;
            wpRange = wpMax - wpMin;
        }
        var workTypeWeights = workTypePriorities.ToDictionary(wp => wp.Key,
            wp => wpRange == 0 ? 1f : 1f - (float)(wp.Value - wpMin) / wpRange);
        var weightSum = workTypeWeights.Sum(w => w.Value);
        return workTypeWeights.ToDictionary(w => w.Key, w => w.Value / weightSum);
    }
}