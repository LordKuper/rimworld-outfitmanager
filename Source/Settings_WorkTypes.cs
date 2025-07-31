using System;
using System.Collections.Generic;
using System.Linq;
using LordKuper.Common;
using LordKuper.Common.UI.Widgets;
using UnityEngine;
using Verse;

namespace LordKuper.OutfitManager
{
    public partial class Settings
    {
        /// <summary>
        ///     The currently selected work type rule.
        /// </summary>
        private static WorkTypeThingRule _selectedWorkTypeRule;

        /// <summary>
        ///     The list of work type thing rules.
        /// </summary>
        private static List<WorkTypeThingRule> _workTypeRules = new List<WorkTypeThingRule>();

        /// <summary>
        ///     The height of the work types content area.
        /// </summary>
        private static float _workTypesContentHeight;

        /// <summary>
        ///     The scroll position for the work types thing box.
        /// </summary>
        private static Vector2 _workTypesThingBoxScrollPosition;

        /// <summary>
        ///     The list of available items for the selected work type.
        /// </summary>
        private static readonly List<ThingDef> WorkTypesAvailableItems = new List<ThingDef>();

        /// <summary>
        ///     Gets or sets the currently selected work type rule.
        ///     Updates the available items when the selection changes.
        /// </summary>
        private static WorkTypeThingRule SelectedWorkTypeRule
        {
            get => _selectedWorkTypeRule;
            set
            {
                _selectedWorkTypeRule = value;
                UpdateWorkTypesAvailableItems();
            }
        }

        /// <summary>
        ///     Gets the read-only list of work type thing rules.
        ///     Ensures initialization before returning the rules.
        /// </summary>
        public static IReadOnlyList<WorkTypeThingRule> WorkTypeRules
        {
            get
            {
                Initialize();
                return _workTypeRules;
            }
        }

        /// <summary>
        ///     Draws the work types tab UI.
        /// </summary>
        /// <param name="rect">The rectangle area to draw the tab in.</param>
        private static void DoWorkTypesTab(Rect rect)
        {
            WorkTypeThingRuleWidget.DoWidgetTab(rect, ref _workTypesContentHeight, ref _scrollPosition, 2,
                WorkTypeRules, SelectedWorkTypeRule, rule => { SelectedWorkTypeRule = rule; },
                UpdateWorkTypesAvailableItems, ref _workTypesThingBoxScrollPosition, WorkTypesAvailableItems);
        }

        /// <summary>
        ///     Exposes the work type rules data for saving/loading.
        /// </summary>
        private static void ExposeWorkTypesData()
        {
            Scribe_Collections.Look(ref _workTypeRules, nameof(WorkTypeRules), LookMode.Deep);
        }

        /// <summary>
        ///     Initializes the work types settings, ensuring default rules are present.
        /// </summary>
        private static void InitializeWorkTypesSettings()
        {
            if (_workTypeRules == null) { _workTypeRules = new List<WorkTypeThingRule>(); }
            foreach (var statWeight in _workTypeRules.SelectMany(rule => rule.StatWeights))
            {
                statWeight.Protected = false;
            }
            foreach (var defaultRule in WorkTypeThingRule.DefaultRules)
            {
                var rule = _workTypeRules.FirstOrDefault(r =>
                    r.WorkTypeDefName.Equals(defaultRule.WorkTypeDefName, StringComparison.OrdinalIgnoreCase));
                if (rule == null) { _workTypeRules.Add(defaultRule); }
                else
                {
                    foreach (var defaultStatWeight in defaultRule.StatWeights)
                    {
                        var statWeight = rule.StatWeights.FirstOrDefault(sw =>
                            sw.StatDefName.Equals(defaultStatWeight.StatDefName, StringComparison.OrdinalIgnoreCase));
                        if (statWeight == null)
                        {
                            rule.SetStatWeight(defaultStatWeight.StatDef, defaultStatWeight.Weight);
                        }
                        else { statWeight.Protected = defaultStatWeight.Protected; }
                    }
                }
            }
#if DEBUG
            Logger.LogMessage("Initializing work type rules...");
            foreach (var rule in _workTypeRules)
            {
                Logger.LogMessage(
                    $"{rule.Label} - {string.Join(", ", rule.StatWeights.Select(sw => $"{(sw.Protected ? "*" : string.Empty)}{sw.StatDefName}={sw.Weight:F2}"))}");
            }
#endif
        }

        /// <summary>
        ///     Updates the list of available items for the currently selected work type rule.
        /// </summary>
        private static void UpdateWorkTypesAvailableItems()
        {
            WorkTypesAvailableItems.Clear();
            if (SelectedWorkTypeRule == null) { return; }
            WorkTypesAvailableItems.AddRange(SelectedWorkTypeRule.GetGloballyAvailableItems());
        }
    }
}