using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace Necro
{
	
	public class StorytellerComp_NecroStuff : StorytellerComp
	{
		
		
		public StorytellerCompProperties_NecroStuff Props
		{
			get
			{
				return (StorytellerCompProperties_NecroStuff)this.props;
			}
		}

		
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (Find.TickManager.TicksGame < 600000)
			{
				yield break;
			}
			if (this.Props.incident == null || !this.Props.incident.TargetAllowed(target))
			{
				yield break;
			}
			if (Rand.MTBEventOccurs(this.Props.baseMtbDays, 60000f, 1000f))
			{
				IncidentParms parms = this.GenerateParms(this.Props.incident.category, target);
				if (this.Props.incident.Worker.CanFireNow(parms))
				{
					yield return new FiringIncident(this.Props.incident, this, parms);
				}
			}
			yield break;
		}
	}
}
