using HarmonyLib;
using Verse;

namespace MechPsyControlRange_Hydroxyapatite
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main()
        {
            var harmony = new Harmony("rimworld.hydroxyapatite.MechPsyControlRange");
            harmony.PatchAll();
        }
    }
}