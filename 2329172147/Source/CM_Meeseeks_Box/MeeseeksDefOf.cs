using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [DefOf]
    public static class MeeseeksDefOf
    {
        public static DesignationDef CM_Meeseeks_Box_Designation_PressButton;
        public static JobDef CM_Meeseeks_Box_Job_PressButton;
        public static WorkGiverDef CM_Meeseeks_Box_WorkGiver_PressButton;

        public static PawnKindDef MeeseeksKind;
        public static ThingDef MeeseeksRace;

        public static FactionDef CM_Meeseeks_Box_Faction_Hostile_Meeseeks;

        public static ThingDef CM_Meeseeks_Box_Thing_Meeseeks_Box;

        public static TraitDef CM_Meeseeks_Box_Trait_Meeseeks;
        public static HediffDef CM_Meeseeks_Box_Hediff_Existence_Is_Pain;
        public static ThoughtDef CM_Meeseeks_Box_Thought_Existence_Is_Pain;

        public static WorkTypeDef CM_Meeseeks_Box_WorkType_Killing;
        public static WorkGiverDef CM_Meeseeks_Box_WorkGiver_Kill;

        public static MentalStateDef CM_Meeseeks_Box_MentalState_MeeseeksKillCreator;
        public static MentalStateDef CM_Meeseeks_Box_MentalState_MeeseeksMakeMeeseeks;
        public static MentalStateDef CM_Meeseeks_Box_MentalState_MeeseeksMakeMeeseeksMajor;

        public static MentalBreakDef CM_Meeseeks_Box_MentalBreak_MeeseeksKillCreator;
        public static MentalBreakDef CM_Meeseeks_Box_MentalBreak_MeeseeksMakeMeeseeks;
        public static MentalBreakDef CM_Meeseeks_Box_MentalBreak_MeeseeksMakeMeeseeksMajor;

        public static DutyDef CM_Meeseeks_Box_Duty_Kill_Creator;

        public static JobDef CM_Meeseeks_Box_Job_AcquireEquipment;
        public static JobDef CM_Meeseeks_Box_Job_EmbraceTheVoid;
        public static JobDef CM_Meeseeks_Box_Job_Kill;
        public static JobDef CM_Meeseeks_Box_Job_UseMeeseeksBox;
        public static JobDef CM_Meeseeks_Box_Job_ApproachTarget;
        public static JobDef CM_Meeseeks_Box_Job_WaitNear;

        public static EffecterDef CM_Meeseeks_Box_Effecter_Progress_Bar;

        public static SoundDef CM_Meeseeks_Box_Poof_In;
        public static SoundDef CM_Meeseeks_Box_Poof_Out;

        public static SoundDef CM_Meeseeks_Box_Sound_OK;
        public static SoundDef CM_Meeseeks_Box_Sound_OK_2;
        public static SoundDef CM_Meeseeks_Box_Sound_Can_Do;
        public static SoundDef CM_Meeseeks_Box_Sound_Can_Do_2;

        public static SoundDef CM_Meeseeks_Box_Sound_All_Done;

        public static SoundDef CM_Meeseeks_Box_Sound_Im_Mr_Meeseeks_Look_At_Me;
        public static SoundDef CM_Meeseeks_Box_Sound_Look_At_Me;
        public static SoundDef CM_Meeseeks_Box_Sound_Im_Mr_Meeseeks;
        public static SoundDef CM_Meeseeks_Box_Sound_Im_Mr_Meeseeks_2;
        public static SoundDef CM_Meeseeks_Box_Sound_Im_Mr_Meeseeks_3;

        public static SoundDef CM_Meeseeks_Box_Sound_I_Cant_Take_It_Anymore;
    }
}
