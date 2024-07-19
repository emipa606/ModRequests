using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FinishingTouchesByTheBestBuilder
{
    internal class JobDriver_ConstructFinishFrame_DisplayClass4_0_Patch
    {
        internal static IEnumerable<CodeInstruction> MakeNewToils_b__1_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            //Log.Message("@@@MakeNewToils_b__1_Transpiler");
            List<CodeInstruction> cis = new List<CodeInstruction>(instructions);
            //Log.Message($"@@@instructions.Count={cis.Count}");
            CodeInstruction ci = null, pre = null;
            Label noBestGoTo = generator.DefineLabel();
            MethodInfo readyForNextToil = AccessTools.Method(typeof(JobDriver), nameof(JobDriver.ReadyForNextToil));
            for (int i = cis.Count - 3; i >= 0; i--)
            {
                if (cis[i].opcode == OpCodes.Call && (MethodInfo)cis[i].operand == readyForNextToil && cis[i + 1].opcode == OpCodes.Ret)
                {
                    cis[i + 1].labels.Add(noBestGoTo);
                    //Log.Message("================");
                    //Log.Message($"@@@instructions[{i}]: 対象発見しました！ => 不適格とび先:{noBestGoTo}");
                    //Log.Message("================");
                    break;
                }
                ci = cis[i];
            }
            for (int i = 0; i < cis.Count; i++)
            {
                ci = cis[i];
                if (pre != null && pre.opcode == OpCodes.Blt_Un_S && pre.operand is Label && ci.opcode == OpCodes.Ldloc_1)
                {
                    ////noBestGoTo = (Label)pre.operand;

                    //yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return ci;
                    yield return CodeInstruction.Call(typeof(CommonHelper), nameof(CommonHelper.FinishingFrame), new Type[] { typeof(Frame) });
                    yield return new CodeInstruction(OpCodes.Ldloc_1); // frame
                    yield return new CodeInstruction(OpCodes.Ldloc_0); // pawn
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0); // force
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1); // false: from JobDriver
                    yield return CodeInstruction.Call(typeof(CommonHelper), nameof(CommonHelper.CanFinisheFrameAndBestPawn), new Type[] { typeof(Frame), typeof(Pawn), typeof(bool), typeof(bool) });
                    //条件を満たさない場合はLabelへジャンプ
                    yield return new CodeInstruction(OpCodes.Brfalse_S, noBestGoTo);
                }
                pre = ci;
                yield return ci;
            }
            //return cis;
        }
    }

    internal class ThingDefGenerator_ReplaceFrame_Patch
    {
        internal static void NewReplaceFrameDef_Thing_Postfix(ThingDef def, ref ThingDef __result)
        {
            CompProperties_FinishingTouchFrame prop = new CompProperties_FinishingTouchFrame();
            prop.isFinishingTouchTarget = def.HasComp(typeof(CompQuality));
            __result.comps.Add(prop);
        }
    }
}
