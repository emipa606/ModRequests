using UnityEngine;
using Verse;
using System;

namespace Mashed_BlackPlague
{
    class Mashed_BlackPlague_Mod : Mod
    {
        Mashed_BlackPlague_ModSettings settings;

        public Mashed_BlackPlague_Mod(ModContentPack contentPack) : base(contentPack)
        {
            this.settings = GetSettings<Mashed_BlackPlague_ModSettings>();
        }

        public override string SettingsCategory() => "BlackPlague_ModName".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.Gap();

            listing_Standard.CheckboxLabeled("BlackPlague_Enable_VesselMentalBreak".Translate(), ref settings.BlackPlague_Enable_VesselMentalBreak);
            listing_Standard.Gap();

            listing_Standard.Label("BlackPlague_Chance_InfectedTouch".Translate() + " (" + settings.BlackPlague_Chance_InfectedTouch * 100 + "%)");
            settings.BlackPlague_Chance_InfectedTouch = (float)Math.Round(listing_Standard.Slider(settings.BlackPlague_Chance_InfectedTouch, 0f, 1f) * 20) / 20;
            listing_Standard.Gap();

            listing_Standard.GapLine();
            listing_Standard.Gap();

            listing_Standard.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}
