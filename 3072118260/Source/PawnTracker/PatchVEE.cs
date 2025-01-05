using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld.QuestGen;
using System.Reflection;
using PawnTrackerMain;
using static PawnTrackerMain.PawnDeathDetails;
using static PawnTrackerMain.DocumentedEventDefOf;
using static PawnTrackerMain.PawnTrackerUtility;


// To add:
// HUNTING PARTY
namespace PawnTrackerMain.VEEPatches
{
    public static class Patches
    {
        // Delegate for dynamic postfix methods
        public delegate void DynamicPostfixDelegate(object __instance = null);

        // Dictionary to map method names to their corresponding delegates
        private static readonly Dictionary<string, DynamicPostfixDelegate> MethodDelegates = new Dictionary<string, DynamicPostfixDelegate>
        {
            { "VEE.RegularEvents.ShuttleCrash.TryExecuteWorker", new DynamicPostfixDelegate(ShuttleCrashPostfix) },
            { "VEE.RegularEvents.SpaceBattle.GameConditionTick", new DynamicPostfixDelegate(SpaceBattlePostfix) },
            { "VEE.RegularEvents.WandererJoinTraitor.TryExecuteWorker", new DynamicPostfixDelegate(WandererJoinTraitorPostfix)},
            { "VEE.RegularEvents.WildMenWanderIn.TryExecuteWorker", new DynamicPostfixDelegate(WildMenWanderInPostfix) },
        };

        public static void ApplyDynamicPatches(Harmony harmony)
        {
            if (ModsConfig.IsActive("VanillaExpanded.VEE"))
            {
                foreach (var kvp in MethodDelegates)
                {
                    var typeName = kvp.Key.Substring(0, kvp.Key.LastIndexOf('.'));
                    var methodName = kvp.Key.Substring(kvp.Key.LastIndexOf('.') + 1);
                    var type = AccessTools.TypeByName(typeName);
                    if (type != null)
                    {
                        var method = AccessTools.Method(type, methodName);
                        if (method != null)
                        {
                            // Ensure the method is public or at least internal
                            var postfixMethod = typeof(Patches).GetMethod(kvp.Value.Method.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                            if (postfixMethod == null)
                            {
                                Log.Warning($"Could not find method: {kvp.Value.Method.Name}");
                                continue;
                            }
                            var postfix = new HarmonyMethod(postfixMethod);
                            harmony.Patch(method, null, postfix);
                        }
                    }
                }
                Log.Message("VEE-specific patches applied successfully.");
            }
            else
                Log.Message("VEE not active, skipping VEE-specific patches.");
        }


        public static void ShuttleCrashPostfix(object __instance)
        {
            // Using reflection to get the value of 't' from the __instance
            var traverse = Traverse.Create(__instance);
            int t = traverse.Field("t").GetValue<int>();
            List<Pawn> newPawns = PawnTrackerUtility.GetPawns(colonistStatus: CSO.AnyNonColonistNonPrisoner);
            if (newPawns.Count != t)
            {
                Log.Error($"Found a differennt number of undocumented pawns than expected: found {newPawns.Count} vs expected {t}");
            }

            foreach (Pawn pawn in newPawns.Where(x => !x.Dead && x.Downed && x.Map != null && x.Map.IsPlayerHome).ToList())
            {
                ArriveEvent arriveEvent = new ArriveEvent(pawn, ShuttleCrashCivilian, priorFaction: pawn.Faction);
                var comp = pawn.GetComp<PawnTrackerMain.PawnTrackerComp>();
                comp.AddEvent(arriveEvent);
            }
        }
    

        public static void SpaceBattlePostfix(object __instance)
        {
            var traverse = Traverse.Create(__instance);
            List<Pawn> newPawns = PawnTrackerUtility.GetPawns(colonistStatus: CSO.AnyNonColonistNonPrisoner, deadOrAlive: DOAO.Alive);
            
                foreach (Pawn pawn in newPawns)
                {
                    ArriveEvent arriveEvent = new ArriveEvent(pawn, SpaceBattleCrash, priorFaction: pawn.Faction);
                    var comp = pawn.GetComp<PawnTrackerMain.PawnTrackerComp>();
                    comp.AddEvent(arriveEvent);
                }
        }

        public static void WandererJoinTraitorPostfix(object __instance = null)
        {
            List<Pawn> newPawns = PawnTrackerUtility.GetPawns(colonistStatus: CSO.AnyNonColonistNonPrisoner, developmentalStage: DSO.Adult);
            if (newPawns.Count > 1)
            {
                Log.Error("MORE THAN ONE NEW PAWN ON MAP");
            }

            List<Pawn> newTraitors = newPawns.Where(x => x.health.hediffSet.hediffs.Any(h => h.def.defName == "VEE_DefOf.Traitor" || h.def.defName == "Traitor")).ToList();

            foreach (Pawn pawn in newTraitors)
            {
                JoinEvent joinEvent = new JoinEvent(pawn, WandererJoins);
                var comp = pawn.GetComp<PawnTrackerMain.PawnTrackerComp>();
                comp.AddEvent(joinEvent);
            }
        }
    
        public static void WildMenWanderInPostfix(object __instance = null)
        {
            List<Pawn> newWildMen = PawnTrackerUtility.GetPawns(colonistStatus: CSO.AnyNonColonistNonPrisoner, pawnKind: PKO.WildMan, deadOrAlive: DOAO.Alive);
            
            if (!newWildMen.Any())
            {
                Log.Error("No new wildmen were found");
            }

            foreach (Pawn pawn in newWildMen)
            {
                ArriveEvent arriveEvent = new ArriveEvent(pawn, WildManWanderedIn);
                var comp = pawn.GetComp<PawnTrackerMain.PawnTrackerComp>();
                comp.AddEvent(arriveEvent);
            }
        }

        public static void HuntingPartyPostfix(object __instance)
        {
            List<Pawn> partyMembers = PawnTrackerUtility.GetPawns(colonistStatus: CSO.AnyNonColonistNonPrisoner, pawnKind: PKO.Humanlike, deadOrAlive: DOAO.Alive);
            
            if (!partyMembers.Any())
            {
                Log.Error("No hunting party members were found");
            }

            foreach (Pawn pawn in partyMembers)
            {
                ArriveEvent arriveEvent = new ArriveEvent(pawn, HuntingPartyArrived);
                var comp = pawn.GetComp<PawnTrackerMain.PawnTrackerComp>();
                comp.AddEvent(arriveEvent);
            }
        }
    }
}