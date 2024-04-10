using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace UglyTogether
{
    [HarmonyPatch(typeof(ThoughtWorker_Ugly))]
    [HarmonyPatch("CurrentSocialStateInternal", new[] { typeof(Pawn), typeof(Pawn) })]
    public static class Patch_ThoughtWorker_Ugly__CurrentSocialStateInternal
    {
        public static void Postfix(ref ThoughtState __result, ref Pawn pawn)
        {
            if(!__result.Active)
            {
                return;
            }

            float statValue = pawn.GetStatValue(StatDefOf.PawnBeauty, true, -1);
            if(statValue < 0f)
            {
                __result = ThoughtState.Inactive;
            }
        }
    }
}
