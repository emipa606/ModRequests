using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace zed_0xff.LoftBed
{
    public class LoftBedSettings : ModSettings
    {
        public float f1 = -28f;
        public float f2 = 0.5f;
        public float maxFillPercent = 0.5f;
        public bool altPerspectiveMode = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref f1, "f1", -28f);
            Scribe_Values.Look(ref f2, "f2", 0.5f);
            Scribe_Values.Look(ref maxFillPercent, "maxFillPercent", 0.5f);
            Scribe_Values.Look(ref altPerspectiveMode, "altPerspectiveMode", false);
            base.ExposeData();
        }
    }

    public class LoftBedMod : Mod
    {
        public static LoftBedSettings Settings { get; private set; }

        public LoftBedMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<LoftBedSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard l = new Listing_Standard();
            l.Begin(inRect);

            l.Label("Loft bed label shift: " + Math.Round(Settings.f1,1));
            Settings.f1 = l.Slider(Settings.f1, -100f, 100f);

            l.Label("Pawn perspective shift when laying on a loft bed: " + Math.Round(Settings.f2,2));
            Settings.f2 = l.Slider(Settings.f2, -10f, 10f);

            l.CheckboxLabeled("Alternate perspective mode (may fix weird pawn offset while in bed)", ref Settings.altPerspectiveMode);

            l.Label("Max fill percent of ground-level building that allows placing a loft bed over it: " + Math.Round(Settings.maxFillPercent,2));
            Settings.maxFillPercent = l.Slider(Settings.maxFillPercent, 0, 1.0f);

            l.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "LoftBed".Translate();
        }
    }
}
