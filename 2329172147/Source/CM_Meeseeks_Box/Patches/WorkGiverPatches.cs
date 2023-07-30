using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class WorkGiverPatches
    {
        [HarmonyPatch(typeof(WorkGiver_CookFillHopper))]
        [HarmonyPatch("JobOnThing", MethodType.Normal)]
        public class WorkGiver_CookFillHopper_JobOnThing
        {
            [HarmonyPostfix]
            public static void Postfix(WorkGiver_CookFillHopper __instance, Pawn pawn, Thing thing, bool forced, Job __result)
            {
                if (__result != null && forced)
                {
                    CompMeeseeksMemory memory = pawn.GetComp<CompMeeseeksMemory>();

                    if (memory != null)
                    {
                        memory.AddToPotentialTargetCache(__instance, thing);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(WorkGiver_DoBill))]
        [HarmonyPatch("JobOnThing", MethodType.Normal)]
        public class WorkGiver_DoBill_JobOnThing
        {
            [HarmonyPostfix]
            public static void Postfix(WorkGiver_DoBill __instance, Pawn pawn, Thing thing, bool forced, Job __result)
            {
                if (__result != null && forced)
                {
                    CompMeeseeksMemory memory = pawn.GetComp<CompMeeseeksMemory>();

                    if (memory != null)
                    {
                        memory.AddToPotentialTargetCache(__instance, thing);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(WorkGiver_Miner))]
        [HarmonyPatch("JobOnThing", MethodType.Normal)]
        public class WorkGiver_Miner_JobOnThing
        {
            [HarmonyPostfix]
            public static void Postfix(WorkGiver_Miner __instance, Pawn pawn, Thing t, bool forced, Job __result)
            {
                if (__result != null && forced)
                {
                    CompMeeseeksMemory memory = pawn.GetComp<CompMeeseeksMemory>();

                    if (memory != null)
                    {
                        memory.AddToPotentialTargetCache(__instance, t);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(WorkGiver_Tame))]
        [HarmonyPatch("JobOnThing", MethodType.Normal)]
        public class WorkGiver_Tame_JobOnThing
        {
            [HarmonyPostfix]
            public static void Postfix(WorkGiver_Tame __instance, Pawn pawn, Thing t, bool forced, Job __result)
            {
                if (__result != null && forced)
                {
                    CompMeeseeksMemory memory = pawn.GetComp<CompMeeseeksMemory>();

                    if (memory != null)
                    {
                        memory.AddToPotentialTargetCache(__instance, t);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(WorkGiver_Train))]
        [HarmonyPatch("JobOnThing", MethodType.Normal)]
        public class WorkGiver_Train_JobOnThing
        {
            [HarmonyPostfix]
            public static void Postfix(WorkGiver_Train __instance, Pawn pawn, Thing t, bool forced, Job __result)
            {
                if (__result != null && forced)
                {
                    CompMeeseeksMemory memory = pawn.GetComp<CompMeeseeksMemory>();

                    if (memory != null)
                    {
                        memory.AddToPotentialTargetCache(__instance, t);
                    }
                }
            }
        }
    }
}
