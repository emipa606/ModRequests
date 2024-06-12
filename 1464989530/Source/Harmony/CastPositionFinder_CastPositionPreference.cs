// Nightvision NightVision CastPositionFinder_CastPositionPreference.cs
// 
// 20 10 2018
// 
// 20 10 2018

#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace NightVision.Harmony
{
    [HarmonyPatch(declaringType: typeof(CastPositionFinder), methodName: "CastPositionPreference")]
    public static class CastPositionFinder_CastPositionPreference
    {
        public static float GlowCoverCoefficient = 0.5f;


        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instrList = instructions.ToList();

            CodeInstruction loadIntVecArg = null;
            CodeInstruction loadCastPositionReq = null;
            CodeInstruction loadCasterPawnFromReq = null;

            var callOurMethod = new CodeInstruction(
                opcode: OpCodes.Call,
                operand: AccessTools.Method(type: typeof(CastPositionFinder_CastPositionPreference), name: nameof(ModifyCoverDesirability))
            );

            var workDone = false;

            for (var index = 0; index < instrList.Count; index++)
            {
                CodeInstruction instr = instrList[index: index];

                yield return instr;

                if (!workDone)
                {
                    if (instr.opcode == OpCodes.Ldarg_0)
                    {
                        loadIntVecArg = instr;
                    }
                    else if (instr.opcode == OpCodes.Ldsflda && instr.operand is FieldInfo fi && fi.FieldType == typeof(CastPositionRequest)
                             && instrList[index + 1].opcode == OpCodes.Ldfld && instrList[index + 1].operand is FieldInfo fi2 && fi2.FieldType == typeof(Pawn))
                    {
                        loadCastPositionReq = new CodeInstruction(instr);
                        loadCasterPawnFromReq = new CodeInstruction(instrList[index + 1]);
                    }

                    else if (instr.opcode == OpCodes.Mul
                             && instrList[index: index - 1].opcode == OpCodes.Ldc_R4
                             && instrList[index: index + 1].opcode == OpCodes.Add)
                    {
                        yield return loadIntVecArg;
                        yield return loadCastPositionReq;
                        yield return loadCasterPawnFromReq;
                        yield return callOurMethod;
                        yield return new CodeInstruction(instr);

                        workDone = true;
                    }
                }
            }
        }


        public static float ModifyCoverDesirability(IntVec3 c, Pawn caster)
        {
            float glow = GlowFor.GlowAt(map: caster.Map, pos: c);

            if (glow.GlowIsDarkness())
            {
                return 1f + GlowCoverCoefficient * ((Constants.MIN_GLOW_NO_GLOW - glow) / Constants.MIN_GLOW_NO_GLOW);
            }

            return 1f;
        }
    }
}