using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Metro
{
    public class JobGiver_LayWeb : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (GridsUtility.Fogged(pawn.Position, pawn.Map))
            {
                return JobMaker.MakeJob(JobDefOf.LayDown);
            }
            if (!pawn.InSafeCondition()) return null;

            var cells = pawn.GetHiveCells();
            if (cells != null)
            {
                Predicate<IntVec3> predicate = delegate (IntVec3 x)
                {
                    if (!x.Walkable(pawn.Map)) return false;
                    if (!pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly)) return false;
                    if (x.GetThingList(pawn.Map).Where(t => t.def == MetroDefOf.Metro_Web).Count() > 3) return false;
                    return true;
                };
                var cellsCandidates = cells.Where(x => predicate(x));
                if (cellsCandidates.Count() > 0)
                {
                    return JobMaker.MakeJob(MetroDefOf.Metro_LayWeb, cellsCandidates.RandomElement());
                }
            }
            return null;
        }

    }
}

