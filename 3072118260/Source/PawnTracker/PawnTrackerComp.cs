using Verse;
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using static PawnTrackerMain.PawnTrackerUtility;
using System.Linq;
using static PawnTrackerMain.DocumentedEventDefOf;
using System.Data.SqlTypes;
using UnityEngine;
using Verse.Noise;

namespace PawnTrackerMain
{
    public class PawnTrackerComp : ThingComp
    {
        private HospitalityLogic hospitalityLogic = new HospitalityLogic();
        public bool startingColonist = false;
        public List<DocumentedEvent> documentedEvents = new List<DocumentedEvent>();
        
        public List<LifeEvent> lifeEvents => documentedEvents.OfType<LifeEvent>().ToList();
        public List<LifeEvent> socialEvents => lifeEvents.Where(le => le.otherPawns != null).ToList();
        public List<JoinEvent> joinEvents => documentedEvents.OfType<JoinEvent>().ToList();
        public List<LeaveEvent> leaveEvents => documentedEvents.OfType<LeaveEvent>().ToList();
        public List<ArriveEvent> arriveEvents => documentedEvents.OfType<ArriveEvent>().ToList();
        public List<DeathEvent> deathEvents => documentedEvents.OfType<DeathEvent>().ToList();
        
        public DocumentedEventDef StopTryingToDocument = null;
        public bool everWasColonist => EverJoined();
        public bool everWasPrisoner => EverWasPrisoner();
        public string Pronoun;
        public string PronounPossessive => Pawn == null ? "their" : Pawn.gender == Gender.Male ? "his" : Pawn.gender == Gender.Female ? "her" : "their";
        public int starterRelationsPopulated = 0;
        public Dictionary<string, Tuple<PawnRelationDef, int>> StarterRelationDict = new Dictionary<string, Tuple<PawnRelationDef, int>>(); //Key: otherPawn's name; Value: {def, startTicks}
        public PawnStatus pawnStatus;
        public bool shouldBeHere = true;
        public DocumentedEvent mostRecentEvent => MostRecentEvent();
        public bool IsDead => (Pawn.Dead || (deathEvents != null && deathEvents.Any())) && !ResurrectAfterDeath();
        private Pawn Pawn => (Pawn) parent;
        public bool NeedPopStarterRelations => (everWasColonist || Pawn.IsColonist) && starterRelationsPopulated < GetComp()?.requiredStarterRelationsPopulatedCount;

        public PawnTrackerComp()
        {
            if (Pawn == null)
            {
                return;
            }

            if (NeedPopStarterRelations && !GetComp().NeedStarterRelations.Contains(Pawn))
            {
                //LogPTMDev.Warning("Add to NeedStarterRelations on construct");
                //GetComp()?.NeedStarterRelations.Add(Pawn);
            }

            if (StarterRelationDict == null)
            {
                StarterRelationDict = new Dictionary<string, Tuple<PawnRelationDef, int>>();
                
            }    

            if (!EverLeft())
                shouldBeHere = true;
            if (EverDied() && !ResurrectAfterDeath())
                shouldBeHere = false;

            UpdateVarious(rels: false);
        }

        public void PopulateStarterRelations()
        {
            if (Pawn == null)
                return;
            if (!Pawn.IsColonist && !everWasColonist)
                return;
            if (!NeedPopStarterRelations)
            {
                /*if (GetComp().NeedStarterRelations.Contains(Pawn))
                {
                    LogPTMDev.Warning($"remove from NeedStarterRelations when we did it enough times (NeedPop...: {NeedPopStarterRelations}(NeedPop...: {NeedPopStarterRelations}; Count: {starterRelationsPopulated})");
                    GetComp().NeedStarterRelations.Remove(Pawn);
                }*/
                return;
            }

            if (!Pawn.RaceProps.Humanlike)
                return;
            
            if (StarterRelationDict == null)
                StarterRelationDict = new Dictionary<string, Tuple<PawnRelationDef, int>>();

            foreach (Pawn potentiallyRelatedPawn in Pawn.relations.RelatedPawns.Where(p => p != null && p.RaceProps.Humanlike).ToList())
            {
                if (potentiallyRelatedPawn == null)
                    continue;

                if (!StarterRelationDict.ContainsKey(PawnName(potentiallyRelatedPawn)))
                {
                    StarterRelationDict[PawnName(potentiallyRelatedPawn)] = new Tuple<PawnRelationDef, int>(Pawn.GetMostImportantRelation(potentiallyRelatedPawn), 0);
                    
                    // Add this pawn to the potentiallyRelatedPawn's StarterRelationDict if they've had their pops created...bc we don't want them to add it *now*!!
                    // (If they haven't already had their dict populated a bunch of times, we should wait for them to do it from their own direction)
                    if (GetComp() != null && !potentiallyRelatedPawn.PawnTrackerComp().NeedPopStarterRelations)
                        potentiallyRelatedPawn.PawnTrackerComp().StarterRelationDict[PawnName(Pawn)] = new Tuple<PawnRelationDef, int>(potentiallyRelatedPawn.GetMostImportantRelation(Pawn), 0);
                }
            }
            starterRelationsPopulated++;

            /*if (!NeedPopStarterRelations)
                if (GetComp().NeedStarterRelations.Contains(Pawn))
                {
                    LogPTMDev.Message($"remove {Pawn} from NeedStarterRelations at end of StarterRelations... (NeedPop...: {NeedPopStarterRelations}(NeedPop...: {NeedPopStarterRelations}; Count: {starterRelationsPopulated})");
                    GetComp().NeedStarterRelations.Remove(Pawn);
                }
            else if (NeedPopStarterRelations && !GetComp().NeedStarterRelations.Contains(Pawn))
            {
                LogPTMDev.Message(text: $"Add {Pawn} to NeedStarterRelations (NeedPop...: {NeedPopStarterRelations}; Count: {starterRelationsPopulated})");
                GetComp().NeedStarterRelations.Add(Pawn);
            }*/
            return;
        }

        public bool EverWasPrisoner()
        {
            if (documentedEvents == null)
            {
                documentedEvents = new();
                return false;
            }

            if (!Pawn.RaceProps.Humanlike || !documentedEvents.Any())
                return false;
            return AnyDocumentedEvents(e => e.eventSubType == EventSubType.Capture);
        }

