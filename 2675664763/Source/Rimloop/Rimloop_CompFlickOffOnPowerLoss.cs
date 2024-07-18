using System;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimloopMod
{
    public class CompFlickOffOnPowerLoss : ThingComp
    {
		CompFlickable flickableComp;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			this.flickableComp = this.parent.GetComp<CompFlickable>();
		}

		public override void ReceiveCompSignal(string signal)
		{
			if (signal == "PowerTurnedOff" && flickableComp.SwitchIsOn)
			{
				flickableComp.DoFlick();
				FlickUtility.UpdateFlickDesignation(flickableComp.parent);
			}
		}
	}

	public class CompProperties_FlickOffOnPowerLoss : CompProperties
	{
		public CompProperties_FlickOffOnPowerLoss()
		{
			this.compClass = typeof(CompFlickOffOnPowerLoss);
		}
	}
}
