using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.BaseGen;
using UnityEngine;

namespace LostTechnology
{

    public class GenStep_ScaledOutpost : GenStep
	{
		public FloatRange defaultPawnGroupPointsRange = SymbolResolver_Settlement.DefaultPawnsPoints;

		private static List<CellRect> possibleRects = new List<CellRect>();
		public override int SeedPart => 398138181;

		public Dictionary<float, float> complexityCurve = new Dictionary<float, float>
		{
			{1f, 200},
			{1.5f, 400},
			{2f, 800 },
			{2.5f, 1200},
			{3f, 1600},
			{3.5f, 2000}
		};
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
            var techprint = parms.sitePart.things.First();
			Faction faction = (map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.RandomEnemyFaction();
			ResolveParams resolveParams = default(ResolveParams);
			var complexityPoints = StorytellerUtility.DefaultThreatPointsNow(Find.World) * LostTechnologySettings.threatMultiplier;
            var complexityLevel = complexityCurve.OrderBy(item => Math.Abs(complexityPoints - item.Value)).First().Key;
			resolveParams.rect = GetOutpostRect(var, var2, map, complexityLevel);
            resolveParams.faction = faction;
            resolveParams.edgeDefenseWidth = Mathf.Min(3, (int)(Rand.RangeInclusive(0, 1) * complexityLevel * 2f));
            resolveParams.edgeDefenseTurretsCount = (int)(Rand.RangeInclusive(0, 1) * complexityLevel * 2f);
            resolveParams.edgeDefenseMortarsCount = (int)(Rand.RangeInclusive(0, 1) * complexityLevel * 2f);
			if (faction.def.pawnGroupMakers.Any(gr => gr.kindDef == PawnGroupKindDefOf.Settlement))
            {
				resolveParams.settlementPawnGroupPoints = Mathf.Max(faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement), complexityPoints);

			}
            else
            {
				resolveParams.settlementPawnGroupPoints = Mathf.Max(faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat), complexityPoints);

			}
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
            RimWorld.BaseGen.BaseGen.globalSettings.minBuildings = 1;
            RimWorld.BaseGen.BaseGen.globalSettings.minBarracks = 1;
            RimWorld.BaseGen.BaseGen.symbolStack.Push("settlement", resolveParams);
            if (faction != null && faction == Faction.OfEmpire)
            {
                RimWorld.BaseGen.BaseGen.globalSettings.minThroneRooms = 1;
                RimWorld.BaseGen.BaseGen.globalSettings.minLandingPads = 1;
            }
            RimWorld.BaseGen.BaseGen.Generate();
            if (faction != null && faction == Faction.OfEmpire && RimWorld.BaseGen.BaseGen.globalSettings.landingPadsGenerated == 0)
            {
                GenStep_Settlement.GenerateLandingPadNearby(resolveParams.rect, map, faction, out CellRect usedRect);
                var2.Add(usedRect);
            }

            var cell = resolveParams.rect.Where(x => x.Walkable(map) && map.thingGrid.ThingsListAt(x).Count == 0 && x.Roofed(map)).RandomElement();
            GenSpawn.Spawn(techprint, cell, map);

            var2.Add(resolveParams.rect);
        }

        private CellRect GetOutpostRect(CellRect rectToDefend, List<CellRect> usedRects, Map map, float complexityLevel)
		{
			var size = (int)(16f * complexityLevel);
			if (size > 60)
			{
				size = 60;
			}
			possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.CenterCell.z - size / 2, size, size));
			possibleRects.Add(new CellRect(rectToDefend.minX - 1 - size, rectToDefend.CenterCell.z - size / 2, size, size));
			possibleRects.Add(new CellRect(rectToDefend.maxX + 1, rectToDefend.CenterCell.z - size / 2, size, size));
			possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.minZ - 1 - size, size, size));
			possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.maxZ + 1, size, size));
			CellRect mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
			possibleRects.RemoveAll((CellRect x) => !x.FullyContainedWithin(mapRect));
			if (possibleRects.Any())
			{
				IEnumerable<CellRect> source = possibleRects.Where((CellRect x) => !usedRects.Any((CellRect y) => x.Overlaps(y)));
				if (!source.Any())
				{
					possibleRects.Add(new CellRect(rectToDefend.minX - 1 - size * 2, rectToDefend.CenterCell.z - size / 2, size, size));
					possibleRects.Add(new CellRect(rectToDefend.maxX + 1 + size, rectToDefend.CenterCell.z - size / 2, size, size));
					possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.minZ - 1 - size * 2, size, size));
					possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.maxZ + 1 + size, size, size));
				}
				if (source.Any())
				{
					return source.First();
				}
				return possibleRects.RandomElement();
			}
			return rectToDefend;
		}

	}
}
