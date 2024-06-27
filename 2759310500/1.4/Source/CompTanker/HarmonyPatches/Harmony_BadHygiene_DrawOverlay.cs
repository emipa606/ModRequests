using HarmonyLib;
using JetBrains.Annotations;
using System.Linq;
using DubsBadHygiene;

namespace CompTankerCompat.HarmonyPatches
{
    internal static class Harmony_BadHygiene_DrawOverlay
    {
        [UsedImplicitly]
        public static void Prefix(MapComponent_Hygiene __instance)
        {
            if (!__instance.MarkTowersForDraw) return;

            foreach (var pipeNet in __instance.PipeComp.PipeNets)
            {
                foreach (var thing in pipeNet.PipedThings.OrderBy(x => x.thingIDNumber))
                {
                    var comp = thing.GetComp<CompTanker.CompTanker>();
                    if (comp != null) comp.drawOverlay = true;
                }
            }
        }

        public static void Patch(Harmony harmony)
        {
            harmony.Patch(typeof(MapComponent_Hygiene).GetMethod(nameof(MapComponent_Hygiene.MapComponentUpdate)),
                new HarmonyMethod(typeof(Harmony_BadHygiene_DrawOverlay).GetMethod(nameof(Prefix))));
        }
    }
}