        public void AddEvent(DocumentedEvent docEvent, bool forceAdd = false)
        {   
            //Skip to adding it, if we're force-adding (such as in dev mode)
            if (forceAdd && Pawn != null && Pawn.RaceProps.Humanlike)
                goto allowAdd;

            if (GetComp().ManuallyUntracked.Contains(Pawn))
                return;
                
            //Don't let it add relationships (or anything else, for that matter) before the pawn has finished being generated
            if (StarterRelationDict == null && !everWasColonist && !everWasColonist && !Pawn.IsColonist)
            {
                LogPTMDev.Warning("StarterRelationDict is null");
                return;
            }   

            if (deathEvents != null && deathEvents.Any() && docEvent.def != Resurrected)
                return;

            //Don't document events on non-humanlikes
            if (!Pawn.RaceProps.Humanlike)
                return;
            
            if (docEvent == null)
            {
                Log.Warning("Attempted to document null event");
                return;
            }
            if (documentedEvents == null)
            {
                documentedEvents = new List<DocumentedEvent>();
            }
            
            if (Pawn != null && docEvent is LeaveEvent && docEvent.def != UnknownLeave && docEvent.def != Kidnapped && Pawn.IsColonist && Pawn.Faction == Faction.OfPlayer)
            {
                Log.Error("You can't remove a colonist if they're still a colonist; leaving the map is not the same as leaving the colony");
                return;
            }

            if (documentedEvents.Any(e => e is not LifeEvent))
            {
                DocumentedEvent firstEvent = documentedEvents.Where(e => e is not LifeEvent).ToList().First();
                if (docEvent is LifeEvent && Math.Abs(docEvent.ticks - firstEvent.ticks) <= 100 && !docEvent.centralEvent)
                {
                    return;
                }
            }

            if (docEvent.def == StartingColonist) {}
            else if (docEvent is LifeEvent && documentedEvents.Count == 0)
                return; //The first event should never ever be a life event...

            else if (GenTicks.TicksAbs <= 0) //Nothing good happens at the VERY start of the game...
                return;

            else if (GetComp() !=  null && GetComp().GameStartTick != 0 && GetComp().TicksSinceGameStart <= 100)
                return;

            else if (docEvent.ticks <= 1)
                return;
            
            else if (docEvent.dayOfYear == 0 && docEvent.year == 0)
                return;
            

            //If the pawn is downed when they arrive as a GUEST (according to Hospitality), use VisitForTreatment...otherwise, they've arrived for a stay!
            if (docEvent is ArriveEvent && docEvent.eventSubType != EventSubType.HospitalityGuestVisit && hospitalityLogic.IsGuest(docEvent.pawn))
            {
                if (Pawn.Downed)
                    docEvent = new ArriveEvent(docEvent.pawn, VisitForTreatment);
                else
                    docEvent = new ArriveEvent(docEvent.pawn, ArrivedAsGuest);
            }
            
            //If they regained the ability to walk before the Resuce was complete,
            //they will have "decided to leave" and THEN "was rescued"...which is no good
            //So we remove the "decided to leave," add new "was rescued," then allow this to be "leave after rescue"
            if (mostRecentEvent != null && mostRecentEvent.def == NoJoinAfterRescue && docEvent.def == Rescued)
            {
                RemovePreviousEvent();
                AddEvent(new LifeEvent(docEvent.pawn, Rescued));
                docEvent = new LeaveEvent(docEvent.pawn, NoJoinAfterRescue);
            }

            int flag1 = 0;
            if (docEvent is LifeEvent lifeEvent1)
            {
                //Thos would happen if it's trying to add a relationship type that we're not tracking
                if (lifeEvent1.def == null)
                    return;

                if (mostRecentEvent != null && mostRecentEvent is LifeEvent recentLifeEvent)
                {
                    if (docEvent.def == mostRecentEvent.def)
                    {
                        // 1 if there are different pawns involved in this even than there were previously; 2 if they're the same...
                        // This way, 0 means we didn't have reason to check it at all?
                        flag1 = lifeEvent1.otherPawns != recentLifeEvent.otherPawns ? 1 : 2;
                        
                        // If the same pawns are involved and/or the events have the same description, don't add it
                        if (flag1 == 2 && !forceAdd)
                        {
                            return;
                        }    
                    }

                    // if this pawn has died before, don't document their death
                    if (recentLifeEvent.def == PawnDied && documentedEvents.Any(d => d is LifeEvent checkLifeEvent && checkLifeEvent.otherPawnSingleName != null && checkLifeEvent.otherPawnSingleName == PawnName(checkLifeEvent.otherPawnSingle)))
                        return;
                }
            }

            //If we didn't check this for who is involved in thing...            
            if (flag1 == 0 && docEvent.def == StopTryingToDocument)
            {
                return;
            }

            if (docEvent is LifeEvent lifeEvent2)
            {
                //If it's trying to add a new relationship where the pawn and the relationship are already documented... don't let it
                if (lifeEvent2.eventSubType == EventSubType.Relationship)
                {
                    if (GetComp() == null)
                        return;

                    if (NeedPopStarterRelations)
                    {
                        PopulateStarterRelations();
                        return;
                    }

                    if (!CanDocumentRelationship(lifeEvent2.otherPawnSingle, lifeEvent2.def, forceAdd))
                        return;    
                }

                if (lifeEvent2.def == BodyPartRemoved && RemovedWasInfected(lifeEvent2))
                {
                    docEvent = new LifeEvent(Pawn, InfectedBodyPartRemoved, lifeEvent2.lostPart);
                }
                
            }
            //if (mostRecentEvent != null && mostRecentEvent.def == docEvent.def && mostRecentEvent.year == docEvent.year && mostRecentEvent.dayOfYear == docEvent.dayOfYear)
            if (mostRecentEvent != null && mostRecentEvent.def == docEvent.def && flag1 != 1)
            {
                StopTryingToDocument = docEvent.def;
                Log.Warning("Tried to log the same event twice");
                //Log.Message($"Event is new relationship: {docEvent.eventSubType == EventSubType.Relationship}");
                if (!forceAdd)
                    return;
            }

            if (mostRecentEvent != null && docEvent.def == Rescued && mostRecentEvent.eventSubType == EventSubType.Capture)
                return;
            if (docEvent is JoinEvent && EverJoined())
            {
                if (!EverLeft())
                {
                    if (!forceAdd)
                    {
                        LogPTMDev.Warning("Pawn has joined previously and never left...");
                        return;
                    }
                }
            }

            if (mostRecentEvent != null)
            {
                if (mostRecentEvent is LeaveEvent && docEvent.def == Rescued)
                {
                    RemovePreviousEvent();
                }
                //Don't document anything besides RANSOMRETURN if the pawn was kidnapped and not returned
                if (docEvent.def != RansomReturn && mostRecentEvent.def == Kidnapped)
                {
                    if (!forceAdd)
                        return;
                }
            }
            docEvent = FixUnknownEvent(docEvent, forceAdd);
            if (docEvent == null)
            {
                return;
            }


        allowAdd:
            //If they just Unknown-ly arrived and now are joining w/ known reason, get rid of unknown arrival
            if (mostRecentEvent != null && docEvent.eventType == "Join" && docEvent.def != UnknownJoin && mostRecentEvent.def == UnknownArrive && Math.Abs(docEvent.ticks - mostRecentEvent.ticks) <= 500)
            {
                LogPTMDev.Message("Get rid of recent unknown arrival");
                RemovePreviousEvent(force: true);
            }

            if (GetComp() != null && !GetComp().TrackedPawns.Contains(Pawn))
            {
                LogPTMDev.Warning("Add to TrackedPawns in AddEvent");
                GetComp().TrackedPawns.Add(Pawn);
            }
            
            if (docEvent is JoinEvent)
            {
                pawnStatus = PawnStatus.Colonist;
            }
            
            if (docEvent.eventSubType == EventSubType.Capture)
            {
                pawnStatus = PawnStatus.Prisoner;
                if (mostRecentEvent != null && mostRecentEvent.def == Rescued)
                    RemovePreviousEvent(forceAdd);
            }
            
            documentedEvents.Add(docEvent);
            StopTryingToDocument = null;
            UpdateShouldBeHere();
            OrderPawnHistory();
            
            if (docEvent.eventSubType == EventSubType.Birth && mostRecentEvent != null && mostRecentEvent.def == UnknownDeath)
            {
                RemovePreviousEvent();
                AddEvent(new DeathEvent(docEvent.pawn, DiedInChildbirth));
            }

            if (docEvent.def is DeathEventDef)
            {
                if (GetComp().ManuallyUntracked.Contains(Pawn))
                    return;
                    
                PawnDeath();
            }

            if (docEvent.def == Resurrected && GetComp() != null && GetComp().DeadTrackedPawns.ContainsKey(docEvent.pawn.Name.ToString()))
                GetComp().DeadTrackedPawns.Remove(docEvent.pawn.Name.ToString());

            return;
        }

