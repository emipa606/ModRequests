using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Hospitality.Utilities;

internal static class GenericUtility
{
    internal const int NoBasesLeft = -1;

    private static readonly Dictionary<Faction, int> travelDaysCache = new();

    public static bool IsMeal(this Thing thing)
    {
        return thing.def.ingestible?.IsMeal == true;
    }

    public static Pawn GetAnyRelatedWorldPawn(Func<Pawn, bool> selector, int minImportance)
    {
        // Get all important relations from all colonists
        var importantRelations = from colonist in PawnsFinder.AllMaps_FreeColonistsSpawned.Where(c => !c.Dead)
            from otherPawn in colonist.relations.RelatedPawns
            where !otherPawn.Dead && !otherPawn.Spawned && otherPawn.Faction != colonist.Faction && selector(otherPawn) && otherPawn.IsWorldPawn()
            select new { otherPawn, colonist, relationDef = colonist.GetMostImportantRelation(otherPawn) };

        var dictRelations = new Dictionary<Pawn, float>();

        // Calculate the total importance to colony
        foreach (var relation in importantRelations.Where(r => r.relationDef.importance >= minImportance))
        {
            if (!dictRelations.ContainsKey(relation.otherPawn))
            {
                dictRelations.Add(relation.otherPawn, relation.relationDef.importance);
            }
            else dictRelations[relation.otherPawn] += relation.relationDef.importance;
        }
        //Log.Message(dictRelations.Count + " distinct pawns:");
        //foreach (var relation in dictRelations)
        //{
        //    Log.Message("- " + relation.Key.Name + ": " + relation.Value +(relation.Key.Faction.leader == relation.Key?" (leader)":""));
        //}

        if (dictRelations.Count > 0)
        {
            var choice = dictRelations.RandomElementByWeight(pair => pair.Value);
            //Log.Message(choice.Key.Name + " with " + choice.Value + " points was chosen.");
            return choice.Key;
        }

        if (minImportance <= 0)
        {
            Log.Message("Couldn't find any pawn that is related to colony.");
            return null;
        }

        return GetAnyRelatedWorldPawn(selector, minImportance - 50);
    }

    public static float GetTravelDays(Faction faction, Map map)
    {
        if (travelDaysCache.TryGetValue(faction, out var minTicks)) return minTicks / (float)GenDate.TicksPerDay;

        minTicks = int.MaxValue;
        foreach (var settlement in Find.WorldObjects.SettlementBases)
        {
            if (settlement.Faction != faction) continue;
            const int ticksPerTile = 3300;
            var distance = Find.WorldGrid.TraversalDistanceBetween(map.Tile, settlement.Tile);
            var travelTicks = ticksPerTile * distance;
            if (travelTicks <= 0) continue;
            if (travelTicks < minTicks) minTicks = travelTicks;
        }

        if (minTicks == int.MaxValue) return NoBasesLeft;

        travelDaysCache.Add(faction, minTicks);

        //Log.Message("It takes the " + faction.def.pawnsPlural + " " + days + " days to travel to the player.");
        return minTicks / (float)GenDate.TicksPerDay;
    }

    internal static void CheckTooManyIncidentsAtOnce(IncidentQueue incidentQueue)
    {
        var maxIncidents = ModSettings_Hospitality.maxIncidentsPer3Days + 1;
        const int rangeOfDays = 3;

        if (incidentQueue.Count < maxIncidents) return;
        var index = 0;
        foreach (QueuedIncident incident in incidentQueue)
        {
            index++;
            if (index == maxIncidents)
            {
                if (incident.FireTick - GenTicks.TicksGame < GenDate.TicksPerDay * rangeOfDays)
                {
                    Log.Message($"More than {maxIncidents - 1} visitor groups planned within the next {rangeOfDays} days. Cancelling half.");
                    RemoveSomeIncidents(incidentQueue);
                    return;
                }
            }
        }
    }

    private static void RemoveSomeIncidents(IncidentQueue incidentQueue)
    {
        const int rangeOfDays = 3;
        var backupQueue = new IncidentQueue();

        var skip = true;
        var amount = 0;
        var newAmount = 0;

        // Copy and thin 
        foreach (QueuedIncident incident in incidentQueue)
        {
            // After range of days copy everything
            if (incident.FireTick - GenTicks.TicksGame >= GenDate.TicksPerDay * rangeOfDays) backupQueue.Add(incident);
            else
            {
                // Before, copy every second incident
                amount++;
                if (!skip)
                {
                    backupQueue.Add(incident);
                    newAmount++;
                }

                skip = !skip;
            }
        }

        // Add them back
        incidentQueue.Clear();
        foreach (QueuedIncident incident in backupQueue)
        {
            incidentQueue.Add(incident);
        }

        Log.Message($"Reduced {amount} visits to {newAmount}, by cancelling every 2nd within the next {rangeOfDays} days.");
    }

