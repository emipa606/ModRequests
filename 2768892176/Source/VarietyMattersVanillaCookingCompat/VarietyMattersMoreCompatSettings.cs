using UnityEngine;
using Verse;

namespace VarietyMattersMoreCompat
{
    public class VarietyMattersMoreCompatSettings : ModSettings
    {
        public bool fixDesserts = true;
        public bool betterGourmet = true;
        public bool betterArchotech = true;

        public void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.ColumnWidth = 270f;

            listing.Label("VVMC_VanillaCookingLabel".Translate());
            listing.CheckboxLabeled("VVMC_FixDesserts".Translate(), ref fixDesserts);
            listing.CheckboxLabeled("VVMC_BetterGourmet".Translate(), ref betterGourmet);

            listing.GapLine();

            listing.Label("VVMC_ArchotechGarbageLabel".Translate());
            listing.CheckboxLabeled("VVMC_BetterArchotech".Translate(), ref betterArchotech);

            listing.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref fixDesserts, nameof(fixDesserts), true);
            Scribe_Values.Look(ref betterGourmet, nameof(betterGourmet), true);
            Scribe_Values.Look(ref betterArchotech, nameof(betterArchotech), true);
        }
    }
}
