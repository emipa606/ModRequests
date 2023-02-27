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
using Verse.Grammar;

namespace RadWorld
{
	public class QuestNode_EnsureSpawnOnCavern : QuestNode
	{
		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			IsValidTileForNewSettlement_Patch.mustBeOnCavern = true;
		}
	}
	public class QuestNode_EndSpawnOnCavernCheck : QuestNode
	{
		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			IsValidTileForNewSettlement_Patch.mustBeOnCavern = false;
		}
	}
	public class SitePartWorker_Vault : SitePartWorker
	{
		public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
		{
			string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
			lookTargets = map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.RaceProps.Humanlike && x.HostileTo(Faction.OfPlayer)).FirstOrDefault();
			return arrivedLetterPart;
		}

		public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
		{
			var faction = Find.FactionManager.FirstFactionOfDef(RW_DefOf.RW_VaultRough);
			part.site.SetFaction(faction);
			base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
			int enemiesCount = GetEnemiesCount(part.site, part.parms);
			outExtraDescriptionRules.Add(new Rule_String("enemiesCount", enemiesCount.ToString()));
			outExtraDescriptionRules.Add(new Rule_String("enemiesLabel", GetEnemiesLabel(part.site, enemiesCount)));
		}

		public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
		{
			return base.GetPostProcessedThreatLabel(site, sitePart) + ": " + "KnownSiteThreatEnemyCountAppend".Translate(GetEnemiesCount(site, sitePart.parms), "Enemies".Translate());
		}

		public override SitePartParams GenerateDefaultParams(float myThreatPoints, int tile, Faction faction)
		{
			SitePartParams sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
			sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement));
			return sitePartParams;
		}

		protected int GetEnemiesCount(Site site, SitePartParams parms)
		{
			return PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms
			{
				tile = site.Tile,
				faction = site.Faction,
				groupKind = PawnGroupKindDefOf.Settlement,
				points = parms.threatPoints,
				inhabitants = true,
				seed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms)
			}).Count();
		}

		protected string GetEnemiesLabel(Site site, int enemiesCount)
		{
			if (site.Faction == null)
			{
				return (enemiesCount == 1) ? "Enemy".Translate() : "Enemies".Translate();
			}
			if (enemiesCount != 1)
			{
				return site.Faction.def.pawnsPlural;
			}
			return site.Faction.def.pawnSingular;
		}
	}

	public class GenStep_Vault : GenStep
	{
		public int size = 16;

		public FloatRange defaultPawnGroupPointsRange = SymbolResolver_Settlement.DefaultPawnsPoints;

		private static List<CellRect> possibleRects = new List<CellRect>();
		public override int SeedPart => 398638181;

		public override void Generate(Map map, GenStepParams parms)
		{
			GetOrGenerateMapPatch.customSettlementGeneration = true;
			if (!MapGenerator.TryGetVar("RectOfInterest", out CellRect var))
			{
				var = CellRect.SingleCell(map.Center);
			}
			if (!MapGenerator.TryGetVar("UsedRects", out List<CellRect> var2))
			{
				var2 = new List<CellRect>();
				MapGenerator.SetVar("UsedRects", var2);
			}
			Faction faction = (map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.RandomEnemyFaction();
			ResolveParams resolveParams = default(ResolveParams);
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
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
			RimWorld.BaseGen.BaseGen.globalSettings.minBuildings = 1;
			RimWorld.BaseGen.BaseGen.globalSettings.minBarracks = 1;
			RimWorld.BaseGen.BaseGen.symbolStack.Push("settlement", resolveParams);
			if (faction != null && faction == Faction.Empire)
			{
				RimWorld.BaseGen.BaseGen.globalSettings.minThroneRooms = 1;
				RimWorld.BaseGen.BaseGen.globalSettings.minLandingPads = 1;
			}
			RimWorld.BaseGen.BaseGen.Generate();
			if (faction != null && faction == Faction.Empire && RimWorld.BaseGen.BaseGen.globalSettings.landingPadsGenerated == 0)
			{
				GenStep_Settlement.GenerateLandingPadNearby(resolveParams.rect, map, faction, out CellRect usedRect);
				var2.Add(usedRect);
			}
			var2.Add(resolveParams.rect);

			var file = SettlementGeneration.GetPresetFor(map.Parent, RW_DefOf.RW_VaultLocation);
			if (file != null)
            {
				SettlementGeneration.DoSettlementGeneration(map, file.FullName, RW_DefOf.RW_VaultLocation, faction, false);
            }
			var deadBodiesCount = Rand.RangeInclusive(2, 5);
			var vaulterFaction = Find.FactionManager.FirstFactionOfDef(RW_DefOf.RW_VaultNatives);
			for (var i = 0; i < deadBodiesCount; i++)
            {
				var pawn = PawnGenerator.GeneratePawn(RW_DefOf.RW_Vault_Villager, vaulterFaction);
				var cell = resolveParams.rect.Cells.Where(x => x.Walkable(map)).RandomElement();
				GenSpawn.Spawn(pawn, cell, map);
				pawn.Kill(null);
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