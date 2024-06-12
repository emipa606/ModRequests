// Nightvision NightVision SettingsCache.cs
// 
// 03 08 2018
// 
// 16 10 2018

using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace NightVision
{
    public class SettingsCache
    {
        [CanBeNull]
        private List<HediffDef> _allHediffsCache;

        public bool CacheInited;

        [CanBeNull]
        private List<ThingDef> _headgearCache;

        public float? MaxCache;

        public float? MinCache;
        public float? NVFullCache;
        public float? NVZeroCache;
        public float? PSFullCache;
        public float? PSZeroCache;

        [NotNull]
        public List<ThingDef> GetAllHeadgear
        {
            get
            {
                if (_headgearCache == null || _headgearCache.Count == 0)
                {
                    _headgearCache = new List<ThingDef>(Settings.Store.AllEyeCoveringHeadgearDefs);

                    foreach (ThingDef appareldef in Settings.Store.NVApparel.Keys)
                    {
                        int appindex = _headgearCache.IndexOf(appareldef);

                        if (appindex > 0)
                        {
                            _headgearCache.RemoveAt(appindex);
                            _headgearCache.Insert(0, appareldef);
                        }
                    }
                }

                return _headgearCache;
            }
        }

        [NotNull]
        public List<HediffDef> GetAllHediffs
        {
            get
            {
                if (_allHediffsCache == null || _allHediffsCache.Count == 0)
                {
                    _allHediffsCache = new List<HediffDef>(Settings.Store.AllSightAffectingHediffs);

                    foreach (HediffDef hediffdef in Settings.Store.HediffLightMods.Keys)
                    {
                        int appindex = _allHediffsCache.IndexOf(hediffdef);

                        if (appindex > 0)
                        {
                            _allHediffsCache.RemoveAt(appindex);
                            _allHediffsCache.Insert(0, hediffdef);
                        }
                    }
                }

                return _allHediffsCache;
            }
        }

        public void Init()
        {
            if (CacheInited)
            {
                return;
            }

            MinCache = (float)Math.Round(Settings.Store.MultiplierCaps.min * 100);
            MaxCache = (float)Math.Round(Settings.Store.MultiplierCaps.max * 100);
            NVZeroCache = SettingsHelpers.ModToMultiPercent(LightModifiersBase.NVLightModifiers[0], true);

            NVFullCache =
                SettingsHelpers.ModToMultiPercent(LightModifiersBase.NVLightModifiers[1], false);

            PSZeroCache = SettingsHelpers.ModToMultiPercent(LightModifiersBase.PSLightModifiers[0], true);

            PSFullCache =
                SettingsHelpers.ModToMultiPercent(LightModifiersBase.PSLightModifiers[1], false);

            CacheInited = true;
        }

        /// <summary>
        ///     Sets new settings
        ///     Clears all cached stuff
        ///     Runs when opening the settings menu and when closing it
        /// </summary>
        public void DoPreWriteTasks()
        {
            // this check is required because this method is run on opening the menu
            if (CacheInited)
            {
                FieldClearer.ResetSettingsDependentFields();

                var settingsStore = Settings.Store;

                if (settingsStore.CustomCapsEnabled)
                {
                    if (MinCache.HasValue)
                    {
                        settingsStore.SetMinMultiplierCap(MinCache.Value);
                    }

                    if (MaxCache.HasValue)
                    {
                        settingsStore.SetMaxMultiplierCap(MaxCache.Value);
                    }
                }

                SetLightModifier(LightModifiersBase.NVLightModifiers, NVZeroCache, NVFullCache);
                SetLightModifier(LightModifiersBase.PSLightModifiers, PSZeroCache, PSFullCache);

                Classifier.ZeroLightTurningPoints = null;
                Classifier.FullLightTurningPoint = null;

                MinCache = null;
                MaxCache = null;
                NVZeroCache = null;
                NVFullCache = null;
                PSZeroCache = null;
                PSFullCache = null;
            }

            CacheInited = false;
            _allHediffsCache?.Clear();
            _headgearCache?.Clear();

            SettingsHelpers.TipStringHolder.Clear();
            Mod.Settings.ClearDrawVariables();


            if (Current.ProgramState == ProgramState.Playing)
            {
                SetDirtyAllComps();
            }
        }


        /// <summary>
        ///     So that the comps will update with the new settings, sets all the comps dirty
        /// </summary>
        public void SetDirtyAllComps()
        {
            foreach (Pawn pawn in PawnsFinder.AllMaps_Spawned)
            {
                if (pawn == null)
                {
                    continue;
                }

                if (pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    comp.SetDirty();
                }
            }
        }

        public void Reset()
        {
            CacheInited = false;
            DoPreWriteTasks();
            Init();
        }

        private void SetLightModifier(LightModifiersBase modifier, float? zeroVal, float? fullVal)
        {
            if (zeroVal.HasValue)
            {
                modifier.Offsets[0] = SettingsHelpers.MultiPercentToMod(zeroVal.Value, true);
            }

            if (fullVal.HasValue)
            {
                modifier.Offsets[1] = SettingsHelpers.MultiPercentToMod(fullVal.Value, false);
            }
        }

    }
}