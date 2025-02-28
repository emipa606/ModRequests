using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace EvolvedOrgansRedux {
    [StaticConstructorOnStartup]
    public static class HarmonyPatches {
        private static bool BodyPartIsMissing = false;
        private static readonly System.Type patchType = typeof(HarmonyPatches);
        private static readonly Harmony harmony = new Harmony(Singleton.Instance.Settings.Mod.Content.Name);
        static HarmonyPatches() {
            PatcherPostFix(typeof(AgeInjuryUtility), nameof(AgeInjuryUtility.RandomHediffsToGainOnBirthday), nameof(RandomHediffsToGainOnBirthday_Postfix), new System.Type[] { typeof(Pawn), typeof(int) });
            //PatcherPostFix(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculateLimbEfficiency), nameof(LimbEfficiencyWorkaround));
            PatcherTranspiler(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculateTagEfficiency), nameof(CalculateTagEfficiency_Transpilerfix));
            PatcherTranspiler(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculateLimbEfficiency), nameof(CalculateLimbEfficiency_Transpilerfix));
            if (Settings.ChangeBestPartEfficiencySpecialWeight)
                PatcherPreFix(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculateTagEfficiency), nameof(CalculateTagEfficiency_Prefix));
        }
        private static void LimbEfficiencyWorkaround(HediffSet diffSet, BodyPartTagDef limbCoreTag, ref float __result) {
            if (diffSet.pawn.RaceProps?.Humanlike == true && Singleton.Instance.BodyPartTagsToRecalculate.ContainsKey(limbCoreTag)) {
                __result = ((__result - 1f) * Singleton.Instance.BodyPartTagsToRecalculate[limbCoreTag]) + 1f;
            }
        }
        private static void RandomHediffsToGainOnBirthday_Postfix(Pawn pawn, ref IEnumerable<HediffGiver_Birthday> __result) {
            if (pawn.health.hediffSet.HasHediff(HediffDef.Named("EVOR_Hediff_AbdominalAndChestcavity_RasVacoule"))) {
                if (Prefs.LogVerbose)
                    Log.Message("EvolvedOrgansRedux -> Pawn is immortal because of Ras Vacoule.");
                __result = Enumerable.Empty<HediffGiver_Birthday>();
            }
        }
        private static void CalculateTagEfficiency_Prefix(ref float bestPartEfficiencySpecialWeight) {
            bestPartEfficiencySpecialWeight = 0.5f;
        }
        //I want to reduce the total efficiency of all parts and the armount of BodyParts by (AmountOfBodyPartsAddedByThisMod - BaseValue)
        //So basically I want to get those 2 values back to basegame value. If 4 eyes are added, I need to reduce those values by 4.
        //This is done to get the Efficiency each of those implants adds back to basegame value.
        //Eyes and ears have a special weight system applied. The best implant counts for 75% of the endvalue and every other counts for 25%.
        //That's why the first archotech eye gives 37.5% and the other one gives 12.5%.
        //If I wouldn't manipulate this method and add 2 more eyes, the first archotech eye still counts as 37.5%, but the other ones only 4.166~%.
        static IEnumerable<CodeInstruction> CalculateTagEfficiency_Transpilerfix(IEnumerable<CodeInstruction> instructions) {
            bool instructionFound = false;
            bool RetFound = false;
            foreach (CodeInstruction instruction in instructions) {
                if (RetFound && !instructionFound) {
                    yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = instruction.ExtractLabels() };
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_0); //num / Total efficiency of parts
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(FixTotalEfficiencyOfAllParts)));
                    yield return new CodeInstruction(OpCodes.Stloc_0);

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_1); //num2 / Amount of BodyParts
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(FixAmountOfBodyPartsWithTag)));
                    yield return new CodeInstruction(OpCodes.Stloc_1);
                    instructionFound = true;
                }
                //I want to set my CondeInstructions after the return
                if (instruction.opcode == OpCodes.Ret && !RetFound) {
                    RetFound = true;
                }
                yield return instruction;
            }
            if (!instructionFound)
                Log.Error("EvolvedOrgansRedux -> Instruction not found in CalculateCapacityLevel_Transpilerfix");
        }
        static IEnumerable<CodeInstruction> CalculateLimbEfficiency_Transpilerfix(IEnumerable<CodeInstruction> instructions) {
            bool instructionFound = false;
            bool RetFound = false;
            foreach (CodeInstruction instruction in instructions) {
                if (RetFound && !instructionFound) {
                    yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = instruction.ExtractLabels() };
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_1); //num / Total efficiency of parts
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(FixTotalEfficiencyOfAllParts)));
                    yield return new CodeInstruction(OpCodes.Stloc_1);

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_2); //num2 / Amount of BodyParts
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(patchType, nameof(FixAmountOfBodyPartsWithTag)));
                    yield return new CodeInstruction(OpCodes.Stloc_2);
                    instructionFound = true;
                }
                //I want to set my CondeInstructions after the return
                if (instruction.opcode == OpCodes.Ret && !RetFound) {
                    RetFound = true;
                }
                yield return instruction;
            }
            if (!instructionFound)
                Log.Error("EvolvedOrgansRedux -> Instruction not found in CalculateLimbEfficiency_Transpilerfix");
        }
        static float FixTotalEfficiencyOfAllParts(HediffSet diffSet, BodyPartTagDef tag, float totalEfficiencyOfAllParts) {
            //If the target is a human and the current calculating part is in the list
            if (diffSet.pawn.RaceProps?.Humanlike == true && Singleton.Instance.BodyPartTagsToRecalculate.ContainsKey(tag)) {
                if (totalEfficiencyOfAllParts - Singleton.Instance.BodyPartTagsToRecalculate[tag] >= 2f)
                    totalEfficiencyOfAllParts -= Singleton.Instance.BodyPartTagsToRecalculate[tag];
                else
                    BodyPartIsMissing = true;
            }
            return totalEfficiencyOfAllParts;
        }
        static int FixAmountOfBodyPartsWithTag(HediffSet diffSet, BodyPartTagDef tag, int AmountOfBodyPartsWithTag) {
            //If the target is a human and the current calculating part is in the list
            if (diffSet.pawn.RaceProps?.Humanlike == true && Singleton.Instance.BodyPartTagsToRecalculate.ContainsKey(tag) && !BodyPartIsMissing)
                AmountOfBodyPartsWithTag -= Singleton.Instance.BodyPartTagsToRecalculate[tag];
            BodyPartIsMissing = false;
            return AmountOfBodyPartsWithTag;
        }
        private static void PatcherPostFix(System.Type typeOfMethodToPatch, string nameOfMethodToPatch, string nameOfPatcherMethod, System.Type[] parameters = null) {
            if (Prefs.LogVerbose)
                Log.Message("EvolvedOrgansRedux -> Currently patching: " + nameOfPatcherMethod);
            harmony.Patch(AccessTools.Method(type: typeOfMethodToPatch, name: nameOfMethodToPatch, parameters),
                      postfix: new HarmonyMethod(methodType: patchType, methodName: nameOfPatcherMethod));
        }
        private static void PatcherPreFix(System.Type typeOfMethodToPatch, string nameOfMethodToPatch, string nameOfPatcherMethod, System.Type[] parameters = null) {
            if (Prefs.LogVerbose)
                Log.Message("EvolvedOrgansRedux -> Currently patching: " + nameOfPatcherMethod);
            harmony.Patch(AccessTools.Method(type: typeOfMethodToPatch, name: nameOfMethodToPatch, parameters),
                      prefix: new HarmonyMethod(methodType: patchType, methodName: nameOfPatcherMethod));
        }
        private static void PatcherTranspiler(System.Type typeOfMethodToPatch, string nameOfMethodToPatch, string nameOfPatcherMethod, System.Type[] parameters = null) {
            if (Prefs.LogVerbose)
                Log.Message("EvolvedOrgansRedux -> Currently patching: " + nameOfPatcherMethod);
            harmony.Patch(AccessTools.Method(type: typeOfMethodToPatch, name: nameOfMethodToPatch, parameters),
                      transpiler: new HarmonyMethod(methodType: patchType, methodName: nameOfPatcherMethod));
        }
    }
}