using System.Collections.Generic;
using Verse;

namespace UINotIncluded
{
    internal static class DesignatorManager
    {
        private static bool cacheUpdated;

        private static List<Designator>[] CacheJobs =
            { new List<Designator>(), new List<Designator>(), new List<Designator>(), new List<Designator>() };

        public static List<Designator>[] GetDesignationConfigs()
        {
            if (cacheUpdated) return CacheJobs;
            foreach (var designatorslist in CacheJobs) designatorslist.Clear();
            CacheJobs = Settings.GetDesignationConfigs();
            cacheUpdated = true;
            return CacheJobs;
        }

        public static void Pull()
        {
            cacheUpdated = false;
        }

        public static void Push()
        {
            for (var i = 0; i < 4; i++) Settings.UpdateDesignations(CacheJobs[i], (DesignationConfig)i);
        }
    }
}