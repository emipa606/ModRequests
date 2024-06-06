using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FF_Drugs
{
    [HarmonyPatch(typeof(PawnCapacityWorker_BloodFiltration), "CalculateCapacityLevel")]
    internal static class Ectostasis_BloodFiltration
    {
        internal static void Postfix(HediffSet diffSet, ref float __result)
        {
            if (diffSet.HasHediff(RemediesDefOf.FF_EctostasisHigh))
                __result = 999f;
        }

    }
    [HarmonyPatch(typeof(PawnCapacityWorker_BloodPumping), "CalculateCapacityLevel")]
    internal static class Ectostasis_BloodBumping
    {
        internal static void Postfix(HediffSet diffSet, ref float __result)
        {
            if (diffSet.HasHediff(RemediesDefOf.FF_EctostasisHigh))
                __result = 999f;
        }

    }
    [HarmonyPatch(typeof(PawnCapacityWorker_Metabolism), "CalculateCapacityLevel")]
    internal static class Ectostasis_Metabolism
    {
        internal static void Postfix(HediffSet diffSet, ref float __result)
        {
            if (diffSet.HasHediff(RemediesDefOf.FF_EctostasisHigh))
                __result = 999f;
        }

    }
    [HarmonyPatch(typeof(PawnCapacityWorker_Breathing), "CalculateCapacityLevel")]
    internal static class Ectostasis_Breathing
    {
        internal static void Postfix(HediffSet diffSet, ref float __result)
        {
            if (diffSet.HasHediff(RemediesDefOf.FF_EctostasisHigh))
                __result = 999f;
        }

    }
}
