
using RimWorld;
using VanillaPsycastsExpanded;
using Verse;
using HarmonyLib;
using VFECore.Abilities;
using VPEArchocaster;
using AbilityDef = VFECore.Abilities.AbilityDef;
using System.ComponentModel;




namespace VPEArchocaster
{
    
    [HarmonyPatch(typeof(Pawn_SkillTracker), nameof(Pawn_SkillTracker.Learn)), Category("ThrallsPatch")]


    public static class Pawn_SkillTracker_Learn_Patch_Patch
    {

        static bool isRemovingThrall = false;

        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        public static bool PrefixBeforeVPE(SkillDef sDef, ref float xp, Pawn ___pawn)
        {
            if ((VPEArchocasterMod_Settings.allowThrallsLearning < float.Epsilon))
            {
                return true;
            }
                if ((___pawn.story?.traits?.HasTrait(VPE_DefOf.VPE_Thrall) ?? false) && xp > 0)
            {
                isRemovingThrall = true;
                xp = -xp;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryLow)]
        public static bool PrefixAfterVPE(SkillDef sDef, ref float xp, Pawn ___pawn)
        {
            if ((VPEArchocasterMod_Settings.allowThrallsLearning < float.Epsilon))
            {
                return true;
            }
            if (!isRemovingThrall)
            {
                return true;
            }
            if ((___pawn.story?.traits?.HasTrait(VPE_DefOf.VPE_Thrall) ?? false))
            {
                xp = -xp;
            }
            isRemovingThrall = false;
            return true;
        }

    }
    
}