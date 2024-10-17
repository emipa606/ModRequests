using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Metro
{
    public class JobGiver_EntagleClosestPawnWithCocoon : ThinkNode_JobGiver
    {
        private readonly float distanceToReact = 50f;
        public override Job TryGiveJob(Pawn pawn)
        {
            if (GridsUtility.Fogged(pawn.Position, pawn.Map))
            {
                return JobMaker.MakeJob(JobDefOf.LayDown);
            }

            if (!pawn.InSafeCondition()) return null;
            var cells = pawn.GetMinedOutHiveCells();
            var cocoons = pawn.Map.listerThings.ThingsOfDef(MetroDefOf.Metro_Cocoon).Where(x => !cells.Contains(x.Position) 
            && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly) && x is Cocoon cocoon && cocoon.ContainedThing != null);
            if (cocoons.Count() > 0)
            {
                Job job = JobMaker.MakeJob(JobDefOf.HaulToCell, cocoons.RandomElement(), cells.Where(x => pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly)).RandomElement());
                job.count = 1;
                return job;
            }

            Pawn pawn2 = pawn.FindPawnTarget(distanceToReact);
            if (pawn2 != null && pawn2.Downed)
            {
                Job job = JobMaker.MakeJob(MetroDefOf.Metro_EntangleTargetWithCocoon, pawn2);
                job.count = 1;
                return job;
            }
            else
            {
                Corpse corpse = pawn.FindCorpseTarget(distanceToReact);
                if (corpse != null)
                {
                    Job job = JobMaker.MakeJob(MetroDefOf.Metro_EntangleTargetWithCocoon, corpse);
                    job.count = 1;
                    return job;
                }
            }
            return null;
        }
    }
}

