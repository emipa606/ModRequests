using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using Verse.Noise;

namespace BeautyStatMod
{
    [StaticConstructorOnStartup]
    public static class BeautyStatMods
    {
        
        static BeautyStatMods()
        {
            var harmony = new Harmony("beautystatmod.inertialmage.patch");
            harmony.PatchAll();

            
        }

    }

}
