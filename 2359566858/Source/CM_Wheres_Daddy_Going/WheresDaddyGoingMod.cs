using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Wheres_Daddy_Going
{
    public class WheresDaddyGoingMod : Mod
    {
        private static WheresDaddyGoingMod _instance;
        public static WheresDaddyGoingMod Instance => _instance;

        public WhereIsDad MemoriesOfDad => Current.Game.World.GetComponent<WhereIsDad>();

        public WheresDaddyGoingMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("CM_Wheres_Daddy_Going");
            harmony.PatchAll();

            _instance = this;
        }
    }
}