    internal static void FillIncidentQueue(Map map)
    {
        // Add some visits
        var days = Rand.Range(10f, 16f); // initial delay
        var amount = Rand.Range(1, 4);
        foreach (var faction in Find.FactionManager.AllFactionsVisible.Where(f => !f.IsPlayer && !f.defeated && !f.HostileTo(Faction.OfPlayer)).OrderBy(f => GetTravelDays(f, map)))
        {
            amount--;
            Log.Message(faction.GetCallLabel() + " are coming after " + days + " days.");
            PlanNewVisit(map, days, faction);
            days += Rand.Range(15f, 25f);
            if (amount <= 0) break;
        }
    }

    public static void PlanNewVisit(IIncidentTarget map, float afterDays, Faction faction = null)
    {
        if (map is not Map realMap) return;
        var incidentParms = StorytellerUtility.DefaultParmsNow(DefDatabase<IncidentCategoryDef>.GetNamed("FactionArrival"), realMap);

        if (faction == null) return;
        if (faction.def.pawnGroupMakers == null || !faction.def.pawnGroupMakers.Any(g => g.kindDef == PawnGroupKindDefOf.Peaceful))
        {
            Log.Warning($"Faction {faction.GetCallLabel()} can't visit, because it doesn't have any peaceful group makers.");
            return;
        }

        incidentParms.faction = faction;
        var incident = new FiringIncident(IncidentDefOf.VisitorGroup, null, incidentParms);
        realMap.GetMapComponent().QueueIncident(incident, afterDays);
    }

    public static void TryCreateVisit(Map map, float days, Faction faction, float travelFactor = 1)
    {
        if (faction.IsPlayer)
        {
            Log.Warning("Trying to create visit for player's faction.");
            return;
        }

        var travelDays = GetTravelDays(faction, map);

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (travelDays == NoBasesLeft) return;

        travelDays = days + travelDays * travelFactor;
        PlanNewVisit(map, travelDays, faction);
    }

    public static void DoAreaRestriction(Rect rect, Area area, Action<Area> setArea, Func<Area, string> getLabel, Map map)
    {
        var newArea = area;
        GuestUtility.DoAllowedAreaSelectors(rect, getLabel, ref newArea, map);
        Text.Anchor = TextAnchor.UpperLeft;

        if (newArea != area) setArea(newArea);
    }

    public static string GetShoppingLabel(Area area)
    {
        if (area != null) return area.Label;
        return "AreaNoShopping".Translate();
    }

    public static bool OnlyOneBed(this Room room)
    {
        return room.ContainedBeds.Count() == 1;
    }

    [DebugAction("General", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void RemoveBrokenGroups()
    {
        static void RemoveLord(Lord lord)
        {
            Log.Message($"Removing lord/group from {(lord.faction == null ? "unset faction" : lord.faction.Name)} with id {lord.loadID}.");
            try
            {
                Find.CurrentMap.lordManager.RemoveLord(lord);
            }
            catch (Exception e)
            {
                Find.CurrentMap.lordManager.lords.Remove(lord);
                Log.Error($"{e.Message}\n{e.StackTrace}");
            }

            {
                Log.Message(Find.CurrentMap.lordManager.lords.Contains(lord) ? $"Failed to remove lord with id {lord?.loadID}." : "Successfully removed lord.");
            }
        }

        foreach (var lord in Find.CurrentMap.lordManager.lords.ToArray())
        {
            if (lord?.faction == null || lord.LordJob == null || lord.ownedPawns == null || lord.ownedBuildings == null)
            {
                RemoveLord(lord);
            }
            else
            {
                var count = 0;
                count += lord.ownedPawns.RemoveAll(p => p == null || !p.SpawnedOrAnyParentSpawned);
                count += lord.ownedBuildings.RemoveAll(b => b == null || !b.SpawnedOrAnyParentSpawned);
                if (count > 0) Log.Message($"Removed {count} invalid pawns or buildings for lord/group.");
            }
        }
    }

    internal static int CombinedHash(object first, object second)
    {
        var hash = 17;
        hash = hash * 31 + first.GetHashCode();
        hash = hash * 31 + second.GetHashCode();
        return hash;
    }

    public static void TryCreateBubble(Pawn pawn1, Pawn pawn2, Texture2D symbol)
    {
        if (pawn1.interactions.InteractedTooRecentlyToInteract()) return;
        MoteMaker.MakeInteractionBubble(pawn1, pawn2, ThingDefOf.Mote_Speech, symbol);
    }
}