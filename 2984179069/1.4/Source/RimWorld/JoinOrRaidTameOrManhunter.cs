using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class JoinOrRaidTameOrManhunter : Building
	{
		public int RandomNumber(int min, int max)
		{
			return Rand.RangeInclusive(min, max);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			yield return new Command_Action
			{
				defaultLabel = "Summon a person to join your colony",
				defaultDesc = "Summons a wanderer that will join your colony, small risk of summoning a raid instead",
				icon = ContentFinder<Texture2D>.Get("UI/WandererJoinsOrRaid"),
				action = delegate
				{
					if (RandomNumber(1, 4) == 2)
					{
						StorytellerComp storytellerComp3 = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
						IncidentParms incidentParms = storytellerComp3.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
						incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
						IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
					}
					else
					{
						StorytellerComp storytellerComp4 = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
						IncidentParms parms3 = storytellerComp4.GenerateParms(IncidentCategoryDefOf.Misc, Find.CurrentMap);
						IncidentDefOf2.WandererJoin.Worker.TryExecute(parms3);
					}
				}
			};
			yield return new Command_Action
			{
				defaultLabel = "Tame a random nearby animal",
				defaultDesc = "Tames a random animal on the current map, small risk of attracting a manhunter pack",
				icon = ContentFinder<Texture2D>.Get("UI/TameOrManhunter"),
				action = delegate
				{
					if (RandomNumber(1, 3) == 2)
					{
						StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
						IncidentParms parms = storytellerComp.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
						IncidentDefOf2.ManhunterPack.Worker.TryExecute(parms);
					}
					else
					{
						StorytellerComp storytellerComp2 = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
						IncidentParms parms2 = storytellerComp2.GenerateParms(IncidentCategoryDefOf.Misc, Find.CurrentMap);
						IncidentDefOf2.SelfTame.Worker.TryExecute(parms2);
					}
				}
			};
		}
	}
}
