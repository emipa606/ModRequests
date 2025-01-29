using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VexedThings
{
    [StaticConstructorOnStartup]
    // Stops corpses with given tags from rotting.
    public static class StopCorpseRot
    {
        static StopCorpseRot()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                SyntheticPawnsExtension tweaker = thingDef.GetModExtension<SyntheticPawnsExtension>();
                if (tweaker != null)
                {
                    ThingDef corpseDef = thingDef?.race?.corpseDef;
                    if (corpseDef != null)
                    {
                        if (tweaker.corpseWillNotRot)
                        {
                            corpseDef.comps.RemoveAll(compProperties => compProperties is CompProperties_Rottable);
                            corpseDef.comps.RemoveAll(compProperties => compProperties is CompProperties_SpawnerFilth);
                        }
                    }
                }
            }
        }

        // Stops corpses with given tags from being edible.
        public static void InedibleCorpses()
        {
            foreach (ThingDef thing in DefDatabase<ThingDef>.AllDefs)
            {
                SyntheticPawnsExtension extend = thing.GetModExtension<SyntheticPawnsExtension>();
                if (extend != null)
                {
                    ThingDef thing2;
                    if (thing == null)
                    {
                        thing2 = null;
                    }
                    else
                    {
                        RaceProperties race = thing.race;
                        thing2 = (race?.corpseDef);
                    }
                    ThingDef thing3 = thing2;
                    if (thing3 != null)
                    {
                        if (extend.corpseWillNotRot)
                        {
                            thing3.comps.RemoveAll(compProperties => compProperties is CompProperties_Rottable);
                            thing3.comps.RemoveAll(compProperties => compProperties is CompProperties_SpawnerFilth);
                        }
                        if (extend.corpseIsEdible)
                        {
                            if (thing3.modExtensions == null)
                            {
                                thing3.modExtensions = new List<DefModExtension>();
                            }
                            thing3.modExtensions.Add(new SyntheticPawnsExtension
                            {
                                corpseIsEdible = false
                            }
                            );
                        }
                    }
                }
            }
        }
    }
}
