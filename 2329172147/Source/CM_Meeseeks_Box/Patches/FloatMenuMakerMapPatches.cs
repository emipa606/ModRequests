using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class FloatMenuMakerMapPatches
    {
        [HarmonyPatch(typeof(FloatMenuMakerMap))]
        [HarmonyPatch("CanTakeOrder", MethodType.Normal)]
        public static class MeeseeksCanTakeOrder
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn pawn, ref bool __result)
            {
                //Logger.MessageFormat(pawn, "CanTakeOrder Postfix");

                if (pawn != null)
                {
                    CompMeeseeksMemory compMeeseeksMemory = pawn.GetComp<CompMeeseeksMemory>();

                    if (compMeeseeksMemory != null)
                    {
                        bool canTakeOrder = compMeeseeksMemory.CanTakeOrders();

                        Logger.MessageFormat(pawn, "CanTakeOrder Meeseeks: {0}", canTakeOrder.ToString());

                        if (!canTakeOrder)
                        {
                            __result = false;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(FloatMenuMakerMap))]
        [HarmonyPatch("ChoicesAtFor", MethodType.Normal)]
        static class FloatMenuMakerMap_ChoicesAtFor_Patch
        {
            [HarmonyPrefix]
            public static void Prefix(Vector3 clickPos, Pawn pawn)
            {
                // Ok this is a bit ridiculous but lets try it anyway... 
                //   We save all existing designations at this cell and remove them
                //   Then for every thing here, we create all designations for them
                //   So they should get flagged as a viable target for every possible job
                //   Wish me luck.
                if (pawn != null)
                {
                    CompMeeseeksMemory compMeeseeksMemory = pawn.GetComp<CompMeeseeksMemory>();

                    if (compMeeseeksMemory != null && compMeeseeksMemory.CanTakeOrders())
                    {
                        IntVec3 cell = IntVec3.FromVector3(clickPos);
                        DesignatorUtility.ForceAllDesignationsOnCell(cell, pawn.MapHeld);
                    }
                }
            }

            [HarmonyPostfix]
            public static void Postfix(List<FloatMenuOption> __result, Vector3 clickPos, Pawn pawn)
            {
                if (pawn != null)
                {
                    CompMeeseeksMemory compMeeseeksMemory = pawn.GetComp<CompMeeseeksMemory>();

                    if (compMeeseeksMemory != null && compMeeseeksMemory.CanTakeOrders())
                    {
                        IntVec3 intVec = IntVec3.FromVector3(clickPos);
                        FloatMenuOption guardOption = GuardLocationOption(compMeeseeksMemory, intVec, pawn);

                        if (guardOption != null)
                        {
                            __result.Add(guardOption);
                        }
                    }
                }
            }

            [HarmonyFinalizer]
            static Exception Finalizer(Exception __exception, Vector3 clickPos, Pawn pawn)
            {
                // Time to clean up our mess
                if (pawn != null)
                {
                    CompMeeseeksMemory compMeeseeksMemory = pawn.GetComp<CompMeeseeksMemory>();

                    if (compMeeseeksMemory != null)
                    {
                        IntVec3 cell = IntVec3.FromVector3(clickPos);
                        DesignatorUtility.RestoreDesignationsOnCell(cell, pawn.MapHeld);
                    }
                }

                return __exception;
            }

            private static FloatMenuOption GuardLocationOption(CompMeeseeksMemory compMeeseeksMemory, IntVec3 clickCell, Pawn pawn)
            {
                int num = GenRadial.NumCellsInRadius(2.9f);
                IntVec3 curLoc;
                for (int i = 0; i < num; i++)
                {
                    curLoc = GenRadial.RadialPattern[i] + clickCell;
                    if (!curLoc.Standable(pawn.Map))
                    {
                        continue;
                    }
                    if (curLoc != pawn.Position)
                    {
                        if (!pawn.CanReach(curLoc, PathEndMode.OnCell, Danger.Deadly))
                        {
                            return new FloatMenuOption("CannotGoNoPath".Translate(), null);
                        }
                        Action action = delegate
                        {
                            //IntVec3 guardPosition = RCellFinder.BestOrderedGotoDestNear(curLoc, pawn);
                            compMeeseeksMemory.guardPosition = curLoc;

                            Job job = JobMaker.MakeJob(JobDefOf.Goto, curLoc);
                            job.playerForced = true;

                            //if (pawn.Map.exitMapGrid.IsExitCell(UI.MouseCell()))
                            //{
                            //    job.exitMapOnArrival = true;
                            //}
                            //else if (!pawn.Map.IsPlayerHome && !pawn.Map.exitMapGrid.MapUsesExitGrid && CellRect.WholeMap(pawn.Map).IsOnEdge(UI.MouseCell(), 3) && pawn.Map.Parent.GetComponent<FormCaravanComp>() != null && MessagesRepeatAvoider.MessageShowAllowed("MessagePlayerTriedToLeaveMapViaExitGrid-" + pawn.Map.uniqueID, 60f))
                            //{
                            //    if (pawn.Map.Parent.GetComponent<FormCaravanComp>().CanFormOrReformCaravanNow)
                            //    {
                            //        Messages.Message("MessagePlayerTriedToLeaveMapViaExitGrid_CanReform".Translate(), pawn.Map.Parent, MessageTypeDefOf.RejectInput, historical: false);
                            //    }
                            //    else
                            //    {
                            //        Messages.Message("MessagePlayerTriedToLeaveMapViaExitGrid_CantReform".Translate(), pawn.Map.Parent, MessageTypeDefOf.RejectInput, historical: false);
                            //    }
                            //}

                            pawn.drafter.Drafted = true;
                            if (pawn.jobs.TryTakeOrderedJob(job))
                            {
                                MoteMaker.MakeStaticMote(curLoc, pawn.Map, ThingDefOf.Mote_FeedbackGoto);
                            }
                            else
                            {
                                pawn.drafter.Drafted = false;
                            }
                        };
                        return new FloatMenuOption("CM_Meeseeks_Box_GuardHere".Translate(), action, MenuOptionPriority.GoHere)
                        {
                            autoTakeable = false,
                            autoTakeablePriority = 10f
                        };
                    }
                    return null;
                }
                return null;
            }
        }

        //[HarmonyPatch(typeof(FloatMenuMakerMap))]
        //[HarmonyPatch("AddJobGiverWorkOrders_NewTmp", MethodType.Normal)]
        //static class FloatMenuMakerMap_AddJobGiverWorkOrders_NewTmp
        //{
        //    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        //    {
        //        List<CodeInstruction> list = instructions.ToList();

        //        MethodInfo addFloatMenuOption = SymbolExtensions.GetMethodInfo(() => new List<FloatMenuOption>().Add(default));

        //        int nextAddFloatMenuOptionIndex = list.FindIndex(0, (instruction => (instruction.Calls(addFloatMenuOption))));
        //        int foundInstructions = 0;

        //        while (nextAddFloatMenuOptionIndex >= 0 && nextAddFloatMenuOptionIndex < list.Count)
        //        {
        //            ++foundInstructions;
        //            ++nextAddFloatMenuOptionIndex;
        //            nextAddFloatMenuOptionIndex = list.FindIndex(nextAddFloatMenuOptionIndex, (instruction => (instruction.Calls(addFloatMenuOption))));
        //        }

        //        Log.Warning("Found " + foundInstructions.ToString() + " attempts to add FloatMenuOptions in AddJobGiverWorkOrders_NewTmp");

        //        foreach (CodeInstruction instruction in list)
        //            yield return instruction;
        //    }
        //}
    }
}
