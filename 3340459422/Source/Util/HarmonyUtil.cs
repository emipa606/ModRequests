// Modified by llunak, l.lunak@centrum.cz .

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace SR.ModRimWorld.RaidExtension.Util
{
    public class HarmonyUtil
    {
        //判断该地图是否为SOS2的太空地图。
        public static bool IsSOS2SpaceMap(Map map)
        {
            var traverse = Traverse.Create(map);
            var isSpaceMethod = traverse.Method("IsSpace");
            if (isSpaceMethod.MethodExists() && (bool)isSpaceMethod.GetValue())
            {
                return true;
            }
            else if (map.Biome.defName.Contains("OuterSpace"))
            {
                return true;
            }
            return false;
        }

        //获取玩家财富值最高的地图。SOS2的太空地图会被排除。
        public static Map GetPlayerMainColonyMapSOS2Excluded()
        {
            var allPlayerHomes = (from x in Find.Maps
                                  where x.IsPlayerHome
                                  select x).ToList();

            var allNonSpaceMaps = new List<Map>();
            foreach (var map in allPlayerHomes)
            {
                if (IsSOS2SpaceMap(map) == false)
                {
                    allNonSpaceMaps.Add(map);
                }
            }

            if (!allNonSpaceMaps.Any())
            {
                return null;
            }

            return allNonSpaceMaps.OrderByDescending(map => map.PlayerWealthForStoryteller).First();
        }
    }
}
