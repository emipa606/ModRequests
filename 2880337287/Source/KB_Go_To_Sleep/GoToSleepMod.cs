using HarmonyLib;
using RimWorld;
using Verse;

namespace KB_Go_To_Sleep
{
    public class GoToSleepMod : Mod
    {
        private static GoToSleepMod _instance;
        public static GoToSleepMod Instance => _instance;

        public GoToSleepMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("KB_Go_To_Sleep");
            harmony.PatchAll();

            _instance = this;
        }
    }
}