        public void OrderPawnHistory()
        {
            if (documentedEvents == null)
                documentedEvents = new List<DocumentedEvent>();
            this.documentedEvents.Sort((x, y) => x.ticks.CompareTo(y.ticks));
        }

        public List<DocumentedEvent> GetDocumentedEvents(Func<DocumentedEvent, bool> condition)
        {
            return documentedEvents.Where(condition).ToList();
        }

        public bool AnyDocumentedEvents(Func<DocumentedEvent, bool> condition)
        {
            if (documentedEvents == null)
            {
                documentedEvents = new();
                return false;
            }
                
            return documentedEvents.Any(condition);
        }

        public bool AlreadyLostPart(BodyPartRecord lostPart)
        {
            if (documentedEvents == null)
            {
                documentedEvents = new List<DocumentedEvent>();
                return false;
            }

            List<LifeEvent> lostPartEvents = new List<LifeEvent>();
            foreach (LifeEvent lifeEvent in this.documentedEvents.Where(e => e is LifeEvent))
            {
                lostPartEvents.Add(lifeEvent);
            }
            
            if (!lostPartEvents.Any())
                return false;
            return lostPartEvents.Any(e => e.lostPart != null && e.lostPart == lostPart);
        }
        
        public bool AlreadyInstalledPart(BodyPartRecord newPart)
        {
            if (documentedEvents == null)
            {
                documentedEvents = new List<DocumentedEvent>();
                return false;
            }

            List<LifeEvent> newPartEvents = new List<LifeEvent>();
            foreach (LifeEvent lifeEvent in this.documentedEvents.Where(e => e is LifeEvent))
            {
                newPartEvents.Add(lifeEvent);
            }
            if (!newPartEvents.Any())
                return false;
            return newPartEvents.Any(e => e.installedPart != null && e.installedPart == newPart.Label);
        }

        public void LogAllEventDetails()
        {
            OrderPawnHistory();
            Log.Warning("- Logging all event details -");
            foreach (DocumentedEvent docEvent in documentedEvents)
                LogEventDetails(docEvent);
        }

        public void LogEventDetails(DocumentedEvent docEvent)
        {
            if (docEvent == null)
            {
                LogPTMDev.Error("WHAT THE HELL!!!!!!!!!!!!!!!");
                return;
            }
            
            Log.Message($"Pawn: {docEvent.pawnName}");
            Log.Message($"Description: {docEvent.def.description}");
            Log.Message($"Occurred: day {docEvent.dayOfYear} of Year {docEvent.year} (day {docEvent.dayOfSeason} of {docEvent.season})");
            Log.Message(""); //newline
        }

        public DocumentedEvent LastEvent(Predicate<DocumentedEvent> condition)
        {
            try
            {
                return documentedEvents.FindLast(condition);
            }
            catch
            {
                return null;
            }
        }

        public DocumentedEvent MostRecentEvent(Predicate<DocumentedEvent> condition)
        {
            if (documentedEvents == null)
            {
                documentedEvents = new();
                return null;
            }

            if (!documentedEvents.Any())
            {
                return null;
            }
            OrderPawnHistory();
            return LastEvent(condition);
        }

        public DocumentedEvent MostRecentEvent()
        {
            if (documentedEvents == null)
            {
                documentedEvents = new();
                return null;
            }   

            if (!documentedEvents.Any())
            {
                return null;
            }

            OrderPawnHistory();
            return documentedEvents.Last();
        }

