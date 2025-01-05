using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static PawnTrackerMain.PawnTrackerUtility;
using static PawnTrackerMain.PawnDeathDetails;
using static PawnTrackerMain.DocumentedEventDefOf;
using System.Reflection;
using UnityEngine;
using System;

namespace PawnTrackerMain
{
    public class DocumentedEvent : IExposable
    {
        public DocumentedEventDef def;
        public int ageBiologicalYears;
        public string description;
        public int ticks;
        public string eventType;
        public EventSubType eventSubType;
        public ModSpecific modSpecific;
        private Map CurrentMap;
        public int year;
        private RimWorld.Season rwSeason;
        public string season;
        public int dayOfSeason;
        public int dayOfYear;
        public int dayOfQuadrum;
        public int hour;
        public string pawnName;
        public Pawn pawn;
        public bool centralEvent;
        public bool forceAdd = false;
        public GameEvent associatedEvent;
        public DocumentedEvent(Pawn pawn, DocumentedEventDef def, bool forceAdd = false) 
        {
            this.def = def;
            this.forceAdd = forceAdd;

            if (GetComp().currentRaid != null)
                this.associatedEvent = GetComp().currentRaid;
            else if (GetComp().currentManhunters != null)
                this.associatedEvent = GetComp().currentRaid;
            
            if (pawn == null)
            {
                try
                {
                    pawn = PawnTrackerUtility.GetCorpsePawn(pawn);
                    if (pawn == null)
                    {
                        return;
                    }
                }
                    
                catch
                {
                    Log.Error("Cannot document null pawn");
                    return;
                }
            }
            CurrentMap = pawn.Map == null ? Find.CurrentMap : pawn.Map;
            //if (CurrentMap == null)
            //    Log.Warning("Null map");
            this.pawn = pawn;
            
            this.ticks = GenTicks.TicksAbs;            
            
            if (CurrentMap != null)
            {
                this.ageBiologicalYears = pawn.ageTracker.AgeBiologicalYears;
                this.year = GenLocalDate.Year(CurrentMap);
                rwSeason = GenLocalDate.Season(CurrentMap);
                this.season = rwSeason == Season.Summer ? "Summer" : rwSeason == Season.Spring ? "Spring" : rwSeason == Season.Fall ? "Fall" : "Winter";
                this.dayOfSeason = GenLocalDate.DayOfSeason(CurrentMap);
                this.dayOfQuadrum = GenLocalDate.DayOfQuadrum(CurrentMap);
                this.dayOfYear = GenLocalDate.DayOfYear(CurrentMap);
                this.hour = GenLocalDate.HourInteger(CurrentMap);
            }
            this.pawnName = pawn.Name.ToString();

            if (this.def != null)
            {
                this.pawn.PawnTrackerComp().UpdatePawnStatus();
                this.centralEvent = this.def.centralForPawnStatus.Contains(this.pawn.PawnTrackerComp().pawnStatus) || this is JoinEvent || this is ArriveEvent || this is LeaveEvent || this is DeathEvent;
                this.eventType = this.def.ToString();
                this.description = this.def.description != null ? this.def.description : null;
                if (def.docEventSubType == null || def.eventSubType == EventSubType.None)
                {
                    if (this.description != null && this.description.ToUpper().Contains("CAPTURE"))
                        this.eventSubType = EventSubType.Capture;
                    else if (this.def is DeathEventDef)
                        this.eventSubType = EventSubType.Death;
                    else if (this.def.defName.Contains("Unknown"))
                        this.eventSubType = EventSubType.Unknown;
                    else if (this.def == PawnDied)
                        this.eventSubType = EventSubType.OtherPawnDied;
                    else
                        this.eventSubType = EventSubType.None;
                }
                else
                {
                    this.eventSubType = def.eventSubType;
                }

                if (def.docModSpecific == null)
                {
                    this.modSpecific = ModSpecific.None;
                }
                else
                {
                    this.modSpecific = def.modSpecific;
                }
            }

        }
        public void ValidateProperties()
        { 
            if (!Prefs.DevMode)
                return;
            if (def.requiredProperties == null)
                return;
                
            List<string> missing = def.requiredProperties.Where(p => GetType().GetField(p) == null && GetType().GetProperty(p) == null).ToList();
            if (missing.Any())
            {
                StringBuilder sb = new();
                foreach (string s in missing)
                    sb.Append(s +" ");
                LogPTMDev.Error($"Required properties are null for def {def.defName}: {sb}");
            }
        }

