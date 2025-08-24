using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using System.IO;
using UnityEngine;
using StuffableCore.SCComps;

namespace StuffableCore.SCPatches
{
    public static class GenPatch
    {
        public static Thing SpawnPatch(ThingDef def, IntVec3 loc, Map map, WipeMode wipeMode, Hediff hediff) {
            HediffCompStuffable comp = hediff.TryGetComp<HediffCompStuffable>();
            ThingDef stuff = ThingDefOf.Steel;
            if (comp != null)
                stuff = hediff.TryGetComp<HediffCompStuffable>().stuff;
            else
                Log.Message("Error spawning prosthetic. Setting default stuff.");
            return GenSpawn.Spawn(ThingMaker.MakeThing(def, stuff), loc, map, wipeMode);
        }
    }

    
    
    [HarmonyPatch(typeof(MedicalRecipesUtility), nameof(MedicalRecipesUtility.SpawnThingsFromHediffs))]
    internal static class SpawnThingsFromHediffs_Harmony_Patch
    {
        static readonly MethodInfo spawn = typeof(GenSpawn).GetMethod("Spawn", new Type[] { typeof(ThingDef), typeof(IntVec3), typeof(Map), typeof(WipeMode)});
        static readonly MethodInfo patch = typeof(GenPatch).GetMethod("SpawnPatch");

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction CI in instructions)
            {
                if (CI.Calls(spawn))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_2); // loads current hediff onto the stack, used as hediff argument for patch
                    yield return new CodeInstruction(OpCodes.Call, patch);
                }
                else
                {
                    yield return CI;
                }
            }
        }
    }
}
