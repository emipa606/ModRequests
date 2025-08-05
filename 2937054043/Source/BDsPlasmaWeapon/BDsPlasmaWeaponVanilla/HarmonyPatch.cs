using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BDsPlasmaWeaponVanilla
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("BDsPlasmaWeapon");

            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "PostProcessGeneratedGear"), postfix: new HarmonyMethod(patchType, nameof(PostProcessGeneratedGear_Postfix)));

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void PostProcessGeneratedGear_Postfix(Thing gear)
        {
            CompReloadableFromFiller comp = gear.TryGetComp<CompReloadableFromFiller>();
            if (comp != null)
            {
                comp.RemainingCharges = comp.MaxCharges;
            }
        }
    }

    /*
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming))]
    internal class Harmony_PlaceWorker_ShowTurretRadius
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            MethodInfo drawMeshMI = typeof(Graphics).GetMethod(nameof(Graphics.DrawMesh),
                new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(int) });
            int index = codes.FindIndex((x) => x.Calls(drawMeshMI));
            MethodInfo myDrawingMethod = typeof(HarmonyPatches).GetMethod(nameof(DrawEquipmentAiming_postfix));
            codes.InsertRange(index + 1, new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Call, myDrawingMethod)
            });
            return codes;
        }

        public static void DrawEquipmentAiming_postfix(Thing eq, Vector3 drawLoc, Mesh mesh, float num)
        {
            DefModExtension_WeaponGlowRender renderExtension = eq.def.GetModExtension<DefModExtension_WeaponGlowRender>();
            if (renderExtension != null)
            {
                Graphics.DrawMesh(material: renderExtension.graphicData.Graphic.MatSingle, mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
            }
        }
    }

    public class DefModExtension_WeaponGlowRender : DefModExtension
    {
        public GraphicData graphicData;
    }
    */
}
