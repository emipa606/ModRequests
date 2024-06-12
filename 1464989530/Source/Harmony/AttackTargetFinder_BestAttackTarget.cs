// Nightvision NightVision AttackTargetFinder_BestAttackTarget.cs
// 
// 20 10 2018
// 
// 20 10 2018

#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using JetBrains.Annotations;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace NightVision.Harmony
{

    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "FindAttackTarget")]
    public static class JobGiverAIFightEnemy_FindAttackTarget
    {

        //// [138 7 - 138 256]
        // IL_003e: ldloc.0      // V_0
        // IL_003f: ldfld        class Verse.Pawn RimWorld.JobGiver_AIFightEnemy/'<FindAttackTarget>c__AnonStorey0'::pawn
        // IL_0044: ldloc.1      // flags
        // IL_0045: ldloc.0      // V_0
        // IL_0046: ldftn        instance bool RimWorld.JobGiver_AIFightEnemy/'<FindAttackTarget>c__AnonStorey0'::'<>m__0'(class Verse.Thing)
        // IL_004c: newobj       instance void class [mscorlib]System.Predicate`1<class Verse.Thing>::.ctor(object, native int)
        // IL_0051: ldc.r4       0.0
        // IL_0056: ldarg.0      // this
        // IL_0057: ldfld        float32 RimWorld.JobGiver_AIFightEnemy::targetAcquireRadius
        // IL_005c: ldarg.0      // this
        // IL_005d: ldloc.0      // V_0
        // IL_005e: ldfld        class Verse.Pawn RimWorld.JobGiver_AIFightEnemy/'<FindAttackTarget>c__AnonStorey0'::pawn
        // IL_0063: callvirt     instance valuetype Verse.IntVec3 RimWorld.JobGiver_AIFightEnemy::GetFlagPosition(class Verse.Pawn)
        // IL_0068: ldarg.0      // this
        // IL_0069: ldloc.0      // V_0
        // IL_006a: ldfld        class Verse.Pawn RimWorld.JobGiver_AIFightEnemy/'<FindAttackTarget>c__AnonStorey0'::pawn
        // IL_006f: callvirt     instance float32 RimWorld.JobGiver_AIFightEnemy::GetFlagRadius(class Verse.Pawn)
        // IL_0074: ldc.i4.0     
        // IL_0075: ldc.i4.1     
        // IL_0076: call         class Verse.AI.IAttackTarget Verse.AI.AttackTargetFinder::BestAttackTarget(class Verse.AI.IAttackTargetSearcher, valuetype Verse.AI.TargetScanFlags, class [mscorlib]System.Predicate`1<class Verse.Thing>, float32, float32, valuetype Verse.IntVec3, float32, bool, bool)
        // IL_007b: castclass    Verse.Thing
        // IL_0080: ret

        //protected virtual Thing FindAttackTarget(Pawn pawn)
        //{
        //    TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
        //    if (this.needLOSToAcquireNonPawnTargets)
        //    {
        //        targetScanFlags |= TargetScanFlags.NeedLOSToNonPawns;
        //    }
        //    if (this.PrimaryVerbIsIncendiary(pawn))
        //    {
        //        targetScanFlags |= TargetScanFlags.NeedNonBurning;
        //    }
        //    return (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, (Thing x) => this.ExtraTargetValidator(pawn, x), 0f, >>>>> this.targetAcquireRadius <<<<<, this.GetFlagPosition(pawn), this.GetFlagRadius(pawn), false, true);
        //}






        [UsedImplicitly]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction ldfldPawn = null;
            CodeInstruction ldarg0 = null;
            CodeInstruction callNVModifyRadius = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AICombatTweaks), nameof(AICombatTweaks.ModifyTargetAcquireRadiusForGlow)));

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (ldarg0 == null && instruction.opcode == OpCodes.Ldarg_0)
                {
                    // this
                    ldarg0 = instruction.Clone();
                }
                else if (ldfldPawn == null && instruction.opcode == OpCodes.Ldfld && instruction.operand is FieldInfo fi && fi.FieldType == typeof(Pawn))
                {
                    // .pawn
                    ldfldPawn = instruction.Clone();
                }
                else if (callNVModifyRadius != null
                         && instruction.opcode == OpCodes.Ldfld
                         && instruction.operand is FieldInfo fi2
                         && fi2.FieldType == typeof(float))
                {
                    // loading a field of type float = .targetAcquireRadius
                    yield return ldarg0;
                    yield return ldfldPawn;
                    yield return callNVModifyRadius;

                    callNVModifyRadius = null;
                }
            }

        }
    }
}