using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace WallStuff.Television
{
    [HarmonyPatch(typeof(JoyGiver_WatchBuilding))]
    [HarmonyPatch("TryGivePlayJob")]
    public class TryGivePlayJobHarmonyPatch
    {    
        private static void Postfix(JoyGiver_WatchBuilding __instance, ref Job __result, Pawn pawn, Thing t)
        {
            //jcLog.Warning(pawn.Name + " Checking If Ediface");
            if (!t.def.building.isEdifice)
            {
                IntVec3 c;
                Building t2;
                //jcLog.Warning(pawn.Name + " It's Not An Ediface");
                if (!WatchWallBuildingUtility.TryFindBestWatchCell(t, pawn, __instance.def.desireSit, out c, out t2))
                {
                    __result = null;
                    return;
                }
                __result = new Job(__instance.def.jobDef, t, c, t2);
                return;
            }
            return;
        }
    }
}
