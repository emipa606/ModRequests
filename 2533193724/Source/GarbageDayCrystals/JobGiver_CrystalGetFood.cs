using System;
using Crystosentient.Dictionary;
using RimWorld;
using Verse;
using Verse.AI;

namespace Crystosentient
{
	// Token: 0x0200000F RID: 15
	public class JobGiver_CrystalGetFood : ThinkNode_JobGiver
	{
		// Token: 0x0600005B RID: 91 RVA: 0x00003E84 File Offset: 0x00002084
		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_CrystalGetFood jobGiver_CrystalGetFood = (JobGiver_CrystalGetFood)base.DeepCopy(resolve);
			jobGiver_CrystalGetFood.minCategory = this.minCategory;
			jobGiver_CrystalGetFood.maxLevelPercentage = this.maxLevelPercentage;
			jobGiver_CrystalGetFood.forceScanWholeMap = this.forceScanWholeMap;
			return jobGiver_CrystalGetFood;
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00003EC8 File Offset: 0x000020C8
		public override float GetPriority(Pawn pawn)
		{
			Need_Food food = pawn.needs.food;
			bool flag = food == null;
			float result;
			if (flag)
			{
				result = 0f;
			}
			else
			{
				bool flag2 = pawn.needs.food.CurCategory < HungerCategory.Starving && FoodUtility.ShouldBeFedBySomeone(pawn);
				if (flag2)
				{
					result = 0f;
				}
				else
				{
					bool flag3 = food.CurCategory < this.minCategory;
					if (flag3)
					{
						result = 0f;
					}
					else
					{
						bool flag4 = food.CurLevelPercentage > this.maxLevelPercentage;
						if (flag4)
						{
							result = 0f;
						}
						else
						{
							bool flag5 = food.CurLevelPercentage < pawn.RaceProps.FoodLevelPercentageWantEat;
							if (flag5)
							{
								result = 9.5f;
							}
							else
							{
								result = 0f;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00003F84 File Offset: 0x00002184
		protected override Job TryGiveJob(Pawn pawn)
		{
			Need_Food food = pawn.needs.food;
			bool flag = food == null || food.CurCategory < this.minCategory || food.CurLevelPercentage > this.maxLevelPercentage;
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = pawn.AnimalOrWildMan();
				bool allowCorpse;
				if (flag2)
				{
					allowCorpse = true;
				}
				else
				{
					Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition, false);
					allowCorpse = (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.4f);
				}
				bool desperate = pawn.needs.food.CurCategory == HungerCategory.Starving;
				Thing thing;
				ThingDef thingDef;
				bool flag3 = !FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out thing, out thingDef, true, true, false, allowCorpse, false, pawn.IsWildMan(), this.forceScanWholeMap, false, false, FoodPreferability.Undefined);
				if (flag3)
				{
					result = null;
				}
				else
				{
					Pawn pawn2 = thing as Pawn;
					bool flag4 = pawn2 != null;
					if (flag4)
					{
						Job job = JobMaker.MakeJob(JobDefOf.PredatorHunt, pawn2);
						job.killIncappedTarget = true;
						result = job;
					}
					else
					{
						bool flag5 = thing is Plant && thing.def.plant.harvestedThingDef == thingDef;
						if (flag5)
						{
							result = JobMaker.MakeJob(JobDefOf.Harvest, thing);
						}
						else
						{
							Building_NutrientPasteDispenser building_NutrientPasteDispenser = thing as Building_NutrientPasteDispenser;
							bool flag6 = building_NutrientPasteDispenser != null && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers();
							if (flag6)
							{
								Building building = building_NutrientPasteDispenser.AdjacentReachableHopper(pawn);
								bool flag7 = building != null;
								if (flag7)
								{
									ISlotGroupParent hopperSgp = building as ISlotGroupParent;
									Job job2 = WorkGiver_CookFillHopper.HopperFillFoodJob(pawn, hopperSgp);
									bool flag8 = job2 != null;
									if (flag8)
									{
										return job2;
									}
								}
								thing = FoodUtility.BestFoodSourceOnMap(pawn, pawn, desperate, out thingDef, FoodPreferability.MealLavish, false, !pawn.IsTeetotaler(), false, false, false, false, false, false, this.forceScanWholeMap, false, false, FoodPreferability.Undefined, null);
								bool flag9 = thing == null;
								if (flag9)
								{
									return null;
								}
							}
							float nutrition = FoodUtility.GetNutrition(thing, thingDef);
							Job job3 = JobMaker.MakeJob(DefOfJob.GDCRYST_JOB_Ingest, thing);
							job3.count = FoodUtility.WillIngestStackCountOf(pawn, thingDef, nutrition);
							result = job3;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x04000031 RID: 49
		private HungerCategory minCategory;

		// Token: 0x04000032 RID: 50
		private float maxLevelPercentage = 1f;

		// Token: 0x04000033 RID: 51
		public bool forceScanWholeMap;
	}
}
