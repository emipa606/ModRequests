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
using static PawnTrackerMain.PawnDeathDetails;
using static PawnTrackerMain.DocumentedEventDefOf;
using static PawnTrackerMain.PawnTrackerUtility;

/*
TO ADD:


- Prison break...?
- caravan ambush 
    CapturedCaravanAmbush
- Became an older brother/sister/sibling

- We might need to patch IncidentWorker_VisitorGroup? I feel like the caravan situation should already handle it, but idk

- Hospitality -> stay as guest\
    - recruited
    - rescue -> guest


TO DO:
- SignalAction_Ambush - DoAction() (check for pawns after this happens)
- Shuttle Crash

PROBLEMS:
- "Present during off-map quest" documnted repeatedly
- Doesn't document loss of limb if it's from installing artificial
*/



namespace PawnTrackerMain.CommonPatches
{

    /*public class PawnTrackerInitializer : Mod
	{
		public PawnTrackerInitializer(ModContentPack content) : base(content)
		{
			var harmony = new Harmony("rimworld.mod.PawnTrackerMain");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}*/

    [HarmonyPatch(typeof(Faction), "Notify_MemberExitedMap")]
    public static class MemberExitedMap_Patch
    {
        public static void Postfix(Pawn member, bool free)
        {
            if (!member.RaceProps.Humanlike)
                return;

            if (member.PawnTrackerComp().MostRecentEvent() == null)
            {
                return;
            }
            if ((member.PawnTrackerComp().MostRecentEvent().def == Captured || member.PawnTrackerComp().MostRecentEvent().def == CapturedAbroad) && !member.IsPrisoner)
            {
                LeaveEvent releaseEvent = new LeaveEvent(member, PrisonerReleased);
                var comp = member.PawnTrackerComp();
                comp.AddEvent(releaseEvent);
            }
            
        }
    }

    [HarmonyPatch(typeof(Faction), "Notify_MemberCaptured")]
    public static class MemberCaptured_Patch
    {
        public static void Postfix(Pawn member, Faction violator)
        {
            LifeEvent captureEvent;
            if (member == null)
            {
                return;
            }
            captureEvent = new LifeEvent(member, member.Map == null ? Captured : member.Map.IsPlayerHome ? Captured : CapturedAbroad, priorFaction: violator);
            var comp = member.PawnTrackerComp();

            //If we had just "rescued" them and now we're capturing them, we should never have "rescued" them
            if (comp.MostRecentEvent() != null && (comp.MostRecentEvent().def == Rescued || comp.MostRecentEvent().def == UnknownLeave))
            {
                comp.RemovePreviousEvent();
            }
            comp.AddEvent(captureEvent);
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kills_Patch
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
        {
            if (__instance == null)
            {
                LogPTMDev.Warning("Pawn was null before we had a chance to get their death details");
                return;
            }

            if (!__instance.RaceProps.Humanlike)
                return;

            if (!__instance.Dead)
                return;

            if (!CHGameComp.PawnDocumented(__instance) && !__instance.IsColonist && !__instance.IsPrisonerOfColony)
                return;

            bool stillborn = __instance.health.hediffSet.hediffs.Any(h => h.def == HediffDefOf.Stillborn) ? true : false;
            if (stillborn == true)
            {
                return;
            }

            var comp = __instance.PawnTrackerComp();

            if (dinfo != null)
            {
                if (exactCulprit == null && !CHGameComp.PawnDocumented(__instance))
                {
                    // Pawn showed up dead --> no need to document death
                    return;
                }
                else if (exactCulprit == null)
                {
                    Log.Warning($"{__instance.Name} is dead with no culprit");
                }
                PawnDeathDetails deathDetails = ExtractDeathDetails(dinfo, exactCulprit, true);
                DeathEvent deathEvent = new DeathEvent(__instance, deathDetails.instigator != null ? Killed : Died, deathDetails);
                comp.shouldBeHere = false;
                comp.AddEvent(deathEvent);
            }
            else if (exactCulprit != null)
            {
                PawnDeathDetails deathDetails = ExtractDeathDetails(dinfo: null, exactCulprit: exactCulprit, true);
                DeathEvent deathEvent = new DeathEvent(__instance, Died, deathDetails);
                
                comp.shouldBeHere = false;
                comp.AddEvent(deathEvent);
            }
            else
            {
                Log.Message("Dead, no info or culprit");
                __instance.PawnTrackerComp().AddEvent(new DeathEvent(__instance, DocumentedEventDefOf.UnknownDeath));
            }
                
        }
    }

    [HarmonyPatch(typeof(ExecutionUtility), "DoExecutionByCut")]
    public static class ExecutionUtility_Patch
    {
        public static void Postfix(Pawn executioner, Pawn victim, int bloodPerWeight, bool spawnBlood)
        {
            if (victim == null)
            {
                LogPTMDev.Error("????");
                return;
            }
                
            if (!victim.RaceProps.Humanlike)
            {
                return;
            }

            var comp = victim.PawnTrackerComp();
            DeathEvent newEvent = new DeathEvent(victim, Executed);
            comp.AddEvent(newEvent);
        }
    }

    [HarmonyPatch(typeof(GuestUtility), "Notify_PrisonerEscaped")]
    public static class PrisonerEscaped_Patch
    {
        public static void Postfix(Pawn prisoner)
        {
            if (prisoner == null)
                return;

            LeaveEvent escapeEvent = new LeaveEvent(prisoner, PrisonerEscaped);
            var comp = prisoner.PawnTrackerComp();
            comp.AddEvent(escapeEvent);
        }
    }

    [HarmonyPatch(typeof(RecruitUtility), "Recruit")]
    public static class RecruitUtility_Recruit_Patch
    {
        
