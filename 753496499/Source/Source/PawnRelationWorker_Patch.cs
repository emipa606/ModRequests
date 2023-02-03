using HarmonyLib;
using RimWorld;
using Verse;
// ReSharper disable UnusedMember.Global

namespace OneBigFamily
{
    internal static class PawnRelationWorker_Patch
    {
        // To check against relationships with colonists
        [HarmonyPatch(typeof(PawnRelationWorker), "BaseGenerationChanceFactor")]
        public class BaseGenerationChanceFactor
        {
            [HarmonyPrefix]
            public static bool Prefix(ref float __result, Pawn other)
            {
                if (ModBaseOneBigFamily.allowColonists) return true;
                
                // Added to prevent new random relationships to existing pawns 
                if (other.IsColonist && Current.ProgramState != ProgramState.Entry)
                {
                    __result = 0;
                    return false;
                }
                return true;
            }
        }
    }
}
