using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;




namespace TargetingModes
{
    public class TargetingModesSettings : ModSettings
    {

        public static bool accuracyPenalties = true;
        #region TargModeResetFrequencyInt
        private static float targModeResetFrequencyInt = 3f;
        public static int TargModeResetFrequencyInt =>
            (int)targModeResetFrequencyInt;
        #endregion
        public static bool raidersUseTargModes = true;
        #region MinimumSkillForRandomTargetingMode
        private static float raiderMinSkillForTargMode = 8f;
        public static int MinimumSkillForRandomTargetingMode =>
            (int)raiderMinSkillForTargMode;
        #endregion
        public static float mechanoidTargModeChance = 0.35f;
        public static float baseManhunterTargModeChance = 0.2f;

        public void DoWindowContents(Rect wrect)
        {
            Listing_Standard options = new Listing_Standard();
            Color defaultColor = GUI.color;
            options.Begin(wrect);

            GUI.color = defaultColor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            options.Gap();

            // General settings
            options.CheckboxLabeled("Settings_AccuracyPenalties".Translate(), ref accuracyPenalties, "Settings_AccuracyPenalties_Tooltip".Translate());
            options.Gap();

            String targModeResetFrequency = "Settings_TargModeResetFrequency_" + targModeResetFrequencyInt;
            options.Label("Settings_TargModeResetFrequency".Translate() + ": " + targModeResetFrequency.Translate(), -1f, targModeResetFrequency.Translate());
            targModeResetFrequencyInt = (int)options.Slider((float)targModeResetFrequencyInt, 0f, 5f);

            options.GapLine(24);

            // Settings for AI
            options.CheckboxLabeled("Settings_RaidersUseTargetingModes".Translate(), ref raidersUseTargModes, "Settings_RaidersUseTargetingModes_Tooltip".Translate());
            options.Gap();

            // Grey out this section if raiders can't use targeting modes
            if (!raidersUseTargModes)
                GUI.color = Color.grey;

            options.Label("Settings_MinRaiderWeaponSkill".Translate() + ": " + raiderMinSkillForTargMode, -1f, raiderMinSkillForTargMode.ToString());
            raiderMinSkillForTargMode = (int)options.Slider((float)raiderMinSkillForTargMode, 0f, 20f);

            options.Gap();

            options.Label("Settings_MechTargModeChance".Translate() + ": " + mechanoidTargModeChance + "%" , -1f, mechanoidTargModeChance.ToString() + "%");
            mechanoidTargModeChance = (int)options.Slider((float)mechanoidTargModeChance, 0f, 100f);

            options.Gap();

            options.Label("Settings_BaseManhunterTargModeChance".Translate() + ": " + baseManhunterTargModeChance + "%", -1f, baseManhunterTargModeChance.ToString() + "%");
            baseManhunterTargModeChance = (int)options.Slider((float)baseManhunterTargModeChance, 0f, 100f );          

            // End of section
            GUI.color = defaultColor;

            // End of settings
            options.End();
            Mod.GetSettings<TargetingModesSettings>().Write();

        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref accuracyPenalties, "accuracyPenalties", true);
            Scribe_Values.Look(ref targModeResetFrequencyInt, "targModeResetFrequencyInt", 3f);
            Scribe_Values.Look(ref raidersUseTargModes, "raidersUseTargModes", true);
            Scribe_Values.Look(ref raiderMinSkillForTargMode, "raiderMinSkillForTargMode", 8f);
            Scribe_Values.Look(ref mechanoidTargModeChance, "mechanoidTargModeChance", 0.35f);
            Scribe_Values.Look(ref baseManhunterTargModeChance, "baseManhunterTargModeChance", 0.2f);
        }

    }

    public class TargetingModes : Mod
    {
        public TargetingModesSettings settings;

        public TargetingModes(ModContentPack content) : base(content)
        {
            GetSettings<TargetingModesSettings>();
        }

        public override string SettingsCategory() => "TargetingModesSettingsCategory".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GetSettings<TargetingModesSettings>().DoWindowContents(inRect);
        }
    }

}