        private DocumentedEvent FixUnknownEvent(DocumentedEvent docEvent, bool forceAdd = false)
        {
            
            if (docEvent == null || docEvent.def == null)
                return null;
            bool unkLeave = docEvent.def == UnknownLeave ? true : false;
            bool unkArrive = docEvent.def == UnknownArrive ? true : false;
            bool unkJoin = docEvent.def == UnknownJoin ? true : false;
            bool unkDeath = docEvent.def == UnknownDeath ? true : false;
            Pawn pawn = docEvent.pawn;
            List<DocumentedEventDef> UseMiscLeaveKnown = new List<DocumentedEventDef>() {NoJoinAfterRescue, GaveUpAndLeft};

            //If it's not an unknown, return the original event
            if (unkLeave == false && unkArrive == false && unkJoin == false && unkDeath == false)
            {  
                return docEvent;
            }

            if (unkLeave && mostRecentEvent != null && UseMiscLeaveKnown.Contains(mostRecentEvent.def))
            {
                shouldBeHere = false;
                return new LeaveEvent(docEvent.pawn, MiscLeaveKnown);
            }

            OrderPawnHistory();
            UpdateShouldBeHere();
            UpdatePawnStatus();

            //If their most-recent event is of the same type, don't document another unknown
            if (mostRecentEvent != null && mostRecentEvent.eventType == docEvent.eventType && mostRecentEvent.def != docEvent.def)
            {
                Log.Message("Recently did a KNOWN version of thing");
                if (docEvent.eventType == "Leave")
                    shouldBeHere = false;
                return null;
            }

            if (hospitalityLogic.IsGuest(docEvent.pawn))
            {
                if (unkArrive)
                    return new ArriveEvent(docEvent.pawn, ArrivedAsGuest);
                if (unkLeave)
                    return new LeaveEvent(docEvent.pawn, FinishVisitAsGuest);
            }  

            if (unkArrive)
            {
                //If we're not on one of our own maps, don't bother marking someone's arrival
                if (!docEvent.pawn.IsColonist && pawn.Map != null && !docEvent.pawn.Map.IsPlayerHome)
                    return null;

                //Figure out arrivals on non-home-map
                if (pawn.Map != null && !pawn.Map.IsPlayerHome)
                {
                    if (pawn.IsColonist) {}
                    else if (pawn.IsPrisoner && !pawn.IsPrisonerOfColony)
                        return new ArriveEvent(pawn, IncidentMapCaptive);
                    else if (pawn.Faction.HostileTo(Faction.OfPlayer))
                        return new ArriveEvent(pawn, IncidentMapHostile);
                    else if (pawn.Downed)
                        return new ArriveEvent(pawn, IncidentMapNeedRescue);
                    else
                        return new ArriveEvent(pawn, IncidentMapFriendly);
                }

                if (GetComp() != null && GetComp().expectRansomed == true && mostRecentEvent.def == Kidnapped)
                {
                    GetComp().expectRansomed = false;
                    GetComp().expectRansomedTick = 0;
                    return new JoinEvent(docEvent.pawn, RansomReturn);
                }
            }
            
            
            if (unkJoin == true)
            {
                //If they're already part of the colony, don't try to re-join them
                if (EverJoined() && !EventAfterEvent(e => e is JoinEvent, e => e is LeaveEvent))
                    return null;
                
                if (GetComp() != null && GetComp().expectReward)
                {
                    if (MostRecentEvent() != null && MostRecentEvent().def == IncidentMapHostile)
                        RemovePreviousEvent();
                    return new JoinEvent(pawn, QuestReward);
                }
                return docEvent;
            }

            
            if (unkLeave)
            {
                //If they've been kidnapped, fix the event to reflect that
                if (Faction.OfPlayer.kidnapped.KidnappedPawnsListForReading.Contains(docEvent.pawn))
                {
                    Log.Message("Kidnapped (in unknown AddEvent");
                    docEvent = new LeaveEvent(docEvent.pawn, Kidnapped, null);
                }

                //If it's an unknown leave but they're part of the colony and should still be here, don't update
                if (EverJoined() && shouldBeHere == true && !forceAdd)
                    return null;
            }

            

            //If they're not a colonist, attempt to update docEvent to be post-rescue action
            if (!docEvent.pawn.IsColonist && !docEvent.pawn.IsPrisonerOfColony && (unkJoin || unkLeave))
            {
                docEvent = PostRescue(docEvent);
            }    

            //idk why I was allowing unknown leaves previously...
            if(unkLeave == true && docEvent.def == UnknownLeave)
            {
                docEvent = null;
            }

            if (unkDeath)
            {
                DocumentedEvent recentBirth = MostRecentEvent(e => e.eventSubType == EventSubType.Birth);
                if (recentBirth != null && docEvent.ticks - recentBirth.ticks <= 100)
                    return new DeathEvent(docEvent.pawn, DiedInChildbirth);

                if (Pawn.health.hediffSet.hediffs != null && Pawn.health.hediffSet.hediffs.Any(h => h.def == HediffDefOf.PostpartumExhaustion))
                    return new DeathEvent(docEvent.pawn, DiedInChildbirth);

                if (docEvent.pawn != null && docEvent.pawn.health.hediffSet.hediffs != null && docEvent.pawn.health.hediffSet.hediffs.Any(h => h.def == HediffDefOf.Stillborn))
                    return new DeathEvent(docEvent.pawn, Stillborn);

                if (Find.CurrentMap != null && docEvent.pawn != null && docEvent.pawn.ageTracker.BirthDayOfYear == GenLocalDate.DayOfYear(Find.CurrentMap) && docEvent.pawn.ageTracker.BirthYear == GenLocalDate.Year(Find.CurrentMap))
                    return new DeathEvent(docEvent.pawn, Stillborn);
            }
            
            return docEvent;
        }

        private DocumentedEvent PostRescue(DocumentedEvent docEvent)
        {
            if (mostRecentEvent != null && mostRecentEvent.def == Rescued)
            {
                if (docEvent is LeaveEvent)
                    return new LeaveEvent(docEvent.pawn, NoJoinAfterRescue);
                else
                    return new JoinEvent(docEvent.pawn, JoinAfterRescue);
            }
            //if they weren't recently rescued, return original event
            else
                return docEvent;
        }

        public void UpdateShouldBeHere()
        {
            int leaveIndex = documentedEvents.FindLastIndex(e => e is LeaveEvent && e.def != UnknownLeave);
            int arriveJoinIndex = documentedEvents.FindLastIndex(e => e is JoinEvent || e is ArriveEvent);
            int deathIndex = ResurrectAfterDeath() ? -1 : documentedEvents.FindLastIndex(e => e is DeathEvent);

            int mostRecentIndex = new int[] {leaveIndex, arriveJoinIndex, deathIndex}.Max();

            //If they've never done any of this stuff........this is based on if theyre on a home map or not
            if (leaveIndex == -1 && arriveJoinIndex == -1 && deathIndex == -1)
            {
                shouldBeHere = Pawn.Map != null && Pawn.Map.IsPlayerHome;
                return;
            }

            // If they arrived/joined more recently than they left/died, they ShouldBeHere!
            shouldBeHere = arriveJoinIndex > leaveIndex && arriveJoinIndex > deathIndex;
        }  

        public void RemovePreviousEvent(bool force = false)
        {
            // If the most recent event was force-added and we have NOT specified to force removal... don't remove it
            if (documentedEvents.Last().forceAdd && !force)
                return;
            documentedEvents.Remove(documentedEvents.Last());
        }

