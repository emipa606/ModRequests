// ******************************************************************
//       /\ /|       @file       LordToilLogging.cs
//       \ V/        @brief      集群AI流程 伐木
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2021-06-17 23:51:05
//    *(__\_\        @Copyright  Copyright (c) 2021, Shadowrabbit
// ******************************************************************
// Modified by llunak, l.lunak@centrum.cz .

using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SR.ModRimWorld.RaidExtension
{
    public class LordToilLogging : LordToil
    {
        /// <summary>
        /// 更新职责
        /// </summary>
        public override void UpdateAllDuties()
        {
            foreach (var pawn in lord.ownedPawns)
            {
                pawn.mindState.duty = new PawnDuty(DutyDefOf.SrLogging);
            }
        }


        public override void Notify_PawnJobDone(Pawn p, JobCondition condition)
        {
            if( condition != JobCondition.Succeeded || p.CurJob?.def != RimWorld.JobDefOf.CutPlant )
                return;
            LordJobLogging logging = lord.LordJob as LordJobLogging;
            logging?.NotifyTreeCut();
        }
    }
}