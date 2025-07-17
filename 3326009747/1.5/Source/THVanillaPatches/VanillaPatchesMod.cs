using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace THVanillaPatches
{
    public class NMFSettings : ModSettings
    {
        private const float ModWindowHeight = 30f;
        private const float GapSize = 10f;
        private Vector2 _scrollPosition;

        public void DoSettingsWindowContents(Rect inRect)
        {
            List<THPatchDef> defs = DefDatabase<THPatchDef>.AllDefsListForReading;
            Rect displayArea = inRect.TopPart(0.75f) with { y = inRect.y + inRect.height * 0.20f };
            float height = ModWindowHeight * (defs.Count + 1);
            Rect sectionRect = new Rect(displayArea.x, displayArea.y, displayArea.width - 50f,
                Math.Max(displayArea.height, height));
            Listing_Standard listingStandard = new Listing_Standard();
            Widgets.BeginScrollView(displayArea, ref _scrollPosition, sectionRect);
            listingStandard.Begin(sectionRect);
            Listing_Standard section = listingStandard.BeginSection(height);
            defs.Do(def =>
            {
                bool enabled = def.PatchGenerator.IsEnabled;
                section.CheckboxLabeled(def.label, ref enabled, def.description);
                def.PatchGenerator.IsEnabled = enabled;
            });
            listingStandard.EndSection(section);
            Write();
            

            listingStandard.End();
            Widgets.EndScrollView();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            DefDatabase<THPatchDef>.AllDefs.Do(def => def.PatchGenerator.SyncWithSavedState());
        }
    }


    [StaticConstructorOnStartup]
    public class SettingsLoader
    {
        static SettingsLoader()
        {
            VanillaPatchesMod.LoadSettingsIfNotYet();
            DefDatabase<THPatchDef>.AllDefsListForReading.Do(def => def.PatchGenerator.SyncPostLoad());
        }
    }

    public class VanillaPatchesMod : Mod
    {
        public static void LoadSettingsIfNotYet()
        {
            if (_hasLoaded) return;
            VanillaPatchesMod mod = LoadedModManager.GetMod<VanillaPatchesMod>();
            mod._settings = mod.LoadSettings();
        }

        private static bool _hasLoaded;

        private NMFSettings LoadSettings()
        {
            _hasLoaded = true;
            return GetSettings<NMFSettings>();
        }

        private NMFSettings _settings;

        public VanillaPatchesMod(ModContentPack content) : base(content) { }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            _settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }


    }
}