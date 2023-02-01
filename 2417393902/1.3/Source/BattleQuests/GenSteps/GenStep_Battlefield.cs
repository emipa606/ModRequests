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

namespace BattleQuests
{
	public class GenStep_Battlefield : GenStep
	{
		private int MinRoomCells = 225;
		public override int SeedPart => 398638181;
		public override void Generate(Map map, GenStepParams parms)
		{
			TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors);
			if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map) && map.reachability.CanReachMapEdge(x, traverseParams)
			&& x.GetRoom(map).CellCount >= MinRoomCells, map, out IntVec3 result))
			{
				var comp = Current.Game.GetComponent<QuestManager>();
				if (comp.allyFactions.TryGetValue(parms.sitePart.site, out Faction allyFaction))
                {
					float points = parms.sitePart.parms.threatPoints * comp.threatLevels[parms.sitePart.site];
					PawnGroupMakerParms enemyMakerParms = new PawnGroupMakerParms();
					enemyMakerParms.tile = map.Tile;
					enemyMakerParms.faction = map.ParentFaction;
					enemyMakerParms.points = points * 2f;
					enemyMakerParms.groupKind = PawnGroupKindDefOf.Combat;
					List<Pawn> enemies = PawnGroupMakerUtility.GeneratePawns(enemyMakerParms).ToList();
					for (int i = 0; i < enemies.Count; i++)
					{
						IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(result, map, 10);
						GenSpawn.Spawn(enemies[i], loc, map, Rot4.Random);
					}
					var lordJob = new LordJob_AssaultColony(map.ParentFaction, false, true, false, true, false);
					LordMaker.MakeNewLord(map.ParentFaction, lordJob, map, enemies);

					PawnGroupMakerParms allyMakerParms = new PawnGroupMakerParms();
					allyMakerParms.tile = map.Tile;
					allyMakerParms.faction = allyFaction;
					allyMakerParms.points = points;
					allyMakerParms.groupKind = PawnGroupKindDefOf.Combat;
					List<Pawn> allies = PawnGroupMakerUtility.GeneratePawns(allyMakerParms).ToList();
					IntVec3 edgeCell = IntVec3.Invalid;
					if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map), map, CellFinder.EdgeRoadChance_Ignore, out edgeCell))
                    {
						edgeCell = CellFinder.RandomEdgeCell(map);
                    }
					for (int i = 0; i < allies.Count; i++)
					{
						IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(edgeCell, map, 10);
						GenSpawn.Spawn(allies[i], loc, map, Rot4.Random);
					}
					var allyLordJob = new LordJob_AssistColony(allyFaction, allies.RandomElement().Position);
					LordMaker.MakeNewLord(allyFaction, allyLordJob, map, allies);
					comp.allyFactions.Remove(parms.sitePart.site);
					comp.threatLevels.Remove(parms.sitePart.site);
				}
			}
		}
	}
}
