using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System;
using UnityEngine;
using Verse;

namespace zed_0xff.GeneCourier;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(GeneUIUtility), "DrawGeneBasics")]
static class Patch_DrawGeneBasics {
    static CachedTexture origGeneBackground_Archite;
    static readonly CachedTexture myGeneBackground = new CachedTexture("Icons/GeneBackground_GeneCourier");
    static readonly GeneDef geneCourier = DefDatabase<GeneDef>.GetNamed("GeneCourier");

    static void Prefix(GeneDef gene, CachedTexture ___GeneBackground_Archite){
        if( gene == geneCourier ){
            origGeneBackground_Archite = ___GeneBackground_Archite;
        } else {
            origGeneBackground_Archite = null;
        }
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        MethodInfo origDrawTexture = AccessTools.Method(typeof(GUI), nameof(GUI.DrawTexture), new []{typeof(Rect), typeof(Texture)} );
        MethodInfo myDrawTexture = AccessTools.Method(typeof(Patch_DrawGeneBasics), nameof(Patch_DrawGeneBasics.DrawTexture));

        foreach( var code in instructions ) {
            if( code.Calls(origDrawTexture) ){
                code.operand = myDrawTexture;
            }
            yield return code;
        }
    }

    static void DrawTexture(Rect rect, Texture texture){
        if( origGeneBackground_Archite != null && texture == origGeneBackground_Archite.Texture ){
            GUI.DrawTexture(rect, myGeneBackground.Texture);
        } else {
            GUI.DrawTexture(rect, texture);
        }
    }
}