        public LeaveEventDef ExitMapType() // This is used in a patch for when pawn literally Exit(s)Map
        {
            if (MostRecentEvent() == null)
                return null;

            if (MostRecentEvent() is LeaveEvent)
                return null;

            if (MostRecentEvent().def == MiscLeaveKnown)
                return null;

            // If the they were rescued more recently than anything else, and if the last thing that happened BEFORE rescue is a rescuable situation...
            if (MostRecentEvent().def == Rescued)
            {
                DocumentedEvent lastBeforeRescue = MostRecentEvent(e => e.def != Rescued);
                if (lastBeforeRescue.def.canJoinAfterRescue || lastBeforeRescue.def == UnknownArrive)
                {
                    return NoJoinAfterRescue;
                }
            }

            if (MostRecentEvent(e => e is ArriveEvent) == null)
                return UnknownLeave;

            DocumentedEventDef recentDef = MostRecentEvent(e => e is ArriveEvent).def;
            
            if (recentDef == VisitForTreatment)
                return LeaveAfterTreatment;

            if (recentDef == TradeCaravanArrived)
                return TradeCaravanLeft;
            
            if (recentDef == PassingThroughArrived)
                return PassingThroughLeft;
            
            if (recentDef == VisitorGroupArrived)
                return VisitorGroupLeft;

            if (recentDef == Raid)
                return FinishedRaid;

            if (recentDef == FriendliesArrived)
                return FriendliesLeft;

            if (recentDef == HuntingPartyArrived)
                return HuntingPartyLeft;

            return UnknownLeave;
        }

        public List<DocumentedEvent> CentralLifeEvents()
        {
            OrderPawnHistory();
            UpdateVarious();
            
            return documentedEvents.Where(d => d.centralEvent == true).ToList();
        }
        
        
        // bool temp turns this into an overload that is NOT used, so we can keep it but not use it currently
        public List<DocumentedEvent> CentralLifeEvents(bool temp)
        {
            OrderPawnHistory();
            UpdateVarious();
            //Make a list of the relevant events
            List<DocumentedEvent> relevant = new List<DocumentedEvent>();
            List<DocumentedEvent> centralEvent;

            if (!everWasColonist && !Pawn.IsColonist)
                centralEvent = documentedEvents.Where(e => e is not LifeEvent).ToList();
            else
                centralEvent = documentedEvents;

            //If the pawn is a prisoner, automatically return ALL their (non-life) events
            //       I disagree with this, it should filter down to only their pre-arrest and arrest events...
            if (Pawn.PawnTrackerComp().pawnStatus == PawnTrackerUtility.PawnStatus.Prisoner)
            {
                return centralEvent;
            }    
                
            // Look at each event from the start of the list of events
            for (int i = 0; i < centralEvent.Count ; i++)
            {
                DocumentedEvent docEvent = centralEvent[i];
                DocumentedEvent nextEvent = i + 1 < centralEvent.Count ? centralEvent[i + 1] : null;
                //If we already added this event to the list based on the previous event's conditions, skip
                if (relevant.Contains(docEvent))
                    continue;

                if (docEvent.def is LifeEventDef || docEvent is LifeEvent || docEvent.def is JoinEventDef || docEvent is JoinEvent || docEvent is DeathEvent || docEvent.def is DeathEventDef)
                {
                    relevant.Add(docEvent);
                    continue;
                }

                //If the event involves leaving, we will always show it
                if ((nextEvent.eventType == "Leave" || docEvent.eventType == "Leave") && !(docEvent.def == UnknownLeave && nextEvent.def == UnknownArrive))
                {
                    relevant.Add(docEvent);
                    continue;
                }   
                //If this is an unknown disappearance and then the pawn unknownly reappears, don't include it
                if (docEvent.def == UnknownLeave && nextEvent.def == UnknownArrive)
                    continue;

                //If the event involves being captured or joining the colony, we will always show it
                if (docEvent.eventSubType == EventSubType.Capture || docEvent.eventType == "Join")
                {
                    relevant.Add(docEvent);
                    continue;
                }

                //If this is the last item in the chronology (and isn't leave, capture, or join), skip it
                if (nextEvent == null)
                    continue;

                // If the FOLLOWING event is capture or join, show this event (and the next event)
                if (nextEvent.eventSubType == EventSubType.Capture || nextEvent.eventType == "Join")
                {
                    relevant.Add(docEvent);
                    relevant.Add(nextEvent);
                    continue;
                }
            }
            return relevant;
        }

        public bool EverJoined() => AnyDocumentedEvents(e => e is JoinEvent);
        public bool EverArrived() => AnyDocumentedEvents(e => e is ArriveEvent);
        public bool EverLeft() => AnyDocumentedEvents(e => e is LeaveEvent);
        public bool EverDied() => AnyDocumentedEvents(e => e is DeathEvent);

        

        public override void PostExposeData()
        {
            base.PostExposeData();
            //Log.Warning("PostExposeData");

            bool tempEverWasColonist = everWasColonist;
            bool tempEverWasPrisoner = everWasPrisoner;
            Scribe_Values.Look(ref startingColonist, "startingColonist");
            Scribe_Values.Look(ref tempEverWasColonist, "everWasColonist");
            Scribe_Values.Look(ref tempEverWasPrisoner, "everWasPrisoner");
            Scribe_Values.Look(ref shouldBeHere, "shouldBeHere");
            Scribe_Values.Look(ref pawnStatus, "pawnStatus");
            Scribe_Values.Look(ref Pronoun, "Pronoun");
            Scribe_Values.Look(ref starterRelationsPopulated, "starterRelationsPopulated");
            Scribe_Collections.Look(ref documentedEvents, "documentedEvents", LookMode.Deep);

            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (Pawn == null || documentedEvents == null)
                {
                    return;
                }
                if (Pawn.RaceProps.Humanlike && (Pawn.IsColonist || Pawn.IsPrisonerOfColony))
                {
                    Log.Message($"\nLoaded {documentedEvents.Count} DocumentedEvents for pawn {Pawn.Name}");                
                    //LogAllEventDetails();
                }
                if (!EverLeft())
                    this.shouldBeHere = true;
            }

            if (Scribe.mode == LoadSaveMode.Saving && this.StarterRelationDict != null)
            {
                // We create a list of entries that RimWorld's Scribe can handle
                List<Entry> entries = StarterRelationDict.Select(
                    kvp => new Entry() { Key = kvp.Key, Value = kvp.Value.Item1, Count = kvp.Value.Item2 }
                ).ToList();

                // Now we save this list
                Scribe_Collections.Look(ref entries, "StarterRelationDict", LookMode.Deep);
            }

            else if (Scribe.mode == LoadSaveMode.LoadingVars && this.StarterRelationDict != null)
            {
                // We prepare a list to receive the loaded data
                List<Entry> loadedEntries = null;
                Scribe_Collections.Look(ref loadedEntries, "StarterRelationDict", LookMode.Deep);

                if (loadedEntries == null)
                    return;
                // And then convert it back into our dictionary
                StarterRelationDict = loadedEntries.ToDictionary(
                    entry => entry.Key, 
                    entry => new Tuple<PawnRelationDef, int>(entry.Value, entry.Count)
                );
            }
        }

        private class Entry : IExposable
        {
            public string Key;
            public PawnRelationDef Value;
            public int Count;

            public void ExposeData()
            {
                Scribe_Values.Look(ref Key, "key");
                Scribe_Defs.Look(ref Value, "value");
                Scribe_Values.Look(ref Count, "count");
            }
        }

