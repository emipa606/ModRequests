using UnityEngine;
using Verse;

namespace ArmorRacks
{
    public class ArmorRacksModSettings : ModSettings
    {
        public bool EquipSetForcedDefault = false;
        public bool TransferSetForcedDefault = false;
        public int RareTicksPerMendDefault = 40;
        public int EquipSpeedFactorUnpoweredDefault = 70;
        public int EquipSpeedFactorPoweredDefault = 20;
        
        public bool EquipSetForced = false;
        public bool TransferSetForced = false;
        public int RareTicksPerMend = 40;
        public int EquipSpeedFactorUnpowered = 70;
        public int EquipSpeedFactorPowered = 20;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref EquipSetForced, "EquipSetForced");
            Scribe_Values.Look(ref TransferSetForced, "TransferSetForced");
            Scribe_Values.Look(ref RareTicksPerMend, "RareTicksPerMend");
            Scribe_Values.Look(ref EquipSpeedFactorUnpowered, "EquipSpeedFactorUnpowered");
            Scribe_Values.Look(ref EquipSpeedFactorPowered, "EquipSpeedFactorPowered");
        }
    }

    public class ArmorRacksMod : Verse.Mod
    {
        public ArmorRacksModSettings Settings;
        
        public ArmorRacksMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ArmorRacksModSettings>();
        }
        
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("ArmorRacks_EquipSetForcedModSetting_Label".Translate(), ref Settings.EquipSetForced);
            listingStandard.CheckboxLabeled("ArmorRacks_TransferSetForcedModSetting_Label".Translate(), ref Settings.TransferSetForced);
            listingStandard.GapLine();
            listingStandard.Label(
                "ArmorRacks_RareTicksPerMendModSetting_Label".Translate(), 
                -1f, 
                "ArmorRacks_RareTicksPerMendModSetting_Tooltip".Translate());
            var buffer1 = Settings.RareTicksPerMend.ToString();
            listingStandard.IntEntry(ref Settings.RareTicksPerMend, ref buffer1, 1);
            listingStandard.Label(
                "ArmorRacks_EquipSpeedFactorUnpowered_Label".Translate(), 
                -1f, 
                "ArmorRacks_EquipSpeedFactor_Tooltip".Translate());
            var buffer2 = Settings.EquipSpeedFactorUnpowered.ToString();
            listingStandard.TextFieldNumeric(ref Settings.EquipSpeedFactorUnpowered, ref buffer2);
            listingStandard.Label(
                "ArmorRacks_EquipSpeedFactorPowered_Label".Translate(), 
                -1f, 
                "ArmorRacks_EquipSpeedFactor_Tooltip".Translate());
            var buffer3 = Settings.EquipSpeedFactorPowered.ToString();
            listingStandard.TextFieldNumeric(ref Settings.EquipSpeedFactorPowered, ref buffer3);
            listingStandard.GapLine();
            if (listingStandard.ButtonText("ArmorRacks_RestoreDefaultSettings_Label".Translate()))
            {
                Settings.EquipSetForced = Settings.EquipSetForcedDefault;
                Settings.TransferSetForced = Settings.TransferSetForcedDefault;
                Settings.RareTicksPerMend = Settings.RareTicksPerMendDefault;
                Settings.EquipSpeedFactorUnpowered = Settings.EquipSpeedFactorUnpoweredDefault;
                Settings.EquipSpeedFactorPowered = Settings.EquipSpeedFactorPoweredDefault;
            }
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        
        public override string SettingsCategory()
        {
            return "ArmorRacks_ModName".Translate();
        }
    }
    
}