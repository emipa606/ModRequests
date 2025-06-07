using System;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Message2Letter
{
    public class Main : Mod
    {
        public Main(ModContentPack contentPack) : base(contentPack)
        {
            Harmony harmony = new Harmony("okthisismyidnow.Message2Letter.Specific");
            harmony.PatchAll();
        }
    }
}
