using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Tradeable_Trinkets
{
    public class TrinketsMod : Mod
    {
        private static TrinketsMod _instance;
        public static TrinketsMod Instance => _instance;

        public TrinketsMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("CM_Tradeable_Trinkets");
            harmony.PatchAll();

            _instance = this;
        }
    }
}