        public static void Prefix(Pawn pawn, Faction faction, Pawn recruiter = null)
        {
            if (pawn == null)
                return;

            JoinEvent recruitmentEvent = null;
            if (!pawn.RaceProps.Humanlike)
                return;
            if (pawn.IsPrisonerOfColony)
            {
                recruitmentEvent = new JoinEvent(pawn, Recruited, pawn.Faction, recruiter);
            }
            else if (pawn.IsWildMan())
            {
                recruitmentEvent = new JoinEvent(pawn, Tamed, (Faction) null, recruiter);
            }
            else
            {
                Log.Error("Pawn was recruited but wasn't a prisoner and wasn't a wild man?");
                return;
            }
            var comp = pawn.PawnTrackerComp();
            comp.AddEvent(recruitmentEvent);
        }
    }

    
    [HarmonyPatch(typeof(IncidentWorker_WildManWandersIn), "TryExecuteWorker")]
    public static class WildManWandersIn_Patch
    {
        public static void Postfix(IncidentWorker_WildManWandersIn __instance)
        {
            List<Pawn> allPawns = PawnsFinder.AllMapsAndWorld_Alive;

            foreach (Pawn pawn in allPawns.Where(x => x != null && x.IsWildMan()))
            {
                if (!CHGameComp.PawnDocumented(pawn) && !pawn.Dead)
                {
                    ArriveEvent arriveEvent = new ArriveEvent(pawn, WildManWanderedIn);
                    var comp = pawn.PawnTrackerComp();
                    comp.AddEvent(arriveEvent);
                }
            }
        }
    }

    [HarmonyPatch(typeof(QuestNode_Root_RefugeePodCrash_Baby), "AddSpawnPawnQuestParts")]
    public static class RefugeePodCrash_Baby_Patch
    {
        public static void Postfix(QuestNode_Root_RefugeePodCrash_Baby __instance, Quest quest, Map map, Pawn pawn)
        {
            if (pawn == null || !pawn.RaceProps.Humanlike)
                return;
            if (!pawn.Dead)
            {
                ArriveEvent arriveEvent = new ArriveEvent(pawn, TransportPodCrash, quest: quest, priorFaction: pawn.Faction);
                var comp = pawn.PawnTrackerComp();
                comp.AddEvent(arriveEvent);
            }

        }
    }

    [HarmonyPatch(typeof(QuestNode_Root_RefugeePodCrash), "SendLetter")]
    public static class RefugeePodCrash_Patch
    {
        public static void Postfix(QuestNode_Root_RefugeePodCrash __instance, Quest quest, Pawn pawn)
        {
            if (pawn == null || !pawn.RaceProps.Humanlike)
                return;
            if (!pawn.Dead)
            {
                ArriveEvent arriveEvent = new ArriveEvent(pawn, TransportPodCrash, priorFaction: pawn.Faction);
                var comp = pawn.PawnTrackerComp();
                comp.AddEvent(arriveEvent);
            }
        }
    }


    [HarmonyPatch(typeof(QuestNode_Root_WandererJoinAbasia), "AddSpawnPawnQuestParts")]
    public static class WandererJoinAbasia_Patch
    {
        public static void Postfix(QuestNode_Root_WandererJoinAbasia __instance, Quest quest, Map map, Pawn pawn)
        {
            string signalAccept = Traverse.Create(__instance).Field("signalAccept").GetValue<string>();
            quest.Signal(signalAccept, (Action) (() =>
            {
                JoinEvent joinEvent = new JoinEvent(pawn, WandererJoins, quest: quest);
                var comp = pawn.PawnTrackerComp();
                comp.AddEvent(joinEvent);
            }));

        }
    }

