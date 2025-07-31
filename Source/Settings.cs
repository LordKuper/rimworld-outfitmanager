using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Verse;
using Strings = LordKuper.OutfitManager.Resources.Strings;

namespace LordKuper.OutfitManager
{
    /// <summary>
    ///     Mod settings for the Outfit Manager.
    /// </summary>
    [UsedImplicitly]
    public partial class Settings : ModSettings
    {
        /// <summary>
        ///     The currently selected tab in the settings window.
        /// </summary>
        private static SettingsTabs _currentTab;

        /// <summary>
        ///     Indicates whether the settings have been initialized.
        /// </summary>
        private static bool _isInitialized;

        /// <summary>
        ///     The scroll position for the settings window.
        /// </summary>
        private static Vector2 _scrollPosition;

        /// <summary>
        ///     The list of tab records for the settings window.
        /// </summary>
        private static readonly List<TabRecord> Tabs = new List<TabRecord>();

        /// <summary>
        ///     Draws the contents of the specified tab in the settings window.
        /// </summary>
        /// <param name="rect">The rectangle area to draw the tab contents.</param>
        /// <param name="tab">The tab to display.</param>
        private static void DoTab(Rect rect, SettingsTabs tab)
        {
            switch (tab)
            {
                case SettingsTabs.General:
                    DoGeneralTab(rect);
                    break;
                case SettingsTabs.WorkTypes:
                    DoWorkTypesTab(rect);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tab));
            }
        }

        /// <summary>
        ///     Draws the window contents for the mod settings.
        /// </summary>
        /// <param name="rect">The rectangle area to draw the window contents.</param>
        public static void DoWindowContents(Rect rect)
        {
            Initialize();
            var activeTabRect = Common.UI.Tabs.DoTabs(rect, Tabs);
            DoTab(activeTabRect, _currentTab);
        }

        /// <summary>
        ///     Exposes the mod settings data for saving and loading.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            ExposeGeneralData();
            ExposeWorkTypesData();
        }

        /// <summary>
        ///     Initializes the settings.
        /// </summary>
        private static void Initialize()
        {
            if (_isInitialized) { return; }
            _isInitialized = true;
            InitializeTabs();
            InitializeGeneralSettings();
            InitializeWorkTypesSettings();
        }

        /// <summary>
        ///     Initializes the tab records for the settings window.
        /// </summary>
        private static void InitializeTabs()
        {
            Tabs.Add(new TabRecord(Strings.Settings.General.Title, () =>
            {
                _currentTab = SettingsTabs.General;
                _scrollPosition.Set(0, 0);
            }, () => _currentTab == SettingsTabs.General));
            Tabs.Add(new TabRecord(Strings.Settings.WorkTypes.Title, () =>
            {
                _currentTab = SettingsTabs.WorkTypes;
                _scrollPosition.Set(0, 0);
            }, () => _currentTab == SettingsTabs.WorkTypes));
        }
    }
}