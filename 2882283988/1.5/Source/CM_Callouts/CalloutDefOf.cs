using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Callouts
{
    [DefOf]
    public static class CalloutDefOf
    {
        public static ThingDef CM_Callouts_Thing_Mote_Text_Ticked;
        public static ThingDef CM_Callouts_Thing_Mote_Text_Wound;
        public static ThingDef CM_Callouts_Thing_Mote_Attached_Text;

        // Combat callout rules
        public static RulePackDef CM_Callouts_RulePack_Ranged_Attack;

        public static RulePackDef CM_Callouts_RulePack_Ranged_Attack_Landed_OriginalTarget;
        public static RulePackDef CM_Callouts_RulePack_Ranged_Attack_Received_OriginalTarget;

        public static RulePackDef CM_Callouts_RulePack_Melee_Attack;

        public static RulePackDef CM_Callouts_RulePack_Melee_Attack_Landed;
        public static RulePackDef CM_Callouts_RulePack_Melee_Attack_Received;


        // Other callout rules
        public static RulePackDef CM_Callouts_RulePack_Wounded;

        public static RulePackDef CM_Callouts_RulePack_Lethal_Hediff_Progression;

        public static RulePackDef CM_Callouts_RulePack_Drafted;


        //Interaction callout rules
        public static RulePackDef CM_Callouts_RulePack_Interaction_Animal_Nuzzle_Initiated;
        public static RulePackDef CM_Callouts_RulePack_Interaction_Animal_Nuzzle_Received;

        public static RulePackDef CM_Callouts_RulePack_Interaction_Animal_Feed_Initiated;
        public static RulePackDef CM_Callouts_RulePack_Interaction_Animal_Feed_Received;

        public static RulePackDef CM_Callouts_RulePack_Interaction_Animal_Tame_Success_Initiated;
        public static RulePackDef CM_Callouts_RulePack_Interaction_Animal_Tame_Success_Received;

        public static RulePackDef CM_Callouts_RulePack_Interaction_Animal_Train_Success_Initiated;
        public static RulePackDef CM_Callouts_RulePack_Interaction_Animal_Train_Success_Received;

        public static RulePackDef CM_Callouts_RulePack_Interaction_Animal_Tend_Initiated;
        public static RulePackDef CM_Callouts_RulePack_Interaction_Animal_Tend_Received;

        public static RulePackDef CM_Callouts_RulePack_Interaction_Animal_Slaughter_Initiated;
        public static RulePackDef CM_Callouts_RulePack_Interaction_Animal_Slaughter_Received;
    }
}
