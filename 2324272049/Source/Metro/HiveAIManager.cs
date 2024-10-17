using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Metro
{
	public class HiveAIManager : MapComponent
	{
		public HiveAIManager(Map map) : base(map)
		{
            if (spidersPawns == null) spidersPawns = new List<Pawn>();
            if (nosalisPawns == null) nosalisPawns = new List<Pawn>();
            if (spidersHiveCells == null) spidersHiveCells = new HashSet<IntVec3>();
            if (nosalisHiveCells == null) nosalisHiveCells = new HashSet<IntVec3>();
            if (spidersMinedOutCellsCached == null) spidersMinedOutCellsCached = new HashSet<IntVec3>();
            if (nosalisesMinedOutCellsCached == null) nosalisesMinedOutCellsCached = new HashSet<IntVec3>();
            if (rocksToBeMined == null) rocksToBeMined = new Dictionary<Mineable, Pawn>();
        }

        private HashSet<IntVec3> rocks = new HashSet<IntVec3>();
        private HashSet<HashSet<IntVec3>> groups = new HashSet<HashSet<IntVec3>>();
        public Dictionary<Mineable, Pawn> rocksToBeMined = new Dictionary<Mineable, Pawn>();
        public HashSet<IntVec3> nosalisHiveCells = new HashSet<IntVec3>();
        private List<Pawn> nosalisPawns = new List<Pawn>();
        public HashSet<IntVec3> spidersHiveCells = new HashSet<IntVec3>();
        public HashSet<IntVec3> spidersMinedOutCellsCached = new HashSet<IntVec3>();
        public HashSet<IntVec3> nosalisesMinedOutCellsCached = new HashSet<IntVec3>();

        private List<Pawn> spidersPawns = new List<Pawn>();
        private IntVec3 spiderEntrance = IntVec3.Invalid;
        private IntVec3 nosalistEntrance = IntVec3.Invalid;

        public List<Thing> activeThreatsForSpiders = new List<Thing>();
        public List<Thing> activeThreatsForNosalises = new List<Thing>();

        public List<Pawn> Spiders
        {
            get
            {
                spidersPawns.RemoveAll(x => x == null || x.Dead || x.Destroyed);
                return spidersPawns;
            }
        }

        public List<Pawn> Nosalises
        {
            get
            {
                nosalisPawns.RemoveAll(x => x == null || x.Dead || x.Destroyed);
                return nosalisPawns;
            }
        }

        public void ProcessCell(IntVec3 initialCell, IntVec3 adjCell)
        {
            if (rocks.Contains(adjCell))
            {
                foreach (var group in groups)
                {
                    if (group.Contains(initialCell))
                    {
                        if (group.Contains(adjCell))
                        {
                            return;
                        }
                        else
                        {
                            foreach (var group2 in groups)
                            {
                                if (group != group2 && group2.Contains(adjCell))
                                {
                                    if (group.Count > group2.Count)
                                    {
                                        group.AddRange(group2);
                                        group2.Clear();
                                    }
                                    else
                                    {
                                        group2.AddRange(group);
                                        group.Clear();
                                    }
                                    return;
                                }
                            }
                            group.Add(adjCell);
                            return;
                        }
                    }
                }
                var newGroup = new HashSet<IntVec3>();
                newGroup.Add(initialCell);
                groups.Add(newGroup);
            }
            return;
        }
        public void GenerateRockGroups()
        {
            rocks.Clear();
            groups.Clear();
            foreach (var cell in this.map.AllCells)
            {
                if (!nosalisHiveCells.Contains(cell) && !spidersHiveCells.Contains(cell) && cell.GetFirstMineable(this.map) != null)
                {
                    rocks.Add(cell);
                }
            }

            foreach (var rock in rocks)
            {
                for (int i = 0; i < 8; i++)
                {
                    ProcessCell(rock, rock + GenAdj.AdjacentCells[i]);
                }
            }
        }

        public HashSet<IntVec3> GetNosalisHiveCells(Pawn pawn)
        {
            if (nosalisHiveCells == null || nosalisHiveCells.Count == 0)
            {
                GenerateRockGroups();
                nosalisHiveCells = groups.Select(x => x).Where(y => y.Count > 200).OrderBy(z => pawn.Position.DistanceTo(z.First())).FirstOrDefault();
                if (nosalisHiveCells == null)
                {
                    nosalisHiveCells = groups.Select(x => x).OrderByDescending(x => x.Count).FirstOrDefault();
                }
                groups.Remove(nosalisHiveCells);
            }
            return nosalisHiveCells;
        }

        public HashSet<IntVec3> GetSpidersHiveCells(Pawn pawn)
        {
            if (spidersHiveCells == null || spidersHiveCells.Count == 0)
            {
                GenerateRockGroups();
                spidersHiveCells = groups.Select(x => x).Where(y => y.Count > 200).OrderBy(z => pawn.Position.DistanceTo(z.First())).FirstOrDefault();
                if (spidersHiveCells == null)
                {
                    spidersHiveCells = groups.Select(x => x).OrderByDescending(x => x.Count).FirstOrDefault();
                }
                groups.Remove(spidersHiveCells);
            }
            return spidersHiveCells;
        }

        public HashSet<IntVec3> GetHiveCellsFor(Pawn pawn)
        {
            if (pawn.IsSpider())
            {
                if (!spidersPawns.Contains(pawn))
                {
                    spidersPawns.Add(pawn);
                }
                return this.GetSpidersHiveCells(pawn);
            }
            else if (pawn.IsNosalis())
            {
                if (!nosalisPawns.Contains(pawn))
                {
                    nosalisPawns.Add(pawn);
                }
                return this.GetNosalisHiveCells(pawn);
            }
            return null;
        }

        public void CheckIfEntranceExists(Pawn pawn, HashSet<IntVec3> cells)
        {
            if (pawn.IsSpider())
            {
                if (!spiderEntrance.IsValid)
                {
                    var minedCells = GetMinedOutCells(cells);
                    if (minedCells.Count > 0)
                    {
                        spiderEntrance = minedCells.First();
                    }
                }
            }
            else if (pawn.IsNosalis())
            {

            }
        }

        public IntVec3 GetSpiderEntrance(HashSet<IntVec3> cells)
        {
            if (!spiderEntrance.IsValid)
            {
                var minedCells = GetMinedOutCells(cells);
                if (minedCells.Count > 0)
                {
                    spiderEntrance = minedCells.First();
                    return spiderEntrance;
                }
            }
            else
            {
                return spiderEntrance;
            }
            return IntVec3.Invalid;
        }

        public IntVec3 GetHiveCenterFor(Pawn pawn)
        {
            var minedCells = GetMinedOutCellsFor(pawn);
            if (minedCells.Count > 0)
            {
                var x_Averages = minedCells.OrderBy(x => x.x);
                var x_average = x_Averages.ElementAt(x_Averages.Count() / 2).x;
                var z_Averages = minedCells.OrderBy(x => x.z);
                var z_average = z_Averages.ElementAt(z_Averages.Count() / 2).z;
                var middleCell = new IntVec3(x_average, 0, z_average);
                //Log.Message(pawn + " has middle cell - " + middleCell, true);
                return middleCell;
            }
            else
            {
                var cells = GetHiveCellsFor(pawn);
                var x_Averages = cells.OrderBy(x => x.x);
                var x_average = x_Averages.ElementAt(x_Averages.Count() / 2).x;
                var z_Averages = cells.OrderBy(x => x.z);
                var z_average = z_Averages.ElementAt(z_Averages.Count() / 2).z;
                var middleCell = new IntVec3(x_average, 0, z_average);
                //Log.Message(pawn + " has hive center - " + middleCell, true);
                return middleCell;
            }
        }

        public HashSet<IntVec3> GetMinedOutCellsFor(Pawn pawn)
        {
            if (pawn.IsSpider())
            {
                if (spidersMinedOutCellsCached == null)
                {
                    spidersMinedOutCellsCached = GetMinedOutCells(spidersHiveCells);
                }
                return spidersMinedOutCellsCached;
            }
            else if (pawn.IsNosalis())
            {
                if (nosalisesMinedOutCellsCached == null)
                {
                    nosalisesMinedOutCellsCached = GetMinedOutCells(nosalisHiveCells);
                }
                return nosalisesMinedOutCellsCached;
            }
            return null;
        }

        public HashSet<IntVec3> GetMinedOutCells(HashSet<IntVec3> cells)
        {
            HashSet<IntVec3> newCells = new HashSet<IntVec3>();
            foreach (var cell in cells)
            {
                if (cell.GetFirstMineable(this.map) == null)
                {
                    newCells.Add(cell);
                }
            }
            return newCells;
        }

        public float GetTotalNutritionFor(HashSet<IntVec3> cells, Pawn pawn)
        {
            float result = 0;
            foreach (var cell in cells)
            {
                foreach (var foodCandidate in cell.GetThingList(map))
                {
                    try
                    {
                        if (foodCandidate is Cocoon cocoon && cocoon.ContainedThing != null)
                        {
                            result += cocoon.ContainedThing.def.ingestible.CachedNutrition;
                        }
                        else if (foodCandidate.def != pawn.def && foodCandidate.def.IsNutritionGivingIngestible)
                        {
                            result += foodCandidate.def.ingestible.CachedNutrition;
                        }
                    }
                    catch
                    {

                    }
                }
            }
            return result;
        }

        public bool GroupNeedsExpansion(Pawn pawn)
        {
            if (pawn.IsSpider())
            {
                return (float)GetMinedOutCellsFor(pawn).Count / (float)Spiders.Count < 4f;
            }
            else if (pawn.IsNosalis())
            {
                return (float)GetMinedOutCellsFor(pawn).Count / (float)Nosalises.Count < 4f;
            }
            return false;
        }

        public bool GroupNeedsMoreFood(Pawn pawn)
        {
            if (pawn.IsSpider())
            {
                return GetTotalNutritionFor(spidersHiveCells, pawn) / (float)Spiders.Count < 3f;
            }
            else if (pawn.IsNosalis())
            {
                return GetTotalNutritionFor(nosalisHiveCells, pawn) / (float)Nosalises.Count < 3f;
            }
            return false;
        }

        public List<Pawn> GetPawnGroup(Pawn pawn)
        {
            if (pawn.IsSpider())
            {
                return Spiders.Where(x => x != pawn).ToList();
            }
            else if (pawn.IsNosalis())
            {
                return Nosalises.Where(x => x != pawn).ToList();
            }
            return null;
        }

        public bool ThingIsNotReservedByPawns(Mineable thing, Pawn pawn)
        {
            if (rocksToBeMined != null && rocksToBeMined.TryGetValue(thing, out Pawn value) && value != pawn)
            {
                return false;
            }
            return true;
        }
        public void Notify_PawnsInHiveAboutCloseThreat(Pawn notifier, Thing pawn)
        {
            if (notifier.IsSpider())
            {
                OrderSpidersInHiveToAttack(pawn);
            }
            else if (notifier.IsNosalis())
            {
                OrderNosalisesInHiveToAttack(pawn);
            }
        }

        public void OrderSpidersInHiveToAttack(Thing pawn)
        {
            foreach (var spider in Spiders.Where(x => spidersHiveCells.Contains(x.Position)))
            {
                if (!spider.IsFighting() && !spider.Downed)
                {
                    spider.jobs.StopAll();
                    spider.jobs.StartJob(spider.MeleeAttackJob(pawn, new IntRange(360, 480).RandomInRange));
                }
            }
            if (!activeThreatsForSpiders.Contains(pawn))
            {
                activeThreatsForSpiders.Add(pawn);
            }
        }

        public void OrderNosalisesInHiveToAttack(Thing pawn)
        {
            foreach (var nosalis in Nosalises.Where(x => nosalisHiveCells.Contains(x.Position)))
            {
                if (!nosalis.IsFighting() && !nosalis.Downed)
                {
                    nosalis.jobs.StopAll();
                    nosalis.jobs.StartJob(nosalis.MeleeAttackJob(pawn, new IntRange(360, 480).RandomInRange));
                }
            }

            if (!activeThreatsForNosalises.Contains(pawn))
            {
                activeThreatsForNosalises.Add(pawn);
            }
        }

        public void Notify_MutantAttacked(Pawn victim, Thing instigator)
        {
            if (victim.IsSpider() && victim.Position.DistanceTo(GetHiveCenterFor(victim)) < 100f)
            {
                OrderSpidersInHiveToAttack(instigator);
            }
            else if (victim.IsNosalis() && victim.Position.DistanceTo(GetHiveCenterFor(victim)) < 100f)
            {
                OrderNosalisesInHiveToAttack(instigator);
            }
        }
        public void UpdateSpiderDuties()
        {
            foreach (var pawn in Spiders)
            {
                if (pawn.mindState.duty == null)
                {
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.DefendHiveAggressively);
                }
                pawn.mindState.duty.focus = GetHiveCenterFor(pawn);
            }
        }

        public void UpdateNosalisesDuties()
        {
            foreach (var pawn in Nosalises)
            {
                if (pawn.mindState.duty == null)
                {
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.DefendHiveAggressively);
                }
                pawn.mindState.duty.focus = GetHiveCenterFor(pawn);
            }
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            UpdateSpiderDuties();
            UpdateNosalisesDuties();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref nosalisHiveCells, "nosalisHiveCells", LookMode.Value);
            Scribe_Collections.Look(ref nosalisesMinedOutCellsCached, "nosalisesMinedOutCellsCached", LookMode.Value);
            Scribe_Collections.Look(ref spidersHiveCells, "spidersHiveCells", LookMode.Value);
            Scribe_Collections.Look(ref spidersMinedOutCellsCached, "spidersMinedOutCellsCached", LookMode.Value);
            Scribe_Collections.Look(ref spidersPawns, "spidersPawns", LookMode.Reference);
            Scribe_Collections.Look(ref nosalisPawns, "nosalisPawns", LookMode.Reference);
            Scribe_Collections.Look(ref activeThreatsForNosalises, "activeThreatsForNosalises", LookMode.Reference);
            Scribe_Collections.Look(ref activeThreatsForSpiders, "activeThreatsForSpiders", LookMode.Reference);
            Scribe_Values.Look(ref spiderEntrance, "spiderEntrance");
        }
    }
}

