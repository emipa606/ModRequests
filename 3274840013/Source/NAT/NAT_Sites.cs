using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using DelaunatorSharp;
using Gilzoide.ManagedJobs;
using Ionic.Crc;
using Ionic.Zlib;
using JetBrains.Annotations;
using KTrie;
using LudeonTK;
using NVorbis.NAudioSupport;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using RimWorld.Utility;
using RuntimeAudioClipLoader;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Grammar;
using Verse.Noise;
using Verse.Profile;
using Verse.Sound;
using Verse.Steam;

namespace NAT
{
	public class GenStep_RustedBox : GenStep
	{

		private static readonly SimpleCurve numFleshSacksFromPoints = new SimpleCurve
		{
			new CurvePoint(100f, 1f),
			new CurvePoint(500f, 2f),
			new CurvePoint(1000f, 3f),
			new CurvePoint(5000f, 5f)
		};

		private bool forceAtLeastOneShard;


		private bool trySpawnInRoom;

		public override int SeedPart => 1234731256;

		public override void Generate(Map map, GenStepParams parms)
		{
			float x = parms.sitePart?.site?.desiredThreatPoints ?? StorytellerUtility.DefaultThreatPointsNow(map);
			int num = Mathf.RoundToInt(numFleshSacksFromPoints?.Evaluate(x) ?? 0f);
			for (int i = 0; i < num; i++)
			{
				if ((!trySpawnInRoom || !CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(c, map, mustBeInRoom: true), out var result)) && !CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(c, map, mustBeInRoom: false), out result))
				{
					continue;
				}
				List<Thing> list;
				if (forceAtLeastOneShard && i == 0)
				{
					list = Gen.YieldSingle(ThingMaker.MakeThing(ThingDefOf.Shard)).ToList();
				}
				else
				{
					ThingSetMakerParams parms2 = default(ThingSetMakerParams);
					parms2.qualityGenerator = QualityGenerator.Reward;
					parms2.makingFaction = Faction.OfEntities;
					list = ThingSetMakerDefOf.MapGen_FleshSackLoot.root.Generate(parms2);
				}
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					Thing thing = list[num2];
					GenSpawn.Spawn(thing, result, map);
				}
			}
		}

		private bool Validator(IntVec3 c, Map map, bool mustBeInRoom)
		{
			if (!c.Standable(map))
			{
				return false;
			}
			if (c.DistanceToEdge(map) <= 2)
			{
				return false;
			}
			if ((mustBeInRoom && c.GetRoom(map) == null) || !c.GetRoom(map).ProperRoom)
			{
				return false;
			}
			if (!map.generatorDef.isUnderground && !map.reachability.CanReachMapEdge(c, TraverseMode.PassDoors))
			{
				return false;
			}
			return true;
		}
	}
	public class SitePartWorker_DistressCall_RustedArmy : SitePartWorker_DistressCall
	{
		private static readonly SimpleCurve FleshbeastsPointsModifierCurve = new SimpleCurve
		{
			new CurvePoint(100f, 200f),
			new CurvePoint(500f, 800f),
			new CurvePoint(1000f, 1200f),
			new CurvePoint(5000f, 1600f)
		};

		public override void PostMapGenerate(Map map)
		{
			Site site = map.Parent as Site;
			Faction faction = site.Faction ?? Find.FactionManager.RandomEnemyFaction();
			List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
			{
				faction = faction,
				groupKind = PawnGroupKindDefOf.Settlement,
				points = SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange * 0.33f,
				tile = map.Tile
			}).ToList();
			float num = Faction.OfEntities.def.MinPointsToGeneratePawnGroup(NATDefOf.NAT_RustedArmy) * 1.1f;
			float num2 = Mathf.Max(FleshbeastsPointsModifierCurve.Evaluate(site.desiredThreatPoints), num);
			List<Pawn> list2 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
			{
				groupKind = NATDefOf.NAT_RustedArmy,
				points = num2,
				faction = Faction.OfEntities,
				raidStrategy = RaidStrategyDefOf.ImmediateAttack
			}).ToList();
			DistressCallUtility.SpawnCorpses(map, list, list2, map.Center, 20);
			DistressCallUtility.SpawnPawns(map, list2, map.Center, 20);
			//map.fogGrid.
			AnomalyIncidentUtility.PawnShardOnDeath(list2.RandomElement());
			foreach (Thing allThing in map.listerThings.AllThings)
			{
				if (allThing.def.category == ThingCategory.Item)
				{
					CompForbiddable compForbiddable = allThing.TryGetComp<CompForbiddable>();
					if (compForbiddable != null && !compForbiddable.Forbidden)
					{
						allThing.SetForbidden(value: true, warnOnFail: false);
					}
				}
			}
			LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_AssaultColony(), map, list2);
			foreach (Pawn p in map.mapPawns.AllPawnsSpawned.Where((Pawn p2)=> p2.Faction == Faction.OfPlayer && p2.drafter != null))
            {
				p.drafter.Drafted = true;
            }
		}
	}
	public class QuestNode_Root_DarkLair : QuestNode
	{
		private const int MaxDistanceFromColony = 9;

		private const int MinDistanceFromColony = 3;

		private const float MinPoints = 100f;

		private const int TimeoutTicks = 900000;

		private const float EmpireSitePointsThreshold = 2000f;

		private const float AmbushChance = 0.75f;

		private static readonly SimpleCurve PointsToRewardValue = new SimpleCurve
		{
			new CurvePoint(0f, 500f),
			new CurvePoint(1000f, 2000f),
			new CurvePoint(4000f, 5000f),
			new CurvePoint(20000f, 10000f)
		};
		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}
		protected override void RunInt()
		{
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			float num = slate.Get("points", 0f);
			if (num < 200f)
			{
				num = 200f;
			}
			slate.Set<float>("rewardValue", PointsToRewardValue.Evaluate(num));
			TryFindSiteTile(out var tile);
			IEnumerable<SitePartDef> source = DefDatabase<SitePartDef>.AllDefs.Where((SitePartDef def) => def.tags != null && def.tags.Contains("NAT_TEst"));
			Site site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
			{
				new SitePartDefWithParams(source.RandomElementByWeight((SitePartDef sp) => sp.selectionWeight), new SitePartParams
				{
					threatPoints = num
				})
			}, tile, Faction.OfEntities);
			quest.SpawnWorldObject(site);
			slate.Set("site", site);
			string inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
			string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
			//quest.Letter(LetterDefOf.NeutralEvent, null, null, label: "DistressSignalLabel".Translate(), text: "DistressSignalText".Translate(), lookTargets: Gen.YieldSingle(site), relatedFaction: site.Faction);
			quest.WorldObjectTimeout(site, 900000);
			quest.Delay(900000, delegate
			{
				QuestGen_End.End(quest, QuestEndOutcome.Fail);
			});
			quest.End(QuestEndOutcome.Success, 0, null, inSignal);
		}
		private bool TryFindSiteTile(out int tile)
		{
			return TileFinder.TryFindNewSiteTile(out tile, 3, 9);
		}
	}
	public class SitePartWorker_DarkLair : SitePartWorker
	{
		public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
		{
			base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
			List<Thing> list = slate.Get<List<Thing>>("stealedThings");
			int num = slate.Get<int>("kidnappedPawnsAmount");
			float num2 = slate.Get<float>("rewardValue");
			for(int i = 0; i < new IntRange(0 , 4).RandomInRange; i++)
            {
				Faction f = null;
				if (Rand.Chance(0.6f))
				{
					f = Find.FactionManager.GetFactions(false, false, false, TechLevel.Undefined, true).RandomElement();
				}
				Pawn p = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, f);
				PodContentsType value = Gen.RandomEnumValue<PodContentsType>(disallowFirstValue: true);
				ThingSetMakerParams parms = default(ThingSetMakerParams);
				parms.podContentsType = value;
				Building_DarkLairCasket building = (Building_DarkLairCasket)ThingMaker.MakeThing(NATDefOf.NAT_DarkLairCasket);
				building.SetFaction(Faction.OfEntities);
				building.containedPawn = p;
				list.Add(building);
				num2 += p.MarketValue;
			}
			FloatRange range = num2 * new FloatRange(1.3f, 1.6f);
			ThingSetMakerParams thingSetParms = default(ThingSetMakerParams);
			thingSetParms.totalMarketValueRange = range;
			list.AddRange(NATDefOf.NAT_Reward_CollectorLair.root.Generate(thingSetParms));
			part.things = new ThingOwner<Thing>(part, oneStackOnly: false);
			part.things.TryAddRangeOrTransfer(list, canMergeWithExistingStacks: false);
			slate.Set("generatedPawns", list);
		}
	}

	public class SymbolResolver_DarkLair_Content : SymbolResolver
	{
		public override void Resolve(RimWorld.BaseGen.ResolveParams rp)
		{
			
			IntVec3 randomCell = rp.rect.RandomCell;
			GenSpawn.Spawn(rp.singleThingToSpawn, randomCell, BaseGen.globalSettings.map);
		}
	}

	public class SymbolResolver_DarkLairThreat_Turret : SymbolResolver//Was removed from lair to prevent cage from be covered with battery
	{
		public override void Resolve(RimWorld.BaseGen.ResolveParams rp)
		{
			foreach (IntVec3 cell in CellRect.CenteredOn(rp.rect.CenterCell, rp.rect.Width + 2, rp.rect.Height + 2).ClipInsideMap(BaseGen.globalSettings.map).EdgeCells)
			{
				GenSpawn.Spawn(ThingDefOf.PowerConduit, cell, BaseGen.globalSettings.map);
			}
			for (int i = 0; i < new IntRange(1,3).RandomInRange; i++)
            {
				IntVec3 randomCell = rp.rect.RandomCell;
				Building_TurretGun turret = (Building_TurretGun)ThingMaker.MakeThing(ThingDefOf.Turret_MiniTurret, new List<ThingDef>{ ThingDefOf.Steel, ThingDefOf.Uranium, ThingDefOf.Bioferrite }.RandomElement());
				turret.SetFaction(Faction.OfEntities);
				GenSpawn.Spawn(turret, randomCell, BaseGen.globalSettings.map);
			}
			int z = 0;
			List<IntVec3> list = new List<IntVec3>();
			foreach (IntVec3 cell2 in rp.rect.EdgeCells)
            {
                if (cell2.z > z)
                {
					z = cell2.z;
				}
            }
			foreach (IntVec3 cell3 in rp.rect.EdgeCells)
			{
				if (cell3.z < z)
				{
					list.Add(cell3);
				}
			}
			Building_Battery battery = (Building_Battery)ThingMaker.MakeThing(ThingDefOf.Battery);
			battery.SetFaction(Faction.OfEntities);
			GenSpawn.Spawn(battery, list.Where((IntVec3 c) => c.GetThingList(BaseGen.globalSettings.map).NullOrEmpty()).RandomElement(), BaseGen.globalSettings.map);
		}
	}

	public class SymbolResolver_DarkLairThreat_MetalHorror : SymbolResolver
	{
		public override void Resolve(RimWorld.BaseGen.ResolveParams rp)
		{
			LordJob_SleepThenAssaultColony lordJob = new LordJob_SleepThenAssaultColony(Faction.OfEntities, rp.sendWokenUpMessage ?? true);
			Lord lord = LordMaker.MakeNewLord(Faction.OfEntities, lordJob, BaseGen.globalSettings.map);
			foreach (Pawn p in PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
			{
				groupKind = PawnGroupKindDefOf.Metalhorrors,
				faction = Faction.OfEntities,
				points = Mathf.Max(rp.threatPoints ?? Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Metalhorrors), Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Metalhorrors))
            }))
            {
				IntVec3 randomCell = rp.rect.RandomCell;
				lord.AddPawn(p);
				GenSpawn.Spawn(p, randomCell, BaseGen.globalSettings.map);
			}
			SignalAction_DormancyWakeUp obj = (SignalAction_DormancyWakeUp)ThingMaker.MakeThing(ThingDefOf.SignalAction_DormancyWakeUp);
			obj.signalTag = rp.ambushSignalTag;
			obj.lord = lord;
			GenSpawn.Spawn(obj, rp.rect.CenterCell, BaseGen.globalSettings.map);
		}
	}

	public class GenStep_DarkLair : GenStep
	{
		private const int Size = 7;

		public override int SeedPart => 913432591;

        public override void Generate(Map map, GenStepParams parms)
        {
			IntVec3 centerCell = CellFinder.RandomNotEdgeCell(10,map);
			CellRect cellRect = CellRect.CenteredOn(centerCell, 15, 15).ClipInsideMap(map);
			CellRect cellRect2 = CellRect.CenteredOn(centerCell, 13, 13).ClipInsideMap(map);
			CellRect cellRect3 = CellRect.CenteredOn(centerCell, 17, 17).ClipInsideMap(map);
			RimWorld.BaseGen.ResolveParams resolveParams = default(RimWorld.BaseGen.ResolveParams);
			GenDebug.ClearArea(cellRect, map);
			resolveParams.rect = cellRect;
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
			RimWorld.BaseGen.ResolveParams rp = resolveParams;
			rp.rect = cellRect2;
			if (parms.sitePart != null && parms.sitePart.things != null && parms.sitePart.things.Any)
			{
				foreach (Thing item in parms.sitePart.things.ToList())
				{
					rp.singleThingToSpawn = item;
					Log.Message("NAT_Pushed");
					BaseGen.symbolStack.Push("darkLairContent_NAT", rp);
				}
			}
			RimWorld.BaseGen.ResolveParams rp4 = resolveParams;
			rp4.rect = cellRect2;
			if (Rand.Chance(0.1f))
            {
				BaseGen.symbolStack.Push("darkLairMetalHorror_NAT", rp4);
			}
			RimWorld.BaseGen.ResolveParams rp3 = resolveParams;
			rp3.rect = cellRect3;
			BaseGen.symbolStack.Push("extraDoor", resolveParams);
			rp3.floorDef = TerrainDefOf.Voidmetal;
			rp3.wallThingDef = ThingDefOf.VoidmetalWall;
			rp3.chanceToSkipWallBlock = 0.7f;
			rp3.chanceToSkipFloor = 0.7f;
			BaseGen.symbolStack.Push("floor", rp3);
			RimWorld.BaseGen.ResolveParams rp2 = resolveParams;
			rp2.floorOnlyIfTerrainSupports = false;
			rp2.floorDef = TerrainDefOf.BrokenAsphalt;
			rp2.chanceToSkipFloor = 0f;
			rp2.chanceToSkipWallBlock = 0f;
			BaseGen.symbolStack.Push("emptyRoom", rp2);
			BaseGen.symbolStack.Push("edgeWalls", rp3);
			BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams);
			BaseGen.Generate();
		}
	}
}