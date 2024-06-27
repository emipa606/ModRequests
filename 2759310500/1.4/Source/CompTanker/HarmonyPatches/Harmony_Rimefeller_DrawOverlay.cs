using HarmonyLib;
using JetBrains.Annotations;
using Rimefeller;

namespace CompTankerCompat.HarmonyPatches
{
    internal static class Harmony_Rimefeller_DrawOverlay
    {

        [UsedImplicitly]
        public static void Prefix(MapComponent_Rimefeller __instance)
        {
            if (!__instance.MarkTowersForDraw) return;

            foreach (var pipeNet in __instance.PipeNets)
            {
                foreach (var thing in pipeNet.PipedThings)
                {
                    var comp = thing.GetComp<CompTanker.CompTanker>();
                    if (comp != null) comp.drawOverlay = true;
                }
            }
        }

        public static void Patch(Harmony harmony)
        {
            harmony.Patch(typeof(MapComponent_Rimefeller).GetMethod(nameof(MapComponent_Rimefeller.MapComponentUpdate)),
                new HarmonyMethod(typeof(Harmony_Rimefeller_DrawOverlay).GetMethod(nameof(Prefix))));
        }
    }
}
