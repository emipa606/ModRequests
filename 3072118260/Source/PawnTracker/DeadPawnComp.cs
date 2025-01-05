using Verse;
using System;
using System.Collections.Generic;
using RimWorld;
using static PawnTrackerMain.PawnTrackerUtility;
using System.Linq;
using static PawnTrackerMain.DocumentedEventDefOf;
using UnityEngine;

namespace PawnTrackerMain
{
    public class DeadPawnComp : IExposable
    {
        public List<DeadPawnEvent> documentedEvents;
        public Pawn pawn;
        public string PawnLabelShort;
        public string Pronoun;
        public string Name;
        public NameTriple PawnTripleName;
        public bool startingColonist;
        public bool everWasColonist;
        public bool everWasPrisoner;
        public PawnTrackerUtility.PawnStatus pawnStatus;
        public bool shouldBeHere;
        public int tickOfDeath;
        public GameEvent associatedEvent;

        public string PortraitPath;

        public DeadPawnComp() {}

        public DeadPawnComp(Pawn pawn)
        {
            this.documentedEvents = new List<DeadPawnEvent>();
            this.pawn = pawn;
            this.Name = pawn.Name.ToString();
            this.PawnLabelShort = pawn.LabelShort;
            this.Pronoun = pawn.gender.GetPronoun();
            try
                {this.PawnTripleName = (NameTriple) pawn.Name;}
            catch
                {this.PawnTripleName = new NameTriple(pawn.Name.ToString().Split(' ')[0], pawn.Name.ToString().Split(' ')[0], pawn.Name.ToString().Split(' ')[1]);}
            PawnTrackerComp ptc = pawn.TryGetComp<PawnTrackerComp>();
            this.startingColonist = ptc.startingColonist;
            this.everWasColonist = ptc.everWasColonist;
            this.everWasPrisoner = ptc.everWasPrisoner;
            this.pawnStatus = ptc.pawnStatus;
            this.shouldBeHere = false;

            if (ptc == null)
            {
                Log.Error("PawnTracker Comp is null");
                return;
            }

            if (PortraitPath == null)
            {
                PortraitManager.SavePortrait(pawn);
                PortraitPath = PortraitManager.SaveFilePath(pawn.Name.ToString());
            }
                
            Vector2 size = new Vector2(100f, 100f);
            Rot4 south = Rot4.South;
            if (ptc.MostRecentEvent(e => e is DeathEvent) != null)
            {
                DeathEvent death = (DeathEvent)ptc.MostRecentEvent(e => e is DeathEvent);
                this.tickOfDeath = death.ticks;

                death.associatedEvent = FindAssociatedEvent(death);
                associatedEvent = death.associatedEvent;
            }
            else
                this.tickOfDeath = GenTicks.TicksAbs;
            if (ptc.documentedEvents == null)
                ptc.documentedEvents = new List<DocumentedEvent>();
            DocumentedEventsToComp(ptc.documentedEvents);
        }

        public GameEvent FindAssociatedEvent(DeathEvent death)
        {
            if (GetComp().gameEvents != null && GetComp().gameEvents.Any())
            {
                // Use current raid if there is one, most-recent if it's over
                GameRaidEvent mostRecentRaid = GetComp().currentRaid != null ? GetComp().currentRaid : GetComp().mostRecentRaid;
                GameManhunterEvent mostRecentManhunter = GetComp().currentManhunters != null ? GetComp().currentManhunters : GetComp().mostRecentManhunters;

                if (mostRecentRaid != null)
                {
                    PawnDeathDetails pdd = death.details;
                    if (pdd != null && pdd.instigator != null && pdd.instigator is Pawn instigator && mostRecentRaid.raidersFaction != null && instigator.Faction == mostRecentRaid.raidersFaction)
                    {
                        return mostRecentRaid;
                    }
                    else if (VictimOfEvent(death, mostRecentRaid))
                    {  
                        return mostRecentRaid;
                    }
                }
                else if (mostRecentManhunter != null)
                {
                    PawnDeathDetails pdd = death.details;
                    if (pdd != null && pdd.instigator != null &&  pdd.instigator is Pawn instigator && instigator.kindDef == mostRecentManhunter.animalDef)
                    {
                        return mostRecentManhunter;
                    }
                    else if (VictimOfEvent(death, mostRecentManhunter))
                    {  
                        return mostRecentManhunter;
                    }
                }
                else if (GetComp().gameEvents != null && GetComp().gameEvents.Any(ge => ge is GameMiscEvent && VictimOfEvent(death, ge)))
                {
                    return GetComp().gameEvents.Where(ge => ge is GameMiscEvent && VictimOfEvent(death, ge)).Last();
                }
            }
            
            return null;
        }

          

        public void DocumentedEventsToComp(List<DocumentedEvent> events)
        {
            if (!GetComp().UseMod)
                return;
            if (documentedEvents == null)
                documentedEvents = new List<DeadPawnEvent>();
            foreach (DocumentedEvent docEvent in events)
            {
                if (docEvent == null)
                    continue;

                // If it's an Unknown Death but we know that they were a victim of a raid, then... it's not an unknown!
                if (docEvent is DeathEvent deathEvent && (deathEvent.def == UnknownDeath || deathEvent.description.Contains("unknown causes")) && deathEvent.associatedEvent != null)
                {
                    DeathEvent victimDeathEvent = new DeathEvent(deathEvent.pawn, GameEventInjuries);
                    victimDeathEvent.SetTicks(deathEvent.ticks, DocumentedEvent.TickTypes.Abs);
                    victimDeathEvent.associatedEvent = deathEvent.associatedEvent;
                    documentedEvents.Add(new DeadPawnEvent(victimDeathEvent));
                    continue;
                }
                documentedEvents.Add(new DeadPawnEvent(docEvent));
            }
            FixUnknownDeath();
        }

        public void OrderPawnHistory()
        {
            documentedEvents.Sort((x, y) => x.ticks.CompareTo(y.ticks));
        }

        public List<DeadPawnEvent> PresenceRelatedEvents()
        {
            OrderPawnHistory();
            //Make a list of the relevant events
            List<DeadPawnEvent> relevant = new List<DeadPawnEvent>();
            //If the pawn is a prisoner, automatically return ALL their events
            //       I disagree with this, it should filter down to only their pre-arrest and arrest events...
            if (pawnStatus == PawnTrackerUtility.PawnStatus.Prisoner)
                return documentedEvents;
            // Look at each event from the start of the list of events
            for (int i = 0; i < this.documentedEvents.Count ; i++)
            {
                DeadPawnEvent docEvent = this.documentedEvents[i];
                DeadPawnEvent nextEvent = i + 1 < documentedEvents.Count ? this.documentedEvents[i + 1] : null;
                //If we already added this event to the list based on the previous event's conditions, skip
                if (relevant.Contains(docEvent))
                    continue;
                //If the event involves leaving, we will always show it
                if (docEvent.eventType == "Leave")
                {
                    //If this is an unknown disappearance and then the pawn unknownly reappears, don't include it
                    if (docEvent.def == UnknownLeave && nextEvent.def == UnknownArrive)
                        continue;
                    relevant.Add(docEvent);
                    continue;
                }   
                //If the event involves being captured or joining the colony, we will always show it
                if ((EventSubType)docEvent.eventSubType == EventSubType.Capture || docEvent.eventType == "Join")
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

        public void FixUnknownDeath()
        {
            if (documentedEvents != null)
            {
                List<DeadPawnEvent> toFix = documentedEvents.Where(e => e.def == UnknownDeath && e.associatedEvent != null && (e.associatedEvent is GameRaidEvent || e.associatedEvent is GameManhunterEvent)).ToList();
                if (toFix.Any())
                {
                    foreach (DeadPawnEvent dpe in toFix)
                    {
                        var assoc = dpe.associatedEvent;
                        
                        dpe.def = GameEventInjuries;
                        dpe.description = GameEventInjuriesDescription(dpe.associatedEvent);
                        dpe.dinfo = dpe.description;
                    }
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref documentedEvents, "documentedEvents", LookMode.Deep);
            Scribe_References.Look(ref pawn, "Pawn");
            Scribe_Values.Look(ref Name, "Name");
            Scribe_Deep.Look(ref PawnTripleName, "PawnTripleName");
            Scribe_Values.Look(ref startingColonist, "startingColonist");
            Scribe_Values.Look(ref everWasColonist, "everWasColonist");
            Scribe_Values.Look(ref everWasPrisoner, "everWasPrisoner");
            Scribe_Values.Look(ref pawnStatus, "pawnStatus");
            Scribe_Values.Look(ref shouldBeHere, "shouldBeHere");
            Scribe_Values.Look(ref PawnLabelShort, "PawnLabelShort");
            Scribe_Values.Look(ref Pronoun, "Pronoun");
            Scribe_Values.Look(ref PortraitPath, "PortraitPath");
            Scribe_Deep.Look(ref associatedEvent, "associatedEvent");
        } 
    }
    
}