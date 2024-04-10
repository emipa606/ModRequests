using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BetterInfestations
{
    public class GenStep_CaveHives : GenStep
    {
        private List<IntVec3> rockCells = new List<IntVec3>();
        private List<IntVec3> possibleSpawnCells = new List<IntVec3>();
        private List<Hive> spawnedHives = new List<Hive>();
        public override int SeedPart => 349641510;
        public override void Generate(Map map, GenStepParams parms)
        {
            if (!Find.Storyteller.difficulty.allowCaveHives)
            {
                return;
            }
            MapGenFloatGrid caves = MapGenerator.Caves;
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            float num = 0.7f;
            int num2 = 0;
            rockCells.Clear();
            foreach (IntVec3 allCell in map.AllCells)
            {
                if (elevation[allCell] > num)
                {
                    rockCells.Add(allCell);
                }
                if (caves[allCell] > 0f)
                {
                    num2++;
                }
            }
            List<IntVec3> list = map.AllCells.Where((IntVec3 c) => map.thingGrid.ThingsAt(c).Any((Thing thing) => thing.Faction != null)).ToList();
            GenMorphology.Dilate(list, 50, map);
            HashSet<IntVec3> hashSet = new HashSet<IntVec3>(list);
            int num3 = GenMath.RoundRandom(num2 / 1000f);
            GenMorphology.Erode(rockCells, 10, map);
            possibleSpawnCells.Clear();
            for (int i = 0; i < rockCells.Count; i++)
            {
                if (caves[rockCells[i]] > 0f && !hashSet.Contains(rockCells[i]))
                {
                    possibleSpawnCells.Add(rockCells[i]);
                }
            }
            spawnedHives.Clear();
            for (int j = 0; j < num3; j++)
            {
                TrySpawnHive(map);
            }
            spawnedHives.Clear();
        }

        private void TrySpawnHive(Map map)
        {
            if (TryFindHiveSpawnCell(map, out IntVec3 spawnCell))
            {
                possibleSpawnCells.Remove(spawnCell);
                Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.BI_Hive), spawnCell, map);
                hive.SetFaction(Faction.OfInsects);
                (from x in hive.GetComps<CompSpawner>()
                 where x.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.GlowPod
                 select x).First().TryDoSpawn();
                hive.CompSpawnerPawns.canSpawnPawns = false;
                hive.GetComp<CompSpawnerHives>().canSpawnHives = false;
                spawnedHives.Add(hive);
            }
        }

        private bool TryFindHiveSpawnCell(Map map, out IntVec3 spawnCell)
        {
            float num = -1f;
            IntVec3 intVec = IntVec3.Invalid;
            for (int i = 0; i < 3; i++)
            {
                if (!possibleSpawnCells.Where((IntVec3 x) => x.Standable(map) && x.GetFirstItem(map) == null && x.GetFirstBuilding(map) == null && x.GetFirstPawn(map) == null).TryRandomElement(out IntVec3 result))
                {
                    break;
                }
                float num2 = -1f;
                for (int j = 0; j < spawnedHives.Count; j++)
                {
                    float num3 = result.DistanceToSquared(spawnedHives[j].Position);
                    if (num2 < 0f || num3 < num2)
                    {
                        num2 = num3;
                    }
                }
                if (!intVec.IsValid || num2 > num)
                {
                    intVec = result;
                    num = num2;
                }
            }
            spawnCell = intVec;
            return spawnCell.IsValid;
        }
    }
}