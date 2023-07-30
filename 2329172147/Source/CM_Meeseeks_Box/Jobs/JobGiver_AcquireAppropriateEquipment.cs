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
    public class JobGiver_AcquireAppropriateEquipment : ThinkNode_JobGiver
    {
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_AcquireAppropriateEquipment obj = (JobGiver_AcquireAppropriateEquipment)base.DeepCopy(resolve);
            return obj;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.equipment == null)
            {
                return null;
            }
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return null;
            }
            if (pawn.GetRegion() == null)
            {
                return null;
            }

            //if (pawn.Faction != Faction.OfPlayer)
            //{
            //    Log.ErrorOnce(string.Concat("Non-colonist ", pawn, " tried to acquire equipment."), 764323);
            //    return null;
            //}
            if (pawn.IsQuestLodger())
            {
                return null;
            }

            Thing selectedEquipment = JobDriver_AcquireEquipment.FindEquipment(pawn);

            if (!DebugViewSettings.debugApparelOptimize && Find.TickManager.TicksGame < pawn.mindState.nextApparelOptimizeTick)
                return null;

            if (selectedEquipment != null)
                return JobMaker.MakeJob(MeeseeksDefOf.CM_Meeseeks_Box_Job_AcquireEquipment, selectedEquipment);

            return null;
        }
    }
}