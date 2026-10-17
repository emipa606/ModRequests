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
using Verse.AI.Group;

namespace SR.ModRimWorld.RaidExtension
{
    [UsedImplicitly]
    public class JobGiverFollowLogger : ThinkNode_JobGiver
    {
        readonly int maxFollowRadius = 20;

        protected override Job TryGiveJob(Pawn pawn)
        {
            // No tree to cut? Try to follow a pawn that has a tree to cut, so that this pawn
            // is not idling somewhere far from pawns cutting.
            Pawn followPawn = GetPawnToFollow(pawn);
            if( followPawn == null )
                return null;
            if( !pawn.pather.Moving && pawn.Position.DistanceTo( followPawn.Position ) <= maxFollowRadius )
                return null; // Do not repeatedly create a follow job that immediately ends.
            Job job = JobMaker.MakeJob(RimWorld.JobDefOf.FollowClose, followPawn);
            job.expiryInterval = 100;
            job.checkOverrideOnExpire = true;
            job.followRadius = maxFollowRadius;
            return job;
        }

        private Pawn GetPawnToFollow(Pawn pawn)
        {
            Lord lord = pawn.GetLord();
            LordJobLogging lordJob = lord?.LordJob as LordJobLogging;
            if( lordJob == null )
                return null;
            // First try if there is a cached value. This improves performance and also
            // saves switching between pawns, which would look odd.
            Pawn followPawn = lordJob.GetPawnToFollow( pawn );
            if( followPawn?.CurJob?.def == RimWorld.JobDefOf.CutPlant )
                return followPawn;

            float minDistance = float.MaxValue;
            for( int i = 0; i < lord.ownedPawns.Count; ++i )
            {
                Pawn otherPawn = lord.ownedPawns[ i ];
                if( otherPawn == pawn )
                    continue;
                if( otherPawn?.CurJob?.def != RimWorld.JobDefOf.CutPlant )
                    continue;
                float distance = pawn.Position.DistanceTo( otherPawn.Position );
                // Try to select a pawn which is close, but do not always select the closest one
                // (as that could be often the last cutter in the group, which will become followed
                // by all others).
                if( distance / 2 >= minDistance )
                    continue;
                if( followPawn != null && !Rand.Chance( 0.50f ))
                    continue;
                followPawn = otherPawn;
                minDistance = distance;
            }
            if( followPawn == null )
                return null;
            lordJob.SetPawnToFollow( pawn, followPawn );
            return followPawn;
        }

    }
}
