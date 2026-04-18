using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using Verse.Noise;
using Verse.Grammar;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

// *Uncomment for Harmony*
// using System.Reflection;
// using HarmonyLib;

namespace MechIndependence
{

    [HarmonyPatch(typeof(WorkGiver_RepairMech), "ShouldSkip")]
    public static class WorkGiver_RepairMech_ShouldSkip_Patch
    {
        public static void Postfix(ref bool __result, Pawn pawn, bool forced = false)
        {
            if(pawn.RaceProps.IsMechanoid) {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Bill), "PawnAllowedToStartAnew")]
    public static class Bill_PawnAllowedToStartAnew_Patch
    {
        public static void Postfix(ref bool __result, ref RecipeDef ___recipe, Pawn p)
        {
            if(!__result && p.RaceProps.IsMechanoid) {
                if(___recipe.recipeUsers != null) {
                    foreach(ThingDef recipeUser in ___recipe.recipeUsers) {
                        if(recipeUser.defName == "SubcoreEncoder") {
                            __result = true;
                            break;
                        }
                    }
                }
                
            }
        }
    }

     [StaticConstructorOnStartup]
    public static class Start
    {
        static Start()
        {
            Harmony harmony = new Harmony("MechIndependence");
            harmony.PatchAll(Assembly.GetExecutingAssembly() );
        }
    }


}
