// ******************************************************************
//       /\ /|       @file       JobGiverLogging.cs
//       \ V/        @brief      行为节点 伐木
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2021-06-19 12:00:02
//    *(__\_\        @Copyright  Copyright (c) 2021, Shadowrabbit
// ******************************************************************
// Modified by llunak, l.lunak@centrum.cz .

using JetBrains.Annotations;
using Verse;
using Verse.AI;

namespace SR.ModRimWorld.RaidExtension
{
    [UsedImplicitly]
    public class JobGiverLogging : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            var tree = pawn.FindTree();
            if( tree == null )
                return null;
            var job = JobMaker.MakeJob(RimWorld.JobDefOf.CutPlant, tree);
            job.checkOverrideOnExpire = true;
            job.expiryInterval = 100;
            job.expireRequiresEnemiesNearby = true;
            return job;
        }
    }
}