        public void UpdateEvents()
        {
            if (documentedEvents == null)
            {
                documentedEvents = new();
                return;
            }

            OrderPawnHistory();

            List<DocumentedEvent> keptEvents = new List<DocumentedEvent>();

            List<DocumentedEventDef> relevantEventDefs = new List<DocumentedEventDef>() {Rescued};
            List<EventSubType> relevantEventSubTypes = new List<EventSubType>() {EventSubType.Capture};
            List<Type> relevantEventTypes = new List<Type>() {typeof(DeathEvent)};

            foreach (DocumentedEvent docEvent in documentedEvents.Where(e => e.eventSubType != e.def.eventSubType || (e is DeathEvent && e.eventSubType != EventSubType.Death)))
                docEvent.eventSubType = docEvent.def.eventSubType;

            // Early out if none of the events we're fixing have ever occurred
            if (!documentedEvents.Any(e => relevantEventDefs.Contains(e.def) || relevantEventSubTypes.Contains(e.eventSubType) || relevantEventTypes.Contains(e.GetType())))
                return;

            for (int i = 0; i < documentedEvents.Count ; i++)
            {
                DocumentedEvent docEvent = documentedEvents[i];
                DocumentedEvent nextEvent = i + 1 < documentedEvents.Count ? documentedEvents[i+1] : null;
                DocumentedEvent prevEvent = i - 1 >= 0 ? documentedEvents[i-1] : null;

                if (docEvent.def == Rescued && nextEvent != null && nextEvent.eventSubType == EventSubType.Capture)
                {
                    continue;
                }
                else if (docEvent.def == Rescued && prevEvent != null && prevEvent.eventSubType == EventSubType.Capture)
                {
                    continue;
                }
                else
                {
                    keptEvents.Add(docEvent);
                }
            }

            documentedEvents = keptEvents;
        }

        public void UpdatePawnStatus()
        {
            if (documentedEvents == null)
            {
                documentedEvents = new();
            }

            OrderPawnHistory();
            UpdateShouldBeHere();
            
            pawnStatus = PawnStatus.Other;
            
            // If they're a prisoner, and they did NOT recently escape after being captured, they're a prisoner plain and simple lol
            if (Pawn.IsPrisonerOfColony && !EventAfterEvent(e => e.eventSubType == EventSubType.Capture, e => e.def == PrisonerEscaped))
            {
                pawnStatus = PawnStatus.Prisoner;
                return;
            }    

            // If they're documented as being a colonist, but they are NOT of the player faction, 
            // and they've not been kidnapped ((idk if their faction changes when kidnapped but just to be safe...))
            // ... then make sure they become Not A Colonist
            if (pawnStatus == PawnStatus.Colonist && Pawn.Faction != Faction.OfPlayer && !AnyDocumentedEvents(e => e.def == Kidnapped))
            {
                pawnStatus = PawnStatus.Other;
                return;
            }

            if (mostRecentEvent != null && mostRecentEvent.def == Kidnapped)
            {
                pawnStatus = PawnStatus.Colonist;
                return;
            }

            if (!documentedEvents.Any())
            {
                if (Pawn != null && Pawn.Map != null && Pawn.Map.IsPlayerHome && Pawn.IsColonist && Pawn.Faction != null && Pawn.Faction.IsPlayer)
                {
                    pawnStatus = PawnStatus.Colonist;
                    return;
                }
                return;
            }

            int joinIndex = documentedEvents.FindLastIndex(e => e is JoinEvent);
            int captureIndex = documentedEvents.FindLastIndex(e => e.eventSubType == EventSubType.Capture);
            int leaveIndex = documentedEvents.FindLastIndex(e => e is LeaveEvent && e.def != UnknownLeave);
            int gaveUpIndex = documentedEvents.FindLastIndex(e => e.def == GaveUpAndLeft);
            
            int MaxIndex = new int[] {joinIndex, captureIndex, leaveIndex, gaveUpIndex}.Max();

            if (MaxIndex == -1)
            {
                return;
            }
            // If they joined more recently than being captured, leaving, or giving up... they're a colonist
            if (joinIndex == MaxIndex)
            {
                pawnStatus = PawnStatus.Colonist;
                return;
            }

            // If they were captured more recently than joining, leaving, or givng up... they're a prisoner
            // (If they were a prisoner and escaped then they will still have EverWasPrisoner, but they're not one now!)
            if (captureIndex == MaxIndex)
            {
                pawnStatus = PawnStatus.Prisoner;
                return;
            }

            // If they left under known circumstances, AFTER giving up and leaving, more recently than joining... they're an Other
            if (MaxIndex == leaveIndex && gaveUpIndex > -1)
            {
                pawnStatus = PawnStatus.Other;
                return;
            }

            //If the most recent thing they did was leave under known circumstances, or if they've never done ANY of these things, they're an other
            // a colonist will never have a KNOWN leave if they're still, yknow, a colonist...so this does not need to be a concern
            pawnStatus = PawnStatus.Other;
            return;
        }

        public void UpdateRelationships()
        {
            if (Pawn == null || !Pawn.RaceProps.Humanlike || (!everWasColonist && !Pawn.IsColonist) || GetComp() == null)
                return;
            if (GetComp().NeedStarterRelations == null)
                return;
            if (NeedPopStarterRelations)
            {
                PopulateStarterRelations();
                return;
            }    
            else if (!NeedPopStarterRelations && GetComp().NeedStarterRelations.Contains(Pawn))
            {
                //Log.Message($"Remove {Pawn} from NeedStarterRelations in UpdateRelations (NeedPop...: {NeedPopStarterRelations}(NeedPop...: {NeedPopStarterRelations}; Count: {starterRelationsPopulated})");
                //GetComp().NeedStarterRelations.Remove(Pawn);
            }    
            
            List<PawnRelationDef> ignoreRelations = new List<PawnRelationDef>() {PawnRelationDefOf.Kin, PawnRelationDefOf.Grandparent, PawnRelationDefOf.Cousin, PawnRelationDefOf.Parent, PawnRelationDefOf.GreatGrandparent, PawnRelationDefOf.GranduncleOrGrandaunt, PawnRelationDefOf.Bond};
            
            foreach (Pawn potentiallyRelatedPawn in Pawn.relations.RelatedPawns.Where(p => p != null && p.RaceProps.Humanlike && !p.Dead).ToList())
            {
                if (potentiallyRelatedPawn == null || potentiallyRelatedPawn.Dead)
                    continue;

                if (Pawn.GetMostImportantRelation(potentiallyRelatedPawn) == null)
                    continue;

                if (CanDocumentRelationship(potentiallyRelatedPawn, Pawn.GetMostImportantRelation(potentiallyRelatedPawn)))
                {
                    if (Pawn.GetMostImportantRelation(potentiallyRelatedPawn) == PawnRelationDefOf.Child && Pawn.gender == Gender.Female)
                        continue;
                    AddEvent(new LifeEvent(Pawn, Pawn.GetMostImportantRelation(potentiallyRelatedPawn), potentiallyRelatedPawn));
                }    
            }
        }

