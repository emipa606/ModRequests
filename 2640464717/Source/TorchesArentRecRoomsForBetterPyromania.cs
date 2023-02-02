using HarmonyLib;
using Verse;

namespace TorchesArentRecRoomsForBetterPyromania
{
    [StaticConstructorOnStartup]
    public static class TorchesArentRecRoomsForBetterPyromania
    {
        static TorchesArentRecRoomsForBetterPyromania()
        {
            var harmony = new Harmony("me.lubar.TorchesArentRecRooms");
            harmony.PatchAll();
        }
    }
}
