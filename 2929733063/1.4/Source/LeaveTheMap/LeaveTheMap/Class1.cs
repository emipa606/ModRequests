using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace LeaveTheMap
{
    [StaticConstructorOnStartup]
    public static class Class1
    {
        static Class1()
        {
            new Harmony("LeaveTheMap.Mod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(ExitMapGrid), "get_MapUsesExitGrid")]
    public static class ExitMapGrid_MapUsesExitGrid_Patch
    {
        public static void Postfix(Map ___map, ref bool __result)
{
            if (___map.IsPlayerHome)
            {
                __result = true;
            }
        }
    }
}
