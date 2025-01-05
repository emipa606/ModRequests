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
using RimWorld.Planet;
using System;

// PLANNED:"If pawn dies within x period of time after event end, and injuries came from the thing ... they should be a casualty

namespace PawnTrackerMain {
    public static class GameEventUtility
    {
        public static List<GameConditionDef> RelevantEvents = new List<GameConditionDef> {
            GameConditionDefOf.HeatWave,
            GameConditionDefOf.ColdSnap,
            GameConditionDefOf.VolcanicWinter,
            GameConditionDefOf.ToxicFallout,
            GameConditionDefOf.ToxicSpewer,
            GameConditionDefOf.NoxiousHaze,
        };
    }
    public class GameEvent : IExposable
    {
        public int tickStart = -1;
        public int tickEnd = -1;
        
        public GameEventDef def;
        public GameConditionDef gameConditionDef;
        public int TicksLasted => tickEnd > 0 && tickStart > 0 ? tickEnd - tickStart : -1;
        public float DaysLasted => TicksLasted == -1 ? -1 : (float)Math.Round(TicksLasted / 60000f,1);
        public float HoursLasted => TicksLasted == -1 ? -1 : (float)Math.Round(TicksLasted / 2500f, 1);
        public float TicksSinceEnd => tickEnd > 0 ? GenTicks.TicksAbs - tickEnd : -1;
        private Map map;
        public int yearStart;
        public int dayOfYearStart;
        public int hourStart;
        public int yearEnd = -1;
        public int dayOfYearEnd = -1;
        public int hourEnd = -1;

        public string startDescription;
        public string Description => tickEnd <= 0 || tickStart <= 0 ? BaseDescription() : PostEventDescription();

        //Don't use this, but it used to be here and things break if I remove it
        public GameEvent() {}

        public GameEvent(GameEventDef def)
        {
            this.map = Find.CurrentMap;
            this.def = def;
            this.tickStart = GenTicks.TicksAbs;
            if (this.map == null)
            {
                //Log.Error("Null map for game condition event");
                return;
            }
            this.yearStart = GenLocalDate.Year(map);
            this.dayOfYearStart = GenLocalDate.DayOfYear(map);
            this.hourStart = GenLocalDate.HourInteger(map);
        }

        public GameEvent(GameConditionDef gameConditionDef)
        {
            this.map = Find.CurrentMap;
            this.gameConditionDef = gameConditionDef;
            this.tickStart = GenTicks.TicksAbs;
            if (this.map == null)
            {
                //Log.Error("Null map for game condition event");
                return;
            }
            this.yearStart = GenLocalDate.Year(map);
            this.dayOfYearStart = GenLocalDate.DayOfYear(map);
            this.hourStart = GenLocalDate.HourInteger(map);
        }

        public virtual string PostEventDescription()
        {
            if (!def.canEnd)
                return null;
            if (tickEnd == -1)
                return null;
            if (def == null)
                return null;

            StringBuilder sb = new();
            sb.Append("The colony ");
            if (def.description != null)
                sb.Append(def.description);
            else
                sb.Append($"went through a {def.label}");

            if (DaysLasted >= 1)
                sb.Append($" {def.resolutionString} {Plural(DaysLasted, "day")}");
            else 
                sb.Append($" {def.resolutionString} {Plural(HoursLasted, "hour")}");
            
            return sb.ToString();
        }

        public virtual string BaseDescription()
        {
            return "The colony " + def.description;
        }

        

        public virtual void EndEvent()
        {
            if (!def.canEnd)
                return;
            if (this.map == null)
            {
                this.map = Find.CurrentMap;
                if (this.map == null)
                {
                    Log.Error("Null map when attempting to end game condition event");
                    return;
                }  
            }
            this.tickEnd = GenTicks.TicksAbs;
            this.yearEnd = GenLocalDate.Year(map);
            this.dayOfYearEnd = GenLocalDate.DayOfYear(map);
            this.hourEnd = GenLocalDate.HourInteger(map);
        }

        

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref tickStart, "tickStart");
            Scribe_Values.Look(ref tickEnd, "tickEnd");
            Scribe_Values.Look(ref yearStart, "yearStart");
            Scribe_Values.Look(ref dayOfYearStart, "dayOfYearStart");
            Scribe_Values.Look(ref hourStart, "hourStart");
            Scribe_Values.Look(ref yearEnd, "yearEnd");
            Scribe_Values.Look(ref dayOfYearEnd, "dayOfYearEnd");
            Scribe_Values.Look(ref hourEnd, "hourEnd");
            Scribe_References.Look(ref map, "map");
            
            
            GameConditionDef tempGameConditionDef = null;
            Scribe_Defs.Look(ref tempGameConditionDef, "def");

            Scribe_Defs.Look(ref gameConditionDef, "gameConditionDef");
            Scribe_Defs.Look(ref def, "def_new"); // Use a new tag for the new 'def' field


            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (tempGameConditionDef != null)
                {
                    gameConditionDef = tempGameConditionDef;
                }
            }
        }
    }

    public class GameConditionEvent : GameEvent
    {
        public GameConditionEvent(GameConditionDef gameConditionDef) : base(gameConditionDef)
        {

        }

        public GameConditionEvent() : base(gameConditionDef: null)
        {

        }

        public override string PostEventDescription()
        {
            if (tickEnd == -1)
                return null;

            if (gameConditionDef == null)
                return "<<<Something went wrong in PostEventDescription>>>";
            
            if (DaysLasted >= 1)
                return $"The {SplitPascalCase(gameConditionDef.ToString())} ended after {Plural(DaysLasted, "day")}";
            else 
                return $"The {SplitPascalCase(gameConditionDef.ToString())} ended after {Plural(HoursLasted, word: "hour")}";
        }

        public override string BaseDescription()
        {
            if (gameConditionDef == null)
                return "<<<Something went wrong in BaseDescription>>>";
            return $"A {SplitPascalCase(gameConditionDef.ToString())} began";
        }
    }

    public class GameRaidEvent : GameEvent
    {
        public Faction raidersFaction;
        public List<Pawn> raiders = new();
        public int raidersTotal => raiders.Count();
        public List<Pawn> raidersDead = new();
        public int raidersDeadCount => raidersDead.Count();
        public List<Pawn> raidersCaptured = new();
        public int raidersCapturedCount => raidersCaptured.Count();
        public List<Pawn> raidersFled = new();
        public int raidersFledCount => raidersFled.Count();
        public List<Pawn> colonistsKidnapped = new();
        public int colonistsKidnappedCount => colonistsKidnapped.Count();
        new public string Description => tickEnd == -1 ? BaseDescription() : PostEventDescription();

        public GameRaidEvent(Faction raidersFaction, List<Pawn> raiders) : base(GameEventDefOf.Raiders)
        {
            this.def = GameEventDefOf.Raiders;
            this.raidersFaction = raidersFaction;
            this.raiders = raiders;
        }

        public GameRaidEvent() : base(def: null)
        {

        }

        public void SetResults()
        {
            this.raidersDead = raiders.Where(p => p == null || p.Dead).ToList();
            this.raidersCaptured = raiders.Where(p => p != null && p.IsPrisonerOfColony).ToList();
            this.raidersFled = raiders.Where(p => !raidersDead.Contains(p) && !raidersCaptured.Contains(p)).ToList();
            
            this.colonistsKidnapped = GetComp().TrackedPawns.Where(p => p.PawnTrackerComp().MostRecentEvent() is LeaveEvent leaveEvent && leaveEvent.def == Kidnapped && leaveEvent.kidnappers == raidersFaction && leaveEvent.ticks > tickStart).ToList();
            foreach (Pawn kidnapped in colonistsKidnapped)
                kidnapped.PawnTrackerComp().MostRecentEvent().associatedEvent = this;
        }

        public override void EndEvent()
        {
            base.EndEvent();
            SetResults();
            GetComp().currentRaid = null;
            GetComp().mostRecentRaid = this;
        }

        public override string BaseDescription()
        {
            return $"{Plural(raidersTotal, "member")} of {raidersFaction} started a raid on the colony";
        }

        public override string PostEventDescription()
        {
            StringBuilder sb = new();

            sb.Append($"The raid by {raidersFaction} ended after {Plural(HoursLasted, "hour")}.");
            
            if (raidersTotal > 1 && (raidersDeadCount == raidersTotal || raidersCapturedCount == raidersTotal || raidersFledCount == raidersTotal))
            {
                sb.Append(" In the end, all raiders ");
                if (raidersDeadCount == raidersTotal)
                    sb.Append("were killed.");
                if (raidersCapturedCount == raidersTotal)
                    sb.Append("were captured.");
                if (raidersFledCount == raidersTotal)
                    sb.Append("managed to flee.");
            }
            else if (raidersTotal == 1)
            {
                sb.Append(" In the end, the raider ");
                if (raidersDeadCount > 0)
                    sb.Append("was killed.");
                if (raidersCapturedCount > 0)
                    sb.Append("was captured by the colony.");
                if (raidersFledCount > 0)
                    sb.Append("managed to get away.");
            }
            else
            {
                List<string> raiderStats = new List<string>();

                if (raidersDeadCount > 0)
                    raiderStats.Add($"{Plural(raidersDeadCount, "raider", singleFollowing: "was", pluralFollowing: "were")} killed");

                if (raidersCapturedCount > 0)
                    raiderStats.Add($"{Plural(raidersCapturedCount, "raider", singleFollowing: "was", pluralFollowing: "were")} captured");

                if (raidersFledCount > 0)
                    raiderStats.Add($"{Plural(raidersFledCount, "raider")} fled");

                string formattedStats = FormatListWithCommasAndAnd(raiderStats);
                sb.Append($" In the end, {formattedStats}.");
            }

            return sb.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref raidersFaction, "raidersFaction");
            Scribe_Collections.Look(ref raiders, "raiders", LookMode.Reference);
            Scribe_Collections.Look(ref raidersDead, "raidersDead", LookMode.Reference);
            Scribe_Collections.Look(ref raidersCaptured, "raidersCaptured", LookMode.Reference);
            Scribe_Collections.Look(ref raidersFled, "raidersFled", LookMode.Reference);
            Scribe_Collections.Look(ref colonistsKidnapped, "colonistsKidnapped", LookMode.Reference);
        }
    }

    public class GameManhunterEvent : GameEvent
    {
        public PawnKindDef animalDef;
        public List<Pawn> animals = new();
        public int totalAnimals => animals.Count();
        public List<Pawn> animalsKilled = new();
        public int animalsKilledCount => animalsKilled.Count();
        public List<Pawn> animalsDeparted = new();
        public int animalsDepartedCount => animalsDeparted.Count();
        public List<Pawn> animalsRecovered = new();
        public int animalsRecoveredCount => animalsRecovered.Count();

        public GameManhunterEvent(PawnKindDef animalDef, List<Pawn> animals) : base(GameEventDefOf.Manhunters)
        {
            this.def = GameEventDefOf.Manhunters;
            this.animalDef = animalDef;
            this.animals = animals;
        }

        public GameManhunterEvent() : base(def: null)
        {

        }

        public void SetResults()
        {
            if (animals == null)
                return;
            this.animalsKilled = animals.Where(a => a == null || a.Dead).ToList();
            this.animalsDeparted = animals.Where(a => a != null && !a.Dead && a.Map != null && !a.Map.IsPlayerHome).ToList();
            this.animalsRecovered = animals.Where(a => a != null && (a.MentalState == null || (a.MentalState.def != MentalStateDefOf.Manhunter && a.MentalState.def != MentalStateDefOf.ManhunterPermanent))).ToList();
        }

        public override string BaseDescription()
        {
            if (animalDef == null && animals != null && !animals.Any())
            {
                FindAnimals();
            }

            if ((animals == null || !animals.Any()) && animalDef != null)
                return $"The colony was threatened by some manhunter {animalDef.label}s";
            if (animalDef == null)
                return $"The colony was threatened by a pack of manhunter animals";
            if (totalAnimals == 1)
            {
                return $"The colony was threatened by a manhunter {animalDef.label}";
            }
            else
            {
                return $"The colony was surrounded by {Plural(totalAnimals, "manhunter "+animalDef.label)}";
            }
        }

        public void FindAnimals()
        {
            if (Find.CurrentMap != null && !animals.Any())
            {
                animals = Find.CurrentMap.mapPawns.AllPawnsSpawned.Where(p => p != null && !p.Dead && p.MentalState != null && p.MentalState.def == MentalStateDefOf.ManhunterPermanent).ToList();
                if (animals.Any() && animalDef == null)
                    animalDef = animals[0].kindDef;
            }
        }

        public override string PostEventDescription()
        {
            StringBuilder sb = new();
            
            string lengthString = DaysLasted >= 1 ? Plural(DaysLasted, "day") : Plural(HoursLasted, "hour");
            
            if (totalAnimals == 1)
            {
                if (animalsKilledCount > 0)
                    return $"The colony managed to kill the manhunter {animalDef.label} after {lengthString}";
                else
                    return $"The colony decided to wait out the manhunter {animalDef.label} and were rid of their attacker after {lengthString}";
            }
            else
            {
                if (animalsKilledCount == totalAnimals)
                {
                    return$"The colony managed to kill all of the manhunter {animalDef.label}s after {lengthString}!";
                }    

                if (animalsDepartedCount + animalsRecoveredCount == totalAnimals)
                {
                    return$"The colony decided to wait out the manunter {animalDef.label}s and were rid of their attackers after {lengthString}";
                }

                if (animalsDepartedCount + animalsRecoveredCount > 0 && animalsKilledCount > 0)
                {
                    return $"The colony killed {Plural(animalsKilledCount, $"of the manhunter {animalDef.label}")} and waited out the rest. They were finally rid of their attackers are {lengthString}";
                }
                else
                {
                    LogPTMDev.Error("animalsKilled != total, animalsLeft != total, and one or the other did not happen at all... this doesn't make sense!");
                    return null;
                }    
            }
            
        }

        public override void EndEvent()
        {
            base.EndEvent();
            SetResults();
            GetComp().currentManhunters = null;
            GetComp().mostRecentManhunters = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Defs.Look(ref animalDef, "animalDef");
            Scribe_Collections.Look(ref animals, "animals", LookMode.Reference);
            Scribe_Collections.Look(ref animalsKilled, "animalsKilled", LookMode.Reference);
            Scribe_Collections.Look(ref animalsDeparted, "animalsDeparted", LookMode.Reference);
        }
    }

    public class GameMiscEvent : GameEvent
    {
        public HediffDef illness;

        public int pawns => colonists + prisoners;
        public int colonists;
        public int prisoners;

        public GameMiscEvent() : base(def: null)
        {

        }

        public GameMiscEvent(GameEventDef def) : base(def)
        {

        }

        public GameMiscEvent(GameEventDef def, HediffDef illness, int colonists = 0, int prisoners = 0) : base(def)
        {
            this.colonists = colonists;
            this.prisoners = prisoners;
            this.illness = illness;
        }

        public override string BaseDescription()
        {
            if (def == GameEventDefOf.Illness)
            {
                if (colonists > 0 && prisoners > 0)
                    return $"{Plural(this.colonists, "colonist", singleFollowing: "was", pluralFollowing: "were")} and {Plural(prisoners, "prisoner", singleFollowing: "was", pluralFollowing: "were")} infected with {this.illness.label}";
                else if (colonists > 0)
                    return $"{Plural(this.colonists, "colonist", singleFollowing: "was", pluralFollowing: "were")} infected with {this.illness.label}";
                else if (prisoners > 0)
                    return $"{Plural(this.prisoners, "prisoner", singleFollowing: "was", pluralFollowing: "were")} infected with {this.illness.label}";
                else 
                    return "Not an illness, something went wrong here";
            }
           
            return base.BaseDescription();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref colonists, "colonists");
            Scribe_Values.Look(ref prisoners, "prisoners");
            Scribe_Defs.Look(ref illness, "illness");
        }
    }

}