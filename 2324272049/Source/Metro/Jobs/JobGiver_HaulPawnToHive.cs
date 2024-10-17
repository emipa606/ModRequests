using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Metro
{
	public class JobGiver_HaulPawnToHive : ThinkNode_JobGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
            var hiveCells = pawn.GetHiveCells();
            Predicate<Thing> downedPawnValidator = delegate (Thing t)
            {
                var p = t as Pawn;
                return !hiveCells.Contains(p.Position) && p.Downed && p.def != pawn.def;
            };

            Thing victim = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                    ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch,
                    TraverseParms.For(pawn, Danger.None, TraverseMode.NoPassClosedDoors, false), 50f, downedPawnValidator, null);

            var cellCandidates = hiveCells.Where(x => x.Walkable(pawn.Map) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly));
            if (cellCandidates.Count() > 0 && victim != null && ReservationUtility.CanReserveAndReach(pawn, victim, PathEndMode.ClosestTouch, Danger.None))
            {
                Job job = JobMaker.MakeJob(JobDefOf.HaulToCell, victim, cellCandidates.RandomElement());
                job.count = 1;
                job.attackDoorIfTargetLost = true;
                return job;
            }

            Predicate<Thing> corpseValidator = delegate (Thing t)
            {
                var corpse = t as Corpse;
                return !hiveCells.Contains(corpse.Position) && corpse.InnerPawn.def != pawn.def;
            };

            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                    ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.ClosestTouch,
                    TraverseParms.For(pawn, Danger.None, TraverseMode.NoPassClosedDoors, false), 50f, corpseValidator, null);
            cellCandidates = hiveCells.Where(x => x.Walkable(pawn.Map) && !x.GetThingList(pawn.Map).Where(y => y is Corpse).Any() && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly));
            if (cellCandidates.Count() > 0 && thing != null && ReservationUtility.CanReserveAndReach(pawn, thing, PathEndMode.ClosestTouch, Danger.None))
            {
                Job job = JobMaker.MakeJob(JobDefOf.HaulToCell, thing, cellCandidates.RandomElement());
                job.count = 1;
                job.attackDoorIfTargetLost = true;
                return job;
            }

            return null;
        }
	}
}
