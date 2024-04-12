using HarmonyLib;
using Verse;

namespace CustomSchedules
{

    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        public static Harmony harmonyInstance;
        static HarmonyInit()
        {
            harmonyInstance = new Harmony("CustomSchedules.Mod");
            harmonyInstance.PatchAll();

            CSDefOf.CS_SchedA.label = CSModStarter.settings.scheduleNameA;
            CSDefOf.CS_SchedB.label = CSModStarter.settings.scheduleNameB;
            CSDefOf.CS_SchedC.label = CSModStarter.settings.scheduleNameC;
            CSDefOf.CS_SchedD.label = CSModStarter.settings.scheduleNameD;
            CSDefOf.CS_SchedE.label = CSModStarter.settings.scheduleNameE;
            CSDefOf.CS_SchedF.label = CSModStarter.settings.scheduleNameF;
        }
    }
}
