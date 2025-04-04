﻿using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace WhatTheHack.Storage;

/**
 * Class initially intended for attaching custom data to pawns. Right now it's misused a bit for other data aswell. Refactoring it to be more general won't be compatible with existing saves, and creating separate classes for each type of data will result in too much boilerplate to my taste.
 */
public class ExtendedDataStorage : WorldComponent
{
    private List<ExtendedPawnData> _extendedPawnDataWorkingList;

    private List<int> _idWorkingList;
    private List<int> _idWorkingListMap;

    private Dictionary<int, ExtendedMapData> _mapData =
        new Dictionary<int, ExtendedMapData>();

    private List<ExtendedMapData> _mapDataWorkingList;

    private Dictionary<int, ExtendedPawnData> _store =
        new Dictionary<int, ExtendedPawnData>();

    internal int lastEmergencySignalCooldown = 0;
    internal int lastEmergencySignalDelay;

    internal int lastEmergencySignalTick;

    public ExtendedDataStorage(World world) : base(world)
    {
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(
            ref _store, "store",
            LookMode.Value, LookMode.Deep,
            ref _idWorkingList, ref _extendedPawnDataWorkingList);
        Scribe_Collections.Look(
            ref _mapData, "mapData",
            LookMode.Value, LookMode.Deep,
            ref _idWorkingListMap, ref _mapDataWorkingList);
        Scribe_Values.Look(ref lastEmergencySignalTick, "lastEmergencySignalTick");
        Scribe_Values.Look(ref lastEmergencySignalDelay, "lastEmergencySignalDelay");
        Scribe_Values.Look(ref lastEmergencySignalDelay, "lastEmergencySignalCooldown");
    }

    // Return the associate extended data for a given Pawn, creating a new association
    // if required.
    public ExtendedPawnData GetExtendedDataFor(Pawn pawn)
    {
        var id = pawn.thingIDNumber;
        if (_store.TryGetValue(id, out var data))
        {
            return data;
        }

        var newExtendedData = new ExtendedPawnData();

        _store[id] = newExtendedData;
        return newExtendedData;
    }

    public void DeleteExtendedDataFor(Pawn pawn)
    {
        _store.Remove(pawn.thingIDNumber);
    }

    public void Cleanup()
    {
        var shouldRemove = new List<int>();
        foreach (var kv in _store)
        {
            if (kv.Value == null || kv.Value.ShouldClean())
            {
                shouldRemove.Add(kv.Key);
            }
        }

        foreach (var key in shouldRemove)
        {
            _store.Remove(key);
        }
    }


    public ExtendedMapData GetExtendedDataFor(Map map)
    {
        if (map == null)
        {
            return null;
        }

        if (_mapData == null) //Added this for compatibility with existing saves
        {
            _mapData = new Dictionary<int, ExtendedMapData>();
        }

        var id = map.uniqueID;
        if (_mapData.TryGetValue(id, out var data))
        {
            return data;
        }

        var newExtendedData = new ExtendedMapData();

        _mapData[id] = newExtendedData;
        return newExtendedData;
    }

    public void DeleteExtendedDataFor(Map map)
    {
        _mapData.Remove(map.uniqueID);
    }
}