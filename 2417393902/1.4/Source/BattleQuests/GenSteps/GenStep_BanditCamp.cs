using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace BattleQuests;

public class GenStep_BanditCamp : GenStep
{
    private static readonly List<CellRect> possibleRects = new List<CellRect>();

    public FloatRange defaultPawnGroupPointsRange = SymbolResolver_Settlement.DefaultPawnsPoints;
    public int size = 16;

    public override int SeedPart => 398638181;

    public override void Generate(Map map, GenStepParams parms)
    {
        if (!MapGenerator.TryGetVar("RectOfInterest", out CellRect var))
        {
            var = CellRect.SingleCell(map.Center);
        }

        if (!MapGenerator.TryGetVar("UsedRects", out List<CellRect> var2))
        {
            var2 = new List<CellRect>();
            MapGenerator.SetVar("UsedRects", var2);
        }

        var faction = map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer
            ? map.ParentFaction
            : Find.FactionManager.RandomEnemyFaction();
        var resolveParams = default(ResolveParams);
        resolveParams.rect = GetOutpostRect(var, var2, map);
        resolveParams.faction = faction;
        resolveParams.edgeDefenseWidth = 2;
        resolveParams.edgeDefenseTurretsCount = Rand.RangeInclusive(0, 1);
        resolveParams.edgeDefenseMortarsCount = 0;
        if (parms.sitePart != null)
        {
            resolveParams.settlementPawnGroupPoints = parms.sitePart.parms.threatPoints;
            resolveParams.settlementPawnGroupSeed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms);
        }
        else
        {
            resolveParams.settlementPawnGroupPoints = defaultPawnGroupPointsRange.RandomInRange;
        }

        BaseGen.globalSettings.map = map;
        BaseGen.globalSettings.minBuildings = 1;
        BaseGen.globalSettings.minBarracks = 1;
        BaseGen.symbolStack.Push("settlement", resolveParams);
        if (faction != null && faction == Faction.OfEmpire)
        {
            BaseGen.globalSettings.minThroneRooms = 1;
            BaseGen.globalSettings.minLandingPads = 1;
        }

        BaseGen.Generate();
        if (faction != null && faction == Faction.OfEmpire && BaseGen.globalSettings.landingPadsGenerated == 0)
        {
            GenStep_Settlement.GenerateLandingPadNearby(resolveParams.rect, map, faction, out var usedRect);
            var2.Add(usedRect);
        }

        var2.Add(resolveParams.rect);
        var comp = Current.Game.GetComponent<QuestManager>();
        if (parms.sitePart == null || !comp.allyFactions.TryGetValue(parms.sitePart.site, out var allyFaction))
        {
            return;
        }

        var points = parms.sitePart.parms.threatPoints * comp.threatLevels[parms.sitePart.site];
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

    private CellRect GetOutpostRect(CellRect rectToDefend, List<CellRect> usedRects, Map map)
    {
        possibleRects.Add(
            new CellRect(rectToDefend.minX - 1 - size, rectToDefend.CenterCell.z - (size / 2), size, size));
        possibleRects.Add(new CellRect(rectToDefend.maxX + 1, rectToDefend.CenterCell.z - (size / 2), size, size));
        possibleRects.Add(
            new CellRect(rectToDefend.CenterCell.x - (size / 2), rectToDefend.minZ - 1 - size, size, size));
        possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - (size / 2), rectToDefend.maxZ + 1, size, size));
        var mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
        possibleRects.RemoveAll(x => !x.FullyContainedWithin(mapRect));
        if (!possibleRects.Any())
        {
            return rectToDefend;
        }

        var source = possibleRects.Where(x => !usedRects.Any(x.Overlaps));
        if (source.Any())
        {
            return source.Any() ? source.RandomElement() : possibleRects.RandomElement();
        }

        possibleRects.Add(new CellRect(rectToDefend.minX - 1 - (size * 2),
            rectToDefend.CenterCell.z - (size / 2), size, size));
        possibleRects.Add(new CellRect(rectToDefend.maxX + 1 + size, rectToDefend.CenterCell.z - (size / 2),
            size, size));
        possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - (size / 2),
            rectToDefend.minZ - 1 - (size * 2), size, size));
        possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - (size / 2), rectToDefend.maxZ + 1 + size,
            size, size));

        return source.Any() ? source.RandomElement() : possibleRects.RandomElement();
    }
}