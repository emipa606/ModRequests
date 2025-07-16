using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Shoo
{
    class WorkGiver_Shoo: WorkGiver_InteractAnimal
    {
        public static float refireTicks = 1250;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (Designation des in pawn.Map.designationManager.SpawnedDesignationsOfDef(ShooDefOf.Shoo))
            {
                yield return des.target.Thing;
            }
        }

        protected override bool CanInteractWithAnimal(Pawn pawn, Pawn animal, bool forced)
        {
            if (!pawn.CanReserve(animal, 1, -1, null, false))
            {
                return false;
            }
            if (animal.Downed)
            {
                return false;
            }
            if (!animal.CanCasuallyInteractNow(false))
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;
            if (pawn2 == null || !pawn2.NonHumanlikeOrWildMan())
            {
                return null;
            }
            if (pawn.Map.designationManager.DesignationOn(t, ShooDefOf.Shoo) == null)
            {
                return null;
            }
            if (Find.TickManager.TicksGame < pawn2.mindState.lastAssignedInteractTime + refireTicks)
            {
                JobFailReason.Is(AnimalInteractedTooRecentlyTrans);
                return null;
            }
            if (!CanInteractWithAnimal(pawn, pawn2, forced))
            {
                return null;
            }
            return JobMaker.MakeJob(ShooDefOf.ShooJob, t);
        }
    }
}
