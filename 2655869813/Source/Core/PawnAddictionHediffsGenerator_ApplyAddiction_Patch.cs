using HarmonyLib;
using RimWorld;
using Verse;

namespace BenLubarsVanillaExpandedPatches
{
    [HarmonyPatch(typeof(PawnAddictionHediffsGenerator), "ApplyAddiction")]
    static class PawnAddictionHediffsGenerator_ApplyAddiction_Patch
    {
        public static bool Prefix(Pawn pawn, ChemicalDef chemicalDef)
        {
            return !BenLubarsVanillaExpandedPatches.noRoyalJellyRefugees || pawn.kindDef.defName != "Refugee" || chemicalDef.defName != "VFEI_RoyalJellyChemical";
        }
    }
}
