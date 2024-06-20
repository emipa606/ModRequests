using HarmonyLib;
using InfiniteReinforce;
using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using static InfiniteReinforce.Building_Reinforcer;

namespace InfiniteReinforceUpgradeWeaponQuality
{
    [DefOf]
    public static class ReinforceDefOf
    {
        public static ReinforceDef Reinforce_QualityOffset;
    }

    [HarmonyPatch(typeof(WindowStack), "Add")]
    public static class WindowStack_Add_Patch
    {
        public static bool patched;

        public static void Postfix(Window window)
        {
            if (!patched && window is Dialog_Reinforcer)
            {
                Startup.harmony.Patch(typeof(Dialog_Reinforcer).GetMethod("BuildCostList"),
                    postfix: new HarmonyMethod(typeof(WindowStack_Add_Patch), nameof(BuildCostListPostfix)));
                Startup.harmony.Patch(typeof(Dialog_Reinforcer).GetMethod("Reinforce"),
                    postfix: new HarmonyMethod(typeof(WindowStack_Add_Patch), nameof(ReinforcePostfix)));
                Startup.harmony.Patch(typeof(ReinforceInstance).GetMethod("CleanUp"),
                    postfix: new HarmonyMethod(typeof(WindowStack_Add_Patch), nameof(CleanUpPostfix)));
                patched = true;
            }
        }

        private static void CleanUpPostfix()
        {
            var window = Find.WindowStack.WindowOfType<Dialog_Reinforcer>();
            if (window != null)
            {
                window.BuildCostList();
            }
        }

        private static void ReinforcePostfix(Dialog_Reinforcer __instance)
        {
            __instance.BuildCostList();
        }

        private static void BuildCostListPostfix(Dialog_Reinforcer __instance)
        {
            var qualityOffsetAmount = __instance.Instance.reinforcementqueue.Count(x => x.reinforcedef == ReinforceDefOf.Reinforce_QualityOffset);
            if (qualityOffsetAmount > 0)
            {
                var list = __instance.costlist[(int)CostMode.Material];
                for (var i = 0; i < list.Count; i++)
                {
                    list[i] = new ThingDefCountClass(list[i].thingDef, list[i].count * (3 * qualityOffsetAmount));
                }
            }
        }
    }

    public class ReinforceWorker_QualityOffset : ReinforceWorker
    {
        public override bool Appliable(ThingWithComps thing)
        {
            return thing.GetComp<CompQuality>() is CompQuality compQuality && compQuality.Quality < QualityCategory.Legendary;
        }

        public override Func<bool> Reinforce(ThingComp_Reinforce comp, int level)
        {
            return delegate ()
            {
                bool res = comp.ReinforceCustom(def, level);
                var compQuality = comp.parent.TryGetComp<CompQuality>();
                if (compQuality != null && compQuality.Quality < QualityCategory.Legendary)
                {
                    compQuality.SetQuality(compQuality.Quality + 1, ArtGenerationContext.Colony);
                }
                return res;
            };
        }
    }



    [StaticConstructorOnStartup]
    public static class Startup
    {
        public static Harmony harmony;
        static Startup()
        {
            harmony = new Harmony("InfiniteReinforceUpgradeWeaponQualityMod");
            harmony.PatchAll();
        }
    }
}
