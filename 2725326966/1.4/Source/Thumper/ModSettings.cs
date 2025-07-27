using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ThumperModSettings
{
    public class ThumperSettings : ModSettings
    {
        public bool enableSound = true;
        public float ThumperRadius = 18f;
        public float ThumperXLRadius = 30f;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableSound, "EnableSound");
            Scribe_Values.Look(ref ThumperRadius, "ThumperRadius", 18f);
            Scribe_Values.Look(ref ThumperXLRadius, "ThumperXLRadius", 30f);
            base.ExposeData();
        }
    }

    public class ThumperMod : Verse.Mod
    {
        public static ThumperSettings settings;
        public ThumperMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<ThumperSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Enable Sound", ref settings.enableSound, "Enables/Disables the thump sound");
            listingStandard.Gap();
            listingStandard.Label($"Regular Thumper radius effect: {settings.ThumperRadius:0} Tiles");
            settings.ThumperRadius = listingStandard.Slider(settings.ThumperRadius, 1f,54f);
            listingStandard.Gap();
            listingStandard.Label($"XL Thumper radius effect: {settings.ThumperXLRadius:0} Tiles");
            settings.ThumperXLRadius = listingStandard.Slider(settings.ThumperXLRadius, 1f, 80f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Thumper";
        }
    }


}