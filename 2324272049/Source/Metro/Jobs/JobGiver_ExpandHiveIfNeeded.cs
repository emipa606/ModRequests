using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Metro
{
    public class JobGiver_ExpandHiveIfNeeded : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (GridsUtility.Fogged(pawn.Position, pawn.Map))
            {
                return JobMaker.MakeJob(JobDefOf.LayDown);
            }

            if (pawn.GroupNeedsExpansion() && pawn.InSafeCondition())
            {
                if (pawn.TryFindClosestMountainToExpand(out Mineable rock))
                {
                    var mapComp = pawn.Map.GetComponent<HiveAIManager>();
                    mapComp.rocksToBeMined[rock] = pawn;
                    return JobMaker.MakeJob(JobDefOf.AttackMelee, rock);
                }
                if (pawn.TryFindClosestMountainToMine(out Mineable rock2))
                {
                    var mapComp = pawn.Map.GetComponent<HiveAIManager>();
                    mapComp.rocksToBeMined[rock2] = pawn;
                    return JobMaker.MakeJob(JobDefOf.AttackMelee, rock2);
                }
            }
            return null;
        }
    }
}

