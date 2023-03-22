using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimTrust.Trade
{
	class StorytellerComp_InterestDrop : StorytellerComp
	{
		private StorytellerCompProperties_InterestDrop Props
		{
			get
			{
				return (StorytellerCompProperties_InterestDrop)this.props;
			}
		}

		private float InterestDropMTBDays
		{
			get
			{
				float x = (float)Find.TickManager.TicksGame / 3600000f;
				return StorytellerComp_InterestDrop.InterestDropMTBDaysCurve.Evaluate(x);
			}
		}
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (Rand.MTBEventOccurs(this.InterestDropMTBDays, 60000f, 1000f))	//change check duration from 1000 -> 180000 -> 100 -> 800 -> 1000
			{
				Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
				IncidentDef InterestDrop = IncidentDefOf.InterestDrop;
				if (anyPlayerHomeMap != null)
				{
					IncidentParms parms = StorytellerUtility.DefaultParmsNow(this.Props.incident.category, anyPlayerHomeMap);
					if (InterestDrop.Worker.CanFireNow(parms))
					{
						QueuedIncident qi = new QueuedIncident(new FiringIncident(this.Props.incident, this, parms), Find.TickManager.TicksGame + this.Props.delayTicks, 0);
						Find.Storyteller.incidentQueue.Add(qi);
					}
				}
			}
			yield break;
		}
		private static readonly SimpleCurve InterestDropMTBDaysCurve = new SimpleCurve
		{
			{
			new CurvePoint(0f, 15f),
			true
			},
			{
			new CurvePoint(1f, 15f),
			true
			}
		};
		public override string ToString()
		{
			return base.ToString() + " " + this.Props.incident;
		}
	}
}
