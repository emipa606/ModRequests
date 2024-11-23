using RimWorld;
using HarmonyLib;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HDream
{
	public class Wish_WantIngestible : Wish_Thing<IngestibleInfo>
	{
		public new IngestibleWishDef Def => (IngestibleWishDef)def;

		protected float amountIngested = 0;

		protected Dictionary<ThingDef, float> thingsIngested = new Dictionary<ThingDef, float>();

		protected override List<IngestibleInfo> GetThingsFromDef()
		{
			List<IngestibleInfo> ingestibles = Def.Ingestibles.ListFullCopy();
			for (int i = ingestibles.Count - 1; i >= 0; i--)
			{
				if (!pawn.def.race.CanEverEat(ingestibles[i].ingestible))
				{
					ingestibles.RemoveAt(i);
				}
			}
			return ingestibles;
		}
		protected override ThingDef GetThingDef(IngestibleInfo thing) => thing.ingestible;
		protected override LookMode ExposeLookModeT() => LookMode.Deep;

		public virtual void CkeckResolve(Thing thing, int amount, float nutriment)
		{
			IngestibleInfo ingestibleInfo = null;
			for (int i = 0; i < ThingsWanted.Count; i++)
			{
				if (thingsIngested[ThingsWanted[i].ingestible] >= ThingsWanted[i].amount) continue;
				if (ThingsWanted[i].ingestible == thing.def)
				{
					ingestibleInfo = ThingsWanted[i];
					break;
				}
				else if (ThingsWanted[i].inIngredient)
				{
					CompIngredients compIngredients = thing.TryGetComp<CompIngredients>();
					if (compIngredients != null && compIngredients.ingredients.Contains(ThingsWanted[i].ingestible))
					{
						RecipeDef recipe = DefDatabase<RecipeDef>.AllDefsListForReading.Find(rec => rec.ProducedThingDef == thing.def);
						IngredientCount ingredient = recipe?.ingredients?.Find(ing => ing.filter.Allows(ThingsWanted[i].ingestible)) ?? null;
						if (ingredient == null) continue;
						amount = ingredient.CountRequiredOfFor(ThingsWanted[i].ingestible, recipe);
						nutriment = ingredient.GetBaseCount();
						ingestibleInfo = ThingsWanted[i];
						break;
					}
				}
			}
			DoUpdate(ingestibleInfo, amount, nutriment);
		}
		protected void DoUpdate(IngestibleInfo ingestibleInfo, int amount, float nutriment)
		{
			if (ingestibleInfo == null) return;
			if (Def.countAmountPerInfo)
			{
				thingsIngested[ingestibleInfo.ingestible] += Def.checkPerNutriment ? nutriment : amount;
				if (thingsIngested[ingestibleInfo.ingestible] >= ingestibleInfo.amount) amountIngested++;
			}
			else
			{
				amountIngested -= thingsIngested[ingestibleInfo.ingestible];
				thingsIngested[ingestibleInfo.ingestible] += Def.checkPerNutriment ? nutriment : amount;
				if (thingsIngested[ingestibleInfo.ingestible] > ingestibleInfo.amount) amountIngested += ingestibleInfo.amount;
				else amountIngested += thingsIngested[ingestibleInfo.ingestible];
			}
			if (amountIngested >= Def.amountNeeded) OnFulfill();
			else ChangeProgress(Mathf.FloorToInt((amountIngested / Def.amountNeeded) / (Def.progressStep * (progressCount + 1f))));

		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref amountIngested, "amountIngested", 0);
			Scribe_Collections.Look(ref thingsIngested, "thingsIngested", LookMode.Def, LookMode.Value);
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				pawn.wishes().ingestibleWishes.Add(this);
			}
		}

		public override void PostAdd()
		{
			base.PostAdd();
			pawn.wishes().ingestibleWishes.Add(this);
			for (int i = 0; i < ThingsWanted.Count; i++)
			{
				thingsIngested.Add(ThingsWanted[i].ingestible, 0);
			}
		}

		public override void PostRemoved()
		{
			base.PostRemoved();
			pawn.wishes().ingestibleWishes.Remove(this);
		}
		public override string FormateText(string text)
		{
			text = text.Replace(Def.amount_Key, Def.amountNeeded.ToString());
			return base.FormateText(text);
		}


		public override string DescriptionToFulfill
		{
			get
			{
				string amount = "(" + amountIngested.ToString() + "/" + Def.amountNeeded.ToString() + ")";
				if (Def.specificAmount < Def.amountNeeded)
				{
					string listing = "";
					for (int i = 0; i < ThingsWanted.Count; i++)
					{
						if (thingsIngested[ThingsWanted[i].ingestible] == 0) continue;
						listing += "(" + ThingsWanted[i].ingestible.label + ":" + thingsIngested[ThingsWanted[i].ingestible].ToString() + "/" + ThingsWanted[i].amount.ToString() + ")";
						if (i != ThingsWanted.Count - 1) listing += def.listing_Separator;
					}
					return base.DescriptionToFulfill + "\n" + listing + "\n" + TranslationKey.WISH_INGEST_TOTAL.Translate() + amount;
				}

				return base.DescriptionToFulfill + " " + amount;
			}
		}
	}
}
