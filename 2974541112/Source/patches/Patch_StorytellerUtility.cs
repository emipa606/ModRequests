using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System;
using System.Linq;
using RimWorld;
using Verse;

namespace zed_0xff.CPS;

// TSS: cound suspended pawn raid points x0.3, like if them were in a CryptosleepCasket
[HarmonyPatch(typeof(StorytellerUtility), nameof(StorytellerUtility.DefaultThreatPointsNow))]
static class Patch_DefaultThreatPointsNow
{
    static MethodInfo mOrigParentHolder = AccessTools.Method(typeof(Thing), "get_ParentHolder");
    static MethodInfo mNewParentHolder  = AccessTools.Method(typeof(Patch_DefaultThreatPointsNow), nameof(Patch_DefaultThreatPointsNow.ParentHolder));

    public static IThingHolder ParentHolder(Thing t) {
        if( t is Pawn pawn && pawn.ParentHolder is Building_TSS tss && tss.IsContentsSuspended ){
            return new Building_CryptosleepCasket();
        }
        return t.ParentHolder;
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        CodeInstruction[] iarr = instructions.ToArray();
        // callvirt virtual Verse.IThingHolder Verse.Thing::get_ParentHolder()
        // isinst RimWorld.Building_CryptosleepCasket
        for( int i=0; i<iarr.Count(); i++ ){
            if( iarr[i].opcode == OpCodes.Callvirt && iarr[i].operand as MethodInfo == mOrigParentHolder &&
                iarr[i+1].opcode == OpCodes.Isinst && iarr[i+1].operand as Type == typeof(Building_CryptosleepCasket)
              ){
                iarr[i].operand = mNewParentHolder;
            }
        }
        return iarr;
    }
}
