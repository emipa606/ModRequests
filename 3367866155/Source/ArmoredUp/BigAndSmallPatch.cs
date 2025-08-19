using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static HarmonyLib.Code;
//using BigAndSmall;
using HarmonyLib;
using Verse;

namespace ArmoredUp
{
    [HarmonyPatchCategory("Big and Small")]
    [HarmonyPatch]
    public static class BigAndSmallPatch
    {
        public static Type TypeOf_ApparelCache;
        
        public static bool Prepare()
        {
            return (TypeOf_ApparelCache = AccessTools.TypeByName("BigAndSmall.ApparelCache")) != null;
        }
			     
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(TypeOf_ApparelCache, "RepairAllDurability");
            //return TypeOf_VehicleStatHandler.GetMethod("ApplyDamageToComponent", new []{typeof(DamageInfo), typeof(IntVec2), typeof(StringBuilder)});
        }
        
        public static bool HasIndestructibleApparelGene(this Pawn pawn)
        {
            return pawn.genes.GenesListForReading.Any(x =>x.Active && x.def.defName == "BS_IndestructibleApparel");
        }
        
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo GetTickManager = typeof(Find).GetProperty("TickManager")?.GetGetMethod();
            MethodInfo GetTicksGame = typeof(TickManager).GetProperty("TicksGame")?.GetGetMethod();
            MethodInfo newTarget = AccessTools.Method(TypeOf_ApparelCache, "RepairDurability");

            yield return Ldarg_0;
            yield return Ldarg_1;
            yield return Call[GetTickManager];
            yield return Callvirt[GetTicksGame];
            yield return Ldc_R4[0.5f];
            yield return Callvirt[newTarget];
            yield return Ret;

        }
    }
}