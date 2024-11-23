using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{

    [StaticConstructorOnStartup]
    static class Main
    {

        public const string Id = "Sielfyr.IHaveADream";
        public const string ModName = "Dream And Desire";
        public const string Version = "0.2.1";
        static Main()
        {
            var harmony = new Harmony(Id);
            harmony.PatchAll();
            Log.Message("Initialized " + ModName + " v" + Version); 

        }
    }
}
