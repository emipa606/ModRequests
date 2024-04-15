using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;

namespace XenXP
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var instance = new Harmony("rimworld.xen.exp");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(SkillRecord))]
        [HarmonyPatch("Interval")]
        static class SkillRecord_Interval_Patch
        {
            static bool Prefix() => false;
        }
    }
}
