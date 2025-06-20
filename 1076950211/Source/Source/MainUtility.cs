using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Therapy;

internal static class MainUtility
{
    private const float ChairRadius = 3.9f;
    private static readonly int numRadiusCells = GenRadial.NumCellsInRadius(ChairRadius);

    private static readonly List<IntVec3> radialPatternMiddleOutward =
        (from c in GenRadial.RadialPattern.Take(numRadiusCells)
            orderby Mathf.Abs((c - IntVec3.Zero).LengthHorizontal - ChairRadius / 2)
            select c).ToList();

    public static readonly RoomRoleDef therapyRoomRoleDef = DefDatabase<RoomRoleDef>.GetNamed("TherapyOffice");
    public static readonly JobDef therapistJobDef = DefDatabase<JobDef>.GetNamed("PerformTherapy");
    public static readonly JobDef patientJobDef = DefDatabase<JobDef>.GetNamed("ReceiveTherapy");
    public static readonly ThingDef couchThingDef = DefDatabase<ThingDef>.GetNamed("TherapyCouch");
    public static readonly WorkTypeDef therapyWorkTypeDef = DefDatabase<WorkTypeDef>.GetNamed("Therapist");

    private static readonly List<IntVec3> cacheChairCells = new();
    private static readonly Dictionary<int, TherapyNeed> therapyNeedCache = new();

    public static List<IntVec3> ChairCellsAround(IntVec3 center, Map map, out bool hasChair)
    {
        hasChair = false;
        cacheChairCells.Clear();
        if (!center.InBounds(map)) return cacheChairCells;
        var region = center.GetRegion(map);
        if (region == null) return cacheChairCells;

        foreach (var pos in radialPatternMiddleOutward)
        {
            var c = pos + center;
            if (!c.InBounds(map)) continue;
            if (!c.Walkable(map)) continue;
            if (!GenSight.LineOfSight(center, c, map, true)) continue;

            var edifice = c.GetEdifice(map);
            if (edifice != null && edifice.def.building.isSittable) hasChair = true;
            cacheChairCells.Add(c);
        }

        return cacheChairCells;
    }


    public static Building_Couch CurrentCouch(this Pawn p)
    {
        if (!p.Spawned || p.CurJob == null || p.jobs.posture != PawnPosture.LayingOnGroundFaceUp) return null;
        Building_Couch couch = null;
        var thingList = p.Position.GetThingList(p.Map);
        foreach (var thing in thingList)
        {
            couch = thing as Building_Couch;
            if (couch != null) break;
        }

        if (couch == null) return null;
        if (couch.GetCurOccupant() == p) return couch;
        return null;
    }

    public static bool TryFindChairNearCouch(this Building_Couch couch, out Thing chair)
    {
        foreach (var v in radialPatternMiddleOutward)
        {
            var c = couch.Position + v;
            if (!c.InBounds(couch.Map)) continue;
            if (!c.Walkable(couch.Map)) continue;
            var edifice = c.GetEdifice(couch.Map);
            if (edifice != null && edifice.def.building.isSittable
                                && GenSight.LineOfSight(couch.Position, edifice.Position, couch.Map, true))
            {
                chair = edifice;
                return true;
            }
        }

        chair = null;
        return false;
    }

    public static void FailOnTimeTable(this Toil toil)
    {
        toil.FailOn(() =>
            toil.actor.TimeTableIs(TimeAssignmentDefOf.Sleep) || toil.actor.TimeTableIs(TimeAssignmentDefOf.Meditate));
    }

    public static bool TimeTableIs(this Pawn pawn, TimeAssignmentDef def)
    {
        return pawn?.GetTimeAssignment() == def;
    }

    public static void FailOnCouchNoLongerUsable(this Toil toil, TargetIndex couchIndex)
    {
        toil.FailOn(() => CouchNoLongerUsableInternal(toil, couchIndex));
    }

    private static bool CouchNoLongerUsableInternal(Toil toil, TargetIndex couchIndex)
    {
        var target = toil.actor.CurJob.GetTarget(couchIndex);
        if (!target.HasThing || !target.IsValid) return true;
        var couch = (Building_Couch)target.Thing;
        return couch.CouchNoLongerUsable(toil.actor);
    }

    public static bool CouchNoLongerUsable(this Building_Couch couch, Pawn pawn)
    {
        if (couch.IsBurning())
        {
            Log.Message($"{pawn.LabelCap}'s couch is burning.");
            return true;
        }

        if (HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn))
        {
            Log.Message($"{pawn.LabelCap} needs medical rest urgently.");
            return true;
        }

        if ((pawn.IsColonist || pawn.IsPrisonerOfColony) && !(pawn.CurJob is { ignoreForbidden: true }) &&
            !pawn.Downed && couch.IsForbidden(pawn))
        {
            Log.Message($"{pawn.LabelCap}'s couch is forbidden.");
            return true;
        }

        return false;
    }

    public static float GetTherapyNeed(this Pawn pawn)
    {
        const int cacheForSeconds = 30;

        if (pawn.needs.mood == null) return 0;

        // In cache?
        if (therapyNeedCache.TryGetValue(pawn.thingIDNumber, out var need))
        {
            // Check if expired
            if (GenTicks.TicksGame <= need.lastUpdated + GenTicks.SecondsToTicks(cacheForSeconds)) return need.amount;
            therapyNeedCache.Remove(pawn.thingIDNumber);
        }

        // Cache new
        var memories = pawn.needs.mood.thoughts.memories.Memories.Where(m =>
            m.VisibleInNeedsTab && m.MoodOffset() < 0 && m.def.durationDays > 0);
        var sum = memories.Sum(m => -m.MoodOffset());

        var newNeed = new TherapyNeed { amount = sum, lastUpdated = GenTicks.TicksGame };
        therapyNeedCache.Add(pawn.thingIDNumber, newNeed);

        return newNeed.amount;
    }

    public static Building_Couch FindRandomCouchWithChair(Pawn pawn, bool checkIfCanReserve, Danger maxDanger)
    {
        var buildingCouches = pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Couch>();
        var couches = buildingCouches.Where(x => x.CanUse(pawn, checkIfCanReserve, maxDanger));
        if (!couches.TryRandomElementByWeight(couch => couch.VisitChanceScore(pawn), out var result))
            return couches.FirstOrDefault();
        return result;
    }

    private static float VisitChanceScore(this Building_Couch couch, Pawn pawn)
    {
        var room = couch.GetRoom();
        if (room == null) return 0;

        //float num = GenMath.LerpDouble(-100f, 100f, 0.05f, 2f, pawn.relations.OpinionOf(therapist));
        var lengthHorizontal = (pawn.Position - couch.Position).LengthHorizontal;
        var scoreDistance = Mathf.Clamp(GenMath.LerpDouble(0f, 150f, 1f, 0.2f, lengthHorizontal), 0.2f, 1f);
        var scoreRoom = Mathf.Max(0.1f, room.GetStat(RoomStatDefOf.Beauty));
        float explicitScore = couch.ExplicitlyAssignedTo(pawn) ? 2 : 1;
        return scoreDistance * scoreRoom * explicitScore;
    }

    public static bool CanUse(this Building_Couch couch, Pawn pawn, bool checkIfCanReserve, Danger maxDanger)
    {
        if (!couch.MayUseThis(pawn)) return false;

        var canReserve = !checkIfCanReserve || pawn.CanReserve(couch);
        var canReach = pawn.CanReach(couch, PathEndMode.InteractionCell, maxDanger);
        var canReserveAndReach = canReach && canReserve && !couch.IsForbidden(pawn);
        if (!canReserveAndReach) return false;

        return couch.HasChair;
    }

    private struct TherapyNeed
    {
        public int lastUpdated;
        public float amount;
    }
}