using System;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace HamsterWheel
{
    public class JobGiver_RunningWheelPrisoner : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            return 0.6f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Map map = pawn.Map;
            //죄수일때만

            if (pawn.IsPrisoner)
            {
                if (pawn.Map.listerThings.ThingsOfDef(ThingDefOf.HamsterWheelGenerator).Any())
                {
                    foreach (Building wheel in pawn.Map.listerThings.ThingsMatching(ThingRequest.ForDef(ThingDefOf.HamsterWheelGenerator)))
                    {
                        if(wheel.Position.IsInPrisonCell(map))
                        {
                            if (pawn.CanReserve(wheel) && wheel.GetComp<CompPowerPlantHamsterWheel>().user == null)
                            {
                                return new Job(JobDefOf.Job_HamsterWheel, wheel);
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
