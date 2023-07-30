using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class StatWorkerPatches
    {
        [HarmonyPatch(typeof(StatWorker))]
        [HarmonyPatch("FinalizeValue", MethodType.Normal)]
        public class StatWorker_FinalizeValue
        {
            [HarmonyPostfix]
            public static void Postfix(StatRequest req, ref float val, bool applyPostProcess, StatDef ___stat)
            {
                if (___stat == StatDefOf.PainShockThreshold)
                {
                    //Log.Message("Checking pain shock threshold");

                    Pawn pawn = req.Pawn;
                    if (pawn == null && req.Thing != null)
                        pawn = req.Thing as Pawn;

                    if (pawn != null)
                    {
                        CompMeeseeksMemory compMeeseeksMemory = pawn.GetComp<CompMeeseeksMemory>();

                        if (compMeeseeksMemory != null)
                        {
                            //Log.Message("Forcing Meeseeks pain shock threshold");
                            val = Mathf.Max(val, 5.0f);
                        }
                    }
                }
            }
        }
    }
}
