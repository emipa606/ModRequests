using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace zed_0xff.GeneCollectorQOL;

static class Patch_Building_GeneAssembler {
    public static Dictionary<int, int> queues = new Dictionary<int, int>();

    class State {
        public List<Genepack> packs;
        public int architesRequired;
        public string xenotypeName;
        public XenotypeIconDef iconDef;
        public bool skipReset;

        public State(List<Genepack> packs, int architesRequired, string xenotypeName, XenotypeIconDef iconDef, bool skipReset){
            this.packs = packs;
            this.architesRequired = architesRequired;
            this.xenotypeName = xenotypeName;
            this.iconDef = iconDef;
            this.skipReset = skipReset;
        }
    }

    static Dictionary<int, State> states = new Dictionary<int, State>();

    private static readonly MethodInfo mStart = AccessTools.Method(typeof(Building_GeneAssembler), "Start");
    private static readonly FastInvokeHandler _Start = MethodInvoker.GetHandler(mStart);

    [HarmonyPatch(typeof(Building_GeneAssembler), nameof(Building_GeneAssembler.Finish))]
    class Patch_Finish {
        static void Prefix(Building_GeneAssembler __instance,
                List<Genepack>  ___genepacksToRecombine,
                int             ___architesRequired,
                string          ___xenotypeName,
                XenotypeIconDef ___iconDef
                ){

            if( !ModConfig.Settings.patchGeneAssembler_addQueue ) return;
            if( ___genepacksToRecombine.NullOrEmpty() ) return; // XXX or clear state?
            if( !queues.ContainsKey(__instance.thingIDNumber) ) return;

            states[__instance.thingIDNumber] = new State(___genepacksToRecombine, ___architesRequired, ___xenotypeName, ___iconDef, true);
        }

        static void Postfix(Building_GeneAssembler __instance){
            if( !ModConfig.Settings.patchGeneAssembler_addQueue ) return;
            if( !queues.ContainsKey(__instance.thingIDNumber) ) return;

            int nQueued = queues[__instance.thingIDNumber];
            if( nQueued <= 0 ) return;

            nQueued--;
            queues[__instance.thingIDNumber] = nQueued;

            if( !states.ContainsKey(__instance.thingIDNumber) ) return;
            State state = states[__instance.thingIDNumber];
            state.skipReset = false;

            _Start(__instance, state.packs, state.architesRequired, state.xenotypeName, state.iconDef);
        }
    }

    // Start() is called by Dialog_CreateXenogerm
    [HarmonyPatch(typeof(Building_GeneAssembler), "Start")]
    class Patch_Start {
        static void Prefix(Building_GeneAssembler __instance){
            if( !ModConfig.Settings.patchGeneAssembler_addQueue ) return;
            if( states.TryGetValue(__instance.thingIDNumber, out State state) ){
                state.skipReset = true;
            }
        }

        static void Postfix(Building_GeneAssembler __instance){
            if( !ModConfig.Settings.patchGeneAssembler_addQueue ) return;
            if( states.TryGetValue(__instance.thingIDNumber, out State state) ){
                state.skipReset = false;
            }
        }
    }

    // XXX: Start() calls Reset() at begin
    // XXX: Finish() calls Reset() at end
    [HarmonyPatch(typeof(Building_GeneAssembler), "Reset")]
    class Patch_Reset {
        static void Postfix(Building_GeneAssembler __instance){
            if( !ModConfig.Settings.patchGeneAssembler_addQueue ) return;
            if( states.TryGetValue(__instance.thingIDNumber, out State state) && state.skipReset ){
                return;
            }
            queues.Remove(__instance.thingIDNumber);
            states.Remove(__instance.thingIDNumber);
        }
    }

    private static readonly MethodInfo mTotalGCX = AccessTools.Method(typeof(Building_GeneAssembler), "get_TotalGCX");
    private static readonly FastInvokeHandler _TotalGCX = MethodInvoker.GetHandler(mTotalGCX);

    [HarmonyPatch(typeof(Building_GeneAssembler), nameof(Building_GeneAssembler.GetInspectString))]
    class Patch_GetInspectString {
        static void Postfix(Building_GeneAssembler __instance, ref string __result){
            if( !ModConfig.Settings.patchGeneAssembler_addQueue ) return;

            if( queues.TryGetValue(__instance.thingIDNumber, out int nQueued) && nQueued != 0 ){
                int numTicks = Mathf.RoundToInt(nQueued * GeneTuning.ComplexityToCreationHoursCurve.Evaluate((int)_TotalGCX(__instance)) * 2500f / __instance.GetStatValue(StatDefOf.AssemblySpeedFactor));
                // TaggedString is for colorizing time left
                __result = new TaggedString(__result + "\n" + "Queued".Translate() + ": " + nQueued + " (" + numTicks.ToStringTicksToPeriod() + ")").Resolve();
            }
        }
    }

    [HarmonyPatch(typeof(Building_GeneAssembler), nameof(Building_GeneAssembler.ExposeData))]
    class Patch_ExposeData {
        static void Postfix(Building_GeneAssembler __instance){
            if( !ModConfig.Settings.patchGeneAssembler_addQueue ) return;

            int tmp = 0;

            switch( Scribe.mode ){
                case LoadSaveMode.LoadingVars:
                    Scribe_Values.Look(ref tmp, "nQueued", 0);
                    queues[__instance.thingIDNumber] = tmp;
                    break;
                case LoadSaveMode.Saving:
                    queues.TryGetValue(__instance.thingIDNumber, out tmp);
                    Scribe_Values.Look(ref tmp, "nQueued", 0);
                    break;
            }
        }
    }
}
