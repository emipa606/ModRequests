// Nightvision NightVision Settings.cs
// 
// 03 08 2018
// 
// 16 10 2018

using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace NightVision
{
    public class Settings : ModSettings
    {
        private bool _isWindowSetup;
        public bool CEDetected = false;

        public static Settings Instance;

        public static Storage Store => Instance._store;
        public static Storage_Combat CombatStore => Instance._combatStore;
        public static SettingsCache Cache => Instance._cache;

        private Storage _store;
        private Storage_Combat _combatStore;
        private SettingsCache _cache;

        // tabs
        private Tab _tab;
        private readonly List<TabRecord> _tabsList = new List<TabRecord>();
        private GeneralTab _generalTab;
        private ApparelTab _apparelTab;
        private HediffTab _hediffTab;
        private RaceTab _raceTab;
        private DebugTab _debugTab;
        private CombatTab _combatTab;

        // draw rects
        private Rect lastRect;
        private Rect menuRect;
        private Rect tabRect;




        [UsedImplicitly]
        public Settings()
        {
            Instance = this;
            _store = new Storage();
            _combatStore = new Storage_Combat();
            _cache = new SettingsCache();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Cache.DoPreWriteTasks();
            Store.ExposeSettings();
            CombatStore.LoadSaveCommit();

        }

        public void Initialise()
        {
            var initialise = new Initialiser();
            initialise.Startup();

            if (Store.NullRefWhenLoading)
            {
                Write();
            }
        }


        private void InitialiseWindow(Rect inRect)
        {
            _tabsList.Clear();

            if (_generalTab == null)
            {
                _generalTab = new GeneralTab();
            }
            _tabsList.Add(
                new TabRecord(
                    "NVGeneralTab".Translate(),
                    delegate
                    {
                        _tab = Tab.General;
                    },
                    () => _tab == Tab.General
                )
            );


            if (_combatTab == null)
            {
                _combatTab = new CombatTab();

            }
            _tabsList.Add(
                new TabRecord(
                    "NVCombat".Translate(),
                    delegate
                    {
                        _tab = Tab.Combat;
                    },
                    () => _tab == Tab.Combat
                )
            );

            if (_raceTab == null)
            {
                _raceTab = new RaceTab();
            }
            _tabsList.Add(
                new TabRecord(
                    "NVRaces".Translate(),
                    delegate
                    {
                        _tab = Tab.Races;
                    },
                    () => _tab == Tab.Races
                )
            );


            if (_apparelTab == null)
            {
                _apparelTab = new ApparelTab();
            }
            _tabsList.Add(
                new TabRecord(
                    "NVApparel".Translate(),
                    delegate
                    {
                        _tab = Tab.Apparel;
                    },
                    () => _tab == Tab.Apparel
                )
            );

            if (_hediffTab == null)
            {
                _hediffTab = new HediffTab();
            }
            _tabsList.Add(
                new TabRecord(
                    "NVHediffs".Translate(),
                    delegate
                    {
                        _tab = Tab.Bionics;
                    },
                    () => _tab == Tab.Bionics
                )
            );

            if (Prefs.DevMode)
            {
                if (_debugTab == null)
                {
                    _debugTab = new DebugTab();
                }
                _tabsList.Add(
                    new TabRecord(
                        "NVDebugTab".Translate(),
                        delegate
                        {
                            _tab = Tab.Debug;
                        },
                        () => _tab == Tab.Debug
                    )
                );
            }

            Cache.Init();

            inRect.yMin += 32f;
            menuRect = inRect;
            tabRect = inRect.ContractedBy(17f);


        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            if (!_isWindowSetup || lastRect != inRect)
            {
                lastRect = inRect;
                InitialiseWindow(inRect);
                _isWindowSetup = true;
            }

            Widgets.DrawMenuSection(menuRect);
            TabDrawer.DrawTabs(menuRect, _tabsList, 1);

            GUI.BeginGroup(tabRect);
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            switch (_tab)
            {
                default:
                    _generalTab.DrawTab(tabRect);

                    break;
                case Tab.Combat:
                    _combatTab.DrawTab(tabRect);
                    break;
                case Tab.Races:
                    _raceTab.DrawTab(tabRect);

                    break;
                case Tab.Apparel:
                    _apparelTab.DrawTab(tabRect);

                    break;
                case Tab.Bionics:
                    _hediffTab.DrawTab(tabRect);

                    break;
                case Tab.Debug:
                    _debugTab.DrawTab(tabRect);

                    break;
            }

            Text.Font = font;
            Text.Anchor = anchor;
            GUI.EndGroup();
        }

        public void ClearDrawVariables()
        {
            /*_debugTab.Clear();
            _apparelTab.Clear();
            _raceTab.Clear();
            _generalTab.Clear();
            _combatTab.Clear();
            _hediffTab.Clear();
            initialised = false;*/
            _debugTab = null;
            _apparelTab = null;
            _raceTab = null;
            _generalTab = null;
            _combatTab = null;
            _hediffTab = null;
            _isWindowSetup = false;
        }
    }
}