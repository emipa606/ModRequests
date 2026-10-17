// ******************************************************************
//       /\ /|       @file       RaidStrategyWorkerLogging.cs
//       \ V/        @brief      袭击策略 伐木
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2021-06-17 19:05:27
//    *(__\_\        @Copyright  Copyright (c) 2021, Shadowrabbit
// ******************************************************************
// Modified by llunak, l.lunak@centrum.cz .

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace SR.ModRimWorld.RaidExtension
{
    [UsedImplicitly]
    public class RaidStrategyWorkerLogging : RaidStrategyWorker
    {
        private bool isSurprise = false;

        /// <summary>
        /// 该策略适用于
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="groupKind"></param>
        /// <returns></returns>
        public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            return base.CanUseWith(parms, groupKind) && parms.faction != null && parms.faction != Faction.OfMechanoids;
        }

        public override bool CanUsePawnGenOption(float pointsTotal, PawnGenOption g, List<PawnGenOptionWithXenotype> chosenGroups, Faction faction = null)
        {
            if( !g.kind.RaceProps.Humanlike )
                return false;
            return base.CanUsePawnGenOption( pointsTotal, g, chosenGroups, faction );
        }

        public override bool CanUsePawn(float pointsTotal, Pawn p, List<Pawn> otherPawns)
        {
            if( !CanCutTreesReasonablyFast( p )) // Avoids pawns that possibly take very long to cut a tree.
                return false;
            return base.CanUsePawn( pointsTotal, p, otherPawns );
        }

        // Pretty much SappersUtility.CanMineReasonablyFast().
        private bool CanCutTreesReasonablyFast(Pawn p)
        {
            if (p.RaceProps.Humanlike && !p.skills.GetSkill(SkillDefOf.Plants).TotallyDisabled && !StatDefOf.PlantWorkSpeed.Worker.IsDisabledFor(p))
            {
                return p.skills.GetSkill(SkillDefOf.Plants).Level >= 4;
            }
            return false;
        }

        public override List<Pawn> SpawnThreats(IncidentParms parms)
        {
            ResolveLateInfo(parms);
            return base.SpawnThreats(parms);
        }

        private void ResolveLateInfo(IncidentParms parms)
        {
            isSurprise = this is RaidStrategyWorkerLoggingSurprise;
            if( isSurprise )
                parms.points *= 0.8f;
            else
            {
                // If the loggers will need to attack the colony, reduce raid size (same logic as with poaching).
                // It is not known how many pawns will be in this raid at this point (since that depends
                // on raid points), so let's say at least 50 trees is enough.
                if (parms.target is Map map && !map.IsEnoughNonColonyTrees( 50 ))
                    parms.points *= 0.8f;
            }
            parms.points = Math.Clamp( parms.points, MiscDef.MinThreatPoints, MiscDef.MaxThreatPoints );
        }

        /// <summary>
        /// 创建集群AI工作
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="map"></param>
        /// <param name="pawns"></param>
        /// <param name="raidSeed"></param>
        /// <returns></returns>
        protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
        {
            var siegePositionFrom =
                RCellFinder.FindSiegePositionFrom(parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld,
                    map);
            return new LordJobLogging(siegePositionFrom, isSurprise);
        }
    }

    public class RaidStrategyWorkerLoggingSurprise : RaidStrategyWorkerLogging
    {
    }
}
