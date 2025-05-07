using DTechprinting;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace DTechprinting.Comps
{
    public class CompShardable : ThingComp
    {

		private ThingDef cachedShard = null;

		public ThingDef shard
		{
			get
			{
				if (cachedShard == null)
                {
					ResearchProjectDef rpd; 
					if (!Base.thingDic.TryGetValue(this.parent.def, out rpd))
                    {
						return null;
                    }
					cachedShard = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(delegate (ThingDef x)
					{
						CompProperties_Techshard compProperties = x.GetCompProperties<CompProperties_Techshard>();
						return compProperties != null && compProperties.project == rpd && x.defName.Contains("Techshard");
					});
				}
				return cachedShard;
			}
		}

		public bool IsBuilding()
        {
			return (this.parent.def.category == ThingCategory.Building || this.parent.def.building != null);
        }


		public virtual CompProperties_Shardable Props
		{
			get
			{
				return this.props as CompProperties_Shardable;
			}
		}	

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
			JobFailReason.Clear();
			if (this.parent.def.building != null || this.parent.def.category == ThingCategory.Building || shard == null || !Base.ThingIsPrintable(this.parent.def))
				yield break;

			if (this.parent.IsForbidden(selPawn))
            {
				JobFailReason.Is("CannotPrioritizeForbidden".Translate(this.parent.Label, this.parent));
			}
			else if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
				JobFailReason.Is("Incapable".Translate()); 
			}
			else if (selPawn.WorkTypeIsDisabled(Base.DefOf.DTechprinting) || selPawn.WorkTagIsDisabled(WorkTags.Intellectual))
            {
				JobFailReason.Is("CannotPrioritizeWorkTypeDisabled".Translate(Base.DefOf.DTechprinting.gerundLabel));
			}
			else if (selPawn.workSettings.GetPriority(Base.DefOf.DTechprinting) == 0)
            {
				JobFailReason.Is("CannotPrioritizeNotAssignedToWorkType".Translate(Base.DefOf.DTechprinting.gerundLabel));
            }
			else if (!selPawn.CanReach(this.parent, PathEndMode.ClosestTouch, Danger.Some, false, TraverseMode.ByPawn))
			{
				JobFailReason.Is("CannotReach".Translate(), null);
			}
			else if (!selPawn.CanReserve(this.parent, 1, -1, null, false))
			{
				Pawn pawn = selPawn.Map.reservationManager.FirstRespectedReserver(this.parent, selPawn);
				if (pawn == null)
				{
					pawn = selPawn.Map.physicalInteractionReservationManager.FirstReserverOf(selPawn);
				}
				if (pawn != null)
				{
					JobFailReason.Is("ReservedBy".Translate(pawn.LabelShort, pawn), null);
				}
				else
				{
					JobFailReason.Is("Reserved".Translate(), null);
				}
			}
			int minAmountToGrab = Base.IsSingleAllowed(this.parent.def) ? 1 : Base.IsStackAllowed(this.parent.def, ShardMaker.stackSize) ? ShardMaker.stackSize : 0;
			if (minAmountToGrab == 0 || this.parent.stackCount < minAmountToGrab)
            {
				JobFailReason.Is("notEnoughToShard".Translate(), null);

			}

			if (JobFailReason.HaveReason)
			{
				yield return new FloatMenuOption("CannotGenericWorkCustom".Translate("techprint") + ": " + JobFailReason.Reason.CapitalizeFirst(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
				JobFailReason.Clear();
				yield break;
			}

			HaulAIUtility.PawnCanAutomaticallyHaul(selPawn, this.parent, true);
			Thing techPrinter = GenClosest.ClosestThingReachable(selPawn.Position, selPawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
				PathEndMode.InteractionCell, TraverseParms.For(selPawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, 
				(Thing thing) => thing.def == Base.DefOf.DTechprinter && selPawn.CanReserve(thing, 1, -1, null, false));

			Job job = null;

			yield return new FloatMenuOption("turnIntoShards".Translate(ShardMaker.CalcNumShardsFor(this.parent), shard.LabelCap, this.parent.LabelCap).CapitalizeFirst(), delegate ()
			{
				if (techPrinter != null)
				{
					IBillGiver billGiver = techPrinter as IBillGiver;
					if (billGiver == null)
					{
						Log.Error("Techprinter isn't bill giver");
						return;
					}
					else
					{
						job = MakeShardingJob(selPawn, billGiver, minAmountToGrab);
					}
				}

				if (job == null)
				{
					Messages.Message("noTechprinterAvailable".Translate(), MessageTypeDefOf.RejectInput, true);
					return;
				}
				selPawn.jobs.TryTakeOrderedJob(job, JobTag.MiscWork);
			}, MenuOptionPriority.VeryLow, null, null, 0f, null, null);
			yield break;
		}

		public Job MakeShardingJob(Pawn selPawn, IBillGiver billGiver, int count)
        {
			var bills = billGiver.BillStack.Bills;
			Bill workBill = null;
			foreach (var bill in bills)
			{
				if (!bill.suspended && (bill.PawnAllowedToStartAnew(selPawn)) && bill.ingredientFilter.Allows(this.parent))
				{
					Log.Message(bill.ingredientFilter.AllowedThingDefs.ToStringSafeEnumerable());
					workBill = bill;
					break;
				}
			}
			if (workBill == null) // this techprinter didn't have a suitable bill, so add one I guess
			{
				Bill newBill;
				if (count == 1)
					newBill = BillUtility.MakeNewBill(Base.DefOf.DTechprintingRecipe);
				else
					newBill = BillUtility.MakeNewBill(Base.DefOf.DTechprintingStackRecipe);
				newBill.ingredientFilter.SetDisallowAll();
				newBill.ingredientFilter.SetAllow(this.parent.def, true);

				billGiver.BillStack.AddBill(newBill);
				workBill = newBill;
			}
			ThingCount tc = new ThingCount(this.parent, count);
			Job haulOff;
			return WorkGiver_DoBill.TryStartNewDoBillJob(selPawn, workBill, billGiver, new List<ThingCount> { tc }, out haulOff, false);
		}

	}
}
