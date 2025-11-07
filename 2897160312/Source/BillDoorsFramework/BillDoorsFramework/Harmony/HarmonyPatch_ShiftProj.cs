using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class HarmonyPatch_ShiftProj
    {
        static readonly MethodInfo searchField = typeof(Thing).GetProperty("DrawPos", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();

        static readonly MethodInfo searchField2 = typeof(CompMannable).GetProperty("ManningPawn", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();


        static readonly FieldInfo burstShotsLeftF = typeof(Verb_LaunchProjectile).GetField("burstShotsLeft", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> transp(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList();
            int n = 0;
            bool manpawn = false;
            for (int i = 0; i <= list.Count; ++i)
            {
                //VFE-D fucked up this patch. Have to do it the more fucked way
                if (ModLister.GetModWithIdentifier("oskarpotocki.vfe.deserters") != null)
                {

                    if (list[i].opcode == OpCodes.Callvirt && (MethodInfo)list[i].operand == searchField2)
                    {
                        manpawn = true;
                        continue;
                    }
                    if (manpawn && list[i].opcode == OpCodes.Callvirt)
                    {
                        n = i + 2;
                        break;
                    }

                }
                else
                {
                    if (list[i].opcode == OpCodes.Callvirt && (MethodInfo)list[i].operand == searchField)
                    {
                        n = i + 2;
                        break;
                    }
                }

            }
            MethodInfo overrideMethod = typeof(HarmonyPatch_ShiftProj).GetMethod("InsertMethod", BindingFlags.Static | BindingFlags.Public);
            list.InsertRange(n, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldloc_S,6),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call,overrideMethod),
                    new CodeInstruction(OpCodes.Stloc_S,6),
                });
            return list;
        }

        public static Vector3 InsertMethod(Vector3 drawPos, Verb_LaunchProjectile verb)
        {
            if (verb.EquipmentSource?.def.GetModExtension<ModExtension_ProjOriginOffset>() is ModExtension_ProjOriginOffset ext)
            {
                drawPos += ext.GetOffsetFor((int)burstShotsLeftF.GetValue(verb)).RotatedBy(ConeExplosionUtility.RWangleToCEangle(verb.CurrentTarget.CenterVector3.AngleToFlat(verb.caster.DrawPos))).ToVector3();
            }
            return drawPos;
        }
    }
}
