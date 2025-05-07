using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using UnityEngine;

namespace DTechprinting
{
    class CompTechshard : CompTechprint
    {
		public new CompProperties_Techprint Props
		{
			get
			{
				return (CompProperties_Techprint)this.props;
			}
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
			JobFailReason.Clear();
			if (selPawn.WorkTypeIsDisabled(WorkTypeDefOf.Research) || selPawn.WorkTagIsDisabled(WorkTags.Intellectual))
			{
				JobFailReason.Is("WillNever".Translate("Research".TranslateSimple().UncapitalizeFirst()), null);
			}
			else if 
				(this.Props.project.TechprintRequirementMet 
					&&	(!TechprintingSettings.enableUnlockedTechPrinting 
						||	(ShardApplier.FindBestProjectToAdvance(this.Props.project) == null
							&& ShardApplier.FindBestProjectToUnlock(this.Props.project) == null)
						)
				)
			{
				JobFailReason.Is("NoResearchToApply".Translate(), null);
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
			HaulAIUtility.PawnCanAutomaticallyHaul(selPawn, this.parent, true);
			Thing thing2 = GenClosest.ClosestThingReachable(selPawn.Position, selPawn.Map, ThingRequest.ForGroup(ThingRequestGroup.ResearchBench), PathEndMode.InteractionCell, TraverseParms.For(selPawn, Danger.Some, TraverseMode.ByPawn, false), 9999f, (Thing thing) => thing is Building_ResearchBench && selPawn.CanReserve(thing, 1, -1, null, false), null, 0, -1, false, RegionType.Set_Passable, false);
			Job job = null;
			if (thing2 != null && this.parent.stackCount > 0)
			{

				job = JobMaker.MakeJob(Base.DefOf.ApplyTechshards);
				job.targetA = thing2;
				job.targetB = this.parent;
				int count = this.Props.project.techprintCount - Current.Game.researchManager.GetTechprints(this.Props.project);
				if (count > 0)
					job.count = Math.Min(this.parent.stackCount, count);
				else
					job.count = this.parent.stackCount;
				job.targetC = thing2.Position;
			}
			if (JobFailReason.HaveReason)
			{
				yield return new FloatMenuOption("CannotGenericWorkCustom".Translate("ApplyTechshard".Translate(this.parent.Label)) + ": " + JobFailReason.Reason.CapitalizeFirst(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
				JobFailReason.Clear();
			}
			else
			{
				yield return new FloatMenuOption("ApplyTechshard".Translate(this.parent.Label).CapitalizeFirst(), delegate ()
				{
					if (job == null)
					{
						Messages.Message("MessageNoResearchBenchForTechshard".Translate(), MessageTypeDefOf.RejectInput, true);
						return;
					}
					selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
			}
			yield break;
		
		}
    }
}
