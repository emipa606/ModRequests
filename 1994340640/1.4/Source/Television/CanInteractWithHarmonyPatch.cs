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
    [HarmonyPatch("CanInteractWith")]
    public class CanInteractWithHarmonyPatch
    {  
        private static void Postfix(JoyGiver_WatchBuilding __instance, ref bool __result, Pawn pawn, Thing t, bool inBed)
        {
            //jcLog.Warning("Can Pawn: " + pawn.Name + " Interact With Thing: " + t);
            if (!t.def.building.isEdifice)
            {
                if (!CanInteractCheck(pawn, t, inBed, __instance))
                {
                    //jcLog.Warning(pawn.Name + " No :(");
                    __result = false;
                    return;
                }

                if (inBed)
                {
                    //jcLog.Warning(pawn.Name + " In Bed");
                    Building_Bed bed = pawn.CurrentBed();

                    //jcLog.Warning(pawn.Name + " is ediface");
                    __result = WatchWallBuildingUtility.CanWatchFromBed(pawn, bed, t);
                    return;
                }
                __result = true;
                return;
            }
            return;
        }

        private static bool CanInteractCheck(Pawn pawn, Thing t, bool inBed, JoyGiver_WatchBuilding parent)// Need
        {
            //jcLog.Warning(pawn.Name + " Can Pawn reserve thing ?");
            if (!pawn.CanReserve(t, parent.def.jobDef.joyMaxParticipants, -1, null, false))
            {
                return false;
            }
            //jcLog.Warning(pawn.Name + " Is thing forbidden ?");
            if (t.IsForbidden(pawn))
            {
                return false;
            }
            //jcLog.Warning(pawn.Name + " Is it socially proper ?");
            if (!t.IsSociallyProper(pawn))
            {
                return false;
            }
            //jcLog.Warning(pawn.Name + " Politically ?");
            if (!t.IsPoliticallyProper(pawn))
            {
                return false;
            }
            CompPowerTrader compPowerTrader = t.TryGetComp<CompPowerTrader>();
            return (compPowerTrader == null || compPowerTrader.PowerOn) && (!parent.def.unroofedOnly || !WatchWallBuildingUtility.GetAdjustedCenter(t).Roofed(t.Map));
        }
    }
}
