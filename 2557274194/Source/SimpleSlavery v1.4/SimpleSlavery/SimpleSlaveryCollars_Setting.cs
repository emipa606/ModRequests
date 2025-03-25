using RimWorld;
using UnityEngine;
using Verse;

namespace SimpleSlaveryCollars
{
    public class SimpleSlaveryCollarsSetting : ModSettings
    {
        public static bool ShacklesDefault = true;
        public static bool SlavestageEnable = true;
        public static bool RebelCycleChangeEnable = true;
        public static bool RemoveWorkspeedDebuffEnable = true;
        public static bool AssignSlaveEnable = true;
        public static bool AssimilationSlaveEnable = true;
        public static float Slavestage1Period = 15f;
        public static float Slavestage2Period = 15f;
        public static float Slavestage3Period = 15f;
        public static float Slavestage4Period = 15f;
        private string Slavestage1PeriodBuffer;
        private string Slavestage2PeriodBuffer;
        private string Slavestage3PeriodBuffer;
        private string Slavestage4PeriodBuffer;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref ShacklesDefault, "ShacklesDefault", true, false);
            Scribe_Values.Look<bool>(ref SlavestageEnable, "SlavestageEnable", true, false);
            Scribe_Values.Look<bool>(ref RebelCycleChangeEnable, "RebelCycleChangeEnable", true, false);
            Scribe_Values.Look<bool>(ref RemoveWorkspeedDebuffEnable, "RemoveWorkspeedDebuffEnable", true, false);
            Scribe_Values.Look<bool>(ref AssignSlaveEnable, "AssignSlaveEnable", true, false);
            Scribe_Values.Look<bool>(ref AssimilationSlaveEnable, "AssimilationSlaveEnable", true, false);
            Scribe_Values.Look<float>(ref Slavestage1Period, "Slavestage1Period", 15f, false);
            Scribe_Values.Look<float>(ref Slavestage2Period, "Slavestage2Period", 15f, false);
            Scribe_Values.Look<float>(ref Slavestage3Period, "Slavestage3Period", 15f, false);
            Scribe_Values.Look<float>(ref Slavestage4Period, "Slavestage4Period", 15f, false);
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("shacklesDefaultSetting_title".Translate(), ref ShacklesDefault, "shacklesDefaultSetting_desc".Translate());
            listingStandard.CheckboxLabeled("slavestageEnableSetting_title".Translate(), ref SlavestageEnable, "slavestageEnableSetting_desc".Translate());
            listingStandard.CheckboxLabeled("rebelcyclechangeEnableSetting_title".Translate(), ref RebelCycleChangeEnable, "rebelcyclechangeEnableSetting_desc".Translate());
            listingStandard.CheckboxLabeled("removeworkspeeddebuffEnableSetting_title".Translate(), ref RemoveWorkspeedDebuffEnable, "removeworkspeeddebuffEnableSetting_desc".Translate());
            listingStandard.CheckboxLabeled("assignslaveEnableSetting_title".Translate(), ref AssignSlaveEnable, "assignslaveEnableSetting_desc".Translate());
            listingStandard.CheckboxLabeled("assimilationslaveEnableSetting_title".Translate(), ref AssimilationSlaveEnable, "assimilationslaveEnableSetting_desc".Translate());
            listingStandard.Label("slavestage1Period_title".Translate(), -1f, "slavestage1Period_desc".Translate());
            listingStandard.TextFieldNumeric(ref Slavestage1Period, ref this.Slavestage1PeriodBuffer);
            listingStandard.Label("slavestage2Period_title".Translate(), -1f, "slavestage2Period_desc".Translate());
            listingStandard.TextFieldNumeric(ref Slavestage2Period, ref this.Slavestage2PeriodBuffer);
            listingStandard.Label("slavestage3Period_title".Translate(), -1f, "slavestage3Period_desc".Translate());
            listingStandard.TextFieldNumeric(ref Slavestage3Period, ref this.Slavestage3PeriodBuffer);
            listingStandard.Label("slavestage4Period_title".Translate(), -1f, "slavestage4Period_desc".Translate());
            listingStandard.TextFieldNumeric(ref Slavestage4Period, ref this.Slavestage4PeriodBuffer);
            if (listingStandard.ButtonText((string)"resetAllSetting_title".Translate()))
            {
                ShacklesDefault = true;
                SlavestageEnable = true;
                RebelCycleChangeEnable = true;
                RemoveWorkspeedDebuffEnable = true;
                AssignSlaveEnable = true;
                AssimilationSlaveEnable = true;
                Slavestage1Period = 15f;
                Slavestage2Period = 15f;
                Slavestage3Period = 15f;
                Slavestage4Period = 15f;
                Slavestage1PeriodBuffer = "15";
                Slavestage2PeriodBuffer = "15";
                Slavestage3PeriodBuffer = "15";
                Slavestage4PeriodBuffer = "15";
            }
            listingStandard.End();
        }
    }
}
