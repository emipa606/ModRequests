using HarmonyLib;
using Verse;

namespace VAGUE
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.VAGUE");
            harmony.PatchAll();
        }
    }
}
