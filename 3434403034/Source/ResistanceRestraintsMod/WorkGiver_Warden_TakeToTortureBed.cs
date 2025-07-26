using System.Linq;
using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace ResistanceRestraintsMod
{
    public class WorkGiver_Warden_TakeToTortureBed : WorkGiver_Warden
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // Only execute if the job is explicitly forced by the player
            if (!forced)
            {
                return null;
            }

            Pawn prisoner = t as Pawn;
            if (prisoner == null || !prisoner.IsPrisoner || prisoner.Downed || !pawn.CanReserve(prisoner))
            {
                return null;
            }

            Building_Bed tortureBed = FindTortureBedFor(prisoner, pawn);
            if (tortureBed != null)
            {
                Job job = JobMaker.MakeJob(JobDefOf.EscortPrisonerToBed, prisoner, tortureBed);
                job.count = 1;
                return job;
            }

            return null;
        }

        private Building_Bed FindTortureBedFor(Pawn prisoner, Pawn warden)
        {
            foreach (Thing thing in prisoner.Map.listerThings.AllThings)
            {
                if (thing.def.thingCategories != null && thing.def.thingCategories.Contains(ThingCategoryDef.Named("BuildingsPrisonerRestraints")))
                {
                    Building_Bed bed = thing as Building_Bed;
                    if (bed != null && !bed.Medical && bed.AnyUnoccupiedSleepingSlot && warden.CanReserveAndReach(bed, PathEndMode.OnCell, Danger.Some))
                    {
                        return bed;
                    }
                }
            }
            return null;
        }

    }
}
