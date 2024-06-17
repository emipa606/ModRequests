using HarmonyLib;
using Verse;

namespace PassionOnLevelUp
{
    [StaticConstructorOnStartup]
    public static class PassionOnLevelUpStartUp
    {
        static PassionOnLevelUpStartUp()
        {
            var harmony = new HarmonyLib.Harmony("CapatainaAndIgo.PassionOnLevelUp");
            harmony.PatchAll();
            Log.Message("Passion On Level Up Says Hello");
        }

    }
    
}