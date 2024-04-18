using System.Reflection;
using Verse;

namespace WildPlantPicker.HarmonyPatches
{
    [StaticConstructorOnStartup]
    internal static class Initializer
    {
        internal static void Initialize()
        {
            var harmony = new HarmonyLib.Harmony("miyamiya.WildPlantPicker");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
