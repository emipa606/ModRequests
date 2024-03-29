using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ExpandedWoodworking
{
	public class RecipeWorkerCounter_MakeWoodLumber : RecipeWorkerCounter
	{
		private List<ThingDef> WoodLumberDefs;

		public override bool CanCountProducts(Bill_Production bill)
		{
			return true;
		}

		public override int CountProducts(Bill_Production bill)
		{
			if (WoodLumberDefs == null)
			{
				ThingCategoryDef woodLumber = ThingCategoryDefOf.WoodLumber;
				WoodLumberDefs = new List<ThingDef>(16);
				foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
				{
					if (item.thingCategories != null && item.thingCategories.Contains(woodLumber))
					{
						WoodLumberDefs.Add(item);
					}
				}
			}
			int num = 0;
			for (int i = 0; i < WoodLumberDefs.Count; i++)
			{
				num += bill.Map.resourceCounter.GetCount(WoodLumberDefs[i]);
			}
			return num;
		}

		public override string ProductsDescription(Bill_Production bill)
		{
			return ThingCategoryDefOf.WoodLumber.label;
		}

		public override bool CanPossiblyStoreInStockpile(Bill_Production bill, Zone_Stockpile stockpile)
		{
			foreach (ThingDef allowedThingDef in bill.ingredientFilter.AllowedThingDefs)
			{
				if (!allowedThingDef.butcherProducts.NullOrEmpty())
				{
					ThingDef thingDef = allowedThingDef.butcherProducts[0].thingDef;
					if (!stockpile.GetStoreSettings().AllowedToAccept(thingDef))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
