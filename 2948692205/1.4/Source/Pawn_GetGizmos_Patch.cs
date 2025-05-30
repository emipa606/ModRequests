using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace IndustrialArmory
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
	public class Pawn_GetGizmos_Patch
	{
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (var g in __result)
            {
                yield return g;
            }
            if (__instance.MentalState is null && __instance.Faction == Faction.OfPlayer)
            {
                if (__instance.equipment?.Primary?.def == IA_DefOf.IA_RocketLance)
                {
                    var charge = new Command_Action
                    {
                        defaultLabel = "IA.Charge".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Charge"),
                        action = delegate
                        {
                            Find.Targeter.BeginTargeting(ForPawn(__instance), delegate (LocalTargetInfo x)
                            {
                                __instance.jobs.TryTakeOrderedJob(JobMaker.MakeJob(IA_DefOf.IA_Charge, x));
                            });
                        },
                    };
                    yield return charge;
                    var comp = __instance.equipment.Primary.GetComp<CompRocketLance>();
                    var toggle = new Command_Toggle
                    {
                        defaultLabel = "IA.UseRockets".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/UseRockets"),
                        isActive = () => comp.rocketsEnabled,
                        toggleAction = delegate ()
                        {
                            comp.rocketsEnabled = !comp.rocketsEnabled;
                        }
                    };
                    yield return toggle;
                }
            }
        }
        public static TargetingParameters ForPawn(Pawn user)
        {
            TargetingParameters targetingParameters = new TargetingParameters();
            targetingParameters.canTargetPawns = true;
            targetingParameters.canTargetAnimals = true;
            targetingParameters.canTargetMechs = true;
            targetingParameters.validator = (TargetInfo x) => x.Thing is Pawn victim && !victim.Downed && !victim.Dead && user.Position.DistanceTo(x.Cell) <= 20;
            return targetingParameters;
        }
    }
}
