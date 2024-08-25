using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace BattleQuests;

public class QuestPart_SpawnAllyPawns : QuestPart
{
    public Faction allyFaction;
    public string inSignal;

    public float points;
    public Settlement settlement;
    public float threatLevel;
    public override IEnumerable<GlobalTargetInfo> QuestLookTargets => new List<GlobalTargetInfo> { settlement };

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag != inSignal)
        {
            return;
        }

        var allyMakerParms = new PawnGroupMakerParms
        {
            tile = settlement.Map.Tile,
            faction = allyFaction,
            points = points * threatLevel,
            groupKind = PawnGroupKindDefOf.Combat
        };
        var allies = PawnGroupMakerUtility.GeneratePawns(allyMakerParms).ToList();
        if (!CellFinder.TryFindRandomEdgeCellWith(
                x => x.Standable(settlement.Map) && !x.Fogged(settlement.Map) &&
                     settlement.Map.reachability.CanReachFactionBase(x, settlement.Map.ParentFaction), settlement.Map,
                CellFinder.EdgeRoadChance_Ignore, out var edgeCell))
        {
            edgeCell = CellFinder.RandomEdgeCell(settlement.Map);
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < allies.Count; i++)
        {
            var loc = CellFinder.RandomSpawnCellForPawnNear(edgeCell, settlement.Map, 10);
            GenSpawn.Spawn(allies[i], loc, settlement.Map, Rot4.Random);
        }

        var allyLordJob = new LordJob_AssistColony(allyFaction, allies.RandomElement().Position);
        LordMaker.MakeNewLord(allyFaction, allyLordJob, settlement.Map, allies);

        var traverseParams = TraverseParms.For(TraverseMode.PassDoors);
        if (!(threatLevel > 1) || !RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(x =>
                    x.Standable(settlement.Map) && !x.Fogged(settlement.Map)
                                                && settlement.Map.reachability.CanReachMapEdge(x, traverseParams),
                settlement.Map, out var result))
        {
            return;
        }

        var enemyMakerParms = new PawnGroupMakerParms
        {
            tile = settlement.Map.Tile,
            faction = settlement.Map.ParentFaction,
            points = points * threatLevel,
            groupKind = PawnGroupKindDefOf.Combat
        };
        var enemies = PawnGroupMakerUtility.GeneratePawns(enemyMakerParms).ToList();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < enemies.Count; i++)
        {
            var loc = CellFinder.RandomSpawnCellForPawnNear(result, settlement.Map, 10);
            GenSpawn.Spawn(enemies[i], loc, settlement.Map, Rot4.Random);
        }

        var lordJob = new LordJob_DefendBase(settlement.Map.ParentFaction, settlement.Map.Center);
        LordMaker.MakeNewLord(settlement.Map.ParentFaction, lordJob, settlement.Map, enemies);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_References.Look(ref settlement, "settlement");
        Scribe_References.Look(ref allyFaction, "allyFaction");
        Scribe_Values.Look(ref points, "points");
        Scribe_Values.Look(ref threatLevel, "threatLevel");
    }
}