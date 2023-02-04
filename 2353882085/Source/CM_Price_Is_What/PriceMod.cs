using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Price_Is_What
{
    public class PriceMod : Mod
    {
        private static PriceMod _instance;
        public static PriceMod Instance => _instance;

        public PriceMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("CM_Price_Is_What");
            harmony.PatchAll();

            _instance = this;
        }
    }
}
