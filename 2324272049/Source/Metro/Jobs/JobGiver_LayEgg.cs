using RimWorld;
using System;
using System.Linq;
using Verse;
using Verse.AI;

namespace Metro
{
	public class JobGiver_LayEgg : ThinkNode_JobGiver
	{
		private const float LayRadius = 5f;
		public override Job TryGiveJob(Pawn pawn)
		{
			CompEggLayer compEggLayer = pawn.TryGetComp<CompEggLayer>();
			if (compEggLayer == null || !compEggLayer.CanLayNow)
			{
				return null;
			}

            if (!pawn.InSafeCondition()) return null;

            var cells = pawn.GetHiveCells();
            if (cells != null)
            {
                Predicate<IntVec3> predicate = delegate (IntVec3 x)
                {
                    if (!x.Walkable(pawn.Map)) return false;
                    if (!pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly)) return false;
                    if (x.GetThingList(pawn.Map).Where(t => t.def == MetroDefOf.Metro_Web).Count() > 0) return false;
                    if (x.GetThingList(pawn.Map).Where(t => t.def == compEggLayer.Props.eggFertilizedDef).Count() > 0) return false;
                    return true;
                };
                var cellsCandidates = cells.Where(x => predicate(x));
                if (cellsCandidates.Count() > 0)
                {
                    return JobMaker.MakeJob(JobDefOf.LayEgg, cellsCandidates.RandomElement());
                }
            }
            return null;
		}
	}
}
