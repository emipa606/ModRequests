using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PodSummoner : Building
	{
		public override IEnumerable<Gizmo> GetGizmos()
		{
			yield return new Command_Action
			{
				defaultLabel = "Summon a Meteorite",
				defaultDesc = "Summons a meteorite nearby, watch your head",
				icon = ContentFinder<Texture2D>.Get("UI/SummonBigFatJuicyRock"),
				action = delegate
				{
					StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
					IncidentParms parms = storytellerComp.GenerateParms(IncidentCategoryDefOf.Misc, Find.CurrentMap);
					IncidentDefOf2.MeteoriteImpact.Worker.TryExecute(parms);
				}
			};
		}
	}
}
