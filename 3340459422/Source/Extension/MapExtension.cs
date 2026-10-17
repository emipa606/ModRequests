// ******************************************************************
//       /\ /|       @file       MapExtension.cs
//       \ V/        @brief      地图扩展
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2021-07-27 11:46:48
//    *(__\_\        @Copyright  Copyright (c) 2021, Shadowrabbit
// ******************************************************************
// Modified by llunak, l.lunak@centrum.cz .

using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace SR.ModRimWorld.RaidExtension
{
    public static class MapExtension
    {
        /// <summary>
        /// 当前地图上是否存在可以列为目标的动物
        /// </summary>
        /// <param name="map"></param>
        /// <param name="minTargetRequireHealthScale">健康缩放最小需求 用来判断动物强度</param>
        /// <returns></returns>
        public static bool IsAnimalTargetExist(this Map map, float minTargetRequireHealthScale)
        {
            return map.mapPawns != null && Enumerable.Any(map.mapPawns.AllPawnsSpawned,
                (Thing t) => ThingValidator.IsAnimal(t, minTargetRequireHealthScale));
        }

        /// <summary>
        /// 寻找最近的 满足体型的动物
        /// </summary>
        /// <param name="minTargetRequireHealthScale"></param>
        /// <returns></returns>
        public static Pawn FindTargetAnimal(this Map map, IntVec3 position, float minTargetRequireHealthScale)
        {
            //验证器
            bool SpoilValidatorAny(Thing t) => ThingValidator.IsAnimal(t, minTargetRequireHealthScale);
            bool SpoilValidatorNotColony(Thing t) => SpoilValidatorAny(t)
                && t.Faction != Faction.OfPlayer && !map.areaManager.Home[ t.Position ];

            // First try to find a non-colony animal outside the home area.
            var targetThing = GenClosest.ClosestThing_Global_Reachable(position, map,
                map.mapPawns.AllPawnsSpawned, PathEndMode.ClosestTouch,
                TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some), validator: SpoilValidatorNotColony);
            if( targetThing != null )
                return (Pawn) targetThing;
            // If that fails, any animal.
            targetThing = GenClosest.ClosestThing_Global_Reachable(position, map,
                map.mapPawns.AllPawnsSpawned, PathEndMode.ClosestTouch,
                TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some), validator: SpoilValidatorAny);

            return (Pawn) targetThing;
        }

        /// <summary>
        /// 当前地图是否存在树
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static bool IsTreeExist(this Map map)
        {
            return map.spawnedThings != null && map.spawnedThings.Any(ThingValidator.IsTree);
        }

        public static bool IsEnoughNonColonyTrees(this Map map, int limit)
        {
            if( map.spawnedThings == null )
                return false;
            int count = 0;
            foreach(Thing t in map.spawnedThings)
            {
                if( !ThingValidator.IsTree( t ))
                    continue;
                if( t.Faction == Faction.OfPlayer || t.Map.areaManager.Home[ t.Position ] )
                    continue;
                ++count;
                if( count >= limit )
                    return true;
            }
            return false;
        }
    }
}
