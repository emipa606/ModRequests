using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class CompSpawnerH : ThingComp
	{
		private int ticksUntilSpawn;

		public CompProperties_SpawnerH PropsSpawner => (CompProperties_SpawnerH)props;

		private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;

		public int RandomNumber(int min, int max)
		{
			Random random = new Random();
			return random.Next(min, max);
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				ResetCountdown();
			}
		}

		public override void CompTick()
		{
			TickInterval(1);
		}

		public override void CompTickRare()
		{
			TickInterval(250);
		}

		private void TickInterval(int interval)
		{
			if (!parent.Spawned)
			{
				return;
			}
			CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
			if (comp != null)
			{
				if (!comp.Awake)
				{
					return;
				}
			}
			else if (parent.Position.Fogged(parent.Map))
			{
				return;
			}
			if (!PropsSpawner.requiresPower || PowerOn)
			{
				ticksUntilSpawn -= interval;
				CheckShouldSpawn();
			}
		}

		private void CheckShouldSpawn()
		{
			if (ticksUntilSpawn <= 0)
			{
				TryDoSpawn();
				ResetCountdown();
			}
		}

		public void TryDoSpawn()
		{
			if (RandomNumber(1, 10) == 2)
			{
				StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
				IncidentParms parms = storytellerComp.GenerateParms(IncidentCategoryDefOf.Misc, Find.CurrentMap);
				IncidentDefOfH.RefugeePodCrash.Worker.TryExecute(parms);
			}
			else
			{
				StorytellerComp storytellerComp2 = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
				IncidentParms parms2 = storytellerComp2.GenerateParms(IncidentCategoryDefOf.Misc, Find.CurrentMap);
				IncidentDefOfH.ResourcePodCrash.Worker.TryExecute(parms2);
			}
		}

		private void ResetCountdown()
		{
			ticksUntilSpawn = PropsSpawner.spawnIntervalRange.RandomInRange;
		}

		public override void PostExposeData()
		{
			string text = (PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (PropsSpawner.saveKeysPrefix + "_"));
			Scribe_Values.Look(ref ticksUntilSpawn, text + "ticksUntilSpawn", 0);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEBUG: Spawn ",
					icon = TexCommand.DesirePower,
					action = delegate
					{
						TryDoSpawn();
						ResetCountdown();
					}
				};
			}
		}
	}
}
