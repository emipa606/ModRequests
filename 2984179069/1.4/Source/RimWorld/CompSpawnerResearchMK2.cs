using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class CompSpawnerResearchMK2 : ThingComp
	{
		private static readonly FloatRange RandomResearchPointAmount = new FloatRange(50f, 500f);

		private int ticksUntilSpawn;

		public CompProperties_SpawnerResearchMK2 PropsSpawner => (CompProperties_SpawnerResearchMK2)props;

		protected ResearchProjectDef AddResearchPoints1(float points)
		{
			ResearchManager researchManager = Find.ResearchManager;
			if (!researchManager.AnyProjectIsAvailable)
			{
				return null;
			}
			IEnumerable<ResearchProjectDef> source = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where((ResearchProjectDef x) => x.CanStartNow);
			ResearchProjectDef currentProj = researchManager.currentProj;
			source.TryRandomElementByWeight((ResearchProjectDef x) => 1f / x.baseCost, out var result);
			points = PropsSpawner.researchPointsRange.RandomInRange;
			researchManager.currentProj = result;
			researchManager.ResearchPerformed(points, null);
			researchManager.currentProj = currentProj;
			return result;
		}

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
			if (parent.Spawned && !parent.Position.Fogged(parent.Map))
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
			ResearchProjectDef researchProjectDef = AddResearchPoints1(RandomResearchPointAmount.RandomInRange);
			string text = "Witty response for when there is no research";
			if (researchProjectDef != null)
			{
				text = researchProjectDef.LabelCap;
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
					defaultLabel = "DEBUG: Finish ",
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