    [HarmonyPatch(typeof(Quest), "End")]
    public static class Quest_End_Patch
    {
        public static void Postfix(Quest __instance, QuestEndOutcome outcome, bool sendLetter)
        {
            if (__instance.root == QuestScriptDefOf.WandererJoins)
            {
                if (__instance.State == QuestState.EndedSuccess)
                {
                    List<Pawn> newPawns = PawnTrackerUtility.GetPawns(colonistStatus: CSO.Colonist);
                    foreach (Pawn pawn in newPawns)
                    {
                        JoinEvent joinEvent = new JoinEvent(pawn, WandererJoins, quest: __instance);
                        pawn.PawnTrackerComp()?.AddEvent(joinEvent);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "GenerateRaidLoot")]
    public static class IncidentWorker_RaidEnemy_Patch
    {
        public static void Prefix(IncidentParms parms,float raidLootPoints,List<Pawn> pawns)
        {
            if (!pawns.Any(p => p != null))
                return;
                
            GameRaidEvent newRaid = new GameRaidEvent(pawns[0].Faction, pawns.Where(p => p != null).ToList());
            GetComp().AddEvent(newRaid);
            GetComp().currentRaid = newRaid;

            foreach (Pawn pawn in pawns.Where(p => p != null))
            {
                if (!pawn.RaceProps.Humanlike)
                    continue;
                ArriveEvent arriveEvent = new ArriveEvent(pawn, Raid, priorFaction: pawn.Faction);
                var comp = pawn.PawnTrackerComp();
                comp.AddEvent(arriveEvent);
            }
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_RaidFriendly), "GetLetterText")]
    public static class IncidentWorker_RaidFriendly_Patch
    {
        public static void Prefix(IncidentParms parms,List<Pawn> pawns)
        {
            foreach (Pawn pawn in pawns.Where(p => p != null))
            {
                if (pawn == null || !pawn.RaceProps.Humanlike)
                    continue;
                ArriveEvent arriveEvent = new ArriveEvent(pawn, FriendliesArrived, priorFaction: pawn.Faction);
                var comp = pawn.PawnTrackerComp();
                comp.AddEvent(arriveEvent);
            }
        }
    }


    [HarmonyPatch(typeof(Designator_Adopt), "DesignateThing")]
    public static class Designator_Adopt_Patch
    {
        public static void Postfix(Designator_Adopt __instance, Thing t)
        {
            JoinEvent adoptEvent = new JoinEvent((Pawn) t, Adopted);
            Pawn pawn = (Pawn) t;
            var comp = pawn.PawnTrackerComp();
            comp.AddEvent(adoptEvent);
        }
    }

    [HarmonyPatch(typeof(Hediff_LaborPushing), "PreRemoved")]
    public static class GeneratePawn_Patch
    {
        public static void Postfix(Hediff_LaborPushing __instance)
        {
            if (!GetComp().UseMod)
                return;
            List<Pawn> newNewborns = PawnTrackerUtility.GetPawns(developmentalStage: DSO.Baby, pawnKind: PKO.Humanlike, deadOrAlive: DOAO.Any);
            
            foreach (Pawn baby in newNewborns)
            {
                Pawn mother = baby.GetMother();
                
                if (baby == null)
                {
                    Log.Message("Baby is null");
                    continue;
                }

                bool prisoner = mother != null && !mother.Dead && mother.IsPrisoner;
                bool healthy = baby.health.hediffSet.hediffs.Any(h => h.def == HediffDefOf.InfantIllness) ? false : true;
                bool stillborn = baby.Dead || baby.health.hediffSet.hediffs.Any(h => h.def == HediffDefOf.Stillborn) ? true : false;
                var comp = baby.PawnTrackerComp();
                var momComp = mother != null ? mother.PawnTrackerComp() : new PawnTrackerComp();

                if (stillborn == false)
                {
                    JoinEvent bornEvent;
                    ArriveEvent arriveEvent;

                    if (healthy == true)
                    {
                        if (prisoner == true)
                            {
                                arriveEvent = new ArriveEvent(baby, BornToPrisoner);
                                comp.AddEvent(arriveEvent);

                                try {momComp.AddEvent(new LifeEvent(mother, GaveBirthHealthy, otherPawns: newNewborns));}
                                catch {}
                            }
                        else //success
                            {
                                bornEvent = new JoinEvent(baby, BornToColonist);
                                comp.AddEvent(bornEvent);
                                
                                try {momComp.AddEvent(new LifeEvent(mother, GaveBirthHealthy, otherPawns: newNewborns));}
                                catch {}
                            }
                    }
                    else
                    {
                        if (prisoner == true)
                            {
                                arriveEvent = new ArriveEvent(baby, BornToPrisonerSickly);
                                comp.AddEvent(arriveEvent);

                                try {momComp.AddEvent(new LifeEvent(mother, GaveBirthSickly, otherPawns: newNewborns));}
                                catch {}
                            }
                        else
                            {
                                bornEvent = new JoinEvent(baby, BornToColonistSickly);
                                comp.AddEvent(bornEvent);

                                try {momComp.AddEvent(new LifeEvent(mother, GaveBirthSickly, otherPawns: newNewborns));}
                                catch {}
                            }
                    }
                    return;
                }
                else if (prisoner == false)
                {
                    DeathEvent stillbornEvent = new DeathEvent(baby, Stillborn, (PawnDeathDetails) null);
                    comp.AddEvent(stillbornEvent);

                    try {momComp.AddEvent(new LifeEvent(mother, GaveBirthStillborn, otherPawns: newNewborns));}
                    catch {}
                    return;
                }
                return;
            }
        }
    }

    /*                         */
    /* VANILLA EVENTS EXPANDED */
    /*                         */

    

    
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "Notify_RescuedBy")]
    public class RelationsTracker_RescuedBy_Patch
    {
        [HarmonyPrefix]
        public static void Postfix(Pawn_RelationsTracker __instance, Pawn rescuer)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn == null)
            {
                return;
            }
            
            if (pawn.Faction.IsPlayer) return;


            var comp = pawn.PawnTrackerComp();
            if (comp != null)
            {
                //If we tagged them as having suddenly disappeared, remove the sudden-disappearance event
                if (comp.MostRecentEvent() != null && comp.MostRecentEvent().eventType == "Leave")
                    comp.RemovePreviousEvent();
                //If they were just captured, they were not rescued!
                if (!pawn.IsPrisonerOfColony && !pawn.PawnTrackerComp().everWasPrisoner && comp.MostRecentEvent() != null && (comp.MostRecentEvent().def == Captured || comp.MostRecentEvent().def == CapturedAbroad))
                    return;
                
                LifeEvent rescuedEvent = new LifeEvent(pawn, Rescued);
                comp.AddEvent(rescuedEvent);
            }
        }
    }

    [HarmonyPatch(typeof(KidnappedPawnsTracker), "Kidnap")]
    public class KidnappedPawnsTracker_Kidnap_Patch
    {
        public static void Postfix(KidnappedPawnsTracker __instance, Pawn pawn, Pawn kidnapper)
        {
            if (pawn == null)
                return;

            LeaveEvent kidnapEvent = new LeaveEvent(pawn, Kidnapped, kidnappers: kidnapper.Faction);
            var comp = pawn.PawnTrackerComp();
            if (comp.MostRecentEvent().def == UnknownLeave)
            {
                comp.documentedEvents.Remove(comp.documentedEvents.Last());
            }
            comp.AddEvent(kidnapEvent);
            comp.shouldBeHere = false;

            
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_WandererJoin), "GeneratePawn")]
    public class IncidentWorker_WandererJoin_Patch
    {
        public static void Postfix(IncidentWorker_WandererJoin __instance, ref Pawn __result)
        {
            IncidentDef def = Traverse.Create(__instance).Field("def").GetValue<IncidentDef>();
            if (def.defName.ToString() == "StrangerInBlackJoin")
            {
                JoinEvent joinEvent = new JoinEvent(__result, StrangerInBlackJoin);
                var comp = __result.PawnTrackerComp();
                comp.AddEvent(joinEvent);
            }           
            
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_TraderCaravanArrival), "TryExecuteWorker")]
    public class TraderCaravanArrival_Patch
    {
        public static void Postfix(IncidentParms parms)
        {
            List<Pawn> newPawns = GetPawns(colonistStatus: CSO.AnyNonColonistNonPrisoner, pawnKind: PKO.Humanlike);
            foreach (Pawn pawn in newPawns)
            {
                ArriveEvent arriveEvent = new ArriveEvent(pawn, TradeCaravanArrived);
                var comp = pawn.PawnTrackerComp();
                comp.AddEvent(arriveEvent);
            }
        }
    }

    [HarmonyPatch(typeof(GuestUtility), "IsSellingToSlavery")]
    public class GuestUtility_SellToSlavery_Patch
    {
        public static void Postfix(Pawn slave, Faction slaveOwner)
        {
            if (slave == null || !slave.RaceProps.Humanlike)
                return;
            var comp = slave.PawnTrackerComp();
            comp.AddEvent(new LeaveEvent(slave, Sold));
            LogPTMDev.Message("Sold pawn to another faction");
        }
    }

    

    [HarmonyPatch(typeof(Settlement_TraderTracker), "GiveSoldThingToPlayer")]
    public class Settlement_TraderTracker_Patch
    {
        public static void Postfix(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            if (toGive != null && toGive is Pawn pawn)
            {
                if (!pawn.RaceProps.Humanlike)
                    return;
                pawn.PawnTrackerComp()?.AddEvent(new JoinEvent(pawn, Bought));
            }
        }
    }

    [HarmonyPatch(typeof(Caravan_TraderTracker), "GiveSoldThingToPlayer")]
    public class Caravan_TraderTracker_Patch
    {
        public static void Postfix(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            if (toGive != null && toGive is Pawn pawn)
            {
                if (!pawn.RaceProps.Humanlike)
                    return;
                pawn.PawnTrackerComp()?.AddEvent(new JoinEvent(pawn, Bought));
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_TraderTracker), "GiveSoldThingToPlayer")]
    public class Pawn_TraderTracker_Patch
    {
        public static void Postfix(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            if (toGive != null && toGive is Pawn pawn)
            {
                if (!pawn.RaceProps.Humanlike)
                    return;
                pawn.PawnTrackerComp()?.AddEvent(new JoinEvent(pawn, Bought));
            }
        }
    }

    [HarmonyPatch(typeof(TradeShip), "GiveSoldThingToPlayer")]
    public class TradeShip_Patch
    {
        public static void Postfix(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            if (toGive != null && toGive is Pawn pawn)
            {
                if (!pawn.RaceProps.Humanlike)
                    return;
                pawn.PawnTrackerComp()?.AddEvent(new JoinEvent(pawn, Bought));
            }
        }
    }


    [HarmonyPatch(typeof(Pawn_GuestTracker), "Notify_PawnUndowned")]
    public class GuestTracker_Undowned_Patch
    {
        public static void Postfix(Pawn_GuestTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn == null)
                return;
            if (pawn.IsPrisonerOfColony)
                return;
            if (pawn.PawnTrackerComp().everWasPrisoner && !pawn.PawnTrackerComp().everWasColonist)
                return;
            if (!pawn.RaceProps.Humanlike)
                return;
            if (pawn.PawnTrackerComp().EverJoined())
                return;
            if (pawn.Faction != null && pawn.Faction == Faction.OfPlayer)
            {
                pawn.PawnTrackerComp()?.AddEvent(new JoinEvent(pawn, JoinAfterRescue));
            }

        }
    }

    [HarmonyPatch(typeof(Pawn),"ExitMap")]
    public class Pawn_ExitMap_Patch
    {
        public static void Postfix(Pawn __instance, bool allowedToJoinOrCreateCaravan, Rot4 exitDir)
        {
            if (!GetComp().UseMod)
                return;
            if (__instance == null)
            {
                Log.Error("Null __instance on Pawn_ExitMap");
                return;
            }
            
            if (__instance.RaceProps == null)
                return;
            if (!__instance.RaceProps.Humanlike)
                return;
            var comp = __instance.PawnTrackerComp();
            if (__instance.IsColonist || __instance.IsPrisonerOfColony)
            {
                if (__instance.GetCaravan() != null || __instance.IsCaravanMember())
                {
                    comp.shouldBeHere = true;
                    return;
                }
            }        

            //We want to ALLOW UnknownLeave events unless they legit left with a caravan
            //This way it won't be an issue if there's events we (actual literal dev, not conceptual) didn't think to patch
            comp.shouldBeHere = false; 

            if (!__instance.IsColonist)
            {
                LeaveEventDef exitMapType = comp.ExitMapType();
                if (exitMapType == null)
                    return;
                LeaveEvent newLeave = new LeaveEvent(__instance, exitMapType);
                comp.shouldBeHere = false;
                if (newLeave == null)
                    return;
                comp.AddEvent(newLeave);
            }
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_VisitorGroup),"TryExecuteWorker")]
    public static class TravelerGroup_ExecuteWorker_Patch
    {
        public static void Postfix()
        {
            List<Pawn> newPawns = GetPawns(colonistStatus: CSO.Friendly);
            foreach (Pawn pawn in newPawns)
            {
                var comp = pawn.PawnTrackerComp();
                comp.AddEvent(new ArriveEvent(pawn, VisitorGroupArrived));
            }
        }
    }

    [HarmonyPatch(typeof(LetterStack), "ReceiveLetter", new Type[] {typeof(Letter), typeof(string)})]
    public static class LetterStack_Patch
    {
        public static void Postfix(Letter let, string debugInfo)
        {
            //If we got a ransom letter, document when it arrived & the fact that we might get a pawn
            if (let.Label == "Ransom demand")
            {
                GetComp().expectRansomed = true;
                GetComp().expectRansomedTick = GenTicks.TicksAbs;
            }
            else if (let.Label == "Rescuee joins")
            {
                List<Pawn> newPawns = PawnTrackerUtility.GetPawns(colonistStatus: CSO.Colonist, documentedStatus: PDSO.Any);
                foreach (Pawn pawn in newPawns)
                {
                    if (!pawn.PawnTrackerComp().EverJoined())
                    {
                        pawn.PawnTrackerComp().AddEvent(new JoinEvent(pawn, RescuedQuest));
                    }
                }
            }
            else if (let.Label == "Payment arrived")
            {
                GetComp().expectReward = true;
                GetComp().expectRewardTick = GenTicks.TicksAbs;
            }   
        }
    }

    [HarmonyPatch(typeof(LetterStack), "ReceiveLetter", new Type[] {typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(Quest), typeof(List<ThingDef>), typeof(string)})]
    public static class LetterStack_Patch2
    {
        public static void Postfix(
            TaggedString label,
            TaggedString text,
            LetterDef textLetterDef,
            LookTargets lookTargets,
            Faction relatedFaction,
            Quest quest,
            List<ThingDef> hyperlinkThingDefs,
            string debugInfo
        )
        {
            
            if (label == "Rescuee joins")
            {
                List<Pawn> newPawns = PawnTrackerUtility.GetPawns(colonistStatus: CSO.Colonist, documentedStatus: PDSO.Any);
                foreach (Pawn pawn in newPawns)
                {
                    if (!pawn.PawnTrackerComp().EverJoined())
                    {
                        pawn.PawnTrackerComp().AddEvent(new JoinEvent(pawn, RescuedQuest, quest: quest));
                    }
                }
            }
            else if (label == "Payment arrived")
            {
                GetComp().expectReward = true;
                GetComp().expectRewardTick = GenTicks.TicksAbs;
                GetComp().expectRewardQuest = quest;
                if (lookTargets != null && lookTargets.targets.Any())
                {
                    foreach (GlobalTargetInfo gti in lookTargets.targets)
                    {
                        if (gti.Thing is Pawn pawn)
                            LogPTMDev.Message($"Pawn: {pawn.Name}");
                    }
                }
            }  
        }
    }

    [HarmonyPatch(typeof(TaleRecorder), "RecordTale")]
    public static class TaleRecorder_RecordTale_Patch
    {
        public static void Postfix(ref Tale __result, TaleDef def, params object[] args)
        {
            if (!GetComp().UseMod)
                return;
            if (def == TaleDefOf.LandedInPod && args[0] is Pawn pawn && pawn != null && pawn.Faction == Faction.OfPlayer)
            {
                if (GetComp().expectRansomed == true && pawn.PawnTrackerComp().MostRecentEvent().def == Kidnapped)
                {
                    pawn.PawnTrackerComp().AddEvent(new JoinEvent(pawn, RansomReturn));
                    GetComp().expectRansomed = false;
                    GetComp().expectRansomedTick = 0;
                }
            }
        }
    }

    [HarmonyPatch(typeof(QuestPart_JoinPlayer), "Notify_QuestSignalReceived")]
    public static class QuestPart_JoinPlayer_SignalReceived_Patch
    {
        public static void Prefix(QuestPart_JoinPlayer __instance, Signal signal)
        {
            if (__instance.joinPlayer && __instance.pawns.Any(p => p != null))
            {
                foreach (Pawn p in __instance.pawns.Where(p => p != null))
                {
                    p.PawnTrackerComp().AddEvent(new JoinEvent(p, QuestReward, quest: __instance.quest));
                }
            }
            if (__instance.makePrisoners && __instance.pawns.Any(p => p != null))
            {
                foreach (Pawn p in __instance.pawns.Where(p => p != null))
                {
                    p.PawnTrackerComp().AddEvent(new LifeEvent(p, CapturedQuestReward));
                }
            }
        }

        [HarmonyPatch(typeof(QuestGen_Rewards), "GiveRewards")]
        public static class QuestGen_Rewards_Patch
        {
            public static void Postfix(ref QuestPart_Choice __result)
            {
                if (__result == null)
                {
                    return;
                }
                List<QuestPart_Choice.Choice> choices = __result.choices;
                if (choices == null)
                {
                    return;
                }    
                else if (!choices.Any())
                {
                    return;
                }

                List<Reward> rewards = choices[0].rewards;
                
                foreach (Reward reward in rewards)
                {
                    if (reward is Reward_Pawn reward_pawn)
                    {
                        Pawn pawn = reward_pawn.pawn;
                        pawn.PawnTrackerComp().AddEvent(new JoinEvent(pawn, QuestReward));
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard")]
        public static class DrawCharacterCard_Patch
        {
            public static void Postfix(Rect rect, Pawn pawn, Action randomizeCallback, Rect creationRect, bool showName)
            {
                if (!GetComp().UseMod || pawn == null)
                {
                    return;
                }
                Rect rect1 = new Rect(rect.xMax - 40f, 20f, 24f, 24f);
                Texture2D buttonTex = ContentFinder<Texture2D>.Get("UI/Icons/PawnHistory_Icon", true);
                if (pawn != null && !pawn.Dead && !pawn.PawnTrackerComp().ResurrectAfterDeath()) // We'll want to open the Dead version, if the pawn is .. yknow .. dead
                {
                    if (Widgets.ButtonImage(rect1, buttonTex, Color.white, GenUI.MouseoverColor))
                    {
                        //Find.WindowStack.Add(new Dialog_PawnTracker_Details(pawn, showAllEvents: pawn.IsColonist || pawn.IsPrisonerOfColony ? false : true));
                        Find.WindowStack.Add(new Dialog_PawnTracker_Details(pawn, showAllEvents: pawn.IsColonist || pawn.IsPrisonerOfColony ? true : false));
                        //Log.Message($"Character card situation: pawn is null: {pawn == null}; pawn name: {pawn.Name}; pawn is dead: {pawn.Dead}; pawn is colonist: {pawn.IsColonist}; pawn is prisoner: {pawn.IsPrisoner}; shouldBeHere: {pawn.PawnTrackerComp().shouldBeHere}; in tracked pawns: {GetComp().TrackedPawns.Contains(pawn)}");
                    }
                }
                else if (pawn != null && GetComp().DeadTrackedPawns.ContainsKey(pawn.Name.ToString())) // If the pawn is null or hasn't been tracked... we can't open their info thingy
                {
                    DeadPawnComp deadComp = GetComp().DeadTrackedPawns[pawn.Name.ToString()];
                    if (Widgets.ButtonImage(rect1, buttonTex, Color.white, GenUI.MouseoverColor))
                    {
                        //Find.WindowStack.Add(new Dialog_PawnTracker_Details(pawn, showAllEvents: pawn.IsColonist || pawn.IsPrisonerOfColony ? false : true));
                        Find.WindowStack.Add(new Dialog_PawnTracker_Details_Dead(deadComp, showLifeEvents: true));
                        //Log.Message($"Character card situation: pawn is null: {pawn == null}; pawn name: {pawn.Name}; pawn is dead: {pawn.Dead}; pawn is colonist: {pawn.IsColonist}; pawn is prisoner: {pawn.IsPrisoner}; shouldBeHere: {pawn.PawnTrackerComp().shouldBeHere}; in tracked pawns: {GetComp().TrackedPawns.Contains(pawn)}");
                    }
                }
                    
                
            }
        }
    }

    [HarmonyPatch(typeof(Map), "FinalizeInit")]
    public static class Map_FinalizeInit_Patch
    {
        public static void Postfix(Map __instance)
        {
            if (__instance != null)
                PawnTrackerUtility.DocumentUndocumentedChanges(__instance);
        }
    }

    [HarmonyPatch(typeof(Recipe_RemoveBodyPart), "ApplyOnPawn")]
    public static class RemoveBodyPart_Patch
    {
        public static void Prefix(Recipe_RemoveBodyPart __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (!GetComp().UseMod)
                return;
            if (!GetComp().TrackedStartingColonists)
                return;
            if (pawn == null || !pawn.RaceProps.Humanlike)
                return;
            var comp = pawn.PawnTrackerComp();
            if (!comp.AlreadyLostPart(part))
            {
                BodyPartRemovalIntent intent = HealthUtility.PartRemovalIntent(pawn, part);
                if (intent == BodyPartRemovalIntent.Amputate)
                {
                    comp.AddEvent(new LifeEvent(pawn, BodyPartAmputated, part));
                    return;
                }
                else if (intent == BodyPartRemovalIntent.Harvest)
                {
                    comp.AddEvent(new LifeEvent(pawn, BodyPartHarvested, part));
                    return;
                }
                else
                {
                    comp.AddEvent(new LifeEvent(pawn, BodyPartRemoved, part));
                }
                
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", new Type[] {typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult)})]
    public static class Pawn_HealthTracker_Patch
    {
        public static void Postfix(Hediff __instance, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageWorker.DamageResult result)
        {
            if (!GetComp().UseMod)
                return;
            if (!GetComp().TrackedStartingColonists)
                return;
            if (hediff == null)
                return;


            if (hediff.def == HediffDefOf.MissingBodyPart)
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                if (!pawn.RaceProps.Humanlike)
                    return;
                var comp = pawn.PawnTrackerComp();
                if (part == null)
                {
                    return;
                }
                if (!comp.AlreadyLostPart(part))
                {
                    comp.AddEvent(new LifeEvent(pawn, LostBodyPart, part));
                }
            }
            if (hediff is Hediff_Pregnant hediffPregnant)
            {
                Pawn pawn = hediffPregnant.pawn;
                Pawn mother = hediffPregnant.Mother;
                Pawn father = hediffPregnant.Father;

                if (pawn == null)
                {
                    return;
                }
                pawn.PawnTrackerComp().AddEvent(new LifeEvent(pawn, Pregnant));
                if (mother != pawn && mother != null)
                    mother.PawnTrackerComp().AddEvent(new LifeEvent(mother, PartnerPregnant, otherPawnSingle: pawn));            
                if (father != pawn && father != null)
                    father.PawnTrackerComp().AddEvent(new LifeEvent(father, PartnerPregnant, otherPawnSingle: pawn));
            }
            if (hediff.def == HediffDefOf.MechlinkImplant)
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                pawn.PawnTrackerComp().AddEvent(new LifeEvent(pawn, BecameMechinator));
            }
        }
    }

    [HarmonyPatch(typeof(IndividualThoughtToAdd), MethodType.Constructor, new Type[] {typeof(ThoughtDef), typeof(Pawn), typeof(Pawn), typeof(float), typeof(float)})]
    public static class IndividualThoughtToAdd_Patch
    {
        public static void Postfix(ThoughtDef thoughtDef, Pawn addTo, Pawn otherPawn, float moodPowerFactor, float opinionOffsetFactor)
        {
            if (!GetComp().UseMod || addTo == null)
                return;
            if (new List<string>() {"MyWifeDied","MyHusbandDied","MySonDied","MyDaughterDied","MyMotherDied","MyFatherDied","MyLoverDied","MyFianceeDied","MyBrotherDied","MySisterDied","MyHalfSiblingDied", "MyBirthMotherDied", "MyGrandchildDied","MyGreatGrandchildDied"}.Contains(thoughtDef.defName))
            {
                addTo.PawnTrackerComp().AddPawnDiedEvent(thoughtDef, otherPawn);
            }
            
        }
    }
    
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawnRelations",MethodType.Normal)]
    public static class GeneratePawnRelations_Patch
    {
        public static void Postfix(Pawn pawn, ref PawnGenerationRequest request)
        {
            if (pawn == null || !pawn.RaceProps.Humanlike)
                return;

            //Log.Warning("GeneratePawnRelations");
            if (pawn.PawnTrackerComp() != null)
                pawn.PawnTrackerComp().PopulateStarterRelations();
            //else
            //    Log.Error("Somethign was null, couldn't popualte start relations");
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new Type[] {typeof(PawnKindDef), typeof(Faction)})]
    public static class PawnGenerator_GeneratePawn_Patch1
    {        
        public static void Postfix(PawnKindDef kindDef, Faction faction, ref Pawn __result)
        {
            if (__result == null || !__result.RaceProps.Humanlike)
                return;
            if (__result.PawnTrackerComp() != null)
            {
                //LogPTMDev.Message("Generating pawn - add to NeedStarterRelations");
                //GetComp().NeedStarterRelations.Add(__result);
                __result.PawnTrackerComp().PopulateStarterRelations();
            }    
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new Type[] {typeof(PawnGenerationRequest)})]
    public static class PawnGenerator_GeneratePawn_Patch2
    {
        public static void Postfix(PawnGenerationRequest request, ref Pawn __result)
        {
            if (!__result.RaceProps.Humanlike)
                return;
            if (__result != null && __result.PawnTrackerComp() != null)
            {
                //LogPTMDev.Message("Generating pawn - add to NeedStarterRelations");
                //GetComp().NeedStarterRelations.Add(__result);
                __result.PawnTrackerComp().PopulateStarterRelations();
            }                
        }
    }

    [HarmonyPatch(typeof(Pawn_RelationsTracker), "AddDirectRelation")]
    public static class Pawn_RelationsTracker_AddDirectRelation_Patch
    {
        public static void Postfix(Pawn_RelationsTracker __instance, PawnRelationDef def, Pawn otherPawn)
        {
            if (GetComp() == null)
                return;
            
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (def == null || pawn == null || otherPawn == null)
                return;
            if (RelationDefLifeEvent(def) == null)
                return;

            if (GenTicks.TicksAbs > 5 && GetComp() != null && otherPawn.PawnTrackerComp().NeedPopStarterRelations && pawn.PawnTrackerComp().CanDocumentRelationship(otherPawn, def))
            {
                //Log.Message($"Adding direct relation for {pawn}");
                if (pawn != null && otherPawn != null && pawn.RaceProps.Humanlike && otherPawn.RaceProps.Humanlike)
                {
                    //Log.Message($"Adding event {def} to both pawns, {pawn} and {otherPawn}");
                    pawn.PawnTrackerComp().AddEvent(new LifeEvent(pawn, def, otherPawnSingle: otherPawn));
                    otherPawn.PawnTrackerComp().AddEvent(new LifeEvent(otherPawn, otherPawn.GetMostImportantRelation(pawn), otherPawnSingle: pawn));
                }
            }
            else
            {
                if (pawn.PawnTrackerComp().NeedPopStarterRelations)
                    pawn.PawnTrackerComp().PopulateStarterRelations();
                if (otherPawn.PawnTrackerComp().NeedPopStarterRelations)
                    otherPawn.PawnTrackerComp().PopulateStarterRelations();
            } 

        }
        
    }

    

    public static class InstallPartPatches
    {
        public static void Prefix(object instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (pawn == null || instance == null || part == null || billDoer == null || ingredients == null || bill == null)
            {
                return;
            }
            // Use reflection to get the recipe field from instance
            RecipeDef recipe = Traverse.Create(instance).Field("recipe").GetValue<RecipeDef>();
            HediffDef addsHediff = recipe.addsHediff;
            IEnumerable<BodyPartRecord> affectedParts = MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn);

            LogPTMDev.Message($"Affected parts regardless if missing: {affectedParts.Count()}");
            affectedParts = affectedParts.Where(part => pawn.health.hediffSet.GetNotMissingParts().Contains(part));
            if (affectedParts.Any())
            {
                LogPTMDev.Message($"Affected parts count: {affectedParts.Count()}");
                foreach (BodyPartRecord replacedPart in affectedParts)
                {
                    LogPTMDev.Message($"Pawn did have part {replacedPart} before we replaced it");
                    pawn.PawnTrackerComp().AddEvent(new LifeEvent(pawn, BodyPartRemoved, bodyPart: replacedPart));
                }
            }
        }

        public static void Postfix(object instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (pawn == null || instance == null || part == null || billDoer == null || ingredients == null || bill == null)
            {
                return;
            }
            RecipeDef recipe = Traverse.Create(instance).Field("recipe").GetValue<RecipeDef>();
            HediffDef addsHediff = recipe.addsHediff;
            List<Hediff> addedHediffs = pawn.health.hediffSet.hediffs.Where(h => h.def == addsHediff).ToList();
            IEnumerable<BodyPartRecord> affectedParts = MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn);
            BodyPartRecord firstAffectedPart = null;
            foreach (BodyPartRecord bpr in affectedParts)
            {
                if (firstAffectedPart == null)
                    firstAffectedPart = bpr;
            }
            if (addedHediffs.Any())
            {
                pawn.PawnTrackerComp().AddEvent(new LifeEvent(pawn, NewBodyPart, addsHediff, firstAffectedPart));
            }    
            else if (affectedParts.Any() && pawn.PawnTrackerComp().MostRecentEvent() != null && pawn.PawnTrackerComp().MostRecentEvent().def == BodyPartRemoved)
            {
                LifeEvent lifeEvent = (LifeEvent) pawn.PawnTrackerComp().MostRecentEvent();
                {
                    foreach (BodyPartRecord affectedPart in affectedParts)
                    {
                        if (affectedPart == lifeEvent.lostPart)
                        {
                            LogPTMDev.Message("Pawn did not get new install, and they did NOT lose their other part!!!");
                            pawn.PawnTrackerComp().RemovePreviousEvent();
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Recipe_InstallImplant), "ApplyOnPawn")]
    public static class Recipe_InstallImplant_Patch
    {
        public static void Prefix(Recipe_InstallArtificialBodyPart __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            InstallPartPatches.Prefix(__instance, pawn, part, billDoer, ingredients, bill);
        }

        public static void Postfix(Recipe_InstallArtificialBodyPart __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            InstallPartPatches.Postfix(__instance, pawn, part, billDoer, ingredients, bill);
        }
    }

    [HarmonyPatch(typeof(Recipe_InstallArtificialBodyPart), "ApplyOnPawn")]
    public static class Recipe_InstallArtificialBodyPart_Patch
    {
        public static void Prefix(Recipe_InstallArtificialBodyPart __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            InstallPartPatches.Prefix(__instance, pawn, part, billDoer, ingredients, bill);
        }

        public static void Postfix(Recipe_InstallArtificialBodyPart __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            InstallPartPatches.Postfix(__instance, pawn, part, billDoer, ingredients, bill);
        }
    }

    [HarmonyPatch(typeof(Recipe_InstallNaturalBodyPart), "ApplyOnPawn")]
    public static class Recipe_InstallNaturalBodyPart_Patch
    {
        public static void Prefix(Recipe_InstallArtificialBodyPart __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            InstallPartPatches.Prefix(__instance, pawn, part, billDoer, ingredients, bill);
        }

        public static void Postfix(Recipe_InstallArtificialBodyPart __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            InstallPartPatches.Postfix(__instance, pawn, part, billDoer, ingredients, bill);
        }
    }

    [HarmonyPatch(typeof(Hediff_Pregnant), "Miscarry")]
    public static class Miscarry_Patch
    {
        public static void Postfix(Hediff_Pregnant __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            Pawn mother = Traverse.Create(__instance).Field("mother").GetValue<Pawn>();
            Pawn father = Traverse.Create(__instance).Field("father").GetValue<Pawn>();

            pawn.PawnTrackerComp().AddEvent(new LifeEvent(pawn, Miscarried));
            if (mother != pawn && mother != null)
                mother.PawnTrackerComp().AddEvent(new LifeEvent(mother, PartnerMiscarried, otherPawnSingle: pawn));            
            if (father != pawn && father != null)
                father.PawnTrackerComp().AddEvent(new LifeEvent(father, PartnerMiscarried, otherPawnSingle: pawn));
        }
    }

    [HarmonyPatch(typeof(Recipe_TerminatePregnancy), "ApplyOnPawn")]
    public static class TerminatePregnancy_Patch
    {
        public static void Postfix(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            pawn.PawnTrackerComp().AddEvent(new LifeEvent(pawn, TerminatedPregnancy));
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_TravelerGroup), "TryExecuteWorker")]
    public static class IncidentWorker_TravelerGroup_Patch
    {
        public static void Postfix(ref bool __result)
        {
            if (__result)
            {
                List<Pawn> newPawns = GetPawns(colonistStatus: CSO.AnyNonColonistNonPrisoner);
                foreach (Pawn pawn in newPawns)
                {
                    var comp = pawn.PawnTrackerComp();
                    comp.AddEvent(new ArriveEvent(pawn, PassingThroughArrived));
                }
            }
        }
    }

    [HarmonyPatch(typeof(MentalState), "GetBeginLetterText")]
    public static class MentalState_GetBeginLetterText
    {
        public static void Postfix(MentalState __instance)
        {
            LogPTMDev.Message($"Mental State Letter Text {__instance.def.defName}");
            if (__instance.def.defName != "GiveUpExit")
            {    
                return;
            }
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            pawn.PawnTrackerComp().AddEvent(new LifeEvent(pawn, GaveUpAndLeft));
        }
    }

    [HarmonyPatch(typeof(MentalState), "RecoverFromState")]
    public static class MentalState_RecoverFromState
    {
        public static void Postfix(MentalState_GiveUpExit __instance)
        {
            if (__instance.def.defName != "GiveUpExit")
                return;
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            pawn.PawnTrackerComp().AddEvent(new LifeEvent(pawn, DecidedToStay));
        }
    }

    [HarmonyPatch(typeof(ResurrectionUtility), "Resurrect")]
    public static class RessurectionUtility_Patch
    {
        public static void Prefix(Pawn pawn)
        {
            if (!pawn.Dead)
                return;
            
            pawn.PawnTrackerComp()?.AddEvent(new ArriveEvent(pawn, Resurrected));
        }
    }

    [HarmonyPatch(typeof(GameConditionManager), "RegisterCondition")]
    public static class GameConditionManager_RegisterCondition_Patch
    {
        public static void Postfix(GameCondition cond)
        {
            if (GameEventUtility.RelevantEvents.Contains(cond.def))
            {
                GetComp()?.AddEvent(new GameConditionEvent(cond.def));
            }
            
        }
    }

    [HarmonyPatch(typeof(GameCondition), "End")]
    public static class GameCondition_End_Patch
    {
        public static void Postfix(GameCondition __instance)
        {
            if (GameEventUtility.RelevantEvents.Contains(__instance.def))
                GetComp()?.EndEvent(__instance.def);
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_ManhunterPack), "TryExecuteWorker")]
    public static class IncidentWorker_ManhunterPack_Patch
    {
        public static void Postfix(IncidentWorker_ManhunterPack __instance, IncidentParms parms, ref bool __result)
        {
            if (__result == false)
                return;

            PawnKindDef animalKind = parms.pawnKind;
            Map target = (Map) parms.target;

            List<Pawn> manhunterAnimals = target.mapPawns.AllPawnsSpawned.Where(p => p.kindDef == animalKind && p.MentalState != null && (p.MentalState.def == MentalStateDefOf.Manhunter || p.MentalState.def == MentalStateDefOf.ManhunterPermanent)).ToList();
            GameManhunterEvent newEvent = new GameManhunterEvent(animalKind, manhunterAnimals);
            LogPTMDev.Message("Add manhunter event");
            GetComp().AddEvent(newEvent);
            GetComp().currentManhunters = newEvent;
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_DiseaseHuman), "ActualVictims")]
    public static class IncidentWorker_DiseaseHuman_Patch
    {
        public static void Postfix(IncidentWorker_DiseaseHuman __instance, ref IEnumerable<Pawn> __result)
        {
            IncidentDef def = Traverse.Create(__instance).Field("def").GetValue<IncidentDef>();

            GetComp().AddEvent(new GameMiscEvent(GameEventDefOf.Illness, illness: def.diseaseIncident, colonists: __result.Where(p => p.IsColonist).Count(), prisoners: __result.Where(p => p.IsPrisonerOfColony).Count()));
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_MechCluster), "TryExecuteWorker")]
    public static class IncidentWorker_MechCluster_Patch
    {
        public static void Postfix(IncidentWorker_MechCluster __instance, ref bool __result)
        {
            if (__result == true)
                GetComp().AddEvent(new GameMiscEvent(GameEventDefOf.MechCluster));
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_Infestation), "TryExecuteWorker")]
    public static class IncidentWorker_Infestation_Patch
    {
        public static void Postfix(IncidentWorker_Infestation __instance, ref bool __result)
        {
            if (__result == true)
                GetComp().AddEvent(new GameMiscEvent(GameEventDefOf.Infestation));
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_DeepDrillInfestation), "TryExecuteWorker")]
    public static class IncidentWorker_DeepDrillInfestation_Patch
    {
        public static void Postfix(IncidentWorker_DeepDrillInfestation __instance, ref bool __result)
        {
            if (__result == true)
                GetComp().AddEvent(new GameMiscEvent(GameEventDefOf.Infestation));
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_WastepackInfestation), "TryExecuteWorker")]
    public static class IncidentWorker_WastepackInfestation_Patch
    {
        public static void Postfix(IncidentWorker_WastepackInfestation __instance, ref bool __result)
        {
            if (__result == true)
                GetComp().AddEvent(new GameMiscEvent(GameEventDefOf.Infestation));
        }
    }
}

