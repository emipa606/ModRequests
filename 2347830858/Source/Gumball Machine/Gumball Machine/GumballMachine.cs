using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Gumball
{
	public class Building_GumballMachine : Building
	{
		List<ThingDef> gumballs = new List<ThingDef> { ThingDefOf.RedGumball, ThingDefOf.BlueGumball, ThingDefOf.GreenGumball, ThingDefOf.YellowGumball, ThingDefOf.PurpleGumball };

		public static int CollectDuration = 50;

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			
			Pawn myPawn2 = myPawn;
			Building_GumballMachine building_GumballMachine = this;
			this.TryGetComp<CompRefuelable>().Props.fuelCapacity = GumballMachineSettings.silverCost * 5;

			Action action = delegate
			{
				Job job = JobMaker.MakeJob(JobDefOf.getGumball, building_GumballMachine);
				myPawn2.jobs.TryTakeOrderedJob(job);
			};
			
			if (!(this.TryGetComp<CompRefuelable>().Fuel >= 5))
			{
				yield return new FloatMenuOption("CannotUseReason".Translate("GumballNotEnoughSilver".Translate()), null);
				yield break;
			}
            else
            {
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GetGumball".Translate(), action), myPawn2, this);
			}
			
		}

		public virtual Thing TryDispenseGumball()
		{
			ThingDef randomGumball = RandomGumball();

			Thing thing = ThingMaker.MakeThing(randomGumball);
			this.TryGetComp<CompRefuelable>().ConsumeFuel(GumballMachineSettings.silverCost);
			return thing;
		}

		private ThingDef RandomGumball()
        {
			int r = Rand.Range(0, gumballs.Count);
			ThingDef randomGumball = gumballs[r];
			return randomGumball;

		}

	}
	public static class JobDefOf
	{
		public static JobDef getGumball = DefDatabase<JobDef>.GetNamed("GetGumball");
		
	}

	public class JobDriver_GetGumball : JobDriver
	{
		public const TargetIndex GumballMachineInd = TargetIndex.A;
		protected Thing GumballMachine => job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return Toils_Gumball.TakeGumballFromMachine(TargetIndex.A, pawn);
		}
	}

	public class Toils_Gumball
	{
		public static Toil TakeGumballFromMachine(TargetIndex ind, Pawn eater)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Thing thing = ((Building_GumballMachine)actor.jobs.curJob.GetTarget(ind).Thing).TryDispenseGumball();
				if (thing == null)
				{
					actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
				}
				else
				{
					actor.carryTracker.TryStartCarry(thing);
					actor.CurJob.SetTarget(ind, actor.carryTracker.CarriedThing);
				}
			};
			toil.FailOnCannotTouch(ind, PathEndMode.Touch);
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = Building_GumballMachine.CollectDuration;
			return toil;
		}

	}
}
