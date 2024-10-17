using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Metro
{
    public class JobGiver_AvoidSunLight : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (GridsUtility.Fogged(pawn.Position, pawn.Map))
            {
                return JobMaker.MakeJob(JobDefOf.LayDown);
            }

            if (!MetroUtils.IsNightNow(pawn.Map) && !pawn.Position.Roofed(pawn.Map) && 
                (!pawn.HarmedRecently() && MetroUtils.IsDayOrClose(pawn.Map) 
                || pawn.UrgentlyNeedsToRunAwayFromSunLight()))
            {
                if (pawn.TryFindClosestMountainRoof(out IntVec3 spot) && pawn.Position != spot)
                {
                    return JobMaker.MakeJob(JobDefOf.Goto, spot);
                }
                else if (pawn.TryFindClosestMountainToMine(out Mineable rock))
                {
                    var mapComp = pawn.Map.GetComponent<HiveAIManager>();
                    mapComp.rocksToBeMined[rock] = pawn;
                    return JobMaker.MakeJob(JobDefOf.AttackMelee, rock);
                }
            }
            return null;
        }
    }
}

