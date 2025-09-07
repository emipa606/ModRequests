using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TargetingModes
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            var harmony = new Harmony("XeoNovaDan.TargetingModes");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

    }

}
