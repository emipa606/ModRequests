using System.Collections.Generic;
using Verse;
using UnityEngine;
using System;

namespace VVO_Obliterator
{
    public class ObliteratorSettings : ModSettings
    {
        /// <summary>
        /// The three settings our mod has.
        /// </summary>
        public float destroyBodyPartChance;
        public bool enableAlert;

        /// <summary>
        /// The part that writes our settings to file. Note that saving is by ref.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref destroyBodyPartChance, "destroyBodyPartChance");
            Scribe_Values.Look(ref enableAlert, "enableAlert");
            base.ExposeData();
        }
    }

    public class ObliteratorMod : Mod
    {
        /// <summary>
        /// A reference to our settings.
        /// </summary>
        ObliteratorSettings settings;

        /// <summary>
        /// A mandatory constructor which resolves the reference to our settings.
        /// </summary>
        /// <param name="content"></param>
        public ObliteratorMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<ObliteratorSettings>();
        }

        /// <summary>
        /// The (optional) GUI part to set your settings.
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Show alert message", ref settings.enableAlert, "Show an alert message indicating which body port was obliterated.");
            listingStandard.Label("The chance of destroying a body part on hit: " + Math.Round(settings.destroyBodyPartChance, 0) + "%");
            settings.destroyBodyPartChance = listingStandard.Slider(settings.destroyBodyPartChance, 0f, 100f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns>The (translated) mod name.</returns>
        public override string SettingsCategory()
        {
            return "Obliterator";
        }
    }
}