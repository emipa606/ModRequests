using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class CompSpawnerExtraRaidsTwo : ThingComp
	{
		private int ticksUntilSpawn;

		public CompProperties_SpawnerExtraRaidsTwo PropsSpawner => (CompProperties_SpawnerExtraRaidsTwo)props;

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
			CompMAG_ArchotechHibernatable comp = parent.GetComp<CompMAG_ArchotechHibernatable>();
			if (comp != null && comp.State == HibernatableStateDefOf.Starting)
			{
				StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
				IncidentParms incidentParms = storytellerComp.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
				incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
				IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
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
