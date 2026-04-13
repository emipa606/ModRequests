using HarmonyLib;
using RimWorld;
using System.Text;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(MassUtility), nameof(MassUtility.Capacity))]
    public static class MassUtility_Capacity_Patch
    {
        private static void Postfix(ref float __result, Pawn p, StringBuilder explanation = null)
        {
            if (p.Wears(BionicleDefOf.BKMOP_Pakari, out var apparel) && apparel.IsMasterworkOrLegendary())//doubles carry capacity with pakari
            {
                __result *= 2f;
            }
        }
    }
}