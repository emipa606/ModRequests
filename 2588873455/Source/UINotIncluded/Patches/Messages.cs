using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace UINotIncluded
{
    [HarmonyPatch]
    internal class MessagesDoGUIPatch
    {
        private static readonly FieldInfo vector2_y = AccessTools.Field(typeof(Vector2), "y");

        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Messages), "MessagesDoGUI");

            var lto_assembly = AccessTools.AllAssemblies().FirstOrDefault(x => x.GetName().Name == "TacticalGroups");
            if (lto_assembly != null) yield return LTOColonistGroup_MessagesDoGUI(lto_assembly);
        }

        private static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen,
            IEnumerable<CodeInstruction> instructions)
        {
            var prev = instructions.First();
            var patched = false;

            foreach (var code in instructions)
            {
                yield return code;
                if (!patched)
                {
                    if (prev.opcode == OpCodes.Ldfld && (FieldInfo)prev.operand == vector2_y)
                    {
                        patched = true;
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(MessagesDoGUIPatch), nameof(YOffsetAdjustment)));
                        yield return new CodeInstruction(OpCodes.Add);
                    }

                    prev = code;
                }
            }
        }

        private static int YOffsetAdjustment()
        {
            return Settings.TabsOnTop ? (int)UIManager.ExtendedBarHeight : 0;
        }

        private static MethodBase LTOColonistGroup_MessagesDoGUI(Assembly lto_assembly)
        {
            foreach (var type in AccessTools.GetTypesFromAssembly(lto_assembly))
                if (type.Name == "HarmonyPatches_DynamicMessages")
                    return AccessTools.Method(type, "MessagesDoGUI");

            throw new Exception("Unable to find HarmonyPatches_DynamicMessages class on current assembly");
        }
    }
}