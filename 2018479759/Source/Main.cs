using HarmonyLib;
using Verse;

namespace CloseAll
{
    public class Main: Mod
    {
        public Main(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("se.gorymoon.rimworld.mod.closeall");
            harmony.PatchAll();
        }
    }
}