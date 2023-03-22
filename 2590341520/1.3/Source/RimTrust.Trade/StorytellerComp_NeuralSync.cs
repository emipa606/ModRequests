using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimTrust.Trade
{
	class StorytellerComp_NeuralSync : StorytellerComp
	{
		private StorytellerCompProperties_NeuralSync Props
		{
			get
			{
				return (StorytellerCompProperties_NeuralSync)this.props;
			}
		}

		private float NeuralSyncMTBDays
		{
			get
			{
				float x = (float)Find.TickManager.TicksGame / 3600000f;
				return StorytellerComp_NeuralSync.NeuralSyncMTBDaysCurve.Evaluate(x);
			}
		}
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (Rand.MTBEventOccurs(this.NeuralSyncMTBDays, 60000f, 1000f))	
			{
				Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
				IncidentDef NeuralSync = IncidentDefOf.NeuralSync;
				if (anyPlayerHomeMap != null)
				{
					IncidentParms parms = StorytellerUtility.DefaultParmsNow(this.Props.incident.category, anyPlayerHomeMap);
					if (NeuralSync.Worker.CanFireNow(parms))
					{
						QueuedIncident qi = new QueuedIncident(new FiringIncident(this.Props.incident, this, parms), Find.TickManager.TicksGame + this.Props.delayTicks, 0);
						Find.Storyteller.incidentQueue.Add(qi);
					}
				}
			}
			yield break;
		}
		private static readonly SimpleCurve NeuralSyncMTBDaysCurve = new SimpleCurve
		{
			{
			new CurvePoint(0f, 3f),
			//new CurvePoint(0f, 0.5f),
			true
			},
			{
			new CurvePoint(1f, 3f),
			//new CurvePoint(1f, 0.5f),
			true
			}
		};
		public override string ToString()
		{
			return base.ToString() + " " + this.Props.incident;
		}
	}
}
