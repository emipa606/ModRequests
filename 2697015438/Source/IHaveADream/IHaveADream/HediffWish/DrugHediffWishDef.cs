using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace HDream
{
    public class DrugHediffWishDef : SearchHediffWishDef
    {
        public DrugCategory drugCategory = DrugCategory.Any;
        protected override List<HediffDef> SearchedDef
        {
            get
            {
                List<HediffDef> hediffDefs = new List<HediffDef>();
                List<ThingDef> things = DefDatabase<ThingDef>.AllDefsListForReading;
                HediffDef hDef;
                for (int j = 0; j < things.Count; j++)
                {
                    hDef = FindDrugHediff(things[j]);
                    if (hDef != null) hediffDefs.Add(hDef);
                }
                return hediffDefs;
            }
        }
        protected virtual HediffDef FindDrugHediff(ThingDef ingestible)
        {
            if (!ingestible.IsIngestible 
                || !ingestible.IsDrug 
                || !(drugCategory == DrugCategory.Any || drugCategory == ingestible.ingestible.drugCategory)) return null;
            IEnumerable<IngestionOutcomeDoer_GiveHediff> outcomes = (IEnumerable<IngestionOutcomeDoer_GiveHediff>)ingestible.ingestible.outcomeDoers.Where(outcome => (outcome as IngestionOutcomeDoer_GiveHediff)?.hediffDef != null);
            if(outcomes.EnumerableNullOrEmpty()) return null;
            if (outcomes.Count() == 1) return outcomes.First().hediffDef;
            outcomes = outcomes.Where(outcome => outcome.toleranceChemical != null);
            if (outcomes.EnumerableNullOrEmpty() || outcomes.Count() > 1)
            {
                Log.Error("HDream : the search of drug for HediffWishDef " + defName + " for ingestible " + ingestible.defName + " failed, too many hediffdef was found and it couldn't determine the drug one");
                return null;
            }
            return outcomes.First().hediffDef;
        }
    }
}
