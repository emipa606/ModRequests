using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
/*
namespace Entomophagy
{
    class JobDriver_ForageForBugs : JobDriver
    {
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			Action swimTick = delegate ()
			{
				JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.GoToNextToil, 1f, null);
			};
			Toil treadWaterToil = Toils_General.Wait(job.def.joyDuration / 3, TargetIndex.C);
			treadWaterToil.tickAction = swimTick;

			//Swim to first spot
			Toil firstSwimToil = Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			firstSwimToil.tickAction = swimTick;
			firstSwimToil.FailOn(() => this.pawn.Position.GetTerrain(this.pawn.Map) == TerrainDef.Named("Marsh"));
			yield return firstSwimToil;
			yield return treadWaterToil;
			//Swim to second spot
			Toil secondSwimToil = Toils_Goto.GotoCell(TargetIndex.C, PathEndMode.OnCell);
			secondSwimToil.tickAction = swimTick;
			secondSwimToil.FailOn(() => this.pawn.Position.GetTerrain(this.pawn.Map) == TerrainDef.Named("Marsh"));
			yield return secondSwimToil;
			yield return treadWaterToil;
			//Swim back to first spot
			yield return firstSwimToil;
			yield return treadWaterToil;
			//Return to shore
			Toil shoreReturnToil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			shoreReturnToil.tickAction = delegate ()
			{
				JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.None, 1f, null);
			};
			yield return shoreReturnToil;
			yield break;
		}

		public override string GetReport()
		{
			return "Foraging for bugs".Translate();
		}
	}
}
*/