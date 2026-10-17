// ******************************************************************
//       /\ /|       @file       PawnExtension.cs
//       \ V/        @brief      角色扩展
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2021-07-11 12:17:00
//    *(__\_\        @Copyright  Copyright (c) 2021, Shadowrabbit
// ******************************************************************
// Modified by llunak, l.lunak@centrum.cz .

using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SR.ModRimWorld.RaidExtension
{
    public static class PawnExtension
    {
        /// <summary>
        /// 目标动物是否合法
        /// </summary>
        /// <param name="searcher"></param>
        /// <param name="target"></param>
        /// <param name="minTargetRequireHealthScale"></param>
        /// <returns></returns>
        public static bool IsTargetAnimalValid(this Pawn searcher, Thing target, float minTargetRequireHealthScale)
        {
            //目标是动物 没死 符合健康缩放 可保留 不是搜寻者派系的 
            return ThingValidator.IsAnimal(target, minTargetRequireHealthScale) && searcher.CanReserve(target) &&
                   searcher.Faction != null && (target.Faction == null || searcher.Faction != target.Faction);
        }

        /// <summary>
        /// 寻找最近的 满足体型的动物
        /// </summary>
        /// <param name="leader"></param>
        /// <param name="minTargetRequireHealthScale"></param>
        /// <returns></returns>
        public static Pawn FindTargetAnimal(this Pawn leader, float minTargetRequireHealthScale)
        {
            Map map = leader.Map;
            if( map == null )
                return null;
            return map.FindTargetAnimal(leader.Position, minTargetRequireHealthScale);
        }

        /// <summary>
        /// 寻找最近的成熟的树
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static Thing FindTree(this Pawn pawn)
        {
            //验证器 是植物 可以保留 没有燃烧中 角色不是什么树木爱好者 成熟了
            bool SpoilValidatorAny(Thing t) => ThingValidator.IsTree(t) && pawn.CanReserve(t)
                                            && PlantUtility.PawnWillingToCutPlant_Job(t, pawn);
            bool SpoilValidatorNotColony(Thing t) => SpoilValidatorAny(t)
                && t.Faction != Faction.OfPlayer && !t.Map.areaManager.Home[ t.Position ];

            // Try to cache the found tree (or nothing found), as this is a relatively expensive operation and
            // done repeatedly.
            Lord lord = pawn.GetLord();
            LordJobLogging lordJob = lord?.LordJob as LordJobLogging;
            int cacheTickTimeout = 60 * 60; // 60 seconds
            if( lordJob != null )
            {
                ( Thing tree, int tick ) = lordJob.GetTreeForPawn( pawn );
                if( Find.TickManager.TicksGame < tick + cacheTickTimeout )
                {
                    if( tree == null )
                        return null;
                    if( SpoilValidatorAny( tree )
                        && pawn.Map.reachability.CanReach(pawn.Position, tree.SpawnedParentOrMe, PathEndMode.ClosestTouch,
                            TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some)))
                    {
                        return tree;
                    }
                }
            }

            // First try to find a plant outside of the colony.
            var targetPlant = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map,
                pawn.Map.spawnedThings, PathEndMode.ClosestTouch,
                TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some), validator: SpoilValidatorNotColony);
            if( targetPlant == null )
            {
                // If that fails, any plant.
                targetPlant = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map,
                    pawn.Map.spawnedThings, PathEndMode.ClosestTouch,
                    TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some), validator: SpoilValidatorAny);
            }
            // Cache negative hits too, for when there are not enough trees. Ticks are primarily used to time out negative
            // hits, for cases like a tree was reserved by another pawn that got killed.
            if( lordJob != null )
                lordJob.SetTreeForPawn( pawn, targetPlant, Find.TickManager.TicksGame );
            return targetPlant;
        }

        /// <summary>
        /// 寻找战利品
        /// </summary>
        /// <param name="searcher"></param>
        /// <param name="root"></param>
        /// <param name="map"></param>
        /// <param name="maxDist"></param>
        /// <param name="disallowed"></param>
        /// <param name="validator"></param>
        /// <returns></returns>
        public static Thing TryFindBestSpoilsToTake(this Pawn searcher, IntVec3 root, Map map, float maxDist,
            ICollection<Thing> disallowed = null, Predicate<Thing> validator = null)
        {
            if (map == null)
            {
                return null;
            }

            if (searcher != null && !searcher.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return null;
            }

            //搜索者存在 但无法触碰地图边界 或者 搜索者不存在
            if (searcher != null && !map.reachability.CanReachMapEdge(searcher.Position,
                TraverseParms.For(searcher)) || (searcher == null && !map.reachability.CanReachMapEdge(root,
                TraverseParms.For(TraverseMode.PassDoors))))
            {
                return null;
            }

            //验证器 搜索者不存在 或者搜索者可以预留当前物体 并且没有禁用 并且物体可以被偷 并且物品周围有敌对派系尸体
            bool SpoilValidator(Thing t) => (searcher == null || searcher.CanReserve(t)) &&
                                            (disallowed == null || !disallowed.Contains(t)) &&
                                            (validator == null || validator(t));


            return GenClosest.ClosestThing_Regionwise_ReachablePrioritized(root, map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableEverOrMinifiable), PathEndMode.ClosestTouch,
                TraverseParms.For(TraverseMode.NoPassClosedDoors), maxDist, SpoilValidator,
                StealAIUtility.GetValue);
        }
    }
}