using UnityEngine;
using Verse;

namespace ArmorRacks
{
    public class ArmorRacksModSettings : ModSettings
    {
        public bool EquipSetForced = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref EquipSetForced, "EquipSetForced");
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
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        
        public override string SettingsCategory()
        {
            return "ArmorRacks_ModName".Translate();
        }
    }
    
}