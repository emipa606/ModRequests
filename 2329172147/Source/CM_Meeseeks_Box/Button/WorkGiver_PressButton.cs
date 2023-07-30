using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using HarmonyLib;
using System;

namespace CM_Meeseeks_Box
{
    public class WorkGiver_PressButton : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Thing> thingList = pawn.Map.listerThings.AllThings.Where(thing => thing.TryGetComp<CompHasButton>() != null).ToList();

            foreach (Thing thing in thingList)
                yield return thing;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!forced && pawn.Map.designationManager.DesignationOn(t, MeeseeksDefOf.CM_Meeseeks_Box_Designation_PressButton) == null)
            {
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            return JobMaker.MakeJob(MeeseeksDefOf.CM_Meeseeks_Box_Job_PressButton, thing);
        }
    }
}
