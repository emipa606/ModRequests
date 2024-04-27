using System.Linq;
using Verse;

namespace RimWorld
{
	public class CompTriggerEventOnceDestroyed : ThingComp
	{
		private CompProperties_TriggerEventOnceDestroyed Props => (CompProperties_TriggerEventOnceDestroyed)props;

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
			IncidentParms incidentParms = storytellerComp.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
			incidentParms.faction = Faction.OfMechanoids;
			incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
			incidentParms.faction = Faction.OfMechanoids;
			incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
			IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
			incidentParms.faction = Faction.OfMechanoids;
			incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
			IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
			incidentParms.faction = Faction.OfMechanoids;
			incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
			IncidentDefOfH.DefoliatorShipPartCrash.Worker.TryExecute(incidentParms);
			IncidentDefOfH.PsychicEmanatorShipPartCrash.Worker.TryExecute(incidentParms);
			IncidentDefOfH.ShortCircuit.Worker.TryExecute(incidentParms);
			IncidentDefOfH.CropBlight.Worker.TryExecute(incidentParms);
			IncidentDefOfH.ToxicFallout.Worker.TryExecute(incidentParms);
		}
	}
}