        public void UpdateVarious(bool beHere = true, bool pawnStatus = true, bool rels = true, bool dEvents = true)
        {

            if (documentedEvents == null)
                documentedEvents = new List<DocumentedEvent>();
            if (pawnStatus)
            {
                UpdatePawnStatus();
            }

            if (dEvents)
            {
                UpdateEvents();
            }

            if (beHere && !pawnStatus)
            {
                UpdateShouldBeHere();
            }
            
            if (rels)
            {
                UpdateRelationships();
            }

            foreach (LifeEvent lifeEvent in FalselyDocumentedEvents())
            {
                documentedEvents.Remove(lifeEvent);
            }
        }

        public List<DocumentedEvent> FalselyDocumentedEvents(bool force = false)
        {
            if (documentedEvents == null)
                documentedEvents = new List<DocumentedEvent>();

            if (Pawn == null)
                return null;

            List<DocumentedEvent> eventsToRemove = new List<DocumentedEvent>();
            
            foreach (DocumentedEvent docEvent in documentedEvents.Where(e => !e.forceAdd || force))
            {
                if (docEvent is LifeEvent lifeEvent)
                {
                    if (lifeEvent.lostPart != null)
                    {
                        // Find out if this part is not actually missing
                        if (Pawn.health.hediffSet.GetNotMissingParts().Any(p => p == lifeEvent.lostPart))
                        {
                            // If the part is NOT actually missing, check if we've documented adding a different body part to replace the original (ie installed a prosthetic)
                            if (!documentedEvents.Any(de => de is LifeEvent deLifeEvent && de.def == NewBodyPart && deLifeEvent.lostPart == lifeEvent.lostPart))
                            {
                                eventsToRemove.Add(docEvent);
                            }
                        }
                    }
                }

            }
            return eventsToRemove;
        }

        public void PawnDeath()
        {
            if (GetComp().ManuallyUntracked.Contains(Pawn))
                return;

            if (Pawn == null)
            {
                if (GetComp().unsavedNullComps.Contains(this))
                {
                    return;
                }    
                else
                {
                    GetComp().unsavedNullComps.Add(this);
                    Log.Warning("Could not do PawnDeath for null pawn");
                    return;
                }                
            }

            if (ResurrectAfterDeath())
                return;

            if (!Pawn.RaceProps.Humanlike)
            {
                LogPTMDev.Message("Non-human death");
                return;
            }

        
            UpdateVarious();

            if (mostRecentEvent != null && mostRecentEvent.def == Stillborn)
            {
                LogPTMDev.Message("Stillborn");
            }
            /*
            // REMOVE CODE where we skip documenting a dead pawn if they're not important...it may be the source of other errors, and doesn't add much to filesize/etc
            if (!everWasColonist && !everWasPrisoner && !(mostRecentEvent != null && mostRecentEvent.def == Stillborn) && !(GetCorpsePawn(Pawn) != null && GetCorpsePawn(Pawn).IsColonist == true) && !Pawn.IsColonist && !Pawn.IsPrisonerOfColony)
            {
                if (!new SpecialThingFilterWorker_CorpsesColonist().Matches(Pawn) && !new SpecialThingFilterWorker_CorpsesColonist().Matches(Pawn.Corpse))
                {
                    LogPTMDev.Message("Death of pawn who wasn't a colonist (and wasn't stillborn) - Remove");
                    GetComp().removePawns.Add(Pawn);
                    return;
                }
            }*/
            LogPTMDev.Message($"Pawn -> DeadTrackedPawns");
            GetComp().DeadTrackedPawns[Pawn.Name.ToString()] = new DeadPawnComp(Pawn);
            try {GetComp().removePawns.Add(Pawn);}
            catch {};
        }

        public void AddPawnDiedEvent(ThoughtDef thoughtDef, Pawn deadPawn)
        {
            string defName = thoughtDef.defName;
            List<Pawn> otherPawn = new List<Pawn>() {deadPawn};
            if (defName == "MyWifeDied" || defName == "MyHusbandDied")
            {
                AddEvent(new LifeEvent(Pawn, PawnDied, "Spouse", otherPawn));
                return;
            }
            if (defName == "MySonDied" || defName == "MyDaughterDied")
            {
                AddEvent(new LifeEvent(Pawn, PawnDied, "Child", otherPawn));
                return;
            }
            if (defName == "MyMotherDied" || defName == "MyFatherDied")
            {
                AddEvent(new LifeEvent(Pawn, PawnDied, "Parent", otherPawn));
                return;
            }
            if (defName == "MyLoverDied" || defName == "MyFianceeDied")
            {
                AddEvent(new LifeEvent(Pawn, PawnDied, "Lover", otherPawn));
                return;
            }
            if (defName == "MyBrotherDied" || defName == "MySisterDied" || defName == "MyHalfSiblingDied")
            {
                AddEvent(new LifeEvent(Pawn, PawnDied, "Sibling", otherPawn));
                return;
            }
            if (defName == "MyGrandchildDied")
            {
                AddEvent(new LifeEvent(Pawn, PawnDied, "Grandchild", otherPawn));
                return;
            }
            if (defName == "MyGreatGrandchildDied")
            {
                AddEvent(new LifeEvent(Pawn, PawnDied, "Great-grandchild", otherPawn));
                return;
            }
            if (defName == "MyNeiceDied")
            {
                AddEvent(new LifeEvent(Pawn, PawnDied, "Neice", otherPawn));
                return;
            }
            if (defName == "MyNephewDied")
            {
                AddEvent(new LifeEvent(Pawn, PawnDied, "Nephew", otherPawn));
                return;
            }
        }

        public bool CanDocumentRelationship(Pawn otherPawn, DocumentedEventDef def, bool forceAdd = false)
        {
            if (forceAdd)
                return true;

            if (!everWasColonist && !everWasPrisoner && !Pawn.IsColonist)
                return false;
            if (GetComp() == null)
                return false;
            if (NeedPopStarterRelations)
            {
                PopulateStarterRelations();
                return false;
            }    

            if (def == null)
            {
                return false;
            }   

            if (def == BecameFather && otherPawn.DevelopmentalStage != DevelopmentalStage.Baby)
                return false;

            if (StarterRelationDict.ContainsKey(PawnName(otherPawn)))
            {
                //If this pawn is in our starter relationships...
                Tuple<PawnRelationDef, int> existingRelation = StarterRelationDict[PawnName(otherPawn)];
                if (RelationDefLifeEvent(existingRelation.Item1) == def)
                {
                    if (lifeEvents != null && lifeEvents.Any(le => le.otherPawnSingle == otherPawn) && lifeEvents.Where(le => le.otherPawnSingle == otherPawn).Last().def != def)
                    {
                        return true;
                    }
                    return false;
                }
            }

            if (lifeEvents != null && !lifeEvents.Any(le => le.otherPawnSingle == otherPawn))
            {
                return true;
            }

            if (lifeEvents != null && lifeEvents.Where(le => le.otherPawnSingle == otherPawn).Last().def == def)
            {
                return false;
            }

            return true;
        }

        public bool CanDocumentRelationship(Pawn otherPawn, PawnRelationDef def, bool forceAdd = false)
        {
            if (forceAdd)
                return true;
            if (Pawn == null || otherPawn == null)
                return false;
            if (!everWasColonist && !everWasPrisoner && !Pawn.IsColonist)
                return false;
            if (GetComp() == null)
                return false;
            if (NeedPopStarterRelations)
            {
                PopulateStarterRelations();
                //Log.Message("pop starter");
                return false;
            }    

            if (Pawn.GetMostImportantRelation(otherPawn) == null)
                return false;
            if (RelationDefLifeEvent(def) == null)
                return false;

            List<PawnRelationDef> ignoreRelations = new List<PawnRelationDef>() {PawnRelationDefOf.Kin, PawnRelationDefOf.Grandparent, PawnRelationDefOf.Cousin, PawnRelationDefOf.Parent, PawnRelationDefOf.GreatGrandparent, PawnRelationDefOf.GranduncleOrGrandaunt, PawnRelationDefOf.Bond};

            if (Pawn.gender == Gender.Female)
            {
                ignoreRelations.Add(PawnRelationDefOf.Child);
            }

            if (ignoreRelations.Contains(def))
            {                
                return false;
            } 

            if (def == PawnRelationDefOf.Child && otherPawn.DevelopmentalStage != DevelopmentalStage.Baby)
                return false;
            
            if (StarterRelationDict.ContainsKey(PawnName(otherPawn)))
            {
                //If this pawn is in our starter relationships...
                Tuple<PawnRelationDef, int> existingRelation = StarterRelationDict[PawnName(otherPawn)];
                
                //If the starter relationship type is the same as the one we're trying to add and we've never had a DIFFERENT relationship change with this pawn, don't let them add the event
                if (existingRelation.Item1 == def)
                {
                    if (lifeEvents != null && lifeEvents.Any(le => le.otherPawnSingle == otherPawn) && lifeEvents.Where(le => le.otherPawnSingle == otherPawn).Last().def != RelationDefLifeEvent(def))
                    {
                        return true;
                    }
                    return false;
                }
                if (def != Pawn.GetMostImportantRelation(otherPawn))
                {
                    return false;
                }
                    
            }

            if (lifeEvents != null && !lifeEvents.Any(le => le.otherPawnSingle == otherPawn))
            {
                return true;
            }

            // If we got to this point, there ARE some results for lifeEvents where the pawn is otherPawnSingle
            if (lifeEvents != null && lifeEvents.Where(le => le.otherPawnSingle == otherPawn).Last().def == RelationDefLifeEvent(def))
            {
                return false;
            }
            
            return true;
        }

        private class HospitalityLogic
        {
            public bool Active => ModsConfig.IsActive("Orion.Hospitality");
            private MethodInfo isGuestMethod;

            public HospitalityLogic()
            {
                if (Active)
                {
                    // Find the type which should be present if the mod is loaded
                    Type guestUtilityType = AccessTools.TypeByName("Hospitality.Utilities.GuestUtility");
                    if (guestUtilityType != null)
                    {
                        // Get the IsGuest method information
                        isGuestMethod = AccessTools.Method(guestUtilityType, "IsGuest", new Type[] { typeof(Pawn) });
                    }
                }
            }
            public bool IsGuest(Pawn pawn)
            {
                if (Active && isGuestMethod != null)
                {
                    // If the method exists, invoke it dynamically
                    // Since IsGuest is static, the first parameter is null because we're not invoking it on an instance
                    return (bool)isGuestMethod.Invoke(null, new object[] { pawn });
                }
                // Return false or some default value if the method can't be invoked (mod not active or method not found)
                return false;
            }
            
        }

        public bool EventAfterEvent(Predicate<DocumentedEvent> isEvent1, Predicate<DocumentedEvent> isEvent2)
        {
            if (documentedEvents == null)
            {
                documentedEvents = new();
                return false;
            }

            if (!documentedEvents.Any(isEvent2) || !documentedEvents.Any(isEvent1))
                return false;

            return documentedEvents.FindLastIndex(isEvent2) > documentedEvents.FindLastIndex(isEvent1);

        }

        public DocumentedEvent GetPrecedingEvent(DocumentedEvent docEvent, DocumentedEventDef precedingEventDef)
        {
            // Check if docEvent has occurred since the time of an event with precedingEventDef
            if (!EventAfterEvent(e => e.def == precedingEventDef, e => e == docEvent))
            {
                return null;
            }

            if (documentedEvents == null)
            {
                documentedEvents = new();
                return null;
            }

            // The last event that occurred BEFORE the docEvent and has the def we're looking for
            return documentedEvents.FindLast(e => e.def == precedingEventDef && e.ticks < docEvent.ticks);
        }

        public DocumentedEvent GetPrecedingEvent(DocumentedEvent docEvent, EventSubType precedingEventSubType)
        {
            // Check if docEvent has occurred since the time of an event with precedingEventDef
            if (!EventAfterEvent(e => e.eventSubType == precedingEventSubType, e => e == docEvent))
            {
                return null;
            }

            if (documentedEvents == null)
            {
                documentedEvents = new();
                return null;
            }

            // The last event that occurred BEFORE the docEvent and has the def we're looking for
            return documentedEvents.FindLast(e => e.eventSubType == precedingEventSubType && e.ticks < docEvent.ticks);
        }

        public bool ResurrectAfterDeath()
        {
            if (documentedEvents == null)
            {
                documentedEvents = new List<DocumentedEvent>();
                return false;
            }

            return EventAfterEvent(
                e => e is DeathEvent,
                e => e.def == Resurrected
            );
        }

        public bool RemovedWasInfected(LifeEvent lifeEvent)
        {
            if (Pawn == null)
                return false;
            List<Hediff> infections = Pawn.health.hediffSet.hediffs.Where(h => h.def == HediffDefOf.WoundInfection).ToList();
            return infections.Any(h => h.Part == lifeEvent.lostPart);
        }

    }
}