using System;
using Verse;
using Harmony;
using System.Reflection;
using RimWorld;

namespace LyingFace {
    [StaticConstructorOnStartup]
    class Main {
        static Main() {
            var harmony = HarmonyInstance.Create("com.tammybee.lyingface");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Building_Bed))]
    [HarmonyPatch("SortOwners")]
    class Building_Bed_SortOwners_Patch {
        [HarmonyPrefix]
        static bool Replace() {
            return false;
        }
    }

    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("LayingFacing")]
    class PawnRenderer_LayingFacing_Patch {
        [HarmonyPrefix]
        static bool UpdateLayingFacing(PawnRenderer __instance,ref Rot4 __result) {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn != null) {
                CompLyingFace comp = pawn.GetComp<CompLyingFace>();
                if (comp != null && pawn.GetPosture() == PawnPosture.LayingInBed) {
                    __result = comp.rotation;
                    return false;
                }
            }
            return true;
        }
    }
}
