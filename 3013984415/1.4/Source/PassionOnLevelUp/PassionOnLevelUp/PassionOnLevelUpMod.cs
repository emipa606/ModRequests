using HarmonyLib;
using UnityEngine;
using Verse;

namespace PassionOnLevelUp
{
    public class PassionOnLevelUpMod : Mod
    {
        /// <summary>
        /// A reference to our settings.
        /// </summary>
        public PassionOnLevelUpModSettings settings;
        
        public PassionOnLevelUpMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<PassionOnLevelUpModSettings>();
        }
        
        /// <summary>
        /// The (optional) GUI part to set your settings.
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            //Chance of Gaining Passions
            //listingStandard.Label("Chance to remove apathy " + settings.chanceToRemoveApathy.ToString());
            settings.chanceToRemoveApathy = listingStandard.SliderLabeled("Chance to remove apathy " + settings.chanceToRemoveApathy,settings.chanceToRemoveApathy, 0f, 100f);
            
            // listingStandard.Label("Chance to Gain Minor Passion " + settings.chanceToGainMinorPassion.ToString());
            settings.chanceToGainMinorPassion = listingStandard.SliderLabeled("Chance to Gain Minor Passion " + settings.chanceToGainMinorPassion, settings.chanceToGainMinorPassion, 0f, 100f);
            
            // listingStandard.Label("Chance to Gain Major Passion " + settings.chanceToGainMajorPassion.ToString());
            settings.chanceToGainMajorPassion = listingStandard.SliderLabeled("Chance to Gain Major Passion " + settings.chanceToGainMajorPassion, settings.chanceToGainMajorPassion, 0f, 100f);
            
            // listingStandard.Label("Chance to Gain Natural Passion " + settings.chanceToGainNaturalPassion.ToString());
            settings.chanceToGainNaturalPassion = listingStandard.SliderLabeled("Chance to Gain Natural Passion " + settings.chanceToGainNaturalPassion, settings.chanceToGainNaturalPassion, 0f, 100f);
            
            // listingStandard.Label("Chance to Gain Critical Passion " + settings.chanceToGainCriticalPassion.ToString());
            settings.chanceToGainCriticalPassion = listingStandard.SliderLabeled("Chance to Gain Critical Passion " + settings.chanceToGainCriticalPassion, settings.chanceToGainCriticalPassion, 0f, 100f);
            
            listingStandard.GapLine();
            //Minimum Skills to Gain Passions
            // listingStandard.Label("Minimum skill to Lose Apathy " + settings.minSkillToLoseApathy.ToString());
            settings.minSkillToLoseApathy = listingStandard.SliderLabeled("Minimum skill to Lose Apathy " + settings.minSkillToLoseApathy, settings.minSkillToLoseApathy, 0f, 20f);
            
            // listingStandard.Label("Minimum skill to Gain Minor Passion on Skill " + settings.minSkillToGainMinorPassion.ToString());
            settings.minSkillToGainMinorPassion = listingStandard.SliderLabeled("Minimum skill to Gain Minor Passion on Skill " + settings.minSkillToGainMinorPassion, settings.minSkillToGainMinorPassion, 0f, 20f);
            
            // listingStandard.Label("Minimum skill to Gain Major Passion on Skill " + settings.minSkillToGainMajorPassion.ToString());
            settings.minSkillToGainMajorPassion = listingStandard.SliderLabeled("Minimum skill to Gain Major Passion on Skill " + settings.minSkillToGainMajorPassion, settings.minSkillToGainMajorPassion, 0f, 20f);
            
            // listingStandard.Label("Minimum skill to Gain Natural Passion on Skill " + settings.minSkillToGainNaturalPassion.ToString());
            settings.minSkillToGainNaturalPassion = listingStandard.SliderLabeled("Minimum skill to Gain Natural Passion on Skill " + settings.minSkillToGainNaturalPassion, settings.minSkillToGainNaturalPassion, 0f, 20f);
            
            // listingStandard.Label("Minimum skill to Gain Critical Passion on Skill " + settings.minSkillToGainCriticalPassion.ToString());
            settings.minSkillToGainCriticalPassion = listingStandard.SliderLabeled("Minimum skill to Gain Critical Passion on Skill " + settings.minSkillToGainCriticalPassion, settings.minSkillToGainCriticalPassion, 0f, 20f);
            
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
            return "PassionOnLevelUp";
        }
    }
}