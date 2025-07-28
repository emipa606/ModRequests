using HarmonyLib;
using RimWorld;
using Verse;

namespace NoobertNebulous
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kill_Patch
    {
        public static void Postfix(Pawn __instance)
        {
            if (NoobertNebulousStoryteller.StorytellerActive && __instance.IsVidtuber())
            {
                foreach (var colonist in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
                {
                    if (colonist != __instance)
                    {
                        colonist.needs?.mood?.thoughts.memories.TryGainMemory(NN_DefOf.NN_InfluencerDied);
                    }
                }
            }
        }
    }
}
