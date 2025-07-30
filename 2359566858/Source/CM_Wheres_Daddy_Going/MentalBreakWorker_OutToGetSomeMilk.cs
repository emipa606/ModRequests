using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Wheres_Daddy_Going
{
    class MentalBreakWorker_OutToGetSomeMilk : MentalBreakWorker
    {
        public override float CommonalityFor(Pawn pawn, bool moodCaused = false)
        {
            float commonality = base.CommonalityFor(pawn, moodCaused);

            WhereIsDad memoriesOfDad = WheresDaddyGoingMod.Instance.MemoriesOfDad;
            if (memoriesOfDad != null)
            {
                bool? didHeGetTheThing = memoriesOfDad.DidParentGetTheThing(pawn);

                // If this happened before and they did not get the thing, then do this break
                if (didHeGetTheThing.HasValue && didHeGetTheThing.Value == false)
                {
                    commonality *= 100.0f;
                }
                // If this is the first time or they got the thing the last time, then only allow extreme
                else if (def.intensity != MentalBreakIntensity.Extreme)
                {
                    return 0.0f;
                }
            }

            List<Pawn> relations = pawn.relations.DirectRelations.Select(relation => relation.otherPawn).ToList();

            if (relations == null || relations.Count == 0)
                return 0.0f;

            int relationsOnMap = relations.Where(relative => relative.Map == pawn.Map).Count();

            // Xeno's paradox between 1 and 2 - For each additional relative add half the remaining distance
            float multiplier = 1.0f;
            float baseMultiplierAdd = 0.5f;
            for (int i = 1; i < relationsOnMap; ++i)
            {
                multiplier += baseMultiplierAdd;
                baseMultiplierAdd *= 0.5f;
            }

            commonality *= multiplier;

            Logger.MessageFormat(this, "Mental break {0} considered with commonality {1}, multiplier {2}", def, commonality, multiplier);

            return commonality;
        }
    }
}
