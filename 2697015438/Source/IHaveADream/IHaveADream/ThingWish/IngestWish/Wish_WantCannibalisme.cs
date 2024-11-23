using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class Wish_WantCannibalisme : Wish_WantIngestible
	{
		protected override List<IngestibleInfo> GetThingsFromDef()
		{
			List<IngestibleInfo> infos = new List<IngestibleInfo>
			{
				new IngestibleInfo()
				{
					ingestible = pawn.def.race.meatDef,
					inIngredient = Def.inIngredient,
					amount = Def.amountNeeded
				}
			};
			return infos;
		}
		public override void CkeckResolve(Thing thing, int amount, float nutriment)
		{
			IngestibleInfo ingestibleInfo = null;
			if (FoodUtility.IsHumanlikeCorpseOrHumanlikeMeat(thing, thing.def))
			{
				ingestibleInfo = ThingsWanted[0];
			}
			else if (ThingsWanted[0].inIngredient)
			{
				CompIngredients compIngredients = thing.TryGetComp<CompIngredients>();
				if (compIngredients != null && !compIngredients.ingredients.NullOrEmpty())
				{
					amount = 0;
					nutriment = 0;
					for (int i = 0; i < compIngredients.ingredients.Count; i++)
					{
						if (FoodUtility.IsHumanlikeCorpseOrHumanlikeMeat(thing, compIngredients.ingredients[i]))
						{
							RecipeDef recipe = DefDatabase<RecipeDef>.AllDefsListForReading.Find(rec => rec.ProducedThingDef == thing.def);
							IngredientCount ingredient = recipe?.ingredients?.Find(ing => ing.filter.Allows(compIngredients.ingredients[i])) ?? null;
							if (ingredient != null)
							{
								amount += ingredient.CountRequiredOfFor(ThingsWanted[0].ingestible, recipe);
								nutriment += ingredient.GetBaseCount();
								ingestibleInfo = ThingsWanted[0];
							}
						}
					}
				}
			}
			DoUpdate(ingestibleInfo, amount, nutriment);
		}

	}
}
