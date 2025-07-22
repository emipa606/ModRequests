using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace YayoCombatWarcaskets
{
    internal static class HarmonyPatches
    {
        [HarmonyPatch]
        private static class PatchWarcasketDurability
        {
            private static Type warcasketType;

            [UsedImplicitly]
            private static bool Prepare()
            {
                warcasketType = AccessTools.TypeByName("VFEPirates.WarcasketDef");
                return warcasketType != null && AccessTools.TypeByName("yayoCombat.ArmorUtility") != null;
            }

            [UsedImplicitly]
            private static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("yayoCombat.ArmorUtility");
                return AccessTools.Method(type, "ApplyArmor");
            }

            [UsedImplicitly]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var target = AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.HitPoints));
                var replacement = AccessTools.Method(typeof(PatchWarcasketDurability), nameof(GetWarcasketDurability));

                foreach (var ci in instructions)
                {
                    if (ci.opcode == OpCodes.Callvirt && ci.operand is MethodInfo method && method == target)
                    {
                        ci.opcode = OpCodes.Call;
                        ci.operand = replacement;
                        if (YayoCombatWarcasketsMod.settings.debugMode)
                            Log.Message("[Yayo's Combat Warcaskets] - Found method to patch, patching warcasket durability check");
                    }

                    yield return ci;
                }
            }

            private static float GetWarcasketDurability(Thing armor)
            {
                if (armor.def.GetType().IsAssignableFrom(warcasketType))
                    return armor.MaxHitPoints * YayoCombatWarcasketsMod.settings.forcedWarcasketDurabilityPercent;
                return armor.HitPoints;
            }
        }

        [HarmonyPatch]
        private static class PatchVfeShieldsGetPostArmorDamage
        {
            [UsedImplicitly]
            private static bool Prepare()
            {
                return AccessTools.TypeByName("VFECore.CompShield") != null && AccessTools.TypeByName("yayoCombat.ArmorUtility") != null;
            }

            [UsedImplicitly]
            private static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("yayoCombat.ArmorUtility");
                return AccessTools.Method(type, "GetPostArmorDamage");
            }

            [UsedImplicitly]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
            {
                var isPatched = false;
                var instr = instructions.ToArray();
                CodeInstruction secondJump = null;

                for (var index = 0; index < instr.Length; index++)
                {
                    var ci = instr[index];
                    yield return ci;

                    if (!isPatched && ci.opcode == OpCodes.Stloc_S && ci.operand is LocalBuilder { LocalIndex: 6 })
                    {
                        if (YayoCombatWarcasketsMod.settings.debugMode)
                            Log.Message("[Yayo's Combat Warcaskets] - Found method to patch, patching VFE shields (part 1)");
                        isPatched = true;

                        var shieldUtilityType = AccessTools.TypeByName("VFECore.ShieldUtility");
                        var compShieldType = AccessTools.TypeByName("VFECore.CompShield");
                        var isShieldMethod = AccessTools.Method(shieldUtilityType, "IsShield", new[] { typeof(Thing), compShieldType.MakeByRefType() });
                        var usableNowGetter = AccessTools.PropertyGetter(compShieldType, "UsableNow");
                        var coversBodyPartMethod = AccessTools.Method(compShieldType, "CoversBodyPart");

                        var doNormalCheck = ilGen.DefineLabel();
                        instr[index + 1].labels.Add(doNormalCheck);

                        var local = ilGen.DeclareLocal(compShieldType);

                        yield return new CodeInstruction(OpCodes.Ldloc_S, (byte)6);
                        yield return new CodeInstruction(OpCodes.Ldloca_S, (byte)local.LocalIndex);
                        yield return new CodeInstruction(OpCodes.Call, isShieldMethod);
                        yield return new CodeInstruction(OpCodes.Brfalse_S, doNormalCheck);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, (byte)local.LocalIndex);
                        yield return new CodeInstruction(OpCodes.Callvirt, usableNowGetter);
                        yield return new CodeInstruction(OpCodes.Brfalse_S, doNormalCheck);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, (byte)local.LocalIndex);
                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Callvirt, coversBodyPartMethod);
                        secondJump = new CodeInstruction(OpCodes.Brtrue_S);
                        yield return secondJump;
                    }
                    else if (secondJump != null && ci.opcode == OpCodes.Brfalse_S)
                    {
                        var success = ilGen.DefineLabel();
                        instr[index + 1].labels.Add(success);
                        secondJump.operand = success;
                        secondJump = null;
                    }
                }
            }
        }

        [HarmonyPatch]
        private static class PatchVfeShieldsApplyArmor
        {
            [UsedImplicitly]
            private static bool Prepare()
                => AccessTools.TypeByName("VFECore.Patch_ArmorUtility") != null && AccessTools.TypeByName("yayoCombat.ArmorUtility") != null;

            [UsedImplicitly]
            private static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("yayoCombat.ArmorUtility");
                return AccessTools.Method(type, "ApplyArmor");
            }

            [UsedImplicitly]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
            {
                var isPatched = false;
                var instr = instructions.ToArray();
                CodeInstruction secondJump = null;

                for (var index = 0; index < instr.Length; index++)
                {
                    var ci = instr[index];
                    yield return ci;

                    if (!isPatched && ci.opcode == OpCodes.Ldarg_S && ci.operand is (byte)6)
                    {
                        if (YayoCombatWarcasketsMod.settings.debugMode)
                            Log.Message("[Yayo's Combat Warcaskets] - Found method to patch, patching VFE shields (part 2)");
                        isPatched = true;

                        var type = AccessTools.TypeByName("VFECore.Patch_ArmorUtility");
                        type = AccessTools.Inner(type, "ApplyArmor");
                        var isShieldMethod = AccessTools.Method(type, "IsShield");
                        var shieldUseDeflectMetalEffect = AccessTools.Method(type, "ShieldUseDeflectMetalEffect");

                        var doNormalCheck = ilGen.DefineLabel();
                        instr[index + 1].labels.Add(doNormalCheck);

                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Call, isShieldMethod);
                        yield return new CodeInstruction(OpCodes.Brfalse, doNormalCheck);
                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Call, shieldUseDeflectMetalEffect);
                        secondJump = new CodeInstruction(OpCodes.Br_S);
                        yield return secondJump;
                    }
                    else if (secondJump != null && ci.opcode == OpCodes.Stind_I1)
                    {
                        secondJump.operand = ci.labels[0];
                        secondJump = null;
                    }
                }
            }
        }

        [HarmonyPatch]
        private static class PatchNoVerbDrawing
        {
            [UsedImplicitly]
            private static bool Prepare()
                => AccessTools.TypeByName("yayoCombat.PawnRenderer_override") != null;

            [UsedImplicitly]
            private static MethodBase TargetMethod()
                => AccessTools.Method("yayoCombat.PawnRenderer_override:animateEquip");

            [UsedImplicitly]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var target = AccessTools.DeclaredPropertyGetter(typeof(LocalTargetInfo), nameof(LocalTargetInfo.IsValid));
                var field = AccessTools.DeclaredField(typeof(Stance_Busy), nameof(Stance_Busy.verb));
                var finished = false;
                var captureNext = false;

                foreach (var ci in instructions)
                {
                    yield return ci;
                    
                    if (finished)
                        continue;

                    if (captureNext)
                    {
                        finished = true;
                        captureNext = false;

                        yield return new CodeInstruction(OpCodes.Ldarg_S, (byte)5);
                        yield return new CodeInstruction(OpCodes.Ldfld, field);
                        yield return new CodeInstruction(OpCodes.Brfalse, ci.operand);
                    }
                    else if (ci.opcode == OpCodes.Call && ci.operand is MethodInfo method && method == target)
                    {
                        captureNext = true;
                    }
                }
            }
        }
    }
}