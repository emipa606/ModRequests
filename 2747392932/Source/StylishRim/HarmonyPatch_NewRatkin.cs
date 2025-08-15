using AlienRace;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace StylishRim
{
    public static class HarmonyPatch_NewRatkin
    {

        public static MethodInfo DrawMesh = AccessTools.Method(typeof(Graphics), nameof(Graphics.DrawMesh), new System.Type[]
        {// Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer
                typeof(Mesh),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Material),
                typeof(int)
        });
        internal static void DrawWornExtras_Prefix(Apparel __instance)
        {
            StylishDrawer.DrawExtras1_Ratkin(__instance.Wearer);
        }
        internal static void DrawWornExtras_Postfix(Apparel __instance)
        {
            StylishDrawer.DynamicPartPostfix();
        }
        internal static IEnumerable<CodeInstruction> DrawShield_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawMesh))
                {
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustDynamicPart_Ratkin));
                    continue;
                }
                yield return code;
            }
        }
        internal static IEnumerable<CodeInstruction> DrawEquipmentPrefix_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo drawSheathMethod = AccessTools.Method(AccessTools.TypeByName("SYS.DrawEquipment_WeaponBackPatch"), "DrawSheath", new System.Type[]
            { // DrawSheath(CompSheath compSheath, Pawn pawn, Vector3 drawLoc, Graphic graphic)
                AccessTools.TypeByName("SYS.CompSheath"), typeof(Pawn), typeof(Vector3), typeof(Graphic)
            });
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(drawSheathMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawExtras2_Ratkin));
                }
                yield return code;
            }
        }
        internal static IEnumerable<CodeInstruction> DrawEquipmentAiming_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawMesh))
                {
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustDynamicPart));
                    continue;
                }
                yield return code;
            }
        }
        internal static IEnumerable<CodeInstruction> DrawSheath_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawMesh))
                {
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustDynamicPart_Ratkin));
                    continue;
                }
                yield return code;
            }
        }
    }
}
