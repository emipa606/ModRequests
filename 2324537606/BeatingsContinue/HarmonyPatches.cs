using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BeatingsContinue
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            //Log.Message("Hello from Harmony in scope: com.github.harmony.rimworld.maarx.beatingscontinue");
            var harmony = new Harmony("com.github.harmony.rimworld.maarx.beatingscontinue");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    [HarmonyPatch(typeof(PawnAttackGizmoUtility))]
    [HarmonyPatch("GetAttackGizmos")]
    [HarmonyPatch(new Type[] { typeof(Pawn) })]
    class Patch_PawnAttackGizmoUtility_GetAttackGizmos
    {
        static void Postfix(ref Pawn pawn, ref IEnumerable<Gizmo> __result)
        {
            //Log.Message("Hello from Harmony in Patch_PawnAttackGizmoUtility_GetAttackGizmos");
            __result = (__result.ToList()).AddItem(UnarmedAttack.GetGizmo(pawn));
            __result = (__result.ToList()).AddItem(BeatAttack.GetGizmo(pawn));
        }
    }

/*
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("GetGizmos")]
    class Patch_Pawn_GetGizmos
    {
        static void Postfix(ref Pawn pawn, ref IEnumerable<Gizmo> __result)
        {
            __result = (__result.ToList()).AddItem(SuppressGizmo.GetGizmo(pawn));
        }
    }
*/
}
