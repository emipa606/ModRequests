using System.Collections.Generic;
using Verse;

namespace PassionOnLevelUp
{
    public class PassionOnLevelUpModSettings : ModSettings
    {
        /// <summary>
        /// The settings our mod has.
        /// </summary>
        
        //Chance to level up passions
        public float chanceToGainMinorPassion = 15f;
        public float chanceToGainMajorPassion = 10f;
        public float chanceToGainNaturalPassion = 5f;
        public float chanceToGainCriticalPassion = 3f;
        public float chanceToRemoveApathy = 20f;
        
        //Minimum Required skill to level up passions
        public float minSkillToGainMinorPassion = 12f;
        public float minSkillToGainMajorPassion = 15f;
        public float minSkillToGainNaturalPassion = 17f;
        public float minSkillToGainCriticalPassion = 19f;
        public float minSkillToLoseApathy = 10f;

        /// <summary>
        /// The part that writes our settings to file. Note that saving is by ref.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref chanceToGainMinorPassion, "chanceToGainMinorPassion", 15f);
            Scribe_Values.Look(ref chanceToGainMajorPassion, "chanceToGainMajorPassion", 10f);
            Scribe_Values.Look(ref chanceToGainNaturalPassion, "chanceToGainNaturalPassion", 5f);
            Scribe_Values.Look(ref chanceToGainCriticalPassion, "chanceToGainCriticalPassion", 3f);
            Scribe_Values.Look(ref chanceToRemoveApathy, "chanceToRemoveApathy", 8f);
            
            Scribe_Values.Look(ref minSkillToGainMinorPassion, "minSkillToGainMinorPassion", 10f);
            Scribe_Values.Look(ref minSkillToGainMajorPassion, "minSkillToGainMajorPassion", 15f);
            Scribe_Values.Look(ref minSkillToGainNaturalPassion, "minSkillToGainNaturalPassion", 16f);
            Scribe_Values.Look(ref minSkillToGainCriticalPassion, "minSkillToGainCriticalPassion", 18f);
            Scribe_Values.Look(ref minSkillToLoseApathy, "minSkillToLoseApathy", 8f);
            
            base.ExposeData();
        }
    }
}