using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace OneBigFamily
{
    internal static class InteractionWorker_RomanceAttempt_Patch
    {
        /// <summary>
        /// So off-colony relationships count less against romance
        /// </summary>
        [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "SuccessChance")]
        public class SuccessChance
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn recipient, ref float __result)
            {
                var pawn = GetRelation(recipient);
                if (pawn == null) return;

                if (pawn.IsWorldPawn() || !pawn.relations.everSeenByPlayer)
                {
                    __result *= 3;
                    __result = Mathf.Clamp01(__result);
                }
            }

            private static Pawn GetRelation(Pawn recipient)
            {
                var pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);
                if (pawn != null && !pawn.Dead) return pawn;
                pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
                if (pawn != null && !pawn.Dead) return pawn;
                foreach (var spouse in recipient.GetSpouses(false))
                {
                    pawn = spouse;
                    if (pawn != null && !pawn.Dead) return pawn;
                }
                return null;
            }
        }
    }
}