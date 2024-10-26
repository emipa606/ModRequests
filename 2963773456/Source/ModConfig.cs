using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace zed_0xff.GeneCourier
{
    public class GeneCourierSettings : ModSettings
    {
        public float probSpaceRefugee = 0.01f;
        public float probAncient = 0.02f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref probSpaceRefugee, "probSpaceRefugee", 0.01f);
            Scribe_Values.Look(ref probAncient, "probAncient", 0.02f);
            base.ExposeData();
        }
    }

    public class ModConfig : Mod
    {
        public static GeneCourierSettings Settings { get; private set; }

        public ModConfig(ModContentPack content) : base(content)
        {
            Settings = GetSettings<GeneCourierSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard l = new Listing_Standard();
            l.Begin(inRect);

            l.Label("Probability of a Space Refugee to be a Gene Courier: " + Math.Round(Settings.probSpaceRefugee,2));
            Settings.probSpaceRefugee = l.Slider(Settings.probSpaceRefugee, 0f, 1.0f);

            l.Label("Probability of an Ancient to be a Gene Courier: " + Math.Round(Settings.probAncient,2));
            Settings.probAncient = l.Slider(Settings.probAncient, 0f, 1.0f);

            l.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "GeneCourier".Translate();
        }
    }
}
