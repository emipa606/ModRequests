using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ArmoredUp
{
    [HarmonyPatch]
    public static class ArmoredUp_DamagePatches_Melee
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(VerbProperties), "AdjustedMeleeDamageAmount", new Type[4]
            {
                typeof(Tool),
                typeof(Pawn),
                typeof(Thing),
                typeof(HediffComp_VerbGiver)
            });
            yield return AccessTools.Method(typeof(VerbProperties), "AdjustedMeleeDamageAmount", new Type[5]
            {
                typeof(Tool),
                typeof(Pawn),
                typeof(ThingDef),
                typeof(ThingDef),
                typeof(HediffComp_VerbGiver)
            });
        }
        public static void Postfix(ref float __result)
        {
            __result *= ArmoredUp.settings.DamageMultiplier * ArmoredUp.settings.MeleeDamageMultiplier;
        }
    }
    
    [HarmonyPatch]
    public static class ArmoredUp_DamagePatches_RangedExtraDamage
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ExtraDamage), "AdjustedDamageAmount", new Type[2]
            {
                typeof(Verb),
                typeof(Pawn)
            });
        }
        public static void Postfix(ref float __result)
        {
            __result *= ArmoredUp.settings.DamageMultiplier * ArmoredUp.settings.RangedDamageMultiplier;
        }
    }
    
    [HarmonyPatch(typeof(ProjectileProperties), "GetDamageAmount", new Type[2]
    {
        typeof(float),
        typeof(StringBuilder)
    })]
    public static class ArmoredUp_DamagePatches_RangedTranspiler
    {
        public static int GetModifiedDamage(int originalDamage)
        {
            return Mathf.RoundToInt(originalDamage * ArmoredUp.settings.DamageMultiplier * ArmoredUp.settings.RangedDamageMultiplier);
        }
        
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo AUGetModifiedDamage = AccessTools.Method(typeof(ArmoredUp_DamagePatches_RangedTranspiler), "GetModifiedDamage");
            bool found = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldarg_2 && !found)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AUGetModifiedDamage);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    found = true;
                }
                else
                    yield return instruction;
                
            }
            if (!found)
                Log.Error("Armored Up could not apply Ranged Damage Mult patch");
            
        }
    }
}