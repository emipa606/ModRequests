using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SimpleWarrants
{
    public class IncidentWorker_Visitors : IncidentWorker_VisitorGroup
    {
        private List<Pawn> SpawnDelivers(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			var pawnCount = Rand.RangeInclusive(2, 5);

			List<Pawn> list = new List<Pawn>();
			for (var i = 0; i < pawnCount; i++)
            {
				list.Add(PawnGenerator.GeneratePawn(parms.faction.RandomPawnKind(), parms.faction));
            }
			foreach (Pawn item in list)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5);
				GenSpawn.Spawn(item, loc, map);
				parms.storeGeneratedNeutralPawns?.Add(item);
			}
			return list;
		}

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            return false;
        }

        public bool SpawnVisitors(Thing toDeliver, IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryResolveParms(parms))
			{
				return false;
			}
			List<Pawn> list = SpawnDelivers(parms);
			if (list.Count == 0)
			{
				return false;
			}
			LordMaker.MakeNewLord(parms.faction, CreateLordJob(parms, list), map, list);
			Pawn leader = list.Find(x => parms.faction.leader == x);
			var deliveree = leader ?? list.RandomElement();
			GenSpawn.Spawn(toDeliver, deliveree.Position, map);
			IntVec3 cell = IntVec3.Invalid;
			cell = map.areaManager.Home.ActiveCells.Where(x => x.Walkable(map) && deliveree.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly))
				.RandomElementByWeight(x => x.DistanceTo(deliveree.Position));
			if (!cell.IsValid && !RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(deliveree.Position, map, 50, out cell))
            {
				cell = map.AllCells.Where(x => x.Walkable(map) && deliveree.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly)).RandomElement();
			}
			var job = JobMaker.MakeJob(JobDefOf.HaulToCell, toDeliver, cell);
			job.count = 1;
			job.locomotionUrgency = LocomotionUrgency.Walk;
			deliveree.jobs.TryTakeOrderedJob(job);
			SendLetter(parms, list, leader, false);
			return true;
		}
    }
}