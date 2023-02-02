using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HediffApplier
{
    [HarmonyPatch]
    [HarmonyPatch(typeof(SkillRecord), "LearnRateFactor")]
    class XpHarmony_Patch
    {
        static float Postfix(float __result, SkillRecord __instance, bool direct)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (PawnChecker.MeetReq(pawn) && PawnChecker.HasHediff(pawn))
            {
                if (__instance.def.defName == "Melee" || __instance.def.defName == "Shooting")
                {
                    return __result * 0.708f;
                }
                if(__instance.def.defName == "Social")
                {
                    return __result * 1.167f;
                }
                if(__instance.def.defName=="Mining" || __instance.def.defName =="Construction")
                {
                    return __result * 0.949f;
                }
            }
            else if (PawnChecker.MeetReqMale(pawn))
            {
                if (__instance.def.defName == "Social")
                {
                    return __result * 0.922f;
                    
                }
            }
            return __result;
        }
    }
}
