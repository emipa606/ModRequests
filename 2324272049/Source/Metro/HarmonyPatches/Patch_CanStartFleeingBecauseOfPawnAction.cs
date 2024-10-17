using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Metro
{
    [HarmonyPatch(typeof(Pawn_MindState))]
    [HarmonyPatch("CanStartFleeingBecauseOfPawnAction")]
    public static class Patch_CanStartFleeingBecauseOfPawnAction
    {

        [HarmonyPrefix]
        private static bool Prefix(ref bool __result, Pawn p)
        {
            bool result;
            if (p is Mutant)
            {
                __result = false;
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
    }
}

