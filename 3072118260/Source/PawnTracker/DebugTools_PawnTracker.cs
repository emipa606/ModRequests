using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RimWorld;
using Verse;

namespace PawnTrackerMain
{
    public static class DebugTools_PawnTracker
    {
        [DebugAction("Pawn_Tracker", "Add arrive event...", false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> AddArriveEventSpecific()
        {
            List<DebugActionNode> debugActionNodeList = new List<DebugActionNode>();
        
            foreach (ArriveEventDef eventDef in PawnTrackerUtility.ArriveEvents)
            {
                
                debugActionNodeList.Add(new DebugActionNode(eventDef.defName, DebugActionType.ToolMapForPawns)
                {
                    pawnAction = (Action<Pawn>) (p =>
                    {
                        ArriveEvent eventInstance = new ArriveEvent(p, eventDef); 
                        p.PawnTrackerComp()?.AddEvent(eventInstance, forceAdd: true);
                        DebugActionsUtility.DustPuffFrom((Thing) p);
                    }),
                    labelGetter = (Func<string>) (() =>
                    {
                        string defName = eventDef.defName;
                        return defName;
                    })
                });
            }
            return debugActionNodeList;
        }

        [DebugAction("Pawn_Tracker", "Add join event...", false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> AddJoinEventSpecific()
        {
            List<DebugActionNode> debugActionNodeList = new List<DebugActionNode>();
        
            foreach (JoinEventDef eventDef in PawnTrackerUtility.JoinEvents)
            {
                
                debugActionNodeList.Add(new DebugActionNode(eventDef.defName, DebugActionType.ToolMapForPawns)
                {
                    pawnAction = (Action<Pawn>) (p =>
                    {
                        JoinEvent eventInstance =  new JoinEvent(p, eventDef); 
                        p.PawnTrackerComp()?.AddEvent(eventInstance, forceAdd: true);
                        DebugActionsUtility.DustPuffFrom((Thing) p);
                    }),
                    labelGetter = (Func<string>) (() =>
                    {
                        string defName = eventDef.defName;
                        return defName;
                    })
                });
            }
            return debugActionNodeList;
        }
        
        [DebugAction("Pawn_Tracker", "Add leave event...", false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> AddLeaveEventSpecific()
        {
            List<DebugActionNode> debugActionNodeList = new List<DebugActionNode>();
        
            foreach (LeaveEventDef eventDef in PawnTrackerUtility.LeaveEvents)
            {
                
                debugActionNodeList.Add(new DebugActionNode(eventDef.defName, DebugActionType.ToolMapForPawns)
                {
                    pawnAction = (Action<Pawn>) (p =>
                    {
                        LeaveEvent eventInstance =  new LeaveEvent(p, eventDef); 
                        p.PawnTrackerComp()?.AddEvent(eventInstance, forceAdd: true);
                        DebugActionsUtility.DustPuffFrom((Thing) p);
                    }),
                    labelGetter = (Func<string>) (() =>
                    {
                        string defName = eventDef.defName;
                        return defName;
                    })
                });
            }
            return debugActionNodeList;
        }

        [DebugAction("Pawn_Tracker", "Add death event...", false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> AddDeathEventSpecific()
        {
            List<DebugActionNode> debugActionNodeList = new List<DebugActionNode>();
        
            foreach (DeathEventDef eventDef in PawnTrackerUtility.DeathEvents)
            {
                
                debugActionNodeList.Add(new DebugActionNode(eventDef.defName, DebugActionType.ToolMapForPawns)
                {
                    pawnAction = (Action<Pawn>) (p =>
                    {
                        DeathEvent eventInstance =  new DeathEvent(p, eventDef); 
                        p.PawnTrackerComp()?.AddEvent(eventInstance, forceAdd: true);
                        DebugActionsUtility.DustPuffFrom((Thing) p);
                    }),
                    labelGetter = (Func<string>) (() =>
                    {
                        string defName = eventDef.defName;
                        return defName;
                    })
                });
            }
            return debugActionNodeList;
        }

        private static Pawn firstPawnSelected = null;
        [DebugAction("Pawn_Tracker", "Add life event (relationship)...", false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> AddLifeEventRelationshipSpecific()
        {
            List<DebugActionNode> debugActionNodeList = new List<DebugActionNode>();
        
            foreach (LifeEventDef eventDef in PawnTrackerUtility.LifeEvents.Where(le => le.eventSubType == EventSubType.Relationship).ToList())
            {
                
                debugActionNodeList.Add(new DebugActionNode(eventDef.defName, DebugActionType.ToolMapForPawns)
                {
                    
                    pawnAction = (Action<Pawn>) (p =>
                    {
                        if (firstPawnSelected == null)
                        {
                            // First pawn selected, now waiting for the second pawn
                            firstPawnSelected = p;
                        }
                        else
                        {
                            // Second pawn selected, perform the action
                            LifeEvent eventInstance = new LifeEvent(firstPawnSelected, eventDef, otherPawnSingle: p); 
                            firstPawnSelected.PawnTrackerComp()?.AddEvent(eventInstance,  forceAdd: true);
                            DebugActionsUtility.DustPuffFrom((Thing)firstPawnSelected);

                            // Reset the first pawn selection for the next use
                            firstPawnSelected = null;
                        }
                        
                        
                        DebugActionsUtility.DustPuffFrom((Thing) p);
                    }),
                    labelGetter = (Func<string>) (() =>
                    {
                        string defName = eventDef.defName;
                        return defName;
                    })
                });
            }
            return debugActionNodeList;
        }

        [DebugAction("Pawn_Tracker", "Add life event (misc)...", false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> AddLifeEventMiscSpecific()
        {
            List<DebugActionNode> debugActionNodeList = new List<DebugActionNode>();
        
            foreach (LifeEventDef eventDef in PawnTrackerUtility.LifeEvents.Where(le => le.eventSubType != EventSubType.Relationship).ToList())
            {
                debugActionNodeList.Add(new DebugActionNode(eventDef.defName, DebugActionType.ToolMapForPawns)
                {
                    pawnAction = (Action<Pawn>) (p =>
                    {
                        LifeEvent eventInstance;
                        if (eventDef == DocumentedEventDefOf.LostBodyPart || eventDef == DocumentedEventDefOf.BodyPartRemoved)
                        {
                            eventInstance = new LifeEvent(p, eventDef, bodyPart: p.health.hediffSet.GetNotMissingParts().First()); 
                        }
                        else if (eventDef == DocumentedEventDefOf.NewBodyPart)
                        {
                            eventInstance = new LifeEvent(p, eventDef, newBodyPart: HediffDefOf.BionicArm, removedBodyPart: new BodyPartRecord()); 
                        }
                        else
                        {
                            eventInstance = new LifeEvent(p, eventDef); 
                        }
                        p.PawnTrackerComp()?.AddEvent(eventInstance, forceAdd: true);
                        DebugActionsUtility.DustPuffFrom((Thing) p);
                    }),
                    labelGetter = (Func<string>) (() =>
                    {
                        string defName = eventDef.defName;
                        return defName;
                    })
                });
            }
            return debugActionNodeList;
        }


        [DebugAction("Pawn_Tracker", "Toggle shouldBeHere", false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ToggleShouldBeHere(Pawn p)
        {
            if (p == null || p.PawnTrackerComp() == null)
                return;
            if (!p.RaceProps.Humanlike)
                return;
            p.PawnTrackerComp().shouldBeHere = !p.PawnTrackerComp().shouldBeHere;
            DebugActionsUtility.DustPuffFrom((Thing) p);
        }

        [DebugAction("Pawn_Tracker", "Remove event...", false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RemoveEvent(Pawn pawn)
        {
            if (pawn == null || pawn.PawnTrackerComp() == null || !pawn.RaceProps.Humanlike)
                return;

            List<DebugMenuOption> options = new List<DebugMenuOption>();
            foreach (DocumentedEvent docEvent in new List<DocumentedEvent>(pawn.PawnTrackerComp().documentedEvents))
            {
                options.Add(new DebugMenuOption($"{docEvent.def.defName} ({docEvent.ticks})", DebugMenuOptionMode.Action, () =>
                {
                    pawn.PawnTrackerComp().documentedEvents.Remove(docEvent);
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
            DebugActionsUtility.DustPuffFrom(pawn);
        }


    }
}