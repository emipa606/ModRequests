using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MechhSummonNoRoyalty : Building
	{
		public int RandomNumber(int min, int max)
		{
            return Rand.RangeInclusive(min, max);
        }

        public override IEnumerable<Gizmo> GetGizmos()
		{
			yield return new Command_Action
			{
				defaultLabel = "Summon a Mechanoid Raid",
				defaultDesc = "Summons a few Mechanoids, don't expect them to be on your side",
				icon = ContentFinder<Texture2D>.Get("UI/SummonMech"),
				action = delegate
				{
					StorytellerComp storytellerComp3 = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
					IncidentParms incidentParms = storytellerComp3.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
					incidentParms.faction = Faction.OfMechanoids;
					incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
					IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
				}
			};
			yield return new Command_Action
			{
				defaultLabel = "Summon a Mechanoid Ship Part",
				defaultDesc = "Summons a piece of a Mechanoid Ship",
				icon = ContentFinder<Texture2D>.Get("UI/SummonMech3"),
				action = delegate
				{
					if (RandomNumber(1, 3) == 2)
					{
						StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
						IncidentParms parms = storytellerComp.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
						IncidentDefOf2.DefoliatorShipPartCrash.Worker.TryExecute(parms);
					}
					else
					{
						StorytellerComp storytellerComp2 = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
						IncidentParms parms2 = storytellerComp2.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
						IncidentDefOf2.PsychicEmanatorShipPartCrash.Worker.TryExecute(parms2);
					}
				}
			};
		}
	}
}
