using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class DrugWishDef : IngestibleWishDef
    {
        public DrugCategory drugCategory = DrugCategory.Any;

        protected override bool CheckMatch(ThingDef ingestible)
        {
            return ingestible.IsDrug
                && base.CheckMatch(ingestible)
                && (drugCategory == DrugCategory.Any || drugCategory == ingestible.ingestible.drugCategory);
        }
    }
}
