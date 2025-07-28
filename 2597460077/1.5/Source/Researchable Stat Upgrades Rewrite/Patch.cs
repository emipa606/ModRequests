using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchableStatUpgrades
{
    [HarmonyPatch(typeof(Thing),"SpawnSetup")]
    [StaticConstructorOnStartup]
    internal static class Patch
    {
        static Patch()
        {
            Patch.inst.Patch(typeof(Thing).GetMethod("SpawnSetup"), null, null, Patch.HarMetCtor("Transpiler1", null, null), null);
            Patch.inst.Patch(typeof(CompUseEffect_Artifact).GetMethod("DoEffect"), null, null, Patch.HarMetCtor("Transpiler2", null, null), null);
            Patch.inst.Patch(typeof(CompUseEffect_LearnSkill).GetMethod("DoEffect"),null,null,Patch.HarMetCtor("Transpiler2",null,null),null);
            Log.Message("ResearchableStatUpgrades :: Fix(es) initialized. (Abolished spawn stack truncation entirely, fixed bug where using neurotrainers/artifacts once cause the stack to deplete entirely)");
        }

        private static HarmonyMethod HarMetCtor(MethodInfo method)
        {
            return new HarmonyMethod(method);
        }

        private static HarmonyMethod HarMetCtor(string method,Type type = null, Type[] args = null)
        {
            return new HarmonyMethod(type ?? typeof(Patch), method, args);
        }

        private static MethodInfo GetLocalMethod(string method, Type[] parameters = null)
        {
            return AccessTools.Method(typeof(Patch), method, parameters, null);
        }

        //kd8lvt here: I don't understand half of what's going on here or in Transpiler2, so it's being copy/pasted directly from the old Assembly.
        //If someone would like to suggest a better, more understandable, way of doing this (without reflection), please do.
        private static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            Label label = gen.DefineLabel();
            bool next = false;
            foreach (CodeInstruction ins in instructions)
            {
                if (ins.opcode == OpCodes.Ldc_I4_7)
                {
                    yield return new CodeInstruction(OpCodes.Br, label);
                }
                else
                {
                    if (ins.opcode == OpCodes.Stfld && ins.operand == typeof(Thing).GetField("stackCount")) {
                        next = true;
                    }
                    else
                    {
                        if (next)
                        {
                            ins.labels.Add(label);
                            next = false;
                        }
                    }
                }
                yield return ins;
            }
            IEnumerator<CodeInstruction> enumerator = null;
            yield break;
            yield break;
        }

        private static IEnumerable<CodeInstruction> Transpiler2(IEnumerable<CodeInstruction> original,ILGenerator gen)
        {
            Label label = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            bool addlabel = false;
            foreach (CodeInstruction cur in original)
            {
                if (cur.operand == typeof(ThingWithComps).GetMethod("Destroy"))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0, null);
                    yield return new CodeInstruction(OpCodes.Call, Patch.GetLocalMethod("TranspilerUtility", null));
                    yield return new CodeInstruction(OpCodes.Brfalse, label);
                    yield return cur;
                    yield return new CodeInstruction(OpCodes.Br, label2);
                    yield return new CodeInstruction(OpCodes.Call, Patch.GetLocalMethod("Absorb", null)).AddLabel(new Label[]{label});
                    addlabel = true;
                }
                else
                {
                    if (addlabel)
                    {
                        yield return cur.AddLabel(new Label[] { label2 });
                        addlabel = false;
                    } else
                    {
                        yield return cur;
                    }
                }
            }
            IEnumerator <CodeInstruction> enumerator = null;
            yield break;
            yield break;
        }

        public static CodeInstruction AddLabel(this CodeInstruction instr, params Label[] ls)
        {
            instr.labels.AddRange(ls);
            return instr;
        }

        public static void Absorb(ThingComp comp,DestroyMode mode)
        {
        }

        public static bool TranspilerUtility(ThingComp tc)
        {
            ThingWithComps parent = tc.parent;
            bool result;
            if (parent.stackCount > 1)
            {
                parent.SplitOff(1);
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        public static readonly Harmony inst = new Harmony("com.kd8lvt.rsu.fixes");
    }
}
