using System;
using HarmonyLib;
using JetBrains.Annotations;
using Multiplayer.API;
using Verse;

namespace MiningYieldChance
{
    [UsedImplicitly]
    public class MiningYieldChanceMod : Mod
    {
        public MiningYieldChanceMod(ModContentPack content) : base(content)
        {
            if (MP.enabled) MP.RegisterAll();
            new Harmony("FusionGeneration.MiningYieldChanceMod").PatchAll();
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal class HotSwappableAttribute : Attribute
    { }
}