using RimWorld;
using Verse;
using Verse.AI;

namespace Therapy
{
    public class WorkGiver_Therapist : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override bool Prioritized => true;

        public override bool HasJobOnThing(Pawn pawn, Thing targetPawn, bool forced = false)
        {
            if(!(targetPawn is Pawn patient)) return false;

            var building = patient.Position.GetEdifice(pawn.Map);
            if (!(building is Building_Couch couch)) return false;

            if(patient == pawn || patient.Dead || patient.Downed || patient.InMentalState) return false;
            if(!pawn.CanReserve(patient, 1, -1, null, forced)) return false;

            couch.TryFindChairNearCouch(out var chair);

            if (chair == null) return false;
            if (chair.IsForbidden(pawn)) return false;
            if (!pawn.CanReserveAndReach(chair, PathEndMode.InteractionCell, Danger.None, 1, -1, null, forced)) return false;
            //var room = t.GetRoom();
            //if (room == null)// || room.Role != ModBaseTherapy.therapyRoomRoleDef) 
            //    return false;

            //return room.ContainedAndAdjacentThings.Any(c => c is Building_Bed);

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing targetPawn, bool forced = false)
        {
            var building = targetPawn.Position.GetEdifice(pawn.Map);
            if (!(building is Building_Couch couch)) return null;

            couch.TryFindChairNearCouch(out var chair);

            return new Job(MainUtility.therapistJobDef, chair, couch.Position.GetFirstPawn(couch.Map));

        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            var patient = t.Cell.GetFirstPawn(t.Map);
            if (patient?.needs.mood == null) return 0;
            return (1 - patient.needs.mood.CurLevelPercentage)*10;
            //return t.Thing.GetStatValue(StatDefOf.SocialImpact);
        }
    }
}