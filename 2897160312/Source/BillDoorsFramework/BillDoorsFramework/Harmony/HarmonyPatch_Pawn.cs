using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class PawnGizmoPatch
    {
        static List<IntVec3> AcceptedCell(Pawn pawn)
        {
            return new List<IntVec3>() { pawn.Position + IntVec3.South, pawn.Position + IntVec3.North, pawn.Position + IntVec3.East, pawn.Position + IntVec3.West };
        }

        public static TargetingParameters TargetParam(Pawn pawn)
        {
            return new TargetingParameters
            {
                canTargetLocations = true,
                canTargetSelf = false,
                canTargetPawns = false,
                canTargetFires = false,
                canTargetBuildings = false,
                canTargetItems = false,
                validator = (TargetInfo x) => AcceptedCell(pawn).Contains(x.Cell) && x.Cell.GetEdifice(pawn.Map) == null
            };
        }

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (Gizmo command in __result) yield return command;
            if (__instance.Spawned && Find.Selector.SingleSelectedThing == __instance && __instance.drafter != null)
            {
                foreach (Thing thing in __instance.inventory.innerContainer)
                {
                    if (thing is MinifiedThingDeployable deployable)
                    {
                        Command_Target command_Target = new Command_Target
                        {
                            defaultLabel = deployable.InnerThing.Label,
                            targetingParams = TargetParam(__instance),
                            icon = deployable.InnerThing.def.GetUIIconForStuff(null),
                            action = delegate (LocalTargetInfo target)
                            {
                                deployable.Deploy(target.Cell, __instance);
                            }
                        };
                        if (!__instance.Drafted)
                        {
                            command_Target.Disable("BDF_DisabledUndrafted".Translate());
                        }
                        yield return command_Target;
                    }
                }
            }
        }
    }



    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain")]
    static class JobGiver_OptimizeApparel_Prefix
    {
        [HarmonyPrefix]
        static bool Prefix(Pawn pawn, Apparel ap, ref float __result)
        {
            if (EquipRestrictionUtil.CannotEquip(pawn, ap, out _))
            {
                __result = -1000f;
                return false;
            }
            return true;
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch]
    static class CanEquip_PostFix
    {
        static MethodBase TargetMethod()
        {
            return typeof(EquipmentUtility).GetMethod("CanEquip", new System.Type[] { typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool) });
        }

        [HarmonyPostfix]
        static void Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {
            if (__result && EquipRestrictionUtil.CannotEquip(pawn, thing, out string str))
            {
                cantReason = str;
                __result = false;
            }
        }
    }
}
