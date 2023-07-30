using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    public class JobGiver_UseMeeseeksBox : ThinkNode_JobGiver
    {
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_UseMeeseeksBox obj = (JobGiver_UseMeeseeksBox)base.DeepCopy(resolve);
            return obj;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return null;
            }
            if (pawn.GetRegion() == null)
            {
                return null;
            }

            Map map = pawn.MapHeld;

            Predicate<Thing> validator = (thing => (thing.def == MeeseeksDefOf.CM_Meeseeks_Box_Thing_Meeseeks_Box && !(thing as ThingWithComps).GetComp<CompMeeseeksBox>().Coolingdown && ReservationUtility.CanReserve(pawn, thing)));

            Thing meeseeksBox = GenClosest.ClosestThingReachable(pawn.PositionHeld, pawn.MapHeld, ThingRequest.ForDef(MeeseeksDefOf.CM_Meeseeks_Box_Thing_Meeseeks_Box), PathEndMode.Touch, TraverseParms.For(pawn), 9999f, validator);
            if (meeseeksBox != null)
                return JobMaker.MakeJob(MeeseeksDefOf.CM_Meeseeks_Box_Job_UseMeeseeksBox, meeseeksBox);

            if (pawn.MentalState != null)
                pawn.MentalState.RecoverFromState();

            return null;
        }
    }
}