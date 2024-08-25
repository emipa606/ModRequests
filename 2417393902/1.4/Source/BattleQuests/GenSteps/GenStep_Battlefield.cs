using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace BattleQuests;

public class GenStep_Battlefield : GenStep
{
    private readonly int MinRoomCells = 225;
    public override int SeedPart => 398638181;

    public override void Generate(Map map, GenStepParams parms)
    {
        var traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors);
        if (!RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(x =>
                x.Standable(map) && !x.Fogged(map) && map.reachability.CanReachMapEdge(x, traverseParams)
                && x.GetRoom(map).CellCount >= MinRoomCells, map, out var result))
        {
            return;
        }

        var comp = Current.Game.GetComponent<QuestManager>();
        if (!comp.allyFactions.TryGetValue(parms.sitePart.site, out var allyFaction))
        {
            return;
        }

        var points = parms.sitePart.parms.threatPoints * comp.threatLevels[parms.sitePart.site];
        var enemyMakerParms = new PawnGroupMakerParms
        {
            tile = map.Tile,
            faction = map.ParentFaction,
            points = points * 2f,
            groupKind = PawnGroupKindDefOf.Combat
        };
        var enemies = PawnGroupMakerUtility.GeneratePawns(enemyMakerParms).ToList();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < enemies.Count; i++)
        {
            var loc = CellFinder.RandomSpawnCellForPawnNear(result, map, 10);
            GenSpawn.Spawn(enemies[i], loc, map, Rot4.Random);
        }

        var lordJob = new LordJob_AssaultColony(map.ParentFaction, false, true, false, true, false);
        LordMaker.MakeNewLord(map.ParentFaction, lordJob, map, enemies);

        var allyMakerParms = new PawnGroupMakerParms
        {
            tile = map.Tile,
            faction = allyFaction,
            points = points,
            groupKind = PawnGroupKindDefOf.Combat
        };
        var allies = PawnGroupMakerUtility.GeneratePawns(allyMakerParms).ToList();
        if (!CellFinder.TryFindRandomEdgeCellWith(x => x.Standable(map) && !x.Fogged(map), map,
                CellFinder.EdgeRoadChance_Ignore, out var edgeCell))
        {
            edgeCell = CellFinder.RandomEdgeCell(map);
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < allies.Count; i++)
        {
            var loc = CellFinder.RandomSpawnCellForPawnNear(edgeCell, map, 10);
            GenSpawn.Spawn(allies[i], loc, map, Rot4.Random);
        }

        var allyLordJob = new LordJob_AssistColony(allyFaction, allies.RandomElement().Position);
        LordMaker.MakeNewLord(allyFaction, allyLordJob, map, allies);
        comp.allyFactions.Remove(parms.sitePart.site);
        comp.threatLevels.Remove(parms.sitePart.site);
    }
}