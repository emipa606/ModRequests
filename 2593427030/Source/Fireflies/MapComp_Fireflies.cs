using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Fireflies
{
    public class FireflyGroup : IExposable
    {
        private List<Firefly> fireflies = new List<Firefly>();
        private bool disappearingInt;

        public bool ShouldBeGone => fireflies.NullOrEmpty();
        public bool IsDisappearing => disappearingInt;

        public void ExposeData()
        {
            Scribe_Values.Look(ref disappearingInt, "disappearing");
            Scribe_Collections.Look(ref fireflies, "fireflies", LookMode.Reference);
        }

        public void SpawnGroupAround(IntVec3 c, Map map, int groups)
        {
            var pawnKind = FFDefOf.FF_FireflyKind;

            int randomInRange = pawnKind.wildGroupSize.RandomInRange / groups;
            int radius = Mathf.CeilToInt(Mathf.Sqrt((float)pawnKind.wildGroupSize.max));
            for (int i = 0; i < randomInRange; i++)
            {
                IntVec3 loc2 = CellFinder.RandomClosewalkCellNear(c, map, radius, null);
                var ff = (Firefly) GenSpawn.Spawn(PawnGenerator.GeneratePawn(pawnKind, null), loc2, map, WipeMode.Vanish);
                
                if (ff != null)
                    fireflies.Add(ff);
            }
        }

        public void DespawnGroupNow()
        {
            if (!ShouldBeGone)
            {
                for (int i = 0; i < fireflies.Count; i++)
                {
                    fireflies[i]?.DeSpawn();
                }
                fireflies.Clear();
            }
        }

        public void StartDisappearing()
        {
            disappearingInt = true;
        }

        public void Tick()
        {
            if (!IsDisappearing) return;
            if (Find.TickManager.TicksGame % FireflySettingsDef.Def.despawnInterval == 0)
            {
                var ff = fireflies.RandomElement();
                if (ff != null && ff.Spawned)
                {
                    ff.DeSpawn();
                }
                fireflies.Remove(ff);
            }
        }
    }

    public class MapComp_Fireflies : MapComponent
    {
        private int nextStartHour = -1;
        private int nextEndHour = -1;
        private FireflyGroup curGroup;

        private FireflySettingsDef Settings => FireflySettingsDef.Def;

        public MapComp_Fireflies(Map map) : base(map)
        {
        }

        public override void FinalizeInit()
        {
            // Don't bother ticking a component that does nothing
            if (Settings.MaxGroupCount(map.Biome) <= 0)
            {
                // Safety measure for between-version folks
                curGroup?.DespawnGroupNow();

                map.components.Remove(this);
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref nextStartHour, "fireflyStartHour");
            Scribe_Values.Look(ref nextEndHour, "fireflyEndHour");
            Scribe_Deep.Look(ref curGroup, "fireflyGroup");
        }

        public override void MapComponentTick()
        {
            if (nextStartHour < 0)
            {
                nextStartHour = Settings.spawnHours.RandomInRange;
            }
            if (nextEndHour < 0)
            {
                nextEndHour = Settings.despawnHours.RandomInRange;
            }

            var hourOfDay = GenLocalDate.HourOfDay(map);
            if (curGroup != null)
            {
                curGroup.Tick();
                if (!curGroup.IsDisappearing && hourOfDay == nextEndHour)
                {
                    curGroup.StartDisappearing();
                    nextEndHour = Settings.despawnHours.RandomInRange;
                }
                if (curGroup.ShouldBeGone)
                {
                    curGroup = null;
                }
                return;
            }

            if (hourOfDay == nextStartHour)
            {
                nextStartHour = Settings.spawnHours.RandomInRange;
                GenerateFireflies();
            }
            //
        }

        private void GenerateFireflies()
        {
            //
            Predicate<IntVec3> predicate = delegate(IntVec3 c)
            {
                if (!c.Standable(map)) return false;
                if (c.GetTerrain(map).avoidWander) return false;
                if (c.Roofed(map)) return false;
                if (c.Fogged(map)) return false;
                District district = c.GetDistrict(map, RegionType.Set_Passable);
                if (district == null) return false;
                //if (district.TouchesMapEdge) return false;

                return true;
            };

            curGroup = new FireflyGroup();
            int groupCount = new IntRange(1, Settings.MaxGroupCount(map.Biome)).RandomInRange;
            for (int i = 0; i < groupCount; i++)
            {
                if (CellFinderLoose.TryGetRandomCellWith(predicate, map, 100, out IntVec3 result))
                {
                    curGroup.SpawnGroupAround(result, map, groupCount);
                }
            }
        }
    }
}
