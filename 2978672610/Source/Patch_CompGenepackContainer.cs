using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System;
using UnityEngine;
using Verse;

namespace zed_0xff.GeneCollectorQOL;

// "DEV: Add specific gene..." gizmo for gene banks
[HarmonyPatch(typeof(CompGenepackContainer), nameof(CompGenepackContainer.CompGetGizmosExtra))]
static class Patch_CompGenepackContainer {
    static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, CompGenepackContainer __instance)
    {
        foreach( Gizmo gizmo in __result ){
            yield return gizmo;
        }
        if (!DebugSettings.ShowDevGizmos) {
            yield break;
        }
        yield return new Command_Action {
            defaultLabel = "DEV: Add specific gene...", action = delegate
            {
                Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugToolsPawns.Options_AddGene(delegate(GeneDef geneDef)
                    {
                    Genepack gp = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
                    gp.Initialize(new[]{geneDef}.ToList());
                    __instance.innerContainer.TryAdd(gp); // allows to add more than the capacity
                    })));
            }
        };
    }
}
