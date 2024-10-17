using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace Metro
{
	public class JobGiver_Mate : ThinkNode_JobGiver
	{
        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.gender != Gender.Male || !pawn.ageTracker.CurLifeStage.reproductive)
            {
                return null;
            }
            if (!pawn.InSafeCondition()) return null;
            var cellsForMating = pawn.GetHiveCells();
            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn3 = t as Pawn;
                if (pawn3.Downed)
                {
                    return false;
                }
                if (!pawn3.CanCasuallyInteractNow() || pawn3.IsForbidden(pawn))
                {
                    return false;
                }
                if (pawn3.Faction != pawn.Faction)
                {
                    return false;
                }
                if (!cellsForMating.Contains(pawn3.Position))
                {
                    return false;
                }
                return PawnUtility.FertileMateTarget(pawn, pawn3) ? true : false;
            };
            Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(pawn.def), PathEndMode.Touch, TraverseParms.For(pawn), 50f, validator);
            if (pawn2 == null)
            {
                return null;
            }
            return JobMaker.MakeJob(JobDefOf.Mate, pawn2);
        }
	}
}
