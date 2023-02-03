using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace OneBigFamily
{
    internal static class LovePartnerRelationUtility_Patch
    {
        /// <summary>
        /// So off-colony partners don't count when romancing
        /// </summary>
        [HarmonyPatch(typeof(LovePartnerRelationUtility), "ExistingMostLikedLovePartner")]
        public class ExistingMostLikedLovePartner
        {
            [HarmonyPostfix]
            public static void Postfix(ref Pawn __result)
            {
                if (__result == null) return;
                
                if (__result.IsWorldPawn() || !__result.relations.everSeenByPlayer)
                {
                    __result = null;
                }
            }
        }
    }
}