using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace KenshiCuisine
{
    public class KenshiCuisineMod : Mod
    {
        private const string modId = "Super.Kenshicuisine";
        Harmony Harmony { get; }
        public KenshiCuisineMod(ModContentPack content) : base(content)
        {
            Harmony = new Harmony(modId);

            var method = AccessTools.Method(typeof(Designator_ZoneAdd_Growing), nameof(Designator.CanDesignateCell), new Type[] { typeof(IntVec3) });
            var patch = new HarmonyMethod(typeof(KenshiCuisineMod), nameof(Patch_Designator));
            Harmony.Patch(method, postfix: patch);
        }

        static void Patch_Designator(IntVec3 c, Designator __instance, ref AcceptanceReport __result)
        {
            float num = ThingDefOf.Plant_Super_PricklyPear.plant.fertilityMin;
            __result = __result || (__instance.Map.fertilityGrid.FertilityAt(c) >= num);
        }
    }
}
