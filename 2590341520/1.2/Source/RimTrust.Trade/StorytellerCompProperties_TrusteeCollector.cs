using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace RimTrust.Trade
{
    class StorytellerCompProperties_TrusteeCollector : StorytellerCompProperties
	{
		public StorytellerCompProperties_TrusteeCollector()
		{
			this.compClass = typeof(StorytellerComp_TrusteeCollector);
		}

		public IncidentDef incident = new IncidentDef();

		public int delayTicks = 60;
	}
}
