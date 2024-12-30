using HarmonyLib;
using Verse;
using System.Reflection;
using ESCP_NecromanticThralls;
using AlienRace;

namespace ESCP_NecromanticThralls_HAR
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.ESCP_NecromanticThralls_HAR");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }


    [HarmonyPatch(typeof(Verb_CastAbilityTouch_ThrallCreate))]
    [HarmonyPatch("IsValidCorpse")]
    public static class Verb_CastAbilityTouch_ThrallCreate_IsValidCorpse_Patch
    {
        [HarmonyPostfix]
        public static void CompAsexualReproduction_SloadThrallFix(ref bool __result, Thing t)
        {
            if (__result)
            {
                Corpse c = t as Corpse;
                if (c.InnerPawn.def is ThingDef_AlienRace a && !a.alienRace.compatibility.IsFlesh)
                {
                    __result = false;
                }
            }
        }
    }
}
