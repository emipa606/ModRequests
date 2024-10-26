using System.Collections.Generic;
using Verse;
using HarmonyLib;
using RimWorld;

namespace zed_0xff.GeneCourier {
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.TickRare))]
    static class Patch_TickRare
    {
        static int prevTick = 0;

        static void Postfix(ref Pawn __instance)
        {
            if( __instance.Dead && GenTicks.TicksGame != prevTick ){
                GeneCache.ClearGenes();
                prevTick = GenTicks.TicksGame;
            }
        }
    }
}
