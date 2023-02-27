using FactionBaseGeneration;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;

namespace RadWorld
{
	public class SitePartWorker_AbandonedVault : SitePartWorker
	{
		public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
		{
			part.site.SetFaction(null);
			base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
		}
	}

	public class GenStep_AbandonedVault : GenStep
	{
		public int size = 16;

		public FloatRange defaultPawnGroupPointsRange = SymbolResolver_Settlement.DefaultPawnsPoints;

		private static List<CellRect> possibleRects = new List<CellRect>();
		public override int SeedPart => 398638181;

		public override void Generate(Map map, GenStepParams parms)
		{
			GetOrGenerateMapPatch.customSettlementGeneration = true;
			Faction faction = (map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.RandomEnemyFaction();

			var file = SettlementGeneration.GetPresetFor(map.Parent, RW_DefOf.RW_AbandonedVault);
			if (file != null)
            {
				var tiles = SettlementGeneration.DoSettlementGeneration(map, file.FullName, RW_DefOf.RW_AbandonedVault, faction, false);
				var irradiatedInsectsCount = Rand.RangeInclusive(8, 16);
				var insects = new List<Pawn>();
				for (var i = 0; i < irradiatedInsectsCount; i++)
                {
					var pawn = PawnGenerator.GeneratePawn(RW_DefOf.RW_IrradiatedMegaspider, null);
					var cell = tiles.Where(x => x.Walkable(map)).RandomElement();
					GenSpawn.Spawn(pawn, cell, map);
					pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
					pawn.SetFaction(Faction.OfInsects);
					insects.Add(pawn);
				}
				LordMaker.MakeNewLord(Faction.OfInsects, new LordJob_DefendBase(Faction.OfInsects, tiles.RandomElement()), map, insects);
				var turretCount = Rand.RangeInclusive(2, 4);
				foreach (var turret in map.listerThings.AllThings.Where(x => x.def.building?.IsTurret ?? false))
                {
					turret.SetFaction(Faction.OfInsects);
				}
				for (var i = 0; i < turretCount; i++)
				{
					var turret = ThingMaker.MakeThing(ThingDefOf.Turret_MiniTurret, ThingDefOf.Steel);
					var cell = tiles.Where(x => x.Walkable(map)).RandomElement();
					GenSpawn.Spawn(turret, cell, map);
					turret.SetFaction(Faction.OfInsects);
				}
				var lootCount = Rand.RangeInclusive(4, 8);
				for (var i = 0; i < lootCount; i++)
                {
					ThingSetMakerParams parms2 = default(ThingSetMakerParams);
					parms2.maxTotalMass = 200f;
					parms2.totalMarketValueRange = new FloatRange(500f, 1200f);
					parms2.tile = map.Tile;
					var loot = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms2);
					for (var j = 0; j < loot.Count; j++)
					{
						var cell = tiles.Where(x => x.Walkable(map) && !x.GetThingList(map).Any()).RandomElement();
						GenSpawn.Spawn(loot[j], cell, map);
						loot[j].SetForbidden(true);
					}
				}

				var percent = new FloatRange(0.3f, 0.8f).RandomInRange;
				var countToTake = (int)(tiles.Count() * percent);
				var tilesToApplyRubble = tiles.InRandomOrder().Take(countToTake).ToList();
				foreach (var tile in tilesToApplyRubble)
                {
					FilthMaker.TryMakeFilth(tile, map, ThingDefOf.Filth_RubbleBuilding);
				}
			}
			GetOrGenerateMapPatch.customSettlementGeneration = false;
			map.GetComponent<MapComponentGeneration>().ReFog = true;

		}

		private CellRect GetOutpostRect(CellRect rectToDefend, List<CellRect> usedRects, Map map)
		{
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
					return source.RandomElement();
				}
				return possibleRects.RandomElement();
			}
			return rectToDefend;
		}
	}
}