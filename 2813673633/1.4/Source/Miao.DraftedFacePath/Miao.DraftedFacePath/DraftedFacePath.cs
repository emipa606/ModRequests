using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using System.Reflection.Emit;
using System.Reflection;

namespace Miao.DraftedFacePath
{


    //[StaticConstructorOnStartup]
    public class DraftedFacePath : Mod
    {
        public DraftedFacePath(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("Miao.Stand");
            harmony.Patch(AccessTools.Method(typeof(Pawn_RotationTracker), nameof(Pawn_RotationTracker.UpdateRotation)), null, null, new HarmonyMethod(typeof(Pawn_RotationTracker_UpdateRotation), "Transpiler"));
        }



    }
    public static class Pawn_RotationTracker_UpdateRotation
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //怪不得别人要转List，单纯一个IEnumerable根本无法顾及找指定调用
            var instructionsList = new List<CodeInstruction>(instructions);
            //var instructionsList2 = new List<CodeInstruction>();

            //for (var i = 0; i < instructionsList.Count; i++)
            //{
            //    if (instructionsList[i].opcode == OpCodes.Ldarg_0)
            //    {
            //        if (instructionsList[i + 1].opcode == OpCodes.Ldfld)
            //        {

            //            if (instructionsList[i + 2].opcode == OpCodes.Callvirt)
            //            {
            //                var get_Drafted = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Drafted));
            //                var field = instructionsList[i + 2].operand as MethodInfo;

            //                if (field != null && field == get_Drafted)
            //                {
            //                    instructionsList2.Add(new CodeInstruction(OpCodes.Ret));
            //                    return instructionsList2;
            //                    //yield return new CodeInstruction(OpCodes.Ldarg_0);
            //                    //yield return new CodeInstruction(OpCodes.Ret);
            //                    //break;
            //                    //i += 4;//直接跳5个指令，跳过判断 ldarg.0 可能是函数返回堆栈
            //                }
            //            }
            //        }
            //        //满足条件的情况下，直接就把IL0110这栈跳出。
            //    }
            //    instructionsList2.Add( new CodeInstruction(instructionsList[i]));
            //    //yield return instructionsList[i];
            //}
            //return null;


            for (var i = 0; i < instructionsList.Count; i++)
            {
                if (instructionsList[i].opcode == OpCodes.Ldarg_0)
                {
                    if (instructionsList[i + 1].opcode == OpCodes.Ldfld)
                    {

                        if (instructionsList[i + 2].opcode == OpCodes.Callvirt)
                        {
                            var get_Drafted = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Drafted));
                            var field = instructionsList[i + 2].operand as MethodInfo;

                            if (field != null && field == get_Drafted)//修改对我来说太难了。
                            {

                                var ret = new CodeInstruction(OpCodes.Ret);
                                ret.labels.Add(instructionsList[i].labels[0]);
                                //标准的情况下跳表是用ILGenerator myIL = myMthdBuilder.GetILGenerator();
                                //  Label[] jumpTable = new Label[] { myIL.DefineLabel(),这种很复杂的情况下构建的
                                //所以一下两种都可以，无论是上述的给Ret添加原先的跳表参数，还是保留函数入栈再出栈再Ret。
                                yield return ret;

                                //yield return instructionsList[i];
                                //yield return new CodeInstruction(OpCodes.Pop);
                                //yield return new CodeInstruction(OpCodes.Ret);
                                yield break;
                                
                            }
                        }
                    }
                    //满足条件的情况下，直接就把IL0110这栈跳出。
                }
                yield return new CodeInstruction(instructionsList[i]);
            }
        }

    }
}
