using System;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;
using Verse.AI;

namespace patch_ApparelLayer
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = HarmonyInstance.Create("com.github.harmony.rimworld.mod.example");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(WindowStack))]
    [HarmonyPatch("Add")]
    [HarmonyPatch(new Type[] { typeof(Window) })]
    class Patch
    {
        static void Prefix(Window window)
        {
            Log.Warning("Window: " + window);
        }
    }
}