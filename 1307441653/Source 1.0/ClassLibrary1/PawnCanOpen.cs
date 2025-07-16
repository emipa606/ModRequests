using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Shoo
{
    [HarmonyPatch(typeof(Building_Door), "PawnCanOpen")]
    class PawnCanOpen
    {
        public static void Postfix(ref bool __result, Pawn p)
        {
            __result = __result || p.CurJob?.def == ShooDefOf.FleeBecauseShoo;
        }
    }
}
