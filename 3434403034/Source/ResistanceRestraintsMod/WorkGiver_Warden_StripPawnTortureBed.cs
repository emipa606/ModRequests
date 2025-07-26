using System.Linq;
using Verse;
using RimWorld;
using Verse.AI;

namespace ResistanceRestraintsMod
{
    public class WorkGiver_Warden_StripPawnTortureBed : WorkGiver_Warden
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn prisoner = t as Pawn;
            if (prisoner == null || !prisoner.IsPrisoner || !pawn.CanReserve(prisoner))
            {
                return null;
            }

            // Ensure the prisoner is currently assigned to or lying in a bed that is either SilkCircuit_PrisonerCage or SilkCircuit_ChemfuelBath
            Building_Bed currentBed = prisoner.CurrentBed();
            if (currentBed == null || (currentBed.def.defName != "SilkCircuit_PrisonerCage" && currentBed.def.defName != "SilkCircuit_ChemfuelBath"))
            {
                return null;
            }

            // Check if prisoner has apparel to strip
            if (prisoner.apparel != null && prisoner.apparel.WornApparel.Any())
            {
                Job stripJob = JobMaker.MakeJob(JobDefOf.Strip, prisoner);
                stripJob.count = 1;
                return stripJob;
            }

            return null;
        }
    }
}