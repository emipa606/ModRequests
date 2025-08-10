using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Komishne.SanguophageTweaks
{
    [HarmonyPatch(typeof(CompAbilityEffect_BloodfeederBite), nameof(CompAbilityEffect_BloodfeederBite.Valid))]
    static class CompAbilityEffect_BloodfeederBite_Valid_Patch
    {
        static void Postfix(ref bool __result, CompAbilityEffect_BloodfeederBite __instance,
            LocalTargetInfo target, bool throwMessages = false)
        {
            if (__result == false)
                return;

            Pawn biter = __instance.parent.pawn;
            Pawn victim = target.Pawn;
            if (biter == null || victim == null || !biter.WorkTagIsDisabled(WorkTags.Violent))
                return;

            if (SanguophageUtility.WouldDieFromAdditionalBloodLoss(victim, __instance.Props.targetBloodLoss))
            {
                if (throwMessages)
                    Messages.Message(
                        "KOM.SanguophageTweaks.MessageCannotUseIncapableOfViolenceWouldKill".Translate(
                            __instance.parent.def.Named("ABILITY"), biter.Named("PAWN")),
                        /*lookTargets=*/victim, MessageTypeDefOf.RejectInput, /*historical=*/false);
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(GeneUtility), nameof(GeneUtility.OffsetHemogen))]
    static class GeneUtility_OffsetHemogen_Patch
    {
        static bool Prefix(Pawn pawn, ref float offset)
        {
            if (offset == 0.0f)
                return true;

            float multiplier = Utility.BloodlossSeverityMultiplierFromBodySize(pawn);
            if (SanguophageTweaksSettings.EnableDebugMode)
            {
                Log.Message(
                    $"[KOM.SanguophageTweaks] Modifying hemogen offset to {pawn.Named("PAWN")}. Original: {offset}. " +
                    $"Computed multiplier: {multiplier}. New value: {offset * multiplier}");
            }
            offset *= multiplier;

            return true;
        }
    }

    [HarmonyPatch(typeof(SanguophageUtility), nameof(SanguophageUtility.DoBite))]
    public static class SanguophageUtility_DoBite_Patch
    {
        static bool Prepare(/*MethodBase original*/)
        {
            // There is only one method being patched in this class, and only one feature being patched, so we do not
            // need to check the MethodBase parameter.
            return SanguophageTweaksSettings.EnableSkipVictimBodySizeEffectOnBiterGains;
        }

        // We want to remove multiplying by the target's body size when computing the amount of hemogen a pawn will
        // gain. From VRES, the amount of blood lost and gained is also modified in the target, which implies a
        // Sanguophage always drains a constant amount of blood, and so its changes should not depend on the size of
        // the victim. Vanilla, however, does take it into account for hemogen gain. Thus, even for VRES's animal
        // bloodfeeding, the victim's body size is taken into account twice.
        //
        // The [decompiled] C# line we are looking for is:
        //   float offset = targetHemogenGain * victim.BodySize * num;
        //
        // The IL code for this line is:
        //     IL_0016: ldarg.2      // targetHemogenGain
        //     IL_0017: ldarg.1      // victim
        //     IL_0018: callvirt instance float32 Verse.Pawn::get_BodySize()
        //     IL_001d: mul
        //     IL_001e: ldloc.0      // num
        //     IL_001f: mul
        //     IL_0020: stloc.1      // offset
        //
        // The goal of this transpiler is to simply change the second, third, and fourth instructions into Nops. This
        // effectively removes the "* victim.BodySize" from the original C# line, and results in no net changes to the
        // stack (two pushes and two pops are skipped).
        //
        // TODO: Do we need to keep the number of CodeInstructions the same? Is that required to preserve jumps? Or can
        // we simply not emit the new Nop instructions?
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo bodySizeMethodInfo = typeof(Pawn).GetMethod("get_BodySize");
            if (bodySizeMethodInfo is null)
            {
                if (SanguophageTweaksSettings.EnableDebugMode)
                {
                    Log.Error(
                        "[KOM.SanguophageTweaks] MethodInfo for Pawn.BodySize (Verse.Pawn::get_BodySize()) is null. " +
                        "Aborting transpiler for SanguophageUtility.DoBite.");
                }
                foreach (CodeInstruction instruction in instructions)
                    yield return instruction;
                yield break;
            }

            int seekedInstructionLength = 7;
            var found = false;
            List<CodeInstruction> codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                if (i < (codes.Count - seekedInstructionLength) &&
                    codes[i].opcode == OpCodes.Ldarg_2 &&
                    codes[i + 1].opcode == OpCodes.Ldarg_1 &&
                    codes[i + 2].opcode == OpCodes.Callvirt &&
                    codes[i + 2].Calls(bodySizeMethodInfo) &&
                    codes[i + 3].opcode == OpCodes.Mul &&
                    codes[i + 4].opcode == OpCodes.Ldloc_0 &&
                    codes[i + 5].opcode == OpCodes.Mul &&
                    codes[i + 6].opcode == OpCodes.Stloc_1)
                {
                    found = true;

                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Nop);  // i + 1
                    yield return new CodeInstruction(OpCodes.Nop);  // i + 2
                    yield return new CodeInstruction(OpCodes.Nop);  // i + 3
                    yield return codes[i + 4];
                    yield return codes[i + 5];
                    yield return codes[i + 6];
                    i += (seekedInstructionLength - 1);  // Move forward one fewer to account for the loop's i++.
                    continue;
                }

                yield return codes[i];
            }

            if (found == false)
            {
                Log.Error(
                    "[KOM.SanguophageTweaks] Could not find code block to modify in transpiler for " + 
                    "SanguophageUtility.DoBite.");
            }
        }
    }
}
