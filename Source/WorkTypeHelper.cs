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
        var workTypePriorities = new Dictionary<string, int>();
        foreach (var workType in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.Where(wt =>
                     pawn.workSettings.WorkIsActive(wt)))
        {
            var rule = Settings.WorkTypeRules.FirstOrDefault(r =>
                r.WorkTypeDefName.Equals(workType.defName, StringComparison.OrdinalIgnoreCase));
            if (rule == null || !rule.StatWeights.Any()) { continue; }
            workTypePriorities[workType.defName] = pawn.workSettings.GetPriority(workType);
        }
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
        var normalizedWorkTypeWeights = workTypeWeights.ToDictionary(w => w.Key, w => w.Value / weightSum);
#if DEBUG
            Logger.LogMessage(
                $"Normalized work type weights for {pawn.LabelShort}: {string.Join(", ", normalizedWorkTypeWeights.Select(w => $"{w.Key}={w.Value:F2}"))}");
#endif
        return normalizedWorkTypeWeights;
    }
}