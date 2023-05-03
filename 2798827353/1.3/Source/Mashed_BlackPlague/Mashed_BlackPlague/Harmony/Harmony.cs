using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Linq;
using System.Reflection;

namespace Mashed_BlackPlague
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.Mashed_BlackPlague");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        /* disables inspirations */

        [HarmonyPatch(typeof(InspirationWorker))]
        [HarmonyPatch("InspirationCanOccur")]
        public static class InspirationWorker_InspirationCanOccur_Patch
        {
            [HarmonyPrefix]
            public static bool TuurngaitInspirationCanOccurPatch(Pawn pawn, ref bool __result)
            {
                return !Utility.PawnIsTuurngait(pawn);
            }
        }

        /* Prevents ideo certainty decay */
        [HarmonyPatch(typeof(Pawn_IdeoTracker))]
        class Pawn_IdeoTracker_CertaintyChangePerDay_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("CertaintyChangePerDay", MethodType.Getter)]
            public static bool TuurngaitCertaintyChangePerDayPatch(ref Pawn ___pawn, ref float __result)
            {
                if (Utility.PawnIsTuurngait(___pawn))
                {
                    __result = 1f;
                    return false;
                }
                return true;
            }
        }
    }
}
