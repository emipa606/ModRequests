using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class IngestibleWishDef : ThingWishDef
    {
        public List<IngestibleInfo> includedThing;

        public FloatRange? joyRange;
        public bool inIngredient = false;
        public bool checkPerNutriment = false;

        public new float amountNeeded;
        public new float specificAmount;

        public bool noCorpse = false;

        private List<IngestibleInfo> cachedIngestibles = null;
        public List<IngestibleInfo> Ingestibles
        {
            get
            {
                if (cachedIngestibles == null)
                {
                    cachedIngestibles = includedThing ?? new List<IngestibleInfo>();
                    CompleteInfo();
                    if (findPossibleWant) CacheData(SearchedDef);
                }
                return cachedIngestibles;
            }
        }
        protected override bool FastSearchMatch(ThingDef thing)
        {
            return base.FastSearchMatch(thing) 
                && thing.IsIngestible
                && !cachedIngestibles.Any(info => info.ingestible == thing)
                && CheckMatch(thing)
                && (!noCorpse || !thing.IsCorpse);
        }
        protected virtual bool CheckMatch(ThingDef ingestible)
        {
            return joyRange?.Includes(ingestible.ingestible.joy) ?? true;
        }


        protected override void CacheData(List<ThingDef> defs)
        {
            if (defs.NullOrEmpty())
            {
                return;
            }
            for (int i = 0; i < defs.Count; i++)
            {
                cachedIngestibles.Add(new IngestibleInfo
                {
                    ingestible = defs[i],
                    amount = specificAmount,
                    inIngredient = inIngredient
                });
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (!includedThing.NullOrEmpty())
            {
                for (int i = 0; i < includedThing.Count; i++)
                {
                    if (!includedThing[i].ingestible.IsIngestible) yield return ("HDream : Wrong ThingDef listed in includedThing for IngestibleWishDef " + defName + ". It's not an ingestible thing ! That ThingDef '" + includedThing[i].ToString() + "' was removed from the list");
                }
            }
        }
        protected virtual void CompleteInfo()
        {
            for (int i = 0; i < cachedIngestibles.Count; i++)
            {
                if (cachedIngestibles[i].amount == -1) cachedIngestibles[i].amount = specificAmount;
            }
        }
    }
}
