using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class BillPatches
    {
        [HarmonyPatch(typeof(Bill))]
        [HarmonyPatch("DeletedOrDereferenced", MethodType.Getter)]
        public static class Bill_DeletedOrDereferenced
        {
            [HarmonyPrefix]
            public static bool Prefix(Bill __instance, ref bool __result)
            {
                MeeseeksBillStorage billStorage = Current.Game.World.GetComponent<MeeseeksBillStorage>();

                if (billStorage.IsDuplicate(__instance))
                {
                    __result = __instance.deleted;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Bill_Medical))]
        [HarmonyPatch("Notify_IterationCompleted", MethodType.Normal)]
        public static class Bill_Medical_Notify_IterationCompleted
        {
            [HarmonyPostfix]
            public static void Postfix(Bill_Medical __instance, Pawn billDoer)
            {
                CompMeeseeksMemory meeseeksMemory = billDoer.GetComp<CompMeeseeksMemory>();
                if (meeseeksMemory != null)
                {
                    foreach (SavedTargetInfo jobTarget in meeseeksMemory.jobTargets)
                    {
                        if (jobTarget.bill != null && !jobTarget.bill.deleted)
                        {
                            Bill_Medical billMedical = jobTarget.bill as Bill_Medical;

                            if (billMedical != null && billMedical.GiverPawn == __instance.GiverPawn && billMedical.Part == __instance.Part && billMedical.recipe.defName == __instance.recipe.defName)
                            {
                                jobTarget.bill.deleted = true;
                                return;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Bill_Production))]
        [HarmonyPatch("Notify_IterationCompleted", MethodType.Normal)]
        public static class Bill_Production_Notify_IterationCompleted
        {
            private static bool recursing = false;

            [HarmonyPostfix]
            public static void Postfix(ref Bill_Production __instance, Pawn billDoer, List<Thing> ingredients)
            {
                if (recursing == true)
                    return;// true;

                MeeseeksBillStorage billStorage = Current.Game.World.GetComponent<MeeseeksBillStorage>();

                Bill_Production originalBill = billStorage.GetOriginalBillFromDuplicate(__instance) as Bill_Production;
                Bill_Production duplicateBill = billStorage.GetDuplicateBillFromOriginal(__instance) as Bill_Production;

                if (originalBill == null && duplicateBill == null)
                {

                }
                // The finished iteration was on an original bill, doesn't matter who did it, mirror the result
                else if (duplicateBill != null)
                {
                    UpdateBill(duplicateBill);
                    return;// true;
                }
                // The finished iteration was on a duplicate bill, copy the result to the original, if it exists and make sure the duplicate is properly updated and check for deletion
                else if (billStorage.IsDuplicate(__instance))
                {
                    UpdateBill(__instance, false);

                    if (originalBill != null)
                    {
                        try
                        {
                            recursing = true;
                            originalBill.Notify_IterationCompleted(billDoer, ingredients);
                        }
                        finally
                        {
                            recursing = false;
                        }
                    }

                    //return false;
                }

                //return true;
            }

            private static void UpdateBill(Bill_Production bill, bool decrement = true)
            {
                if (bill.repeatMode == BillRepeatModeDefOf.RepeatCount)
                {
                    if (decrement && bill.repeatCount > 0)
                    {
                        bill.repeatCount--;
                    }
                    if (bill.repeatCount == 0)
                    {
                        bill.deleted = true;
                    }
                }
                else if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
                {
                    int productCount = bill.recipe.WorkerCounter.CountProducts(bill);
                    if (productCount >= bill.targetCount)
                    {
                        bill.deleted = true;
                    }
                }

                if (decrement)
                {
                    Bill_ProductionWithUft billWithUft = bill as Bill_ProductionWithUft;
                    if (billWithUft != null)
                        billWithUft.ClearBoundUft();
                }
            }
        }
    }
}
