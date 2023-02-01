using HarmonyLib;
using Verse;

namespace PawnStorages
{

    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        public static Harmony harmonyInstance;
        static HarmonyInit()
        {
            harmonyInstance = new Harmony("PawnStorages.Mod");
            harmonyInstance.PatchAll();
        }
    }
}