        // Copied this method from the base game GenLocalDate because it is necessary but private
        private static Vector2 LocationForDate(Map thing)
        {
            int tile = thing.Tile;
            return tile >= 0 ? Find.WorldGrid.LongLatOf(tile) : Vector2.zero;
        }
        //...also this one
        private static float LongitudeForDate(Map thing) => LocationForDate(thing).x;

        public enum TickTypes{Game, Abs}

        public void SetTicks(int ticks, TickTypes tickType, int? ageBio = null)
        {
            float longitude;
            Map map = Find.CurrentMap;
            if (map != null)
            {
                longitude = LongitudeForDate(map);
            }
            else
            {
                return;
            }

            this.ticks = tickType == TickTypes.Game ? GenDate.TickGameToAbs(ticks) : ticks;
            this.year = GenDate.Year(ticks, longitude);
            this.dayOfYear = GenDate.DayOfYear(ticks, longitude);
            this.hour = GenDate.HourOfDay(ticks, longitude);
            rwSeason = GenDate.Season(ticks, LocationForDate(map));
            this.season = rwSeason == Season.Summer ? "Summer" : rwSeason == Season.Spring ? "Spring" : rwSeason == Season.Fall ? "Fall" : "Winter";
            this.dayOfSeason = GenDate.DayOfSeason(ticks, longitude);
            this.dayOfQuadrum = GenDate.DayOfQuadrum(ticks, longitude);

            if (ageBio != null)
                ageBiologicalYears = (int)ageBio;
        }

        public virtual string Description()
        {
            if (pawn == null || pawn.PawnTrackerComp() == null)
                return description;
            if (this is LeaveEvent leaveEvent && leaveEvent.def == Kidnapped && leaveEvent.kidnappers != null)
            {
                return $"was kidnapped by members of {leaveEvent.kidnappers.Name}";
            }

            // Get the capture even that occurred most-recently before recruitment
            if (def == Recruited && pawn.PawnTrackerComp().GetPrecedingEvent(this, EventSubType.Capture) != null)
            {
                int tickCaptured = pawn.PawnTrackerComp().GetPrecedingEvent(this, EventSubType.Capture).ticks;
                return $"was recruited after spending {Math.Ceiling(((float)ticks - tickCaptured)/60000)} days as a prisoner";
            }
            
            string poss = pawn.PawnTrackerComp().PronounPossessive == null ? "their" : pawn.PawnTrackerComp().PronounPossessive;
            if (this.description != null && poss != null)
                return this.description.Replace("their", poss);
            return this.description;
        }

        public bool IsDeathEvent()
        {
            return def != null && def is PawnTrackerMain.DeathEventDef;
        }

        public string ToSummary()
        {
            return $"Day {dayOfSeason} of {season}, year {year}: {char.ToUpper(description[0]) + description.Substring(1)}";
        }

        public List<GameRaidEvent> RelevantRaids(int withinTicks = 60000)
        {   
            List<GameRaidEvent> raids = new();
            foreach (GameEvent ge in GetComp().gameEvents.Where(ge => ge is GameRaidEvent raid && ticks - raid.tickStart <= withinTicks))
                raids.Add((GameRaidEvent) ge);
            return raids;
        }

        public List<GameRaidEvent> RelevantRaids(Faction faction, int withinTicks = 60000)
        {   
            List<GameRaidEvent> raids = new();
            foreach (GameEvent ge in GetComp().gameEvents.Where(ge => ge is GameRaidEvent raid && ticks - raid.tickStart <= withinTicks && raid.raidersFaction == faction))
                raids.Add((GameRaidEvent) ge);
            return raids;
        }

        public List<GameManhunterEvent> RelevantManhunters(int withinTicks = 60000)
        {   
            List<GameManhunterEvent> manhunters = new();
            foreach (GameEvent ge in GetComp().gameEvents.Where(ge => ge is GameManhunterEvent manhunters && ticks - manhunters.tickStart <= withinTicks))
                manhunters.Add((GameManhunterEvent) ge);
            return manhunters;
        }

        public List<GameManhunterEvent> RelevantManhunters(PawnKindDef animalDef, int withinTicks = 60000)
        {   
            List<GameManhunterEvent> manhunters = new();
            foreach (GameEvent ge in GetComp().gameEvents.Where(ge => ge is GameManhunterEvent manhunters && ticks - manhunters.tickStart <= withinTicks && manhunters.animalDef == animalDef))
                manhunters.Add((GameManhunterEvent) ge);
            return manhunters;
        }

        public List<GameMiscEvent> RelevantMiscEvents(List<GameEventDef> relevantEvents = null, int withinTicks = 60000)
        {   
            if (relevantEvents == null)
                relevantEvents = new List<GameEventDef>();
            List<GameMiscEvent> miscs = new();
            foreach (GameEvent ge in GetComp().gameEvents.Where(ge => ge is GameMiscEvent miscEvent && ticks - miscEvent.tickStart <= withinTicks))
                miscs.Add((GameMiscEvent) ge);
            return miscs;
        }

        public virtual void ExposeData()
        {
            if (pawn != null)
            {
                if (centralEvent == false && def.centralForPawnStatus.Contains(pawn.PawnTrackerComp().pawnStatus))
                    centralEvent = true;
                else if (centralEvent == false && this is not LifeEvent)
                    centralEvent = true;
            }
            
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref description, "description");
            Scribe_Values.Look(ref eventType, "eventType");
            Scribe_Values.Look(ref pawnName, "pawnName");
            Scribe_References.Look(ref CurrentMap, "CurrentMap");
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Values.Look(ref ageBiologicalYears, "ageBiologicalYears");
            Scribe_Values.Look(ref year, "year");
            Scribe_Values.Look(ref ticks, "ticks");
            Scribe_Values.Look(ref season, "season");
            Scribe_Values.Look(ref dayOfSeason, "dayOfSeason");
            Scribe_Values.Look(ref hour, "hour");
            Scribe_Values.Look(ref dayOfYear, "dayOfYear");
            Scribe_Values.Look(ref dayOfQuadrum, "dayOfQuadrum");
            Scribe_Values.Look(ref eventSubType, "eventSubType");
            Scribe_Values.Look(ref modSpecific, "modSpecific");
            Scribe_Values.Look(ref centralEvent, "centralEvent");
            Scribe_Values.Look(ref forceAdd, "forceAdd");
            Scribe_Deep.Look(ref associatedEvent, "associatedEvent");
        }
    }

    public class JoinEvent : DocumentedEvent
    {
        public Faction priorFaction = null;
        public Pawn recruiter = null;
        public Quest quest = null;

        public JoinEvent(Pawn pawn, JoinEventDef def, Faction priorFaction = (Faction) null, Pawn recruiter = (Pawn) null, bool forceAdd = false) : base(pawn, def, forceAdd)
        {
            this.priorFaction = priorFaction;
            this.recruiter = recruiter;      
            ValidateProperties();      
        }

        public JoinEvent(Pawn pawn, JoinEventDef def, Quest quest, Faction priorFaction = (Faction) null, Pawn recruiter = (Pawn) null, bool forceAdd = false) : base(pawn, def, forceAdd)
        {
            this.priorFaction = priorFaction;
            this.recruiter = recruiter;
            this.quest = quest;  
            ValidateProperties();          
        }

        public JoinEvent() : base(null, null, false)
        {
            // Set default values here
            this.priorFaction = null;
            this.recruiter = null;
            this.quest = null;
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.priorFaction, "priorFaction");
            Scribe_References.Look(ref this.recruiter, "recruiter");
            Scribe_References.Look(ref this.quest, "quest");
        }
    }

    public class ArriveEvent : DocumentedEvent
    {   
        public string precedingEvent = null;
        public Faction priorFaction = null;
        public Quest quest = null;
        public ArriveEvent(Pawn pawn, ArriveEventDef def, Faction priorFaction = null, bool forceAdd = false) : base(pawn, def, forceAdd)
        {
            this.priorFaction = priorFaction != null ? priorFaction : pawn.Faction != Faction.OfPlayer ? pawn.Faction : null;
            ValidateProperties();
        }

        public ArriveEvent(Pawn pawn, ArriveEventDef def, Quest quest, Faction priorFaction = null, bool forceAdd = false) : base(pawn, def, forceAdd)
        {
            this.priorFaction = priorFaction != null ? priorFaction : pawn.Faction != Faction.OfPlayer ? pawn.Faction : null;
            this.quest = quest;
            ValidateProperties();
        }

        public ArriveEvent() : base(null, null, false)
        {
            // Set default values here
            this.priorFaction = null;
            this.quest = null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.priorFaction, "priorFaction");
            Scribe_References.Look(ref this.quest, "quest");
        }
    }
    
    public class LeaveEvent : DocumentedEvent
    {
        public Faction kidnappers = null;

        public LeaveEvent(Pawn pawn, LeaveEventDef def, Faction kidnappers = null, bool forceAdd = false) : base(pawn, def, forceAdd)
        {
            this.kidnappers = kidnappers;
            if (def == Kidnapped)
                this.associatedEvent = GetComp().currentRaid != null ? GetComp().currentRaid : GetComp().mostRecentRaid;

            
            if (this.def != null && this.def == Kidnapped && this.kidnappers == null)
            {
                Log.Error("Kidnappers required when event is Kidnapped");
                return;
            }
            ValidateProperties();
        }

        public LeaveEvent() : base(null, null, false)
        {
            this.kidnappers = null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.kidnappers, "kidnappers");
        }
    }

    public class DeathEvent : DocumentedEvent
    {        
        public DamageInfo? dinfo = null;
        public string causeOfDeath = null;
        public PawnDeathDetails details = null;
        public List<Hediff> hediffsAtDeath = null;
        
        public DeathEvent(Pawn pawn, DeathEventDef def, PawnDeathDetails details = null, bool forceAdd = false) : base(pawn, def, forceAdd)
        {
            if (details != null)
            {
                this.causeOfDeath = details.exactCulprit.ToString();
                this.details = details;
            }
            hediffsAtDeath = pawn.health.hediffSet.hediffs;
            ValidateProperties();
        }

        

        public DeathEvent() : base(null, null, false)
        {
            // Set default values here
            this.causeOfDeath = null;
            this.details = null;
        }

        public override string Description()
        {
            //if (this.pawn == null)
            //{
            //   Log.Error("Tried to return DeathDescription for null pawn");
            //    return "";
            //}
            return PawnTrackerUtility.DeathDescription(this, this.pawn);
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.causeOfDeath, "causeOfDeath");
            Scribe_Deep.Look(ref this.details, "pawnDeathDetails");
            Scribe_Collections.Look(ref this.hediffsAtDeath, "hediffsAtDeath", LookMode.Reference);
            
        }
    }

    public class LifeEvent : DocumentedEvent
    {
        public List<Pawn> otherPawns;
        public List<string> otherPawnsNames;
        public Pawn otherPawnSingle;
        public string otherPawnSingleName;
        public Faction priorFaction;
        
        public LifeEventCategory category => def == null ? LifeEventCategory.None : def is LifeEventDef led ? led.Category : LifeEventCategory.None;
        public BodyPartRecord lostPart;
        public string lostPartLabel => lostPart == null ? null : lostPart.Label;
        public string installedPart;
        public PawnRelationDef relationDef;
        public string relation;

        public LifeEvent(Pawn pawn, PawnRelationDef relationDef, Pawn otherPawnSingle, bool forceAdd = false) : base(pawn: pawn, def: RelationDefLifeEvent(pawn, relationDef), forceAdd)
        {
            LifeEventDef lifeEventDef = pawn != null ? RelationDefLifeEvent(pawn, relationDef) : RelationDefLifeEvent(relationDef);
            this.def = lifeEventDef;
            this.otherPawnSingle = otherPawnSingle;
            this.otherPawnSingleName = PawnName(otherPawnSingle);
            this.otherPawns = new List<Pawn>() {otherPawnSingle}; // Also make the otherPawns list have this single pawn

            this.eventSubType = EventSubType.Relationship;

            OtherPawnsToNames();

            //if it involves other pawns, save the description w/ their names as a string, so it doesn't break if/when they die and become null
            this.description = Description();
            ValidateProperties();
        }

        public LifeEvent(Pawn pawn, LifeEventDef def, List<Pawn> otherPawns, bool forceAdd = false) : base(pawn, def, forceAdd) 
        {
            if (def.eventSubType == EventSubType.BodyPartLoss)
            {
                Log.Error("Body part is required when def is LostBodyPart");
                return;
            }
            this.otherPawns = otherPawns;
            if (this.otherPawns.Count == 1)
            {
                this.otherPawnSingle = this.otherPawns[0];
                this.otherPawnSingleName = PawnName(otherPawnSingle);
            }

            OtherPawnsToNames();

            //if it involves other pawns, save the description w/ their names as a string, so it doesn't break if/when they die and become null
            this.description = Description();
            ValidateProperties();
        }

        public LifeEvent(Pawn pawn, LifeEventDef def, string relation, List<Pawn> otherPawns, bool forceAdd = false) : base(pawn, def, forceAdd) 
        {
            if (def.eventSubType == EventSubType.BodyPartLoss)
            {
                Log.Error("Body part is required when def is LostBodyPart");
                return;
            }
            this.otherPawns = otherPawns;
            if (this.otherPawns.Count == 1)
            {
                this.otherPawnSingle = this.otherPawns[0];
                this.otherPawnSingleName = PawnName(otherPawnSingle);
            }
            this.relation = relation;

            OtherPawnsToNames();

            //if it involves other pawns, save the description w/ their names as a string, so it doesn't break if/when they die and become null
            this.description = Description();
            ValidateProperties();
            
        }

        public LifeEvent(Pawn pawn, LifeEventDef def, bool forceAdd = false) : base(pawn, def, forceAdd) 
        {
            ValidateProperties();
        }
        public LifeEvent(Pawn pawn, LifeEventDef def, Pawn otherPawnSingle, bool forceAdd = false) : base(pawn, def, forceAdd) 
        {
            if (def.eventSubType == EventSubType.BodyPartLoss)
            {
                Log.Error("Body part is required when def is LostBodyPart");
                return;
            }
            this.otherPawnSingle = otherPawnSingle;
            this.otherPawns = new List<Pawn>() {otherPawnSingle}; // Also make the otherPawns list have this single pawn

            OtherPawnsToNames();

            //if it involves other pawns, save the description w/ their names as a string, so it doesn't break if/when they die and become null
            this.description = Description();
            ValidateProperties();
        }

        public LifeEvent(Pawn pawn, LifeEventDef def, string relation, Pawn otherPawnSingle, bool forceAdd = false) : base(pawn, def, forceAdd) 
        {
            if (def.eventSubType == EventSubType.BodyPartLoss)
            {
                Log.Error("Body part is required when def is LostBodyPart");
                return;
            }
            this.otherPawnSingle = otherPawnSingle;
            this.otherPawns = new List<Pawn>() {otherPawnSingle}; // Also make the otherPawns list have this single pawn
            this.relation = relation;

            OtherPawnsToNames();

            //if it involves other pawns, save the description w/ their names as a string, so it doesn't break if/when they die and become null
            this.description = Description();
            ValidateProperties();
        }

        public LifeEvent(Pawn pawn, LifeEventDef def, BodyPartRecord bodyPart, bool forceAdd = false) : base(pawn, def, forceAdd) 
        {
            this.lostPart = bodyPart;
            ValidateProperties();
        }

        public LifeEvent(Pawn pawn, LifeEventDef def, HediffDef newBodyPart, BodyPartRecord removedBodyPart, bool forceAdd = false) : base(pawn, def, forceAdd) 
        {
            if (def != NewBodyPart)
            {
                Log.Error("Don't supply a HediffDef to a LifeEvent unless a new body part was installed");
                return;
            }
            if (newBodyPart == null)
            {
                Log.Error("You GOTTA supply the HediffDef when installing a new body part");
                return;
            }
            this.lostPart = removedBodyPart;
            this.installedPart = newBodyPart.label;
            ValidateProperties();
        }

        public LifeEvent(Pawn pawn, LifeEventDef def, Faction priorFaction, bool forceAdd = false) : base(pawn, def, forceAdd) 
        {
            if (def.eventSubType == EventSubType.BodyPartLoss)
            {
                Log.Error("Body part is required when def is LostBodyPart");
                return;
            }
            if (def == PawnDied)
            {
                Log.Error("Other pawn required when PawnDied");
                return;
            }
            this.priorFaction = priorFaction;
            ValidateProperties();
        }

        public LifeEvent() : base(null, null, false) {}

        public void OtherPawnsToNames()
        {
            if (otherPawns == null && otherPawnSingle == null)
                return;
            if (otherPawns != null && !otherPawns.Any(p => p != null) && otherPawnSingle == null)
                return;

            if (otherPawns != null)
            {
                this.otherPawnsNames = otherPawns.Where(pawn => pawn != null)
                            .Select(pawn => PawnName(pawn))
                            .ToList();
                if (otherPawns.Any(pawn => pawn == null))
                {
                    foreach (Pawn pawn in otherPawns.Where(p => p == null).ToList())
                        otherPawnsNames.Add("Unknown Pawn");
                }
            }

            if (otherPawnSingle != null)
                this.otherPawnSingleName = PawnName(otherPawnSingle);
        }

        public override string Description()
        {
            string desc;
            string poss = "their";
            //If there's any null pawns involved, use the existing description we've likely previous generated; otherwise, start with the def's core description
            desc = otherPawns != null && !otherPawns.Any(p => p == null) ? description : def.description != null ? def.description : def.description != null ? def.description : "";
            

            OtherPawnsToNames();

            if (pawn != null)
            {
                poss = pawn.PawnTrackerComp().PronounPossessive;
                desc = desc != null && poss != null ? desc.Replace("their", poss) : desc;
            }    
            else
            {    
                poss = "their";
            }

            if (eventSubType == EventSubType.Birth)
            {
                desc = def.description;
                string babies = "";
                if (otherPawnSingleName != null)
                {
                    return desc + $" ({otherPawnSingleName})";
                }    
                else
                {
                    if (otherPawnsNames.Count == 1)
                    {
                        return desc + $" ({otherPawnsNames[0]})";
                    }
                    babies = $" ({otherPawnsNames[0]}";
                    if (otherPawnsNames.Count > 1)
                    {
                        for (int i = 1; i < otherPawnsNames.Count; i++)
                        {
                            babies += ", " + otherPawnsNames[i];
                        }
                    }
                    desc += " " + babies +")";
                }
                return desc;
            }

            string otherPawnName = otherPawns != null && otherPawns.Any() && otherPawns[0] != null ? PawnName(otherPawns[0]) : null;

            if (def == LostBodyPart)
            {
                return $"lost {poss} {lostPartLabel}";
            }

            if (def == BodyPartRemoved)
            {
                return $"had {poss} {lostPartLabel} removed";
            }

            if (def == InfectedBodyPartRemoved)
            {
                return $"had {poss} infected {lostPartLabel} removed";
            }

            if (def == BodyPartAmputated)
            {
                return $"had {poss} infected {lostPartLabel} amputated";
            }

            if (def == BodyPartHarvested)
            {
                return $"had {poss} {lostPartLabel} harvested for other uses";
            }

            if (def == NewBodyPart)
            {
                return $"had a new {installedPart} installed";
            }

            if (def == PawnDied)
            {
                if (otherPawnSingleName == null)
                    return $"experienced the death of {poss} {relation.ToLower()}";
                return $"experienced the death of {poss} {relation.ToLower()}, {otherPawnSingleName}";
            }

            if (def == GotMarried)
            {
                if (otherPawnSingleName == null)
                    return "got married";
                return $"got married to {otherPawnSingleName}";
            }

            if (def == NewLovers)
            {
                if (otherPawnSingleName == null)
                    return "became lovers with another pawn";
                return $"became lovers with {otherPawnSingleName}";
            }

            if (def == ExLovers)
            {
                if (otherPawnSingleName == null)
                    return $"broke up with {poss} lover";
                return $"broke up with {otherPawnSingleName}";
            }

            if (def == Divorced)
            {
                if (otherPawnSingleName == null)
                    return "got divorced";
                return $"divorced {poss} spouse, {otherPawnSingleName}";
            }

            if (def == BecameUncleOrAunt)
            {
                string uaType = pawn.gender == Gender.Male ? " uncle" : "n aunt";
                if (otherPawnSingleName == null)
                    return $"became a{uaType}";
                return $"became a{uaType} to {otherPawnSingleName}";
            }

            if (def == BecameFather)
            {
                if (otherPawnSingleName == null)
                    return "became the father to a new baby";
                return $"became the father to a new baby, {otherPawnSingleName}";
            }

            if (def == BecameGrandparent)
            {
                if (otherPawnSingleName == null)
                    return $"became a grandparent to {poss} child's new child";
                return $"became a grandparent to {poss} child's new child, {otherPawnSingleName}";
            }

            if (def == BecameSibling)
            {
                if (otherPawnSingleName == null)
                    return $"got a new sibling";
                return $"got a new sibling, {otherPawnSingleName}";
            }

            if (def == BecameHalfSibling)
            {
                if (otherPawnSingleName == null)
                    return $"got a new half-sibling";
                return $"got a new half-sibling, {otherPawnSingleName}";
            }

            if (def == BecameGreatGrandparent)
            {
                if (otherPawnSingleName == null)
                    return $"became a great-grandparent to {poss} child's child's child!";
                return $"became a great-grandparent to {poss} child's child's child, {otherPawnSingleName}!";
            }

            if (def == GotEngaged)
            {
                if (otherPawnSingleName == null)
                    return $"got engaged to another pawn";
                return $"got engaged to {poss} lover, {otherPawnSingleName}!";
            }

            if (def == PartnerPregnant)
            {
                if (pawn != null)
                {
                    if (otherPawnSingle != null)
                    {
                        if (new List<PawnRelationDef>() {PawnRelationDefOf.Lover, PawnRelationDefOf.Fiance, PawnRelationDefOf.Spouse}.Contains(pawn.GetMostImportantRelation(otherPawnSingle)))
                        {
                            return $"became an expecting {(pawn.gender == Gender.Male ? "father" : "mother")} when {poss} lover, {otherPawnSingleName}, became pregnant with {poss} baby!";
                        }
                        else
                        {
                            return $"became an expecting {(pawn.gender == Gender.Male ? "father" : "mother")} when {otherPawnSingleName} became pregnant with {poss} baby!";
                        }
                    } 
                    else
                    {
                        return $"became an expecting {(pawn.gender == Gender.Male ? "father" : "mother")} to another pawn's baby!";
                    }
                }
            }

            if (def == Pregnant)
            {
                if (otherPawnSingle != null)
                    return $"became pregnant with {otherPawnSingle}'s baby!";
                else
                    return "became pregnant!";
            }

            return desc;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_BodyParts.Look(ref this.lostPart, "lostPart");
            Scribe_Values.Look(ref this.relation, "relation");
            Scribe_Values.Look(ref this.installedPart, "installedPart");
            Scribe_Collections.Look(ref this.otherPawns, "otherPawns", LookMode.Reference);
            Scribe_References.Look(ref this.otherPawnSingle, "otherPawnSingle");
            Scribe_Collections.Look(ref this.otherPawnsNames, "otherPawnsNames", LookMode.Value);
            Scribe_Values.Look(ref this.otherPawnSingleName, "otherPawnSingleName");
            Scribe_References.Look(ref this.priorFaction, "priorFaction");
            
        }
    }

    public enum EventSubType {Death, Capture, Birth, OtherPawnDied, HospitalityGuestVisit, Relationship, Unknown, None, BodyPartLoss}
    public enum LifeEventCategory {Positive, Negative, Neutral, None}
    public enum ModSpecific {Hospitality, VEE, None};
    public enum CentralForPawnKind {All, Colonist, Prisoner, ColonistOrPrisoner, NonColonistNonPrisoner}
}