using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using VREAndroids;

namespace Psychic_Coiling_VRE_Addon
{
    [HarmonyPatch(typeof(MeditationFocusTypeAvailabilityCache_PawnCanUseInt_Patch), "Postfix")]
    public static class MeditationTypeAvailabilityUnPreventionPatch
    {
        public static bool Prefix(Pawn p)
        {
            if (p.HasActiveGene(VREA_DefOf.VREA_JoyDisabled) && p.HasActiveGene(VREAPC_InternalDefs.VREA_Addon_PsychicCoils))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MeditationUtility_CanMeditateNow_Patch), "Postfix")]
    public static class CanMeditateNowFixer
    {
        public static bool Prefix(Pawn pawn)
        {
            if (pawn.HasActiveGene(VREA_DefOf.VREA_JoyDisabled) && pawn.HasActiveGene(VREAPC_InternalDefs.VREA_Addon_PsychicCoils))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MeditationUtility), "CanMeditateNow")]
    public static class MeditationJobgiverUnPreventionPatch
    {
        [HarmonyPriority(Priority.High)]
        public static void PostFix(ref bool __result, Pawn pawn)
        {
            if (pawn.HasActiveGene(VREAndroids.VREA_DefOf.VREA_JoyDisabled) && !(pawn.HasActiveGene(VREAPC_InternalDefs.VREA_Addon_PsychicCoils)))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(JoyGiver_Meditate), "TryGiveJob")]
    public static class MeditationJobGiverFixer
    {
        [HarmonyPriority(Priority.High)]
        public static void PostFix(ref Job __result, Pawn pawn)
        {
            if (pawn.HasActiveGene(VREAndroids.VREA_DefOf.VREA_JoyDisabled) && pawn.HasActiveGene(VREAPC_InternalDefs.VREA_Addon_PsychicCoils))
            {
                __result = MeditationUtility.GetMeditationJob(pawn, forJoy: false);
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker), "ShouldShowFor")]
    public static class ShowMeditationFocusGainPatch
    {
        [HarmonyPriority(Priority.High)]
        public static void Postfix(StatWorker __instance, ref bool __result, StatRequest req, StatDef ___stat)
        {
            if (__result && req.Thing is Pawn pawn && pawn.IsAndroid())
            {
                if (___stat == StatDefOf.MeditationFocusGain && pawn.HasActiveGene(VREAPC_InternalDefs.VREA_Addon_PsychicCoils))
                {
                    __result = true;
                }
            }
        }
    }
}
