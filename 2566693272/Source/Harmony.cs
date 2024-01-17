using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;

namespace PrisonWalls
{
    [StaticConstructorOnStartup]
    public static class StartUp
    {
        static StartUp()
        {
            var harmony = new Harmony("com.example.patch");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(RegionTypeUtility), "GetExpectedRegionType")]
    class GetExpectedRegionTypePatch
    {
        public static void Postfix(ref RegionType __result, IntVec3 c, Map map)
        {
            if (__result != RegionType.None)
            {
                List<Thing> thingList = c.GetThingList(map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i] is Building_PrisonBar)
                    {
                        __result = RegionType.None;
                    }
                }
            }

        }
    }
}
