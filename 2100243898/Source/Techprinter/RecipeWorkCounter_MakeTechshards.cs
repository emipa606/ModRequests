using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DTechprinting
{
    class RecipeWorkCounter_MakeTechshards : RecipeWorkerCounter
    {

        public override bool CanCountProducts(Bill_Production bill)
        {
            return true;
        }

        public override int CountProducts(Bill_Production bill)
        {

            return bill.Map.resourceCounter.GetCountIn(Base.DefOf.DTechshards);
        }

        public override string ProductsDescription(Bill_Production bill)
        {
            return Base.DefOf.DTechshards.label;
        }

        public override bool CanPossiblyStoreInStockpile(Bill_Production bill, Zone_Stockpile stockpile)
        {
            foreach (ThingDef thingDef in bill.ingredientFilter.AllowedThingDefs)
            {
                ResearchProjectDef rpd;
                if (!Base.thingDic.TryGetValue(thingDef, out rpd))
                    return false;
                ThingDef shardDef = ShardMaker.ShardForProject(rpd);
                if (!stockpile.GetStoreSettings().AllowedToAccept(shardDef))
                {
                    return false;
                }

            }
            return true;
        }
    }
}
