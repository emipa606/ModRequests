using Verse;
using RimWorld;
using Verse.AI;
using System.Linq;

namespace ResistanceRestraintsMod
{
    public class WorkGiver_Warden_RemoveImmobility : WorkGiver_Warden
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {

            // Ensure the job is explicitly forced by the player
            if (!forced)
            {
                return null;
            }


            // Ensure the target is a prisoner in the torture bed
            if (t is Pawn prisoner && prisoner.IsPrisoner)
            {
                // Check if the prisoner is in a bed that belongs to the BuildingsPrisonerRestraints category
                Building_Bed tortureBed = prisoner.CurrentBed() as Building_Bed;
                if (tortureBed == null ||
                    tortureBed.def.thingCategories == null ||
                    !tortureBed.def.thingCategories.Contains(ThingCategoryDef.Named("BuildingsPrisonerRestraints")))
                {
                    return null;
                }


                // Ensure the prisoner has the immobility hediff
                Hediff immobilityHediff = prisoner.health.hediffSet.GetFirstHediffOfDef(HediffDefOf_SilkCircuit.SilkCircuit_Immobile);
                if (immobilityHediff == null)
                {
                    return null;
                }

                // Ensure the warden can reach the prisoner
                if (!pawn.CanReserveAndReach(prisoner, PathEndMode.ClosestTouch, Danger.Some))
                {
                    return null;
                }

                // Create the job to go to the prisoner and remove the hediff
                Job job = JobMaker.MakeJob(JobDefOf_SilkCircuit.SilkCircuit_RemoveImmobility, prisoner);
                job.targetA = prisoner;
                return job;
            }
            return null;
        }
    }

    [DefOf]
    public static class JobDefOf_SilkCircuit
    {
        public static JobDef SilkCircuit_RemoveImmobility;

        static JobDefOf_SilkCircuit()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf_SilkCircuit));
        }

    }
}
