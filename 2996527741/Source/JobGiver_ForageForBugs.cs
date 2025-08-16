using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
/*
namespace Entomophagy
{
    class JobGiver_ForageForBugs : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (HealthAIUtility.ShouldSeekMedicalRest(pawn) 
                || !JoyUtility.EnjoyableOutsideNow(pawn, null) 
                || pawn.needs?.food?.CurLevelPercentage >= .8f 
                || pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(Entomophagy_DefOf.RecentlyForaged) != null)
            {
                return null;
            }
            IntVec3 c;
            if (!RCellFinder.TryFindSkygazeCell(pawn.Position, pawn, out c))
            {
                return null;
            }
            return JobMaker.MakeJob(Entomophagy_DefOf.ForageForBugs, c);
        }
    }
}
*/