using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;

namespace BattleQuests
{
	public class QuestPart_SpawnAllyPawns : QuestPart
	{
		public string inSignal;
		public Settlement settlement;
		public Faction allyFaction;

		public float points;
		public float threatLevel;
        public override IEnumerable<GlobalTargetInfo> QuestLookTargets => new List<GlobalTargetInfo> { settlement };
        public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				PawnGroupMakerParms allyMakerParms = new PawnGroupMakerParms();
				allyMakerParms.tile = settlement.Map.Tile;
				allyMakerParms.faction = allyFaction;
				allyMakerParms.points = points * threatLevel;
				allyMakerParms.groupKind = PawnGroupKindDefOf.Combat;
				List<Pawn> allies = PawnGroupMakerUtility.GeneratePawns(allyMakerParms).ToList();
				IntVec3 edgeCell = IntVec3.Invalid;
				if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(settlement.Map) && !x.Fogged(settlement.Map)
						&& settlement.Map.reachability.CanReachFactionBase(x, settlement.Map.ParentFaction), settlement.Map, CellFinder.EdgeRoadChance_Ignore, out edgeCell))
				{
					edgeCell = CellFinder.RandomEdgeCell(settlement.Map);
				}
				for (int i = 0; i < allies.Count; i++)
				{
					IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(edgeCell, settlement.Map, 10);
					GenSpawn.Spawn(allies[i], loc, settlement.Map, Rot4.Random);
				}
				var allyLordJob = new LordJob_AssistColony(allyFaction, allies.RandomElement().Position);
				LordMaker.MakeNewLord(allyFaction, allyLordJob, settlement.Map, allies);

				TraverseParms traverseParams = TraverseParms.For(TraverseMode.PassDoors);
				if (threatLevel > 1 && RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => x.Standable(settlement.Map) && !x.Fogged(settlement.Map)
				&& settlement.Map.reachability.CanReachMapEdge(x, traverseParams), settlement.Map, out IntVec3 result))
				{
					float points2 = points * threatLevel;
					PawnGroupMakerParms enemyMakerParms = new PawnGroupMakerParms();
					enemyMakerParms.tile = settlement.Map.Tile;
					enemyMakerParms.faction = settlement.Map.ParentFaction;
					enemyMakerParms.points = points;
					enemyMakerParms.groupKind = PawnGroupKindDefOf.Combat;
					List<Pawn> enemies = PawnGroupMakerUtility.GeneratePawns(enemyMakerParms).ToList();
					for (int i = 0; i < enemies.Count; i++)
					{
						IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(result, settlement.Map, 10);
						GenSpawn.Spawn(enemies[i], loc, settlement.Map, Rot4.Random);
					}
					var lordJob = new LordJob_DefendBase(settlement.Map.ParentFaction, settlement.Map.Center);
					LordMaker.MakeNewLord(settlement.Map.ParentFaction, lordJob, settlement.Map, enemies);
				}
			}
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
}
