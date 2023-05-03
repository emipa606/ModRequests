using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Mashed_BlackPlague
{
    class MentalState_TouchVessel : MentalState
    {
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Thing>(ref this.vessel, "vessel", false);
			Scribe_Values.Look<bool>(ref this.alreadyTouchedVessel, "alreadyTouchedVessel", false, false);
		}

		public override void MentalStateTick()
		{
			if (this.alreadyTouchedVessel)
			{
				base.MentalStateTick();
				return;
			}
			bool flag = false;
			if (this.pawn.IsHashIntervalTick(AnyVesselStillValidCheckInterval) && !VesselObsessionMentalStateUtility.IsVesselValid(this.vessel, this.pawn, false))
			{
				this.vessel = VesselObsessionMentalStateUtility.GetClosestVessel(this.pawn);
				if (this.vessel == null)
				{
					base.RecoverFromState();
					flag = true;
				}
			}
			if (!flag)
			{
				base.MentalStateTick();
			}
		}

		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			this.vessel = VesselObsessionMentalStateUtility.GetClosestVessel(this.pawn);
		}

		public void Notify_VesselTouched()
		{
			this.alreadyTouchedVessel = true;
		}

		public Thing vessel;

		public bool alreadyTouchedVessel;

		private const int AnyVesselStillValidCheckInterval = 500;
	}
}
