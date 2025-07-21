using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LordKuper.Common;
using UnityEngine;
using Verse;

namespace LordKuper.OutfitManager
{
    /// <summary>
    ///     Mod settings for the Outfit Manager.
    /// </summary>
    [UsedImplicitly]
    public class Settings : ModSettings
    {
        /// <summary>
        ///     The maximum allowed value for the work type score factor.
        /// </summary>
        private const float WorkTypeScoreFactorMax = 5f;

        /// <summary>
        ///     The minimum allowed value for the work type score factor.
        /// </summary>
        private const float WorkTypeScoreFactorMin = 0.1f;

        /// <summary>
        ///     Indicates whether the settings have been initialized.
        /// </summary>
        private static bool _isInitialized;

        /// <summary>
        ///     The list of work type thing rules.
        /// </summary>
        private static List<WorkTypeThingRule> _workTypeRules = new List<WorkTypeThingRule>();

        private static float _workTypeScoreFactor = 2.0f;

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
        ///     Gets the score factor used for work type evaluation.
        ///     Value is clamped between <see cref="WorkTypeScoreFactorMin" /> and <see cref="WorkTypeScoreFactorMax" />.
        /// </summary>
        public static float WorkTypeScoreFactor
        {
            get => _workTypeScoreFactor;
            private set => _workTypeScoreFactor = value;
        }

        /// <summary>
        ///     Exposes the mod settings data for saving and loading.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _workTypeRules, nameof(_workTypeRules), LookMode.Deep);
            Scribe_Values.Look(ref _workTypeScoreFactor, nameof(WorkTypeScoreFactor));
        }

        /// <summary>
        ///     Initializes the settings and ensures default rules are present.
        ///     Clamps the <see cref="WorkTypeScoreFactor" /> value and adds missing default rules.
        /// </summary>
        private static void Initialize()
        {
            if (_isInitialized) { return; }
            _isInitialized = true;
            WorkTypeScoreFactor = Mathf.Clamp(WorkTypeScoreFactor, WorkTypeScoreFactorMin, WorkTypeScoreFactorMax);
            if (_workTypeRules == null) { _workTypeRules = new List<WorkTypeThingRule>(); }
            foreach (var rule in WorkTypeThingRule.DefaultRules)
            {
                if (!_workTypeRules.Any(r =>
                        r.WorkTypeDefName.Equals(rule.WorkTypeDefName, StringComparison.OrdinalIgnoreCase)))
                {
                    _workTypeRules.Add(rule);
                }
            }
#if DEBUG
            Logger.LogMessage("Initializing work type rules...");
            foreach (var rule in _workTypeRules)
            {
                Logger.LogMessage(
                    $"{rule.Label} - {string.Join(", ", rule.StatWeights.Select(sw => $"{sw.StatDefName}={sw.Weight:F2}"))}");
            }
#endif
        }
    }
}