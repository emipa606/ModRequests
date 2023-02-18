using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Crystosentient
{
	// Token: 0x02000006 RID: 6
	public class JobDriver_CrystalIngest : JobDriver
	{
		// Token: 0x0600001C RID: 28 RVA: 0x000021BC File Offset: 0x000003BC
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.usingNutrientPasteDispenser, "usingNutrientPasteDispenser", false, false);
			Scribe_Values.Look<bool>(ref this.eatingFromInventory, "eatingFromInventory", false, false);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002DF0 File Offset: 0x00000FF0
		public override string GetReport()
		{
			bool flag = this.usingNutrientPasteDispenser;
			bool flag2 = flag;
			string result;
			if (flag2)
			{
				result = JobUtility.GetResolvedJobReportRaw(this.job.def.reportString, ThingDefOf.MealNutrientPaste.label, ThingDefOf.MealNutrientPaste, "", "", "", "");
			}
			else
			{
				Thing thing = this.job.targetA.Thing;
				bool flag3 = thing != null && thing.def.ingestible != null;
				bool flag4 = flag3;
				if (flag4)
				{
					bool flag5 = !thing.def.ingestible.ingestReportStringEat.NullOrEmpty() && (thing.def.ingestible.ingestReportString.NullOrEmpty() || this.pawn.RaceProps.intelligence < Intelligence.ToolUser);
					bool flag6 = flag5;
					if (flag6)
					{
						return thing.def.ingestible.ingestReportStringEat.Formatted(this.job.targetA.Thing.LabelShort, this.job.targetA.Thing);
					}
					bool flag7 = !thing.def.ingestible.ingestReportString.NullOrEmpty();
					bool flag8 = flag7;
					if (flag8)
					{
						return thing.def.ingestible.ingestReportString.Formatted(this.job.targetA.Thing.LabelShort, this.job.targetA.Thing);
					}
				}
				result = base.GetReport();
			}
			return result;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002FA0 File Offset: 0x000011A0
		public override void Notify_Starting()
		{
			base.Notify_Starting();
			this.usingNutrientPasteDispenser = (this.IngestibleSource is Building_NutrientPasteDispenser);
			this.eatingFromInventory = (this.pawn.inventory != null && this.pawn.inventory.Contains(this.IngestibleSource));
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002FF8 File Offset: 0x000011F8
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			bool flag = this.pawn.Faction != null && !(this.IngestibleSource is Building_NutrientPasteDispenser);
			bool flag2 = flag;
			if (flag2)
			{
				Thing ingestibleSource = this.IngestibleSource;
				bool flag3 = !this.pawn.Reserve(ingestibleSource, this.job, 10, FoodUtility.GetMaxAmountToPickup(ingestibleSource, this.pawn, this.job.count), null, errorOnFailed);
				bool flag4 = flag3;
				if (flag4)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x000021EC File Offset: 0x000003EC
		protected override IEnumerable<Toil> MakeNewToils()
		{
			bool flag = !this.usingNutrientPasteDispenser;
			bool flag2 = flag;
			if (flag2)
			{
				this.FailOn(() => !this.IngestibleSource.Destroyed && !this.IngestibleSource.IngestibleNow);
			}
			Toil chew = Toils_Ingest.ChewIngestible(this.pawn, this.ChewDurationMultiplier, TargetIndex.A, TargetIndex.B).FailOn((Toil x) => !this.IngestibleSource.Spawned && (this.pawn.carryTracker == null || this.pawn.carryTracker.CarriedThing != this.IngestibleSource)).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			foreach (Toil toil in this.PrepareToIngestToils(chew))
			{
				yield return toil;
			}
			IEnumerator<Toil> enumerator = null;
			yield return chew;
			yield return Toils_Ingest.FinalizeIngest(this.pawn, TargetIndex.A);
			yield return Toils_Jump.JumpIf(chew, () => this.job.GetTarget(TargetIndex.A).Thing is Corpse && this.pawn.needs.food.CurLevelPercentage < 0.9f);
			yield break;
			yield break;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00003080 File Offset: 0x00001280
		private IEnumerable<Toil> PrepareToIngestToils(Toil chewToil)
		{
			return this.PrepareToIngestToils_NonToolUser();
		}

		// Token: 0x06000022 RID: 34 RVA: 0x000021FC File Offset: 0x000003FC
		private IEnumerable<Toil> PrepareToIngestToils_Dispenser()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, this.pawn);
			yield return Toils_Ingest.CarryIngestibleToChewSpot(this.pawn, TargetIndex.A).FailOnDestroyedNullOrForbidden(TargetIndex.A);
			yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
			yield break;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x0000220C File Offset: 0x0000040C
		private IEnumerable<Toil> PrepareToIngestToils_NonToolUser()
		{
			yield return this.ReserveFood();
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield break;
		}

		// Token: 0x06000024 RID: 36 RVA: 0x0000221C File Offset: 0x0000041C
		private IEnumerable<Toil> TakeExtraIngestibles()
		{
			bool flag = !this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
			bool flag2 = flag;
			if (flag2)
			{
				yield break;
			}
			Toil reserveExtraFoodToCollect = Toils_Ingest.ReserveFoodFromStackForIngesting(TargetIndex.C, null);
			Toil findExtraFoodToCollect = new Toil
			{
				initAction = delegate()
				{
					bool flag3 = this.pawn.inventory.innerContainer.TotalStackCountOfDef(this.IngestibleSource.def) < this.job.takeExtraIngestibles;
					bool flag4 = flag3;
					if (flag4)
					{
						Thing thing = GenClosest.ClosestThingReachable(this.pawn.Position, this.pawn.Map, ThingRequest.ForDef(this.IngestibleSource.def), PathEndMode.Touch, TraverseParms.For(this.pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), 30f, (Thing x) => this.pawn.CanReserve(x, 10, 1, null, false) && !x.IsForbidden(this.pawn) && x.IsSociallyProper(this.pawn), null, 0, -1, false, RegionType.Normal | RegionType.Portal, false);
						bool flag5 = thing != null;
						bool flag6 = flag5;
						if (flag6)
						{
							this.job.SetTarget(TargetIndex.C, thing);
							this.JumpToToil(reserveExtraFoodToCollect);
						}
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield return Toils_Jump.Jump(findExtraFoodToCollect);
			yield return reserveExtraFoodToCollect;
			yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
			yield return Toils_Haul.TakeToInventory(TargetIndex.C, () => this.job.takeExtraIngestibles - this.pawn.inventory.innerContainer.TotalStackCountOfDef(this.IngestibleSource.def));
			yield return findExtraFoodToCollect;
			yield break;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00003098 File Offset: 0x00001298
		private Toil ReserveFood()
		{
			return new Toil
			{
				initAction = delegate()
				{
					bool flag = this.pawn.Faction != null;
					bool flag2 = flag;
					if (flag2)
					{
						Thing thing = this.job.GetTarget(TargetIndex.A).Thing;
						bool flag3 = this.pawn.carryTracker.CarriedThing != thing;
						bool flag4 = flag3;
						if (flag4)
						{
							int maxAmountToPickup = FoodUtility.GetMaxAmountToPickup(thing, this.pawn, this.job.count);
							bool flag5 = maxAmountToPickup != 0;
							bool flag6 = flag5;
							if (flag6)
							{
								bool flag7 = !this.pawn.Reserve(thing, this.job, 10, maxAmountToPickup, null, true);
								bool flag8 = flag7;
								if (flag8)
								{
									Log.Error(string.Concat(new object[]
									{
										"Pawn food reservation for ",
										this.pawn,
										" on job ",
										this,
										" failed, because it could not register food from ",
										thing,
										" - amount: ",
										maxAmountToPickup
									}), false);
									this.pawn.jobs.EndCurrentJob(JobCondition.Errored, true, true);
								}
								this.job.count = maxAmountToPickup;
							}
						}
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}

		// Token: 0x06000026 RID: 38 RVA: 0x000030D0 File Offset: 0x000012D0
		public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
		{
			IntVec3 cell = this.job.GetTarget(TargetIndex.B).Cell;
			return JobDriver_CrystalIngest.ModifyCarriedThingDrawPosWorker(ref drawPos, ref behind, ref flip, cell, this.pawn);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00003108 File Offset: 0x00001308
		public static bool ModifyCarriedThingDrawPosWorker(ref Vector3 drawPos, ref bool behind, ref bool flip, IntVec3 placeCell, Pawn pawn)
		{
			bool moving = pawn.pather.Moving;
			bool flag = moving;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				Thing carriedThing = pawn.carryTracker.CarriedThing;
				bool flag2 = carriedThing == null || !carriedThing.IngestibleNow;
				bool flag3 = flag2;
				if (flag3)
				{
					result = false;
				}
				else
				{
					bool flag4 = placeCell.IsValid && placeCell.AdjacentToCardinal(pawn.Position) && placeCell.HasEatSurface(pawn.Map) && carriedThing.def.ingestible.ingestHoldUsesTable;
					bool flag5 = flag4;
					if (flag5)
					{
						drawPos = new Vector3((float)placeCell.x + 0.5f, drawPos.y, (float)placeCell.z + 0.5f);
						result = true;
					}
					else
					{
						bool flag6 = carriedThing.def.ingestible.ingestHoldOffsetStanding != null;
						bool flag7 = flag6;
						if (flag7)
						{
							HoldOffset holdOffset = carriedThing.def.ingestible.ingestHoldOffsetStanding.Pick(pawn.Rotation);
							bool flag8 = holdOffset != null;
							bool flag9 = flag8;
							if (flag9)
							{
								drawPos += holdOffset.offset;
								behind = holdOffset.behind;
								flip = holdOffset.flip;
								return true;
							}
						}
						result = false;
					}
				}
			}
			return result;
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000028 RID: 40 RVA: 0x00003264 File Offset: 0x00001464
		private Thing IngestibleSource
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000029 RID: 41 RVA: 0x0000328C File Offset: 0x0000148C
		private float ChewDurationMultiplier
		{
			get
			{
				Thing ingestibleSource = this.IngestibleSource;
				bool flag = ingestibleSource.def.ingestible != null && !ingestibleSource.def.ingestible.useEatingSpeedStat;
				bool flag2 = flag;
				float result;
				if (flag2)
				{
					result = 1f;
				}
				else
				{
					result = 1f / this.pawn.GetStatValue(StatDefOf.EatingSpeed, true);
				}
				return result;
			}
		}

		// Token: 0x04000011 RID: 17
		private bool usingNutrientPasteDispenser;

		// Token: 0x04000012 RID: 18
		private bool eatingFromInventory;

		// Token: 0x04000013 RID: 19
		public const float EatCorpseBodyPartsUntilFoodLevelPct = 0.9f;

		// Token: 0x04000014 RID: 20
		public const TargetIndex IngestibleSourceInd = TargetIndex.A;

		// Token: 0x04000015 RID: 21
		private const TargetIndex TableCellInd = TargetIndex.B;

		// Token: 0x04000016 RID: 22
		private const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;
	}
}
