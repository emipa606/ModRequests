using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using CM_Callouts.PendingCallouts.Interaction;

namespace CM_Callouts.Patches
{
    [StaticConstructorOnStartup]
    public static class JobDriver_InteractAnimal_Patches
    {
        [HarmonyPatch(typeof(JobDriver_InteractAnimal))]
        [HarmonyPatch("StartFeedAnimal", MethodType.Normal)]
        public static class JobDriver_InteractAnimal_StartFeedAnimal
        {
           
            [HarmonyPostfix]
            public static void Postfix(JobDriver_InteractAnimal __instance, TargetIndex tameeInd, Toil __result)
            {
                if (__result == null)
                    return;

                __result.AddPreInitAction(delegate
                {
                    try
                    {
                        //FoodUtility.BestFoodInInventory();
                        Pawn initiator = __instance.GetActor();
                        Pawn recipient = initiator.CurJob.GetTarget(tameeInd).Pawn;
                        MethodInfo methodInfo = typeof(FoodUtility).GetMethod("BestFoodInInventory", BindingFlags.Public | BindingFlags.Static);
                        Thing thing;
                        if (methodInfo.GetParameters().Length == 6)
                        {
                             thing = (Thing)methodInfo.Invoke(null, new object[] { initiator, recipient, FoodPreferability.NeverForNutrition, FoodPreferability.RawTasty, 0f, false});
                        }
                        else //ugly fucking harmony hack for unstable/normal branch compat for 1.4, 16.11.2022
                        {
                            thing = (Thing)methodInfo.Invoke(null, new object[] { initiator, recipient, FoodPreferability.NeverForNutrition, FoodPreferability.RawTasty, 0f, false, false});
                        }

                        if (thing != null)
                        {
                            new PendingCalloutEventAnimalInteractionWithFood(initiator, recipient, thing.def, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Feed_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Feed_Received).AttemptCallout();
                        }
                    } catch(Exception e){
                        Log.Error("Exception in Callouts: " + e.ToString());
                    }
                });
            }
        }
    }
}