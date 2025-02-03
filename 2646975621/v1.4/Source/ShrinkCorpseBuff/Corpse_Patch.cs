using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace ShrinkCorpseBuff {
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Corpse))]
    [HarmonyPatch("ButcherProducts")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(float) })]
    class Corpse_Patch {
        /// <summary>
        /// 初回起動でハーモニの起動を行います。
        /// </summary>
        static Corpse_Patch() {
            var harmony = new Harmony("com.github.harmony.rimworld.mod.ShrinkCorpseBuff");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            results = new Dictionary<Corpse, IEnumerable<Thing>>();
        }

        /// <summary>
        /// prefix から postfix にデータ移植するための保存用
        /// </summary>
        static Dictionary<Corpse, IEnumerable<Thing>> results;

        static bool Prefix(Corpse __instance, Pawn butcher, ref float efficiency) {

            var result = new List<Thing>();

            var things = __instance.InnerPawn.ButcherProducts(butcher, efficiency);

            foreach(var thing in things) {
                result.Add(thing);
            }

            results[__instance] = result;

            //Spread blood
            if(__instance.InnerPawn.RaceProps.BloodDef != null) {
                FilthMaker.TryMakeFilth(butcher.Position,
                    butcher.Map, __instance.InnerPawn.RaceProps.BloodDef, __instance.InnerPawn.LabelIndefinite(), 1, FilthSourceFlags.None);
            }
            //Thought / tale for butchering humanlike
            if(__instance.InnerPawn.RaceProps.Humanlike) {
                butcher.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ButcheredHumanlikeCorpse);
                foreach(var p in butcher.Map.mapPawns.SpawnedPawnsInFaction(butcher.Faction)) {
                    if(p == butcher || p.needs == null || p.needs.mood == null || p.needs.mood.thoughts == null
                        /* 今回の追加分！ → */ || p.GetRoom().ID != __instance.GetRoom().ID /* 同じ部屋にいる場合のみ効果発動！ */
                        ) {
                        continue;
                    }
                    p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse);
                }
                TaleRecorder.RecordTale(TaleDefOf.ButcheredHumanlikeCorpse, butcher);
            }
            return false; // これで元のメソッドは実行されないのよね？
        }

        static void Postfix(Corpse __instance, ref IEnumerable<Thing> __result) {
            if(results.ContainsKey(__instance)) {
                __result = results[__instance];
                results.Remove(__instance);
            }
        }
    }
}