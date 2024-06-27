using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Patch_Undraft
{
    [HarmonyPatch(typeof(AutoUndrafter), "ShouldAutoUndraft")]
    static class StopAutoUndraft
    {
        [HarmonyPrefix]
        static bool StopAutoUndrafts(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}