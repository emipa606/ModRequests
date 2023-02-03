using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CM_Who_Next
{
    public class WhoNextModSettings : ModSettings
    {
        public bool allowSwitchingBetweenCorpsesAndLiving = true;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref allowSwitchingBetweenCorpsesAndLiving, "allowSwitchingBetweenCorpsesAndLiving", true);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);

            listing_Standard.CheckboxLabeled("CM_Who_Next_Setting_AllowSwitchingBetweenCorpsesAndLiving_Label".Translate(), ref allowSwitchingBetweenCorpsesAndLiving, "CM_Who_Next_Setting_AllowSwitchingBetweenCorpsesAndLiving_Description".Translate());

            listing_Standard.End();
        }

        public void UpdateSettings()
        {
        }
    }
}
