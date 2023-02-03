using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.Planet;

namespace YayoNature
{


    public static class dataUtility
    {
        public static readonly Dictionary<Map, mapData> dic_map = new Dictionary<Map, mapData>();
        public static readonly Dictionary<World, worldData> dic_world = new Dictionary<World, worldData>();

        public static mapData GetData(Map key)
        {
            if (!dic_map.ContainsKey(key)) dic_map[key] = new mapData();
            dic_map[key].setParent(key);
            return dic_map[key];
        }

        public static void Remove(Map key)
        {
            dic_map.Remove(key);
        }


        public static worldData GetData(World key)
        {
            if (!dic_world.ContainsKey(key)) dic_world[key] = new worldData();
            dic_world[key].setParent(key);
            return dic_world[key];
        }

        public static void Remove(World key)
        {
            dic_world.Remove(key);
        }

    }


}
