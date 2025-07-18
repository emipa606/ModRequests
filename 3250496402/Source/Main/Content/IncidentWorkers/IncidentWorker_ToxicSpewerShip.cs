using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Kraltech_Industries;

public class IncidentWorker_ToxicSpewerShip : IncidentWorker
{
    private const float ShipPointsFactor = 2.5f;

    private const int IncidentMinimumPoints = 9000;

    private const float DefendRadius = 200f;

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return ((Map)parms.target).listerThings.ThingsOfDef(def.mechClusterBuilding).Count <= 0;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var map = (Map)parms.target;
        var list = new List<TargetInfo>();
        var mechClusterBuilding = def.mechClusterBuilding;
        IntVec3 intVec;
        CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.ToxicShipLanding, map, out intVec, 14, default, -1, false, true, true, true);
        var thing = ThingMaker.MakeThing(ThingDefOf.ToxicSpewerShip);
        bool result;
        if (intVec == IntVec3.Invalid)
        {
            result = false;
        }
        else
        {
            var points = Mathf.Max(parms.points * 2.5f, 9000f);
            var list2 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.MechVariantInvaders,
                tile = map.Tile,
                faction = Faction.OfMechanoids,
                points = points
            }).ToList();
            thing.SetFaction(Faction.OfMechanoids);
            LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_SleepThenMechanoidsDefend(new List<Thing>
            {
                thing
            }, Faction.OfMechanoids, 28f, intVec, false, false), map, list2);
            DropPodUtility.DropThingsNear(intVec, map, list2);
            foreach (var thing2 in list2)
            {
                var compCanBeDormant = thing2.TryGetComp<CompCanBeDormant>();
                if (compCanBeDormant != null) compCanBeDormant.ToSleep();
            }

            list.AddRange(from p in list2
                select new TargetInfo(p));
            GenSpawn.Spawn(SkyfallerMaker.MakeSkyfaller(ThingDefOf.ToxicShipLanding, thing), intVec, map);
            list.Add(new TargetInfo(intVec, map));
            SendStandardLetter(parms, list, Array.Empty<NamedArgument>());
            result = true;
        }

        return result;
    }

    private static IntVec3 FindDropPodLocation(Map map, Predicate<IntVec3> validator)
    {
        for (var i = 0; i < 200; i++)
        {
            var intVec = RCellFinder.FindSiegePositionFrom(DropCellFinder.FindRaidDropCenterDistant(map, true), map, true);
            if (validator(intVec)) return intVec;
        }

        return IntVec3.Invalid;
    }
}