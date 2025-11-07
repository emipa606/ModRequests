using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BillDoorsFramework
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public class PatchMain
    {
        private static Harmony harmonyInstance;

        internal static Harmony HarmonyInstance
        {
            get
            {
                if (harmonyInstance == null)
                {
                    harmonyInstance = new Harmony("BD_HarmonyPatches");
                }
                return harmonyInstance;
            }
        }

        static PatchMain()
        {
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }


    [HarmonyPatch]
    public class Patch_JD_ManTurret
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.FirstInner(typeof(JobDriver_ManTurret), t => t.Name.Contains("DisplayClass7"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("b__1"));
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> transp(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList();

            FieldInfo searchField = typeof(Job).GetField("count", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo overrideMethod = typeof(Patch_JD_ManTurret).GetMethod("OverrideMethod", BindingFlags.Static | BindingFlags.Public);
            int index = -1;

            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (list[i].opcode == OpCodes.Stfld && (FieldInfo)list[i].operand == searchField)  //不用postfix因为没法传这个b参
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                Log.Error("error");
                return list;
            }
            else
            {
                Log.Message($"patched at {index} {list[index].opcode.OpCodeType} {list[index].opcode == OpCodes.Ldc_I4_1}");
            }

            /*list[index] = new CodeInstruction(OpCodes.Ldloc_1);
            list.InsertRange(index, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, overrideMethod)
                }) ;*/

            list.InsertRange(index, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldloc_1),
                    //new CodeInstruction(OpCodes.Ldloca, 0),
                    new CodeInstruction(OpCodes.Call, overrideMethod),
                });

            return list;
        }

        public static int OverrideMethod(int _, Building_TurretGun building)
        {
            if (building.gun.TryGetComp<CompChangeableProjectileMagazineI>() is CompChangeableProjectileMagazineI comp2)
            {
                return comp2.RequestAmount;
            }
            return 1;
        }
    }


    [HarmonyPatch]
    public class Patch_JD_ManTurretRLD
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.FirstInner(typeof(JobDriver_ManTurret), t => t.Name.Contains("DisplayClass7"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("b__2"));
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> transp(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList();

            MethodInfo overrideMethod = typeof(Patch_JD_ManTurretRLD).GetMethod("OverrideMethod", BindingFlags.Static | BindingFlags.Public);
            int index = -1;

            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (list[i].opcode == OpCodes.Ldc_I4_1)  //不用postfix因为没法传这个b参
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                Log.Error("error");
                return list;
            }
            else
            {
                Log.Message($"man turret reload patched at {index} {list[index].opcode.OpCodeType} {list[index].opcode == OpCodes.Ldc_I4_1}");
            }
            list.InsertRange(index + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, overrideMethod),
                });
            return list;
        }

        public static int OverrideMethod(int _, Pawn p)
        {
            return p.carryTracker.CarriedThing.stackCount;
        }
    }

    [HarmonyPatch]
    public class Patch_JD_ManTurretRSV
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.FirstInner(typeof(JobDriver_ManTurret), t => t.Name.Contains("MakeNewToils"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("MoveNext"));
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> transp(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList();
            int index = -1;
            bool reserveFound = false;
            MethodInfo overrideMethod = typeof(Patch_JD_ManTurretRSV).GetMethod("OverrideMethod", BindingFlags.Static | BindingFlags.Public);

            MethodInfo searchField = typeof(Toils_Reserve).GetMethod("Reserve", BindingFlags.Public | BindingFlags.Static);
            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (list[i].opcode == OpCodes.Call && (MethodInfo)list[i].operand == searchField)
                {
                    reserveFound = true;
                }
                if (reserveFound && list[i].opcode == OpCodes.Ldc_I4_1)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                Log.Error("error");
                return list;
            }
            else
            {
                Log.Message($"man turret reserve patched at {index} {list[index].opcode} {list[index].opcode.OpCodeType} {list[index].opcode == OpCodes.Ldc_I4_1}");
            }
            list.InsertRange(index + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, overrideMethod),
                });
            return list;
        }

        public static int OverrideMethod(int _, JobDriver_ManTurret p)
        {
            if (p.job.targetA.Thing is Building_TurretGun turret && turret.gun.TryGetComp<CompChangeableProjectileMagazineI>() is CompChangeableProjectileMagazineI comp2)
            {
                return comp2.RequestAmount;
            }
            return 1;
        }
    }
}
