using System.Collections.Generic;
using System.Linq;
using LordKuper.Common;
using RimWorld;
using Verse;

namespace LordKuper.OutfitManager;

/// <summary>
///     Provides methods for scoring apparel items based on work type rules and pawn preferences.
/// </summary>
public static class ApparelScoring
{
    /// <summary>
    ///     Indicates whether the stat ranges have been initialized.
    /// </summary>
    private static bool _isInitialized;

    /// <summary>
    ///     Caches <see cref="ApparelCache" /> instances for each <see cref="Apparel" /> item.
    /// </summary>
    private static readonly Dictionary<Apparel, ApparelCache> ApparelCache = new();

    /// <summary>
    ///     Gets the <see cref="ApparelCache" /> for the specified <see cref="Apparel" /> item,
    ///     creating and caching it if necessary.
    /// </summary>
    /// <param name="apparel">The apparel item to cache.</param>
    /// <returns>
    ///     The <see cref="ApparelCache" /> for the specified apparel.
    /// </returns>
    private static ApparelCache GetApparelCache(Apparel apparel)
    {
        if (!ApparelCache.TryGetValue(apparel, out var cache))
        {
            cache = new ApparelCache(apparel);
            ApparelCache[apparel] = cache;
        }
        return cache;
    }

    /// <summary>
    ///     Calculates the work score for a given apparel item using provided work type weights and game time.
    /// </summary>
    /// <param name="apparel">
    ///     The apparel item to score.
    /// </param>
    /// <param name="workTypeWeights">
    ///     A dictionary mapping work type definition names to their weights.
    /// </param>
    /// <param name="time">
    ///     The current game time.
    /// </param>
    /// <returns>
    ///     The calculated work score for the apparel.
    /// </returns>
    private static float GetApparelWorkScore(Apparel apparel, Dictionary<string, float> workTypeWeights,
        RimWorldTime time)
    {
        if (workTypeWeights == null || workTypeWeights.Count == 0) { return 0f; }
        return GetApparelCache(apparel).GetWorkTypesScore(workTypeWeights, time);
    }

    /// <summary>
    ///     Calculates the work score of a specific apparel item for a given pawn.
    /// </summary>
    /// <param name="pawn">The pawn for whom the score is calculated.</param>
    /// <param name="apparel">The apparel item to score.</param>
    /// <returns>
    ///     The calculated work score for the apparel for the specified pawn.
    /// </returns>
    public static float GetPawnApparelWorkScore(Pawn pawn, Apparel apparel)
    {
#if DEBUG
            Logger.LogMessage(
                $"Calculating work score of '{apparel.LabelCapNoCount}' ({apparel.def.defName}) for '{pawn.Name}'");
#endif
        Initialize();
        var workScore = GetApparelWorkScore(apparel, WorkTypeHelper.GetNormalizedWorkTypeWeights(pawn),
            RimWorldTime.GetHomeTime());
        var totalScore = workScore * Settings.WorkTypeScoreFactor;
#if DEBUG
            Logger.LogMessage(
                $"Work score of '{apparel.LabelCapNoCount}' ({apparel.def.defName}) for '{pawn.Name}' = {workScore:F2} * {Settings.WorkTypeScoreFactor:F2} = {totalScore:F2}");
#endif
        return totalScore;
    }

    /// <summary>
    ///     Initializes stat ranges for all apparel definitions if not already initialized.
    /// </summary>
    private static void Initialize()
    {
        if (_isInitialized) { return; }
        _isInitialized = true;
        InitializeStatRanges();
    }

    /// <summary>
    ///     Initializes stat ranges for all apparel definitions by evaluating work type rules.
    /// </summary>
    private static void InitializeStatRanges()
    {
        foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(def => def.IsApparel))
        {
            var thing = def.MadeFromStuff
                ? ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def))
                : ThingMaker.MakeThing(def);
            foreach (var rule in Settings.WorkTypeRules) { rule.GetThingScore(thing); }
        }
    }
}