using System;
using Verse;
using RimWorld;
using Verse.AI;

namespace HLVR
{
	public class CompInitialManhunter : ThingComp
	{
		public override void CompTick()
		{
			base.CompTick();
			if (this.addManhunterOnce)
			{
				(this.parent as Pawn).mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
				this.addManhunterOnce = false;
			}
		}

		private bool addManhunterOnce = true;
	}
}