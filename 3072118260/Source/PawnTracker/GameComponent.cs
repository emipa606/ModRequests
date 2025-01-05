using Verse;
using System.Collections.Generic;
using RimWorld;
using static PawnTrackerMain.PawnTrackerUtility;
using System.Linq;
using UnityEngine;
using PawnTackerMain;
using System.Threading;
using Verse.Noise;
using System;

namespace PawnTrackerMain
{
    public class CHGameComp : GameComponent
    {

        public List<Pawn> StartingColonists;
        public float MaxStartingColonistTick = 0f;
        public List<Pawn> NeedStarterRelations => UpdateNeedStarterRelations();
        public List<Pawn> ManuallyUntracked = new List<Pawn>();
        public List<Pawn> TrackedPawns = new List<Pawn>();
        public int TrackedPawnsCount => TrackedPawns.Count;
        public Dictionary<string, DeadPawnComp> DeadTrackedPawns = new Dictionary<string, DeadPawnComp>();
        public bool expectRansomed = false;
        public int expectRansomedTick = 0;
        public bool expectReward = false;
        public int expectRewardTick = 0;
        public Quest expectRewardQuest;
        public bool TrackedStartingColonists = true; // Default to true; it will set to False if you start a NEW game, but this way it won't do it when loading an existing save
        public int GameStartTick = 0;
        public int requiredStarterRelationsPopulatedCount = 100;
        public int TicksSinceGameStart => GenTicks.TicksAbs - GameStartTick;

        public List<GameEvent> gameEvents = new List<GameEvent>();
        public List<GameConditionEvent> GameConditionEvents => gameEvents.OfType<GameConditionEvent>().ToList();
        public List<GameEvent> NonConditionEvents => gameEvents.Where(ge => ge is not GameConditionEvent).ToList();
        public GameRaidEvent currentRaid;
        public GameRaidEvent mostRecentRaid;
        public GameManhunterEvent currentManhunters;
        public GameManhunterEvent mostRecentManhunters;
        public List<PawnTrackerComp> unsavedNullComps = new List<PawnTrackerComp>();
        //public List<Pawn> DeepPawns = new List<Pawn>();
        public bool UseMod = false;
        public List<Pawn> removePawns = new List<Pawn>();
        public static Font BerlinSansDemibold;
        public Color joinColor = new Color(0.65f, 1f, 0.71f, 0.3f);
        public Color gameEventColor = new Color(1f, 1f, 1f, 0.3f);
        public Color missingColor = new Color(0.89f, 0.78f, 0.48f, 0.3f);
        public Color deathColor = new Color(1f, 0.3f, 0.3f, 0.3f);
        public Color lifeColorPositive =  new Color(0.65f, 1f, 0.71f, 0.07f);
        public Color lifeColorNegative = new Color(1f, 0.3f, 0.3f, 0.07f);
        public Color lifeColorNeutral = new Color(66/255f, 198/255f, 255/255f, 0.1f);
        public CHGameComp(Game game) {}

        public static bool PawnDocumented(Pawn pawn) 
        {
            return GetComp().TrackedPawns.Contains(pawn) && pawn.PawnTrackerComp().documentedEvents != null && pawn.PawnTrackerComp().documentedEvents.Any();
        }

        public List<Pawn> UpdateNeedStarterRelations()
        {
            return PawnTrackerMainSettings.updatePawnRelationshipsTicks == 0 ? new List<Pawn>() : TrackedPawns.Where(p => p != null && p.PawnTrackerComp() != null && p.PawnTrackerComp().NeedPopStarterRelations).ToList();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (!UseMod)
            {
                if (Find.CurrentMap != null)
                    UseMod = true;
                else
                    return;
            }

            if (DeadTrackedPawns == null)
            {
                Log.Error("DeadTrackedPawns is null");
                return;
            }

            if (expectReward == true && GenTicks.TicksAbs - expectRewardTick >= 200)
            {
                expectReward = false;
            }

            if (expectRansomed == true && GenTicks.TicksAbs - expectRansomedTick >= 60000)
            {
                expectRansomed = false;
            }
            
            if (GenTicks.TicksAbs % 2000 == 0)
                LogPTMDev.Warning($"Tracked pawns: {TrackedPawns.Count}. EverWasColonist: {TrackedPawns.Where(p => p != null && p.PawnTrackerComp().everWasColonist).ToList().Count}");
            
            if (DoChecks())
            {
                List<Pawn> needStarterTemp = NeedStarterRelations.ToList();
                if (needStarterTemp.Any())
                {
                    foreach (Pawn p in needStarterTemp)
                    {
                        if (p.PawnTrackerComp().NeedPopStarterRelations)
                        {
                            p.PawnTrackerComp().PopulateStarterRelations();
                        }    
                        else
                        {
                            int prect = NeedStarterRelations.Count();
                        }
                    }
                }
            }

            if (GenTicks.TicksAbs % 10 != 0 && !TrackedStartingColonists && Find.CurrentMap != null && GetAllHumanlike().Any())
            {
                Log.Message("Track starting colonists");
                StartingColonists = new List<Pawn>();
                foreach (Pawn pawn in GetPawns(pawnKind: PKO.Humanlike, colonistStatus: CSO.Colonist, documentedStatus: PDSO.Any))
                {
                    if (pawn == null)
                        continue;
                    if (!pawn.IsColonist || pawn.Faction == null || !pawn.Faction.IsPlayer || Find.WorldPawns.Contains(pawn) || pawn.Map == null || (pawn.Map != null && !pawn.Map.IsPlayerHome))
                    {
                        continue;
                    }    
                    //Log.Message($"Starting colonist: {pawn.Name}");
                    var comp = pawn.PawnTrackerComp();
                    comp.AddEvent(new JoinEvent(pawn, DocumentedEventDefOf.StartingColonist));
                    StartingColonists.Add(pawn);
                    MaxStartingColonistTick = GenTicks.TicksAbs;
                }
                if (StartingColonists != null && StartingColonists.Any())
                {    
                    TrackedStartingColonists = true;
                }
            }
            else
            {
                bool doChecks = DoChecks();
                bool updateStatus = DoUpdatePawnStatus(doChecks);
                bool updateRelations = DoUpdatePawnRelationships(doChecks);
                bool updateEvents = DoUpdateEvents(doChecks);
                bool checkDead = DoCheckDeadPawns(doChecks);
                bool checkArriveDisappear = DoCheckArriveDisappear(doChecks);

                if (DoRaidEnd(doChecks))                
                {
                    currentRaid.EndEvent();
                }    

                if (DoManhunterEnd(doChecks))
                {
                    currentManhunters.EndEvent();
                }

                if (updateStatus || updateRelations || updateEvents)
                {
                    List<Pawn> trackedPawnsTemp = TrackedPawns.ToList();

                    if (trackedPawnsTemp != null)
                    {
                        foreach (Pawn p in trackedPawnsTemp)
                        {
                            if (p != null && p.RaceProps.Humanlike)
                            {
                                p.PawnTrackerComp().UpdateVarious(
                                    pawnStatus: updateStatus, 
                                    dEvents: updateEvents, 
                                    rels: updateRelations
                                );

                                if (checkDead)
                                {
                                    if (
                                        (
                                            p.Dead 
                                            || (
                                                p.PawnTrackerComp().deathEvents != null 
                                                && p.PawnTrackerComp().deathEvents.Any())
                                        ) 
                                        && !p.PawnTrackerComp().ResurrectAfterDeath()
                                    )
                                    {
                                        if (
                                            (
                                                p.PawnTrackerComp().documentedEvents != null
                                                && (
                                                    p.PawnTrackerComp().everWasColonist 
                                                    || p.PawnTrackerComp().everWasPrisoner 
                                                )
                                            )
                                            || p.IsPrisonerOfColony
                                        )
                                        {
                                            Log.Message($"Convert pawn comp to death comp: {p.Name}");
                                            p.PawnTrackerComp().PawnDeath();
                                        }
                                    }     
                                }
                            }
                        }
                    }
                }
            
                if (checkDead)
                {
                    List<Pawn> DeadPawns = TrackedPawns.Where(p => 
                        p != null 
                        && p.PawnTrackerComp() != null 
                        && p.PawnTrackerComp().IsDead
                    ).ToList();

                    foreach (Pawn pawn in DeadPawns)
                    {
                        if (DeadTrackedPawns.ContainsKey(pawn.Name.ToString()))
                        {
                            removePawns.Add(pawn);
                            continue;
                        }

                        if (pawn.PawnTrackerComp().deathEvents != null && !pawn.PawnTrackerComp().deathEvents.Any())
                        {
                            pawn.PawnTrackerComp().AddEvent(new DeathEvent(pawn, DocumentedEventDefOf.UnknownDeath));
                        }
                        pawn.PawnTrackerComp().PawnDeath();

                        if (TrackedPawns.Contains(pawn))
                            removePawns.Add(pawn);
                    }
                }

            if (checkArriveDisappear)
                DocumentUndocumentedChanges();
        }

        if (removePawns.Any())
        {
            TrackedPawns = TrackedPawns.Where(p => !removePawns.Contains(p)).ToList();
        }

    }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            DeadTrackedPawns = new Dictionary<string, DeadPawnComp>();
            TrackedStartingColonists = false;
            GameStartTick = GenTicks.TicksAbs;
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            UpdateTrackedPawns();
            BerlinSansDemibold = ContentFinder<Font>.Get("Contents/BRLNSDB.TTF");
            if (GameStartTick == 0)
                GameStartTick = GenTicks.TicksAbs;

            List<GameEvent> cleanedConditions = new List<GameEvent>();
            foreach (GameEvent ge in gameEvents)
            {
                if (ge == null)
                    continue;
                else if (ge.gameConditionDef != null && ge is not GameConditionEvent)
                {
                    GameConditionEvent newEvent = new GameConditionEvent(ge.gameConditionDef);
                    try
                    {
                        newEvent.tickStart = ge.tickStart;
                        newEvent.yearStart = ge.yearStart;
                        newEvent.dayOfYearStart = ge.dayOfYearStart;
                        newEvent.hourStart = ge.hourStart;
                        newEvent.tickEnd = ge.tickEnd;
                        newEvent.yearEnd = ge.yearEnd;
                        newEvent.dayOfYearEnd = ge.dayOfYearEnd;
                        newEvent.hourEnd = ge.hourEnd;
                        cleanedConditions.Add(newEvent);
                    }
                    catch
                    {
                        Log.Error($"Had to offload a GameConditionEvent that had some unresolvable null data");
                    }
                }
                else
                {
                    try
                    {cleanedConditions.Add(ge);}
                    catch
                    {
                        Log.Error("Had a problem adding an object that is a GameConditionEvent, or does not have a gameConditionDef");
                    }
                }
            }
            gameEvents = cleanedConditions;
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (DeadTrackedPawns == null)
            {
                Log.Message("Make new DeadTrackedPawns in FinalizeInit");
                DeadTrackedPawns = new Dictionary<string, DeadPawnComp>();
            }
            //PawnTracker_SpecialInjector.Inject();
        }

        public bool DoChecks()
        {
            return TrackedStartingColonists && GenTicks.TicksAbs > 0 && Find.CurrentMap != null;
        }

        public bool DoCheckArriveDisappear(bool doDoChecks = false)
        {
            return PawnTrackerMainSettings.checkArriveDisappearTicks != 0 && GenTicks.TicksAbs %  PawnTrackerMainSettings.checkArriveDisappearTicks == 0 && (doDoChecks || DoChecks());
        }

        public bool DoCheckDeadPawns(bool doDoChecks = false)
        {
            return PawnTrackerMainSettings.checkDeadPawnsTicks != 0 && GenTicks.TicksAbs % PawnTrackerMainSettings.checkDeadPawnsTicks == 0 && (doDoChecks || DoChecks());
        }

        public bool DoUpdatePawnStatus(bool doDoChecks = false)
        {
            return PawnTrackerMainSettings.updatePawnStatusTicks != 0 && GenTicks.TicksAbs % PawnTrackerMainSettings.updatePawnStatusTicks == 0 && (doDoChecks || DoChecks());
        }

        public bool DoUpdatePawnRelationships(bool doDoChecks = false)
        {
            return PawnTrackerMainSettings.updatePawnRelationshipsTicks != 0 && GenTicks.TicksAbs % PawnTrackerMainSettings.updatePawnRelationshipsTicks == 0 && (doDoChecks || DoChecks());
        }

        public bool DoUpdateEvents(bool doDoChecks = false)
        {
            return PawnTrackerMainSettings.updateEventsTicks != 0 && GenTicks.TicksAbs % PawnTrackerMainSettings.updateEventsTicks == 0 && (doDoChecks || DoChecks());
        }

        public bool DoRaidEnd(bool doDoChecks = false)
        {
            if (currentRaid != null && (doDoChecks || DoChecks()))
            {
                if (currentRaid.raiders == null)
                    currentRaid.raiders = new();
                //If there are any remaining raiders who are non-null, alive, NOT captured, and are still on this map.... it hasn't ended yet!
                if (currentRaid.raiders.Any(p => p != null && !p.Dead && !p.IsPrisonerOfColony && p.PawnTrackerComp().shouldBeHere))
                    return false;
                //If all raiders are null, dead, captured, or have exited... it's time to do raid end
                return true;
            }
            else
                return false;
        }

        public bool DoManhunterEnd(bool doDoChecks = false)
        {
            if (currentManhunters != null && (doDoChecks || DoChecks()))
            {
                if (currentManhunters.animals == null || !currentManhunters.animals.Any())
                {
                    currentManhunters.FindAnimals();
                    // If we still didn't find any animals, and the event started less than an hour ago, don't end it!
                    if (currentManhunters.animals == null || (!currentManhunters.animals.Any() && GenTicks.TicksAbs - currentManhunters.tickStart <= 2500))
                    {
                        return false;
                    }
                }    
                //If there are any remaining raiders who are non-null, alive, STILL MANHUNTER, and are still on this map.... it hasn't ended yet!
                if (currentManhunters.animals.Any(p => p != null && !p.Dead && p.MentalState?.def == MentalStateDefOf.ManhunterPermanent && (p.Map == null || p.Map.IsPlayerHome)))
                    return false;
                //If all animals are null, dead, not-manhunter, or have left... it's over!
                if (currentManhunters.animals.All(p => p == null || p.Dead || p.MentalState == null || p.MentalState.def != MentalStateDefOf.ManhunterPermanent))
                    return true;
                return true;
            }
            else
                return false;
        }

        public void AddEvent(GameEvent newEvent)
        {
            if (newEvent == null)
                return;

            if (gameEvents == null)
                this.gameEvents = new List<GameEvent>();

            if (gameEvents.Any() && newEvent.def != null && gameEvents.Last().def == newEvent.def && Math.Abs(gameEvents.Last().tickStart - newEvent.tickStart) <= 200)
                return;

            gameEvents.Add(newEvent);
        }

        public void EndEvent(GameConditionDef gameConditionDef)
        {
            if (gameEvents == null)
            {
                LogPTMDev.Warning("No documented events exist");
                gameEvents = new List<GameEvent>();
                return;
            }  
            
            if (GameConditionEvents == null)
                return;

            if (GameConditionEvents.Any() && GameConditionEvents.Last().gameConditionDef == gameConditionDef && GameConditionEvents.Last().tickEnd > -1)
                return;

            try
            {
                GameEvent target = GameConditionEvents.FindLast(ge => ge.gameConditionDef == gameConditionDef && ge.tickEnd == -1);
                target.EndEvent();
            }
            catch
            {
                LogPTMDev.Error("Tried to end a game condition event when there is no un-ended event of that type");
            }
        }

        public void EndEvent(GameEventDef gameEventDef)
        {
            if (gameEvents == null)
            {
                LogPTMDev.Warning("No documented events exist");
                gameEvents = new List<GameEvent>();
                return;
            }  

            if (!NonConditionEvents.Any())
            {
                LogPTMDev.Warning("No Non-condition events");
                //foreach (GameEvent ge in gameEvents)
                //{
                //    Log.Message($"def: {ge.def}... type: {ge.GetType()}... cond: {ge.gameConditionDef}");
                //}
                return;
            }

            if (NonConditionEvents.Last().def == gameEventDef && NonConditionEvents.Last().tickEnd > -1)
                return;

            try
            {
                GameEvent target = NonConditionEvents.Last();
                target.EndEvent();
            }
            catch
            {
                LogPTMDev.Error("Tried to end a game event but... it didnt work");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (gameEvents == null)
                gameEvents = new List<GameEvent>();
            int tempTrackedPawnsCount = TrackedPawnsCount;
            Scribe_Values.Look(ref tempTrackedPawnsCount, "tempTrackedPawnsCount");
            Scribe_Collections.Look(ref TrackedPawns, "TrackedPawns", LookMode.Reference);
            Scribe_Collections.Look(ref ManuallyUntracked, "ManuallyUntracked", LookMode.Reference);
            Scribe_Collections.Look(ref StartingColonists, "StartingColonists", LookMode.Reference);
            Scribe_Values.Look(ref expectRansomed, "expectRansomed");
            Scribe_Values.Look(ref expectRansomedTick, "expectRansomedTick");
            Scribe_Values.Look(ref TrackedStartingColonists, "TrackedStartingColonists");
            Scribe_Values.Look(ref MaxStartingColonistTick, "MaxStartingColonistTick");
            //Scribe_Collections.Look(ref DeepPawns, "DeepPawns", LookMode.Deep);
            List<string> tempDeadKeys = DeadTrackedPawns.Keys.ToList();
            List<DeadPawnComp> tempDeadValues = DeadTrackedPawns.Values.ToList();
            Scribe_Collections.Look(ref DeadTrackedPawns, "DeadTrackedPawns", LookMode.Value, LookMode.Deep, ref tempDeadKeys, ref tempDeadValues);
            Scribe_Values.Look(ref UseMod, "UseMod");
            Scribe_Values.Look(ref GameStartTick, "GameStartTick");
            Scribe_Collections.Look(ref gameEvents, "gameEvents", LookMode.Deep);
            Scribe_Collections.Look(ref unsavedNullComps, "unsavedNullComps", LookMode.Reference);
            Scribe_Deep.Look(ref currentRaid, "currentRaid");
            Scribe_Deep.Look(ref mostRecentRaid, "mostRecentRaid");
            Scribe_Deep.Look(ref mostRecentManhunters, "mostRecentManhunters");
            Scribe_Deep.Look(ref currentManhunters, "currentManhunters");

            if (DeadTrackedPawns == null)
            {
                Log.Message("Make new DeadTrackedPawns after PostLoad");
                DeadTrackedPawns = new Dictionary<string, DeadPawnComp>();
            }
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (gameEvents != null && currentManhunters == null && gameEvents.Any(e => e is GameManhunterEvent && e.tickEnd < 0))
                {
                    currentManhunters = (GameManhunterEvent) gameEvents.Last(e => e is GameManhunterEvent && e.tickEnd < 0);
                }
            }
            
        }
    }
}
