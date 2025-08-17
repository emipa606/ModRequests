using System;
using System.Reflection;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using Verse.AI;
using System.Collections.Generic;

namespace BDsWeaponJamming
{
    public class HarmonyPatch
    {
        [StaticConstructorOnStartup]
        public static class HarmonyPatches
        {
            private static readonly Type patchType = typeof(HarmonyPatches);
            static HarmonyPatches()
            {
                Harmony harmony = new Harmony("BDsWeaponJam");

                harmony.Patch(AccessTools.Method(typeof(StatsReportUtility), "StatsToDraw", new Type[] { typeof(Thing) }), postfix: new HarmonyMethod(patchType, nameof(StatsToDraw_Postfix)));

                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            public static void StatsToDraw_Postfix(ref IEnumerable<StatDrawEntry> __result, Thing thing)
            {
                CompQuality compQuality = thing.TryGetComp<CompQuality>();
                DefModExtension_Jamming modExtension = thing.def.GetModExtension<DefModExtension_Jamming>();
                if (compQuality != null && (modExtension == null || !modExtension.disableJamming))
                {
                    __result = __result.AddItem(new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, "BDJ_WeaponJamChance".Translate().CapitalizeFirst(), BDJamMod.GetJamChance(thing, compQuality, modExtension).ToString("P"), "BDJ_WeaponJamChanceDesc".Translate(), 0));
                }
            }
        }
    }

    public class DefModExtension_Jamming : DefModExtension
    {
        public bool disableJamming = false;
        public float jamChanceMulti = 1;
        public float jamChancePostfix = 0;
        public bool NeverReloadWhenJam = false;
        public bool AlwaysReloadWhenJam = false;
    }
}
