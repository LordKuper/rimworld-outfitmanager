using System.Collections.Generic;
using System.Linq;
using Verse;

namespace LordKuper.OutfitManager.Tests;

/// <summary>
///     Unit tests for <see cref="WorkTypeHelper" /> work-type weight normalization logic.
///     Tests that call <see cref="WorkTypeHelper.NormalizeWorkTypeWeights" /> directly exercise the pure
///     math (no Pawn, no DefDatabase) and run as real [Test]s.
///     Tests that exercise the public <see cref="WorkTypeHelper.GetNormalizedWorkTypeWeights(Pawn)" />
///     overload require a live RimWorld Pawn + workSettings context and remain [Ignore] until
///     in-game verification is available (TODO(sprint-001): MS-3 in-game verification).
/// </summary>
[TestFixture]
[NonParallelizable]
public class WorkTypeHelperTests : StateIsolationTestBase
{
    // -------------------------------------------------------------------------
    // NormalizeWorkTypeWeights — pure-math tests (no Pawn / no DefDatabase)
    // -------------------------------------------------------------------------

    /// <summary>
    ///     When the input priority map is empty, the result is an empty dictionary.
    ///     Exercises the early-return guard for zero-entry input.
    /// </summary>
    [Test]
    public void NormalizeWorkTypeWeights_WithEmptyInput_ReturnsEmpty()
    {
        var result = WorkTypeHelper.NormalizeWorkTypeWeights(new Dictionary<string, int>());
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     A single entry always normalizes to a weight of exactly 1.0, because it is the only
    ///     contributor to the sum.
    /// </summary>
    [Test]
    public void NormalizeWorkTypeWeights_WithSingleEntry_ReturnsWeightOfOne()
    {
        var priorities = new Dictionary<string, int> { ["Mining"] = 2 };
        var result = WorkTypeHelper.NormalizeWorkTypeWeights(priorities);
        result.Should().HaveCount(1);
        result["Mining"].Should().BeApproximately(1.0f, 1e-5f);
    }

    /// <summary>
    ///     When all entries share the same priority value (wpMin == wpMax, wpRange == 0),
    ///     every entry gets a uniform weight of 1/count so the weights still sum to 1.
    ///     Two entries at the same priority should each receive 0.5.
    /// </summary>
    [Test]
    public void NormalizeWorkTypeWeights_WithUniformPriorities_DistributesWeightsUniformly()
    {
        var priorities = new Dictionary<string, int>
        {
            ["Cooking"] = 3,
            ["Cleaning"] = 3
        };
        var result = WorkTypeHelper.NormalizeWorkTypeWeights(priorities);
        result.Should().HaveCount(2);
        result["Cooking"].Should().BeApproximately(0.5f, 1e-5f);
        result["Cleaning"].Should().BeApproximately(0.5f, 1e-5f);
        result.Values.Sum().Should().BeApproximately(1.0f, 1e-5f);
    }

    /// <summary>
    ///     With varying priorities {A:1, B:2, C:3} the formula produces:
    ///     wpMin=1, wpMax → 4 (incremented), wpRange=3.
    ///     weight(A)=1-(1-1)/3=1.0, weight(B)=1-(2-1)/3≈0.6667, weight(C)=1-(3-1)/3≈0.3333
    ///     sum≈2.0 → normalized: A≈0.5, B≈0.3333, C≈0.1667.
    ///     Confirms weights sum to 1 and the priority ordering is preserved (A > B > C weight).
    /// </summary>
    [Test]
    public void NormalizeWorkTypeWeights_WithVaryingPriorities_ProducesCorrectNormalizedValues()
    {
        var priorities = new Dictionary<string, int>
        {
            ["A"] = 1,
            ["B"] = 2,
            ["C"] = 3
        };
        var result = WorkTypeHelper.NormalizeWorkTypeWeights(priorities);
        result.Should().HaveCount(3);
        result["A"].Should().BeApproximately(0.5f, 1e-5f);
        result["B"].Should().BeApproximately(1f / 3f, 1e-5f);
        result["C"].Should().BeApproximately(1f / 6f, 1e-5f);
        result.Values.Sum().Should().BeApproximately(1.0f, 1e-5f);
    }

    /// <summary>
    ///     A lower priority number (higher game priority) must always yield a strictly greater
    ///     normalized weight than a higher priority number, confirming the inversion is correct.
    /// </summary>
    [Test]
    public void NormalizeWorkTypeWeights_WithVaryingPriorities_HigherGamePriorityGetsGreaterWeight()
    {
        var priorities = new Dictionary<string, int>
        {
            ["HighPriority"] = 1,
            ["LowPriority"] = 4
        };
        var result = WorkTypeHelper.NormalizeWorkTypeWeights(priorities);
        result["HighPriority"].Should().BeGreaterThan(result["LowPriority"]);
    }

    // -------------------------------------------------------------------------
    // GetNormalizedWorkTypeWeights(Pawn) — deferred: requires live game context
    // -------------------------------------------------------------------------

    /// <summary>
    ///     When a pawn has null workSettings, GetNormalizedWorkTypeWeights returns an empty dictionary.
    ///     IGNORED: requires a live RimWorld Pawn with workSettings context.
    ///     TODO(sprint-001): enable once in-game verification infrastructure is available (MS-3).
    /// </summary>
    [Test]
    [Ignore("Requires live RimWorld Pawn + workSettings context; deferred to in-game verification (MS-3)")]
    public void GetNormalizedWorkTypeWeights_WithNullWorkSettings_ReturnsEmpty()
    {
        // Expected: empty result when pawn.workSettings is null.
        // Coverage: null workSettings guard in GetNormalizedWorkTypeWeights.
    }

    /// <summary>
    ///     When a pawn has no active work types with matching rules, GetNormalizedWorkTypeWeights returns empty.
    ///     IGNORED: requires live RimWorld context.
    ///     TODO(sprint-001): enable once in-game verification infrastructure is available (MS-3).
    /// </summary>
    [Test]
    [Ignore("Requires live RimWorld Pawn context; deferred to in-game verification (MS-3)")]
    public void GetNormalizedWorkTypeWeights_WithNoActiveWorkTypes_ReturnsEmpty()
    {
        // Expected: empty result when no work types pass the rule+StatWeights filter.
        // Coverage: NormalizeWorkTypeWeights called with an empty map → early return.
    }

    /// <summary>
    ///     When a work type has no matching rule or empty stat weights, it is excluded from the result.
    ///     IGNORED: requires live RimWorld context.
    ///     TODO(sprint-001): enable once in-game verification infrastructure is available (MS-3).
    /// </summary>
    [Test]
    [Ignore("Requires live RimWorld Pawn context; deferred to in-game verification (MS-3)")]
    public void GetNormalizedWorkTypeWeights_WithNoRulesForWorkType_ExcludesWorkType()
    {
        // Expected: work types with null rules or empty StatWeights are filtered out before normalization.
        // Coverage: rule.StatWeights.Any() guard in GetNormalizedWorkTypeWeights.
    }
}
