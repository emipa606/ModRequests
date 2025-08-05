using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using HarmonyLib;
using Verse;
using VREAndroids;

namespace Psychic_Coiling_VRE_Addon
{
    [HarmonyPatch(typeof(PawnComponentsUtility), "CreateInitialComponents")]
    public static class CreateInitialComponentsPatch
    {
        public static bool Prefix(Pawn pawn)
        {
            if (pawn.RaceProps.IsFlesh)
            {
                return true;
            }
            if (pawn.IsAndroid() && pawn.HasActiveGene(VREAPC_InternalDefs.VREA_Addon_PsychicCoils) && pawn.psychicEntropy == null)
            {
                pawn.psychicEntropy = new Pawn_PsychicEntropyTracker(pawn);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnComponentsUtility), "AddComponentsForSpawn")]
    public static class  AddComponentsForSpawnPatch
    {
        public static bool Prefix(Pawn pawn)
        {
            if (pawn.RaceProps.IsFlesh)
            {
                return true;
            }
            if (pawn.IsAndroid() && pawn.HasActiveGene(VREAPC_InternalDefs.VREA_Addon_PsychicCoils) && pawn.psychicEntropy == null)
            {
                pawn.psychicEntropy = new Pawn_PsychicEntropyTracker(pawn);
            }
            return true;
        }
    }
}
