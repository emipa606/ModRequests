using System.Reflection;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Linq;
using Verse.Sound;
using Verse.AI;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MaliExtinguishRefuelables
{
    [StaticConstructorOnStartup]
    public static class MaliPatch
    {
        static MaliPatch()
        {
            Harmony harmony = new Harmony("mali.Extinguishable.patch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            //disabled for now, breaks devmode if unloaded
            //try
            //{
            //    ((Action)(() =>
            //    {
            //        if (ModsConfig.IsActive("Aelanna.BetterPyromania"))
            //        {
            //            harmony.Patch(AccessTools.Method(typeof(BetterPyromania.Need_Pyromania), nameof(BetterPyromania.Need_Pyromania.CheckFiresNear)),
            //                prefix: new HarmonyMethod(typeof(BetterPyromaniaPatch), nameof(BetterPyromaniaPatch.Prefix)));
            //        }
            //    }))();
            //}
            //catch (TypeLoadException ex)
            //{
            //}

        }
        //[HarmonyPatch(typeof(Building_WorkTable), "CurrentlyUsableForBills")]

        //class WorktablePatch
        //{
        //    public static void Postfix(ref bool __result, ref Building_WorkTable __instance)
        //    {
        //        CompFlickable extinguishableComp = __instance.TryGetComp<CompFlickable>();
        //        CompFireOverlayExtinguishable fireOverlayComp = __instance.TryGetComp<CompFireOverlayExtinguishable>();
        //        if (__result && !extinguishableComp.SwitchIsOn && fireOverlayComp != null)
        //        {
        //            //magically turn it on remotely cuz i dont wanna learn how to start a job
        //            extinguishableComp.DoFlick();
        //        }
        //    }
        //}

        [HarmonyPatch(typeof(CompFlickable), "DoFlick")]

        class DoFlickPatch
        {
            public static void Postfix(ref CompFlickable __instance)
            {
                CompFireOverlayExtinguishable fireOverlayComp = __instance.parent.TryGetComp<CompFireOverlayExtinguishable>();
                CompDarklightOverlayExtinguishable darklightOverlayComp = __instance.parent.TryGetComp<CompDarklightOverlayExtinguishable>();
                bool hasFireComp = fireOverlayComp != null || darklightOverlayComp != null;
                CompRefuelable refuelableComp = __instance.parent.TryGetComp<CompRefuelable>();
                //ThingDef thingdef = __instance.parent.def;

                if (hasFireComp)
                {
                    if (__instance.SwitchIsOn)
                    {
                        MaliSoundDefOf.LightFire.PlayOneShot(new TargetInfo(__instance.parent.Position, __instance.parent.Map));
                        //thingdef.hasInteractionCell = true;
                        if (refuelableComp.Props.fuelConsumptionPerTickInRain == 0f)
                            refuelableComp.Props.fuelConsumptionPerTickInRain = 0.0006f; //yeah we're not even using the original prop value, bite me
                    }
                    if (!__instance.SwitchIsOn)
                    { 
                        MaliSoundDefOf.Extinguish.PlayOneShot(new TargetInfo(__instance.parent.Position, __instance.parent.Map));
                        //thingdef.hasInteractionCell = false;
                        if (refuelableComp.Props.fuelConsumptionPerTickInRain > 0f)
                            refuelableComp.Props.fuelConsumptionPerTickInRain = 0f;
                    }

                    //Log.Message($"{refuelableComp.Props.fuelConsumptionPerTickInRain}");
                }
            }
        }
        [HarmonyPatch(typeof(CompFlickable), "CompGetGizmosExtra")]

        class CompGizmoPatch
        {
            public static void Postfix(ref CompFlickable __instance, ref IEnumerable<Gizmo> __result)
            {
                CompFireOverlayExtinguishable fireOverlayComp = __instance.parent.TryGetComp<CompFireOverlayExtinguishable>();
                CompDarklightOverlayExtinguishable darklightOverlayComp = __instance.parent.TryGetComp<CompDarklightOverlayExtinguishable>();
                bool hasFireComp = fireOverlayComp != null || darklightOverlayComp != null;
                Texture2D NewTex = ContentFinder<Texture2D>.Get("UI/Commands/DesireExtinguish");
                if (__instance.cachedCommandTex != NewTex && hasFireComp)
                {
                    __instance.Props.commandTexture = "UI/Commands/DesireExtinguish";
                    __instance.Props.commandLabelKey = "CommandDesignateExtinguishLable";
                    __instance.Props.commandDescKey = "CommandDesignateExtinguishDesc";
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]

        class DoBillPatch
        {
            static bool Prefix(WorkGiver_DoBill __instance, Thing thing, ref Job __result)
            {
                CompFlickable extinguishableComp = thing.TryGetComp<CompFlickable>();
                if (extinguishableComp != null && !extinguishableComp.SwitchIsOn)
                {
                    __result = null;
                    return false;
                }
                return true;
            }
        }
        //[HarmonyPatch(typeof(CompRefuelable), "ConsumeFuel")]
        //class ConsumptionPatch
        //{
        //    public static void Prefix(ref CompRefuelable __instance, ref float amount)
        //    {
        //        CompFlickable extinguishableComp = __instance.parent.TryGetComp<CompFlickable>();
        //        if (!extinguishableComp.SwitchIsOn && amount > 0)
        //        {
        //            amount = 0;
        //        }
        //    }
        //}
    }

}
