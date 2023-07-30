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
    public static class WorldPawnsPatches
    {
        private static List<RecordDef> meeseeksImportantRecords = new List<RecordDef> { RecordDefOf.KillsHumanlikes, RecordDefOf.PawnsDownedHumanlikes, RecordDefOf.PrisonersRecruited, RecordDefOf.OperationsPerformed };

        [HarmonyPatch(typeof(WorldPawns))]
        [HarmonyPatch("PassToWorld", MethodType.Normal)]
        public class WorldPawns_PassToWorld
        {
            [HarmonyPrefix]
            public static void Prefix(Pawn pawn, ref PawnDiscardDecideMode discardMode)
            {
                if (pawn != null && discardMode != PawnDiscardDecideMode.Discard && pawn.GetComp<CompMeeseeksMemory>() != null)
                {
                    bool rememberMeeseeks = false;
                    if (pawn.records != null)
                    {
                        foreach(RecordDef importantRecord in meeseeksImportantRecords)
                        {
                            float recordValue = pawn.records.GetValue(importantRecord);
                            if (recordValue > 0)
                            {
                                rememberMeeseeks = true;
                                Logger.MessageFormat(pawn, "Remembering {0} because of {1} x{2}", pawn, importantRecord, recordValue);
                                break;
                            }
                        }
                    }
                    //Logger.MessageFormat(pawn, "Marking {0} to be discarded.", pawn);
                    if (!rememberMeeseeks)
                        discardMode = PawnDiscardDecideMode.Discard;
                }
            }
        }

        [HarmonyPatch(typeof(WorldPawns))]
        [HarmonyPatch("DiscardPawn", MethodType.Normal)]
        public class WorldPawns_DiscardPawn
        {
            [HarmonyPrefix]
            public static void Prefix(Pawn p, ref bool silentlyRemoveReferences)
            {
                if (p != null && !silentlyRemoveReferences && p.GetComp<CompMeeseeksMemory>() != null)
                {
                    Logger.MessageFormat(p, "Marking {0} to be discarded silently.", p);
                    silentlyRemoveReferences = true;
                }
            }
        }
    }
}
