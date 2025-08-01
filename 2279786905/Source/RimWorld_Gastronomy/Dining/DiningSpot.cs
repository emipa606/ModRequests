using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Gastronomy.Dining;

public enum SpotState
{
    Blocked = -1,
    Clear = 0, // 0-5
    Ready = 6,
    Messy1 = 7,
    Messy2 = 8
}

public class DiningSpot : Building_NutrientPasteDispenser
{
    public const string jobReportString = "DiningJobReportString";
    public static SpotState RandomMessyState => (SpotState)Rand.RangeInclusive((int)SpotState.Messy1, (int)SpotState.Messy2);
    private int decoVariation;

    private List<SpotState> spotStates = Enumerable.Repeat(SpotState.Clear, 4).ToList();

    public override ThingDef DispensableDef => throw new NotImplementedException();
    public bool MayDineStanding { get; } = false;

    public int DecoVariation
    {
        get => decoVariation;
        set
        {
            decoVariation = value;
            UpdateMesh();
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref spotStates, "spotStates");
        Scribe_Values.Look(ref decoVariation, "decoVariation");
    }

    public override void PostMapInit()
    {
        base.PostMapInit();
        UpdateMesh();
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        if (!respawningAfterLoad)
        {
            DiningUtility.OnDiningSpotCreated(this);
        }
    }

    public int GetMaxReservations()
    {
        return GetReservationSpots().Count(s => s >= SpotState.Clear);
    }

    public int GetMaxSeats()
    {
        return GetReservationSpots().Count(s => s != SpotState.Blocked);
    }

    /// <summary>
    ///     [0] = up, [1] = right, [2] = down, [3] = left
    ///     chairs gets filled with unblocked chairs.
    /// </summary>
    [NotNull]
    public SpotState[] GetReservationSpots(List<Building> chairs = null)
    {
        spotStates ??= [SpotState.Clear, SpotState.Clear, SpotState.Clear, SpotState.Clear];
        chairs?.Clear();
        var position = Position;
        var map = Map;
        var result = new SpotState[4];
        for (var i = 0; i < 4; i++)
        {
            result[i] = SpotState.Blocked;
            var intVec = position + new Rot4(i).FacingCell;
            if (MayDineStanding && intVec.Standable(map))
            {
                result[i] = spotStates[i];
            }
            else
            {
                if (!Spawned || Destroyed)
                {
                    DiningUtility.OnDiningSpotRemoved(MapHeld);
                    return [];
                }

                if (map == null) Log.WarningOnce($"Trying to get chair at {intVec} on null map", 4643474);
                var chair = intVec.GetEdifice(map);
                if (chair == null) continue;

                var facingCorrectly = !chair.def.rotatable || intVec + chair.Rotation.FacingCell == position;

                if (chair.def.building.isSittable && facingCorrectly)
                {
                    result[i] = spotStates[i];
                    chairs?.Add(chair);
                }
            }

            //Log.Message($"Checked {intVec}: {result[i]} chair? {intVec.GetEdifice(map)?.Label} sittable? {intVec.GetEdifice(map)?.def.building.isSittable} facing? {intVec + intVec.GetEdifice(map)?.Rotation.FacingCell}");
        }

        return result;
    }

    [UsedImplicitly]
    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
    }

    [UsedImplicitly]
    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        var map = MapHeld;
        base.Destroy(mode);
        DiningUtility.OnDiningSpotRemoved(map);
    }

    private void UpdateMesh()
    {
        if (Spawned)
        {
            Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things.mask, false, false);
        }
    }

    public void SetSpotReady(IntVec3 chairPos)
    {
        SetSpotState(chairPos, SpotState.Ready);
    }

    public bool IsSpotReady(IntVec3 chairPos)
    {
        return GetSpotState(chairPos) == SpotState.Ready;
    }

    public void SetSpotMessy(IntVec3 chairPos)
    {
        SetSpotState(chairPos, RandomMessyState);
    }

    public bool IsSpotMessy(IntVec3 chairPos)
    {
        return GetSpotState(chairPos) > SpotState.Ready;
    }

    private void SetSpotState(IntVec3 chairPos, SpotState state)
    {
        var position = Position;
        for (var i = 0; i < 4; i++)
        {
            var intVec = position + new Rot4(i).FacingCell;
            if (intVec == chairPos)
            {
                //Log.Message($"Changed spot at {position} towards {chairPos} from state {spotStates[i]} to {state}.");
                spotStates[i] = state;
                UpdateMesh();
                return;
            }
        }

        Log.Warning($"Tried to set dining spot {position} with an invalid chair position {chairPos}.");
    }

    private SpotState GetSpotState(IntVec3 chairPos)
    {
        if (!Spawned || Destroyed) return SpotState.Blocked;
        var position = Position;
        for (var i = 0; i < 4; i++)
        {
            var intVec = position + new Rot4(i).FacingCell;
            if (intVec == chairPos)
            {
                //Log.Message($"Checked spot state of {position} from {chairPos}: {spotStates[i]} ({i})");
                return spotStates[i];
            }
        }

        Log.Warning($"Tried to get state of dining spot {position} with an invalid spot position {chairPos}. This message can probably be ignored.");
        return SpotState.Blocked;
    }

    public override Thing TryDispenseFood()
    {
        return null;
    }

    public IEnumerable<LocalTargetInfo> GetUnmadeSpotCells()
    {
        var spots = GetReservationSpots();
        for (var i = 0; i < 4; i++)
        {
            if (spots[i] == SpotState.Clear || spots[i] > SpotState.Ready)
            {
                yield return Position + new Rot4(i).FacingCell;
            }
        }
    }

    public bool IsValidDineCell(IntVec3 chairPos)
    {
        if (!Spawned || Destroyed) return false;
        var position = Position;
        for (var i = 0; i < 4; i++)
        {
            var intVec = position + new Rot4(i).FacingCell;
            if (intVec == chairPos) return true;
        }

        return false;
    }

    public bool IsSociallyProper(Pawn pawn)
    {
        var table = Position.GetEdifice(Map);
        return table?.IsSociallyProper(pawn) == true;
    }

    #region NutrientPasteDispenser overrides

    public override bool HasEnoughFeedstockInHoppers()
    {
        return true;
    }

    public override Building AdjacentReachableHopper(Pawn pawn)
    {
        return null;
    }

    #endregion
}