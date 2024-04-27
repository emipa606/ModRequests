using RimWorld;
using Verse;
using Verse.AI;

namespace MoreArchotechGarbage
{
    public class WorkGiver_CarryCorpseToArchoGeneRipper : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Corpse);
        public ThingRequest ThingRequest => ThingRequest.ForDef(MAG_DefOf.MAG_ArchotechGeneRipper);
        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (!base.ShouldSkip(pawn, forced))
            {
                return !pawn.RaceProps.Humanlike;
            }
            return !ModsConfig.BiotechActive;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            Pawn pawn2 = ((Corpse)t).InnerPawn;
            if (FindBuildingFor(pawn2, pawn, forced) != null)
            {
                return true;
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = ((Corpse)t).InnerPawn;
            Building building = FindBuildingFor(pawn2, pawn, forced);
            if (building == null)
            {
                return null;
            }
            Job job = JobMaker.MakeJob(MAG_DefOf.MAG_CarryCorpseToBuilding, building, t);
            job.count = 1;
            return job;
        }

        private Building FindBuildingFor(Pawn pawn, Pawn traveller, bool forced)
        {
            return (Building)GenClosest.ClosestThingReachable(traveller.Position, traveller.Map, ThingRequest, PathEndMode.InteractionCell, TraverseParms.For(traveller), 9999f, CanUse);
            bool CanUse(Thing thing)
            {
                Building_Enterable building_Enterable;
                if ((building_Enterable = thing as Building_Enterable) == null)
                {
                    return false;
                }
                if (!ThingRequest.Accepts(thing))
                {
                    return false;
                }
                if (thing.IsForbidden(traveller))
                {
                    return false;
                }
                if (!traveller.CanReserve(thing, 1, -1, null, forced))
                {
                    return false;
                }
                if (traveller.Map.designationManager.DesignationOn(thing, DesignationDefOf.Deconstruct) != null)
                {
                    return false;
                }
                if (building_Enterable.SelectedPawn != pawn)
                {
                    return false;
                }
                if (!building_Enterable.CanAcceptPawn(pawn))
                {
                    return false;
                }
                return true;
            }
        }
    }
}
