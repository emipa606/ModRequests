using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MoreTimeInfo
{
    public class ClockAccuracyManager : MapComponent
    {
        static int accuracyCache = -1;
        static long lastUpdateTick = -1;
        public ClockAccuracyManager(Map map) : base(map)
        {
        }
        private void UpdateAcccuracyCache()
        {
            accuracyCache = -1;
            IEnumerable<CompClock> clocks = from Building b in map.listerBuildings.allBuildingsColonist
                                            where b.GetComp<CompClock>() != null && (b.GetComp<CompPowerTrader>()?.PowerOn ?? true)
                                            select b.GetComp<CompClock>();
            foreach (CompClock c in clocks)
            {
                accuracyCache = Math.Max(accuracyCache, c.ClockAccuracy);
                if (accuracyCache >= 1)
                    break;
            }
            lastUpdateTick = Find.TickManager.TicksAbs;
        }
        public int ClockAccuracy
        {
            get
            {
                if (lastUpdateTick < 0 || lastUpdateTick < (Find.TickManager.TicksAbs - 60))
                {
                    UpdateAcccuracyCache();
                }
                return accuracyCache;
            }
        }
    }
}
