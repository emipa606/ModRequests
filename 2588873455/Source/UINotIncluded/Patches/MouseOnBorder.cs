using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace UINotIncluded.Patches
{
    [HarmonyPatch(typeof(CameraDriver), "CalculateCurInputDollyVect")]
    internal class CameraDriverPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen,
            IEnumerable<CodeInstruction> instructions)
        {
            var detected = false;
            var unload = false;
            var buffer = new List<CodeInstruction>();
            var target = new[]
            {
                OpCodes.Ldloca_S, OpCodes.Ldflda, OpCodes.Dup, OpCodes.Ldind_R4, OpCodes.Ldarg_0, OpCodes.Ldfld,
                OpCodes.Ldfld, OpCodes.Add, OpCodes.Stind_R4
            };
            var matches = 0;

            var numberMatches = 0;
            var numberInjected = false;

            var Field_mouseTouchingScreenBottomEdgeStartTime =
                AccessTools.Field(typeof(CameraDriver), "mouseTouchingScreenBottomEdgeStartTime");

            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 20f) numberMatches++;
                if (numberMatches == 3 && !numberInjected)
                {
                    code.operand = 6f;
                    numberInjected = true;
                }

                if (!detected && code.opcode == OpCodes.Ldloca_S) detected = true;
                if (detected)
                {
                    buffer.Add(code);

                    for (var i = 0; i < buffer.Count(); i++)
                        if (buffer[i].opcode != target[i])
                            unload = true;

                    if (buffer.Count() == target.Count() && !unload)
                    {
                        matches++;
                        if (matches < 2)
                        {
                            unload = true;
                        }
                        else
                        {
                            //Call the function
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld,
                                AccessTools.Field(typeof(CameraDriver), "config"));
                            yield return new CodeInstruction(OpCodes.Ldfld,
                                AccessTools.Field(typeof(CameraMapConfig),
                                    "dollyRateScreenEdge")); // this.config.dollyRateScreenEdge

                            yield return new CodeInstruction(OpCodes.Ldloca_S, 9); //Vector2 vector2

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldflda,
                                Field_mouseTouchingScreenBottomEdgeStartTime); // ref this.mouseTouchingScreenBottomEdgeStartTime

                            yield return new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(CameraDriverPatch), nameof(MoveIfTimer)));

                            // Set the flag
                            yield return new CodeInstruction(OpCodes.Ldc_I4_1); //
                            yield return new CodeInstruction(OpCodes.Stloc_1); // flag -> true

                            detected = false;
                            buffer.Clear();
                        }
                    }
                }
                else
                {
                    yield return code;
                }

                if (unload)
                {
                    foreach (var codeInstruction in buffer) yield return codeInstruction;
                    buffer.Clear();
                    detected = false;
                    unload = false;
                }
            }
        }

        public static void MoveIfTimer(float dollyRateScreenEdge, ref Vector2 vector2,
            ref float mouseTouchingScreenBottomEdgeStartTime)
        {
            if (!Settings.TabsOnTop)
            {
                vector2.y += dollyRateScreenEdge;
                return;
            }

            if (mouseTouchingScreenBottomEdgeStartTime < 0.0)
                mouseTouchingScreenBottomEdgeStartTime = Time.realtimeSinceStartup;
            if (Time.realtimeSinceStartup - mouseTouchingScreenBottomEdgeStartTime >= 0.280000001192093)
                vector2.y += dollyRateScreenEdge;
        }
    }

    [HarmonyPatch(typeof(WorldCameraDriver), "CalculateCurInputDollyVect")]
    internal class WorldCameraDriverPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen,
            IEnumerable<CodeInstruction> instructions)
        {
            var detected = false;
            var unload = false;
            var buffer = new List<CodeInstruction>();
            var target = new[]
            {
                OpCodes.Ldloca_S, OpCodes.Ldflda, OpCodes.Dup, OpCodes.Ldind_R4, OpCodes.Ldarg_0, OpCodes.Ldfld,
                OpCodes.Ldfld, OpCodes.Add, OpCodes.Stind_R4
            };
            var matches = 0;

            var numberMatches = 0;
            var numberInjected = false;

            var Field_mouseTouchingScreenBottomEdgeStartTime = AccessTools.Field(typeof(WorldCameraDriver),
                "mouseTouchingScreenBottomEdgeStartTime");

            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 20f) numberMatches++;
                if (numberMatches == 3 && !numberInjected)
                {
                    code.operand = 6f;
                    numberInjected = true;
                }

                if (!detected && code.opcode == OpCodes.Ldloca_S) detected = true;
                if (detected)
                {
                    buffer.Add(code);

                    for (var i = 0; i < buffer.Count(); i++)
                        if (buffer[i].opcode != target[i])
                            unload = true;

                    if (buffer.Count() == target.Count() && !unload)
                    {
                        matches++;
                        if (matches < 2)
                        {
                            unload = true;
                        }
                        else
                        {
                            //Call the function
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld,
                                AccessTools.Field(typeof(WorldCameraDriver), "config"));
                            yield return new CodeInstruction(OpCodes.Ldfld,
                                AccessTools.Field(typeof(WorldCameraConfig),
                                    "dollyRateScreenEdge")); // this.config.dollyRateScreenEdge

                            yield return new CodeInstruction(OpCodes.Ldloca_S, 8); //Vector2 zero

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldflda,
                                Field_mouseTouchingScreenBottomEdgeStartTime); // ref this.mouseTouchingScreenBottomEdgeStartTime

                            yield return new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(WorldCameraDriverPatch), nameof(MoveIfTimer)));

                            // Set the flag
                            yield return new CodeInstruction(OpCodes.Ldc_I4_1); //
                            yield return new CodeInstruction(OpCodes.Stloc_1); // flag -> true

                            detected = false;
                            buffer.Clear();
                        }
                    }
                }
                else
                {
                    yield return code;
                }

                if (unload)
                {
                    foreach (var codeInstruction in buffer) yield return codeInstruction;
                    buffer.Clear();
                    detected = false;
                    unload = false;
                }
            }
        }

        public static void MoveIfTimer(float dollyRateScreenEdge, ref Vector2 zero,
            ref float mouseTouchingScreenBottomEdgeStartTime)
        {
            if (!Settings.TabsOnTop)
            {
                zero.y += dollyRateScreenEdge;
                return;
            }

            if (mouseTouchingScreenBottomEdgeStartTime < 0.0)
                mouseTouchingScreenBottomEdgeStartTime = Time.realtimeSinceStartup;
            if (Time.realtimeSinceStartup - mouseTouchingScreenBottomEdgeStartTime >= 0.280000001192093)
                zero.y += dollyRateScreenEdge;
        }
    }
}