using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ExecutionModByYuno
{


		public class JobDriver_ModdedExecution : JobDriver
		{
	
			protected Pawn Victim
			{
				get
				{
					return (Pawn)this.job.targetA.Thing;
				}
			}
			
			public Building_SpotExecution building
			{
				get
				{
					return (Building_SpotExecution)this.job.targetB.Thing;
				}
			}

			public override bool TryMakePreToilReservations(bool errorOnFailed)
			{
				return this.pawn.Reserve(this.Victim, this.job, 1, -1, null, errorOnFailed);
			}


			protected override IEnumerable<Toil> MakeNewToils()
			{
				this.FailOnAggroMentalState(TargetIndex.A);
				yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Victim, PrisonerInteractionModeDefOf.NoInteraction).FailOn(() => !this.Victim.IsPrisonerOfColony || !this.Victim.guest.PrisonerIsSecure);
				Toil execute = new Toil();
				execute.initAction = delegate ()
				{
					ExecutionUtility.DoExecutionByCut(execute.actor, this.Victim);
					ExecutionThoughtGiverUtility.GiveThoughtsForPawnExecuted(this.Victim, PawnExecutionKind.GenericBrutal);
					TaleRecorder.RecordTale(TaleDefOf.ExecutedPrisoner, new object[]
					{
					this.pawn,
					this.Victim
					});
					this.building.currentState = Building_SpotExecution.State.rest;
				};
				execute.defaultCompleteMode = ToilCompleteMode.Instant;
				yield return execute;
				yield break;
			}
		}
	}


