using RimWorld;
using Verse;
using Verse.AI;

namespace Therapy
{
    public class WorkGiver_ReceiveTherapy : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(MainUtility.couchThingDef);

        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (!forced) return true;

            var couch = MainUtility.FindRandomCouchWithChair(pawn, false, MaxPathDanger(pawn));
            if (couch == null)
            {
                return true;
            }

            if (couch.CouchNoLongerUsable(pawn)) return true;
            return false;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.Faction != pawn.Faction)
            {
                return false;
            }

            if (!(t is Building building))
            {
                return false;
            }

            if (building.IsForbidden(pawn))
            {
                return false;
            }

            if (!pawn.CanReserve(building, 1, -1, null, forced))
            {
                return false;
            }

            var couch = MainUtility.FindRandomCouchWithChair(pawn, false, MaxPathDanger(pawn));
            if (couch == null)
            {
                return false;
            }

            if (couch.CouchNoLongerUsable(pawn))
            {
                return false;
            }

            if (building.IsBurning())
            {
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(MainUtility.patientJobDef, t);
        }
    }
}
