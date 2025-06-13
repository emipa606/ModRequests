using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospitality;
using Hospitality.Utilities;
using Verse;

namespace Hospitality.Patches
{
    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch("FinalizeInit")]
    [HarmonyPatch(new Type[]
    {

    })]
    internal static class Game_FinalizeInit_Patch
    {
        [HarmonyPostfix]
        private static void WorldLoadedHook()
        {
            foreach (var map in Find.Maps)
            {
                map.GetMapComponent().OnWorldLoaded();
            }
            GuestUtility.Initialize();
        }
    }
}
