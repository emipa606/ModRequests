using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace NMLDT
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("Jdalt.RimWorld.NMLDT.Main");
            harmony.Patch(typeof(Pawn_HealthTracker).GetMethod("ShouldBeDeadFromLethalDamageThreshold"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(LDTCorrection)));
        }
        public static void LDTCorrection(ref bool __result)
        {
            __result = false;
            return;
        }
    }
}
