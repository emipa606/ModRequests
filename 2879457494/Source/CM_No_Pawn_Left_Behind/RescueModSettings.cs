using RimWorld;
using UnityEngine;
using Verse;

namespace CM_No_Pawn_Left_Behind
{
    public class RescueModSettings : ModSettings
    {
        public bool logPriorities = false;

        public bool rescueChanceOverride = false;
        public float rescueChance = 1.0f;

        public bool searchRadiusOverride = false;
        public float searchRadius = 10.0f;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref logPriorities, "logPriorities", false);

            Scribe_Values.Look(ref rescueChanceOverride, "rescueChanceOverride", false);
            Scribe_Values.Look(ref rescueChance, "rescueChance", 0.5f);

            Scribe_Values.Look(ref searchRadiusOverride, "searchRadiusOverride", false);
            Scribe_Values.Look(ref searchRadius, "searchRadius", 10.0f);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (inRect.width - 34f) / 2f;

            listing_Standard.Begin(inRect);

            listing_Standard.CheckboxLabeled("CM_No_Pawn_Left_Behind_Settings_Log_Priorities_Label".Translate(), ref logPriorities);

            listing_Standard.CheckboxLabeled("CM_No_Pawn_Left_Behind_Settings_Rescue_Chance_Override_Label".Translate(), ref rescueChanceOverride, "CM_No_Pawn_Left_Behind_Settings_Rescue_Chance_Override_Description".Translate());
            listing_Standard.Label("CM_No_Pawn_Left_Behind_Settings_Rescue_Chance_Label".Translate());
            listing_Standard.Label(rescueChance.ToString("P0"));
            rescueChance = listing_Standard.Slider(rescueChance, 0.0f, 2.0f);

            listing_Standard.CheckboxLabeled("CM_No_Pawn_Left_Behind_Settings_Search_Radius_Override_Label".Translate(), ref searchRadiusOverride, "CM_No_Pawn_Left_Behind_Settings_Search_Radius_Override_Description".Translate());
            listing_Standard.Label("CM_No_Pawn_Left_Behind_Settings_Search_Radius_Label".Translate());
            listing_Standard.Label(searchRadius.ToString("F0"));
            searchRadius = listing_Standard.Slider(searchRadius, 1.0f, 100.0f);

            listing_Standard.End();
        }

        public void UpdateSettings()
        {

        }
    }
}
