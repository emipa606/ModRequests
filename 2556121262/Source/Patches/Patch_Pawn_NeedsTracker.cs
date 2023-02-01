using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;

namespace HalfDragons.Patch1
{
    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    class Patch_ShouldHaveNeed
    {
        [HarmonyPostfix]
        private static void RestrictDragonBloodNeedToHalfDragons(Pawn_NeedsTracker __instance, ref bool __result, ref NeedDef nd, Pawn ___pawn)
        {
            if(nd != HD_Common.dragonBlood)
            {
                return;
            }
            if(__result == false)
            {
                return;
            }
            if (___pawn.IsHalfDragon())
            {
                __result = true;
            }
            else
            {
                __result = false;
            }
        }
    }
}
