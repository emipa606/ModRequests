using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace HDream
{
    public class WishItemStuffSetting
    {

        private List<StuffCategoryDef> fromCategories;
        private List<StatModifier> minStatFactor;
        private List<StatModifier> minStatOffset;
        private List<StatModifier> minBaseStat;

        public List<ThingDef> MatchingStuffs()
        {
            List<ThingDef> match = new List<ThingDef>();

            List<ThingDef> allThing = DefDatabase<ThingDef>.AllDefsListForReading;

            StatModifier stuffStat;
            bool shoulContinue;
            for (int i = 0; i < allThing.Count; i++)
            {
                if (!allThing[i].IsStuff || (fromCategories != null && allThing[i].stuffProps.categories.Any(cat => !fromCategories.Contains(cat)))) continue;

                if (minStatFactor != null)
                {
                    if (allThing[i].stuffProps.statFactors == null) continue;
                    shoulContinue = false;
                    for (int j = 0; j < minStatFactor.Count; j++)
                    {
                        stuffStat = allThing[i].stuffProps.statFactors.Find(stat => stat.stat == minStatFactor[j].stat);
                        if (stuffStat == null || stuffStat.value < minStatFactor[j].value)
                        {
                            shoulContinue = true;
                            break;
                        }
                    }
                    if (shoulContinue) continue;
                }

                if (minStatOffset != null)
                {
                    if (allThing[i].stuffProps.statOffsets == null) continue;
                    shoulContinue = false;
                    for (int j = 0; j < minStatOffset.Count; j++)
                    {
                        stuffStat = allThing[i].stuffProps.statOffsets.Find(stat => stat.stat == minStatOffset[j].stat);
                        if (stuffStat == null || stuffStat.value < minStatOffset[j].value)
                        {
                            shoulContinue = true;
                            break;
                        }
                    }
                    if (shoulContinue) continue;
                }
                if (minBaseStat != null)
                {
                    if (allThing[i].statBases == null) continue;
                    shoulContinue = false;
                    for (int j = 0; j < minBaseStat.Count; j++)
                    {
                        stuffStat = allThing[i].statBases.Find(stat => stat.stat == minBaseStat[j].stat);
                        if (stuffStat == null || stuffStat.value < minBaseStat[j].value)
                        {
                            shoulContinue = true;
                            break;
                        }
                    }
                    if (shoulContinue) continue;
                }
                match.Add(allThing[i]);
            }
            return match;
        }
    }
}
