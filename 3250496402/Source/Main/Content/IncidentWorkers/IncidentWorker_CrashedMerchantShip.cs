﻿using RimWorld;
using UnityEngine;
using Verse;

namespace Kraltech_Industries;

public class IncidentWorker_CrashedMerchantShip : IncidentWorker
{
    private static readonly Pair<int, float>[] CountChance =
    {
        new(1, 3f)
    };

    static IncidentWorker_CrashedMerchantShip()
    {
    }

    private int RandomCountToDrop
    {
        get
        {
            var x2 = Find.TickManager.TicksGame / 3600000f;
            var timePassedFactor = Mathf.Clamp(GenMath.LerpDouble(0f, 1.2f, 1f, 0.1f, x2), 0.1f, 1f);
            return CountChance.RandomElementByWeight(delegate(Pair<int, float> x)
            {
                float result;
                if (x.First == 1)
                    result = x.Second;
                else
                    result = x.Second * timePassedFactor;
                return result;
            }).First;
        }
    }

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        bool result;
        if (!base.CanFireNowSub(parms))
        {
            result = false;
        }
        else
        {
            var map = (Map)parms.target;
            IntVec3 intVec;
            result = TryFindCrashedShipDropCell(map.Center, map, 999999, out intVec);
        }

        return result;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var map = (Map)parms.target;
        IntVec3 firstChunkPos;
        bool result;
        if (!TryFindCrashedShipDropCell(map.Center, map, 999999, out firstChunkPos))
        {
            result = false;
        }
        else
        {
            SpawnShipChunks(firstChunkPos, map, RandomCountToDrop);
            result = true;
        }

        return result;
    }

    private void SpawnShipChunks(IntVec3 firstChunkPos, Map map, int count)
    {
        SpawnChunk(firstChunkPos, map);
        for (var i = 0; i < count - 1; i++)
        {
            IntVec3 pos;
            if (TryFindCrashedShipDropCell(firstChunkPos, map, 5, out pos)) SpawnChunk(pos, map);
        }
    }

    private void SpawnChunk(IntVec3 pos, Map map)
    {
        SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MerchantShipCrashing, ThingDefOf.CrashedMerchantShip, pos, map);
    }

    private bool TryFindCrashedShipDropCell(IntVec3 nearLoc, Map map, int maxDist, out IntVec3 pos)
    {
        return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MerchantShipCrashing, map, out pos, 10, nearLoc, maxDist, false);
    }
}
