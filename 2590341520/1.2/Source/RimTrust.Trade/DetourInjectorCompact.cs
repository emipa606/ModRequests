using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimTrust.Trade
{
    [StaticConstructorOnStartup]
    internal static class DetourInjectorCompact
    {
        static DetourInjectorCompact()
        {
            var harmony = new Harmony("aeonamber.RimTrust");
            //Harmony.DEBUG = true;
            harmony.PatchAll();
        }
    }   

}