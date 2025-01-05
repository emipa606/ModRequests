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
    public class DeadPawnEvent : IExposable
    {
        public DocumentedEventDef def;
        public string description;
        public int ticks;
        public string eventType;
        public EventSubType eventSubType;
        public int year;
        public string season;
        public int dayOfSeason;
        public int dayOfYear;
        public int dayOfQuadrum;
        public int hour;
        public string priorFaction;
        public string recruiter;
        public string otherPawns;
        public string lostPart;
        public string newPart;
        public string dinfo;
        public string kidnappers;
        public string pronounPossessive;
        public string installedPart;
        public GameEvent associatedEvent;
        public LifeEventCategory lifeEventCategory;

        public DeadPawnEvent() {}

        public DeadPawnEvent(DocumentedEvent docEvent)
        {
            if (docEvent.def == DocumentedEventDefOf.UnknownDeath && associatedEvent != null)
            {
                docEvent.def = GameEventInjuries;
                docEvent.description = GameEventInjuries.description;
            }
            this.def = docEvent.def;
            this.pronounPossessive = docEvent.pawn.PawnTrackerComp()?.PronounPossessive;
            this.description = docEvent.Description();
            this.ticks = docEvent.ticks;
            this.eventType = docEvent is JoinEvent ? "Join" : docEvent is ArriveEvent ? "Arrive" : docEvent is LeaveEvent ? "Leave" : docEvent is DeathEvent ? "Death" : docEvent is LifeEvent ? "Misc" : null;
            this.eventSubType = docEvent.eventSubType;
            this.year = docEvent.year;
            this.season = docEvent.season;
            this.dayOfSeason = docEvent.dayOfSeason;
            this.dayOfYear = docEvent.dayOfYear;
            this.dayOfQuadrum = docEvent.dayOfQuadrum;
            this.hour = docEvent.hour;
            this.associatedEvent = docEvent.associatedEvent;

            
            
            
            if (docEvent is JoinEvent jEvent)
            {
                this.priorFaction = jEvent.priorFaction != null ? (string) jEvent.priorFaction.ToString() : null;
                this.recruiter = jEvent.recruiter != null ? jEvent.recruiter.Name.ToString() : null;
            }

            if (docEvent is ArriveEvent aEvent)
            {
                this.priorFaction = aEvent.priorFaction != null ? (string) aEvent.priorFaction.ToString() : null;
            }

            if (docEvent is LeaveEvent lEvent)
            {
                this.kidnappers = lEvent.kidnappers != null ? (string) lEvent.kidnappers.ToString() : null;
            }

            if (docEvent is DeathEvent dEvent)
            {
                if (docEvent.def == GameEventInjuries || this.def == GameEventInjuries)
                {
                    this.dinfo = this.description;
                }
                else
                {
                    this.dinfo = PawnTrackerUtility.DeathDescription(dEvent);
                }
            }

            if (docEvent is LifeEvent lfEvent)
            {
                string others = "";
                if (lfEvent.otherPawns != null && lfEvent.otherPawns.Any())
                {
                    if (lfEvent.otherPawns[0] == null)
                        others = "Unknown Pawn";
                    else
                        others = PawnName(lfEvent.otherPawns[0]);

                    if (lfEvent.otherPawns.Count > 1)
                    {
                        for (int i = 1; i < lfEvent.otherPawns.Count; i++)
                        {
                            if (lfEvent.otherPawns[i] == null)
                            {
                                others += ", Unknown Pawn";
                            }
                            else
                            {
                                others += ", " + PawnName(lfEvent.otherPawns[i]);
                            }
                        }
                    }
                    this.otherPawns = others;
                }
                if (lfEvent.lostPart != null)
                    this.lostPart = lfEvent.lostPartLabel;
                if (lfEvent.installedPart != null)
                    this.installedPart = lfEvent.installedPart;

                this.lifeEventCategory = lfEvent.category;
            }
        }
        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref description, "description");
            Scribe_Values.Look(ref pronounPossessive, "pronounPossessive");
            Scribe_Values.Look(ref ticks, "ticks");
            Scribe_Values.Look(ref eventType, "eventType");
            Scribe_Values.Look(ref eventSubType, "eventSubType");
            Scribe_Values.Look(ref year, "year");
            Scribe_Values.Look(ref season, "season");
            Scribe_Values.Look(ref dayOfYear, "dayOfYear");
            Scribe_Values.Look(ref dayOfSeason, "dayOfSeason");
            Scribe_Values.Look(ref dayOfQuadrum, "dayOfQuadrum");
            Scribe_Values.Look(ref hour, "hour");
            Scribe_Values.Look(ref priorFaction, "priorFaction");
            Scribe_Values.Look(ref recruiter, "recruiter");
            Scribe_Values.Look(ref otherPawns, "otherPawns");
            Scribe_Values.Look(ref lostPart, "lostPart");
            Scribe_Values.Look(ref dinfo, "dinfo");
            Scribe_Values.Look(ref kidnappers, "kidnappers");       
            Scribe_Deep.Look(ref associatedEvent, "associatedEvent");     
        }
    }

    
}