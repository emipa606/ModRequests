using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace AccuraSight
{

    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.github.harmony.rimworld.maarx.accurasight");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    class Computations
    {
        public static bool shouldTagAccuracy(Thing t)
        {
            if (t.def.IsRangedWeapon && t.def.equipmentType == EquipmentType.Primary && t.TryGetQuality(out _))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool shouldTagDPS(Thing t)
        {
            // We use TryGetQuality to filter out crap like Beer Bottles and Wood, which can be used as melee weapons.
            // Why Clubs specifically don't have Quality is beyond me.
            // TODO: Find a better filtering mechanism.
            if (t.def.IsMeleeWeapon && t.def.equipmentType == EquipmentType.Primary && (t.TryGetQuality(out _) || t.def.defName == "MeleeWeapon_Club"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string computeAccuracy(Thing t)
        {
            float accuracy = t.GetStatValue(StatDefOf.AccuracyMedium, true);
            string sAcc = (accuracy * 100f).ToString("F0") + "%";
            return sAcc;
        }

        public static string computeDPS(Thing t)
        {
            float dps = t.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS, true);
            string sDPS = dps.ToString("00.0");
            return sDPS;
        }
    }

    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("DrawGUIOverlay")]
    class DrawGUIOverlay
    {
        static bool Prefix(Thing __instance)
        {
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                if (Computations.shouldTagAccuracy(__instance))
                {
                    GenMapUI.DrawThingLabel(__instance, Computations.computeAccuracy(__instance));
                    return false;
                }
                else if (Computations.shouldTagDPS(__instance))
                {
                    GenMapUI.DrawThingLabel(__instance, Computations.computeDPS(__instance));
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("Label", MethodType.Getter)]
    class ThingLabel
    {
        static void Postfix(Thing __instance, ref string __result)
        {
            Thing t = __instance;
            if (Computations.shouldTagAccuracy(t))
            {
                string sAcc = Computations.computeAccuracy(t);
                __result = __result.Insert(__result.IndexOf("("), "(ACC:" + sAcc + ") ");
            }
            else if (Computations.shouldTagDPS(t))
            {
                string sDPS = Computations.computeDPS(t);
                __result = __result.Insert(0, "(DPS:" + sDPS + ") ");
            }
        }
    }

    [HarmonyPatch(typeof(TransferableComparer_Name))]
    [HarmonyPatch("Compare")]
    class TransferableLabel
    {
        // We can't Prefix Transferable.Label because it doesn't have a body.
        // Rather than use a Transpiler, we just Prefix the TransferableComparer to use LabelCap instead of Label.
        static bool Prefix(Transferable lhs, Transferable rhs, ref int __result)
        {
            __result = lhs.LabelCap.CompareTo(rhs.LabelCap);
            return false;
        }
    }

    //[HarmonyPatch(typeof(Transferable), "LabelCap", new Type[0])]

    [HarmonyPatch(typeof(Transferable))]
    [HarmonyPatch("LabelCap", MethodType.Getter)]
    static class Patch_Transferable_LabelCap
    {
        static void Postfix(ref string __result, Transferable __instance)
        {
            if (__instance.AnyThing is null)
            {
                return;
            }
            Thing t = (Thing)__instance.AnyThing;
            if (Computations.shouldTagAccuracy(t))
            {
                string sAcc = Computations.computeAccuracy(t);
                __result = __result.Insert(__result.IndexOf("("), "(ACC:" + sAcc + ") ");
            }
            else if (Computations.shouldTagDPS(t))
            {
                string sDPS = Computations.computeDPS(t);
                __result = __result.Insert(0, "(DPS:" + sDPS + ") ");
            }
        }
    }
}
