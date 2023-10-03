using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ExecutionModByYuno
{

	public class JobDriver_HaulAndStartExecution : JobDriver
	{
	
		private Pawn Takee
			{
				get
				{
					return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
				}
			}
		
		private Building_SpotExecution Spot
			{
				get
				{
					return (Building_SpotExecution)this.job.GetTarget(TargetIndex.B).Thing;
				}
			}

		private IntVec3 cell
		{
			get
			{
				return this.job.GetTarget(TargetIndex.C).Cell;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
			{
				return this.pawn.Reserve(this.Takee, this.job, 1, 1, null, errorOnFailed) && this.pawn.Reserve(Spot,this.job,1,1,null,errorOnFailed);
			}
			
		
		protected override IEnumerable<Toil> MakeNewToils()
			{
			
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.B);
			this.FailOn(() => ((Pawn)((Thing)this.GetActor().CurJob.GetTarget(TargetIndex.A))).guest.interactionMode != PrisonerInteractionModeDefOf.Execution && Spot.currentState != Building_SpotExecution.State.rest);
			this.FailOnDowned(TargetIndex.A);
			this.FailOnAggroMentalState(TargetIndex.A);

			Toil WorkPls = new Toil
			{
				initAction = delegate ()
				{
				
					Spot.StartExecution(pawn, Takee, Spot.Position);
				}
			};
			yield return WorkPls;
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, false);

			Toil TryToIdlePrisoner = new Toil
			{
				initAction = delegate ()
				{

					Job BeIdleIBegYou = new Job(JobDefOf.BeExecuted,Spot,9000);
					this.Takee.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
					this.Takee.jobs.StartJob(BeIdleIBegYou);
					this.Spot.currentState = Building_SpotExecution.State.inMotion;
				}
			};
			yield return TryToIdlePrisoner;
			yield break;
			}


		private void HaulFinished()
		{
			this.Takee.Position = this.cell;
			this.Takee.Notify_Teleported(false, true);
			this.Takee.stances.CancelBusyStanceHard();
		}

	}




	}





