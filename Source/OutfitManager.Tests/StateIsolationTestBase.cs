using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using RimWorld;

namespace LordKuper.OutfitManager.Tests;

/// <summary>
///     Base class for tests that touch OutfitManager or RimWorld static state.
///     Snapshots all mutable statics before each test [SetUp] and restores them after [TearDown],
///     ensuring tests don't bleed state to each other.
///     Mark the fixture [NonParallelizable] when deriving from this class.
/// </summary>
[NonParallelizable]
public abstract class StateIsolationTestBase
{
    /// <summary>
    ///     Snapshot of OutfitManager and RimWorld static state, keyed by field name.
    /// </summary>
    private Dictionary<string, (Type FieldType, object? Value)> _stateSnapshot = new();

    /// <summary>
    ///     Snapshots all mutable static state before the test runs.
    ///     Called by NUnit [SetUp] before each test method.
    /// </summary>
    [SetUp]
    public void SnapshotState()
    {
        _stateSnapshot = new Dictionary<string, (Type, object?)>();

        // ApparelScoring statics
        SnapshotField(typeof(ApparelScoring), "_isInitialized");
        SnapshotField(typeof(ApparelScoring), "ApparelCache");

        // Settings (main) statics
        SnapshotField(typeof(Settings), "_isInitialized");
        SnapshotField(typeof(Settings), "_currentTab");
        SnapshotField(typeof(Settings), "_scrollPosition");
        SnapshotField(typeof(Settings), "Tabs");

        // Settings_WorkTypes statics (part of the partial Settings class)
        SnapshotField(typeof(Settings), "_selectedWorkTypeRule");
        SnapshotField(typeof(Settings), "_workTypeRules");
        SnapshotField(typeof(Settings), "_workTypesContentHeight");
        SnapshotField(typeof(Settings), "_workTypesThingBoxScrollPosition");
        SnapshotField(typeof(Settings), "_workTypesMapThingIconBoxScrollPosition");
        SnapshotField(typeof(Settings), "WorkTypesAvailableItems");

        // Settings_General statics (part of the partial Settings class)
        SnapshotField(typeof(Settings), "_generalContentHeight");
        SnapshotField(typeof(Settings), "_workTypeScoreFactor");
    }

    /// <summary>
    ///     Restores all static state to its pre-test snapshot.
    ///     Called by NUnit [TearDown] after each test method.
    /// </summary>
    [TearDown]
    public void RestoreState()
    {
        foreach (var entry in _stateSnapshot)
            if (!RestoreField(typeof(ApparelScoring), entry.Key, entry.Value.FieldType, entry.Value.Value))
                RestoreField(typeof(Settings), entry.Key, entry.Value.FieldType, entry.Value.Value);
    }

    /// <summary>
    ///     Snapshots a single static field value by name, storing it for later restoration.
    ///     Fails loudly if the field does not exist.
    /// </summary>
    private void SnapshotField(Type type, string fieldName)
    {
        var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (field == null)
            throw new InvalidOperationException(
                $"StateIsolationTestBase.SnapshotField: Field '{fieldName}' not found on type {type.FullName}. " +
                "Check that the field exists and is not being renamed or removed.");

        var value = field.GetValue(null);
        var fieldType = field.FieldType;

        // For collections, store a shallow copy to detect mutations.
        // The branches test heterogeneous conditions (an IList of a generic list type, then a
        // specific ConditionalWeakTable type), so a switch would not read more clearly here.
        var snapshotValue = value;
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (value is IList sourceList && fieldType.IsGenericType)
        {
            var listType = fieldType.GetGenericTypeDefinition();
            if (listType == typeof(List<>))
            {
                var clonedList = Activator.CreateInstance(fieldType);
                if (clonedList is IList clonedIList)
                {
                    foreach (var item in sourceList) clonedIList.Add(item);

                    snapshotValue = clonedIList;
                }
            }
        }
        else if (value is ConditionalWeakTable<Apparel, ApparelCache>)
        {
            // ConditionalWeakTable cannot be cloned; we reset it to empty on restore
            snapshotValue = "ConditionalWeakTable";
        }

        _stateSnapshot[fieldName] = (fieldType, snapshotValue);
    }

    /// <summary>
    ///     Restores a static field to its snapshotted value.
    ///     Returns true if the field was restored, false if it doesn't exist on the target type.
    /// </summary>
    private bool RestoreField(Type type, string fieldName, Type fieldType, object? snapshotValue)
    {
        var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (field == null) return false;

        if (snapshotValue is "ConditionalWeakTable")
        {
            // ConditionalWeakTable cannot be restored; create a fresh one
            var cwt = Activator.CreateInstance(fieldType)!;
            field.SetValue(null, cwt);
        }
        else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
        {
            // For lists, clear and re-populate
            var currentValue = field.GetValue(null);
            if (currentValue is IList currentList && snapshotValue is IList snapshotList)
            {
                currentList.Clear();
                foreach (var item in snapshotList) currentList.Add(item);
            }
        }
        else
        {
            field.SetValue(null, snapshotValue);
        }

        return true;
    }
}