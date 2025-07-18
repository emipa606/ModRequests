using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncidentTweaker.Patch
{

    [HarmonyPatch(typeof(IncidentWorker), "BaseChanceThisGame", MethodType.Getter)]
    class PatchIncidentWorkerBaseChanceThisGame
    {
        static void Postfix(ref float __result, IncidentWorker __instance)
        {
            IncidentTweak tweak = IncidentTweaker.settings.GetTweak(__instance.def);
            if (tweak == null) return;

            __result *= tweak.baseChanceMultiplier;
        }
    }
}
