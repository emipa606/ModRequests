using UnityEngine;
using Verse;

namespace RedAlert
{
    public class RedAlertSettings : ModSettings
    {
        public bool playRedAlertSound = true;
        public bool playLooped;
        public bool autoRestrictUndraftedPawnsToHomeArea;
        public bool flashingLights;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref playRedAlertSound, "playRedAlertSound", true);
            Scribe_Values.Look(ref playLooped, "playLooped");
            Scribe_Values.Look(ref flashingLights, "flashingLights");
            Scribe_Values.Look(ref autoRestrictUndraftedPawnsToHomeArea, "autoRestrictUndraftedPawnsToHomeArea");
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled("Play red alert sound", ref playRedAlertSound);
            listingStandard.CheckboxLabeled("Play red alert sound in loop", ref playLooped);
            listingStandard.CheckboxLabeled("Auto restrict undrafted pawns to home area during threats", ref autoRestrictUndraftedPawnsToHomeArea);
            listingStandard.CheckboxLabeled("Flash lights every 1 second", ref flashingLights);
            listingStandard.End();
        }
    }
}
