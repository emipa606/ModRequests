using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using static HarmonyLib.Code;
using System.Reflection.Emit;
using Verse.AI;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography;

namespace ToxicPlants
{
    [StaticConstructorOnStartup]
    public static class ToxicPlants
    {
        static ToxicPlants()
        {
            var harmony = new Harmony("ac.mwsad.patch");
            harmony.PatchAll();


        }
        public static void applyToxic(Pawn pawn)
        {
            HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, (0.1f * Mathf.Max(0f , 1f - pawn.GetStatValue(StatDefOf.ToxicResistance) - (pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance)/2))));
        }

    }
    public class ToxicPlantModExtension : DefModExtension
    {
        public float ToxicAmount;
    }
    [HarmonyPatch(typeof(TendUtility), "DoTend")]
    public static class Heal_Patch
    {
        public static void Postfix(Pawn patient, Medicine medicine)
        {
            if ((medicine != null) && (patient != null) && (medicine.def.HasModExtension<ToxicPlantModExtension>() == true))
            {
                ToxicPlants.applyToxic(patient);
            }
        }
    }
    [HarmonyPatch(typeof(Recipe_InstallArtificialBodyPart), "ApplyOnPawn")]
    public static class Surgery_Patch
    {
        public static void Postfix(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
        {
            if (pawn != null)
            {
                for (var i = 0; i < ingredients.Count; i++)
                {
                    if (ingredients[i].def.HasModExtension<ToxicPlantModExtension>() == true)
                    {
                        ToxicPlants.applyToxic(pawn);
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(Recipe_Surgery), "CheckSurgeryFail")]
    public static class Surgery1_Patch
    {
        public static void Postfix(Pawn surgeon, Pawn patient, List<Thing> ingredients)
        {
            if (patient != null)
            {
                for (var i = 0; i < ingredients.Count; i++)
                {
                    if (ingredients[i].def.HasModExtension<ToxicPlantModExtension>() == true)
                    {
                        ToxicPlants.applyToxic(patient);
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(Recipe_ExtractHemogen), "ApplyOnPawn")]
    public static class Surgery2_Patch
    {
        public static void Postfix(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (pawn != null)
            {
                for (var i = 0; i < ingredients.Count; i++)
                {
                    if (ingredients[i].def.HasModExtension<ToxicPlantModExtension>() == true)
                    {
                        ToxicPlants.applyToxic(pawn);
                    }
                }
            }
        }
    }
    /*[HarmonyPatch(typeof(RefuelWorkGiverUtility))]
    [HarmonyPatch("CanRefuel")]
    public static class refuel_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ThingDef), nameof(Thing.def)));
            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(ToxicPlants_DefOf), nameof(ToxicPlants_DefOf.Plant_BloodRose)));
            yield return new CodeInstruction(OpCodes.Bne_Un_S, OpCodes.Ret);
            foreach (CodeInstruction inst in insts)
            {
                if (inst.Calls(AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.Faction))))
                {
                }
                yield return inst;
            }
        }
    }*/
}
