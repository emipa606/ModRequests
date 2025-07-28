using HarmonyLib;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace NoobertNebulous
{
    [HarmonyPatch(typeof(Lord), "Cleanup")]
    public static class Lord_Cleanup_Patch
    {
        public static void Postfix(Lord __instance)
        {
            if (NoobertNebulousStoryteller.StorytellerActive 
                && __instance.Map.lordManager.lords.Any(x => x.faction == __instance.faction) is false)
            {
                foreach (var colonist in __instance.Map.mapPawns.FreeColonistsSpawned)
                {
                    colonist.needs?.mood?.thoughts.memories.TryGainMemory(NN_DefOf.NN_PassedTest);
                }
            }
        }
    }
}
