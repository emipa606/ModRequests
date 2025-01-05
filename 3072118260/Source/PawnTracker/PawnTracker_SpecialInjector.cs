using System;
using System.Linq;
using Verse;

namespace PawnTrackerMain
{
    public static class PawnTracker_SpecialInjector
    {
        public static void Inject()
        {
            //Give it to animals, as well, so that we don't have to be so careful about checking if someone's human
            InjectComp(typeof(PawnTrackerComp), def => def.race?.Humanlike == true || def.race?.Humanlike == false);
        }

        private static void InjectComp(Type compType, Func<ThingDef, bool> qualifier)
        {
            var defs = DefDatabase<ThingDef>.AllDefsListForReading.Where(qualifier).ToList();
            defs.RemoveDuplicates();

            foreach (var def in defs)
            {
                if (def.comps == null) 
                {
                    continue;
                }
                if (!def.comps.Any(c => c.compClass == compType))
                {
                    def.comps.Add(new CompProperties_PawnTracker());
                }
            }
        }
    }
}
