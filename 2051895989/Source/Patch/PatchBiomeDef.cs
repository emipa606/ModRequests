using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncidentTweaker.Patch
{
    [HarmonyPatch(typeof(BiomeDef), "CommonalityOfDisease")]
    class PatchBiomeDef
    {
        static void Postfix(ref float __result, IncidentDef diseaseInc)
        {
            IncidentTweak tweak = IncidentTweaker.settings.GetTweak(diseaseInc);
            if (tweak == null) return;

            __result *= tweak.baseChanceMultiplier;
        }
    }
}
