using HarmonyLib;
using RimWorld;
using VanillaSocialInteractionsExpanded;
using Verse;

namespace VSIERationalTraitDevelopment
{
    [HarmonyPatch(typeof(MarriageCeremonyUtility), nameof(MarriageCeremonyUtility.Married))]
    public static class MarriageCeremonyUtility_Married_Patch
    {
        public static void Postfix(Pawn firstPawn, Pawn secondPawn)
        {
            VSIE_Utils.TryDevelopNewTrait(firstPawn, VSIE_Extra_DefOf.VSIE_MarriedTraitChange.key, 0.1f);
            VSIE_Utils.TryDevelopNewTrait(secondPawn, VSIE_Extra_DefOf.VSIE_MarriedTraitChange.key, 0.1f);
        }
    }
}
