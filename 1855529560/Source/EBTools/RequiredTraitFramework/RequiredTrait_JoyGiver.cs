using RimWorld;
using Verse;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

namespace EBTools.RequiredTraitFramework
{
    
    
  
    
    //Override Default GetChance
    [HarmonyPatch(typeof(JoyGiver),"GetChance")]
    class RequiredTrait_JoyGiver_GetChance
    {
        static void Postfix(JoyGiver __instance,Pawn pawn,ref float __result)
        {
            __result *= RequiredTrait_Utils.GetChanceModifierFromTraits(__instance,pawn);
        }
    }
    
    //Override Drug GetChance
    [HarmonyPatch(typeof(JoyGiver_TakeDrug),"GetChance")]
    class RequiredTrait_JoyGiver_TakeDrug_GetChance
    {
        static void Postfix(JoyGiver_TakeDrug __instance,Pawn pawn,ref float __result)
        {
            __result *= RequiredTrait_Utils.GetChanceModifierFromTraits(__instance,pawn);
        }
    }

    //Override Skywatch Get Chance
    [HarmonyPatch(typeof(JoyGiver_Skygaze),"GetChance")]
    class RequiredTrait_JoyGiver_Skygaze_GetChance
    {
        static void Postfix(JoyGiver_Skygaze __instance,Pawn pawn,ref float __result)
        {
            __result *= RequiredTrait_Utils.GetChanceModifierFromTraits(__instance,pawn);
        }
    }
         
   




}
