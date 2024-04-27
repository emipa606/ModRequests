using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class CompSpawnerResearch : ThingComp
	{
		private static readonly FloatRange RandomResearchPointAmount = new FloatRange(50f, 500f);

		private int ticksUntilSpawn;

		public CompProperties_SpawnerResearch PropsSpawner => (CompProperties_SpawnerResearch)props;

		private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;

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
			points = 50000f;
			researchManager.currentProj = result;
			researchManager.ResearchPerformed(points, null);
			researchManager.currentProj = currentProj;
			return result;
		}

		protected ResearchProjectDef AddResearchPoints2(float points)
		{
			ResearchManager researchManager = Find.ResearchManager;
			if (!researchManager.AnyProjectIsAvailable)
			{
				return null;
			}
			IEnumerable<ResearchProjectDef> source = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where((ResearchProjectDef x) => x.CanStartNow);
			ResearchProjectDef currentProj = researchManager.currentProj;
			source.TryRandomElementByWeight((ResearchProjectDef x) => 1f / x.baseCost, out var result);
			points = 5000f;
			researchManager.currentProj = result;
			researchManager.ResearchPerformed(points, null);
			researchManager.currentProj = currentProj;
			return result;
		}

		protected ResearchProjectDef AddResearchPoints3(float points)
		{
			ResearchManager researchManager = Find.ResearchManager;
			if (!researchManager.AnyProjectIsAvailable)
			{
				return null;
			}
			IEnumerable<ResearchProjectDef> source = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where((ResearchProjectDef x) => x.CanStartNow);
			ResearchProjectDef currentProj = researchManager.currentProj;
			source.TryRandomElementByWeight((ResearchProjectDef x) => 1f / x.baseCost, out var result);
			points = 500f;
			researchManager.currentProj = result;
			researchManager.ResearchPerformed(points, null);
			researchManager.currentProj = currentProj;
			return result;
		}

		protected ResearchProjectDef AddResearchPoints4(float points)
		{
			ResearchManager researchManager = Find.ResearchManager;
			if (!researchManager.AnyProjectIsAvailable)
			{
				return null;
			}
			IEnumerable<ResearchProjectDef> source = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where((ResearchProjectDef x) => x.CanStartNow);
			ResearchProjectDef currentProj = researchManager.currentProj;
			source.TryRandomElementByWeight((ResearchProjectDef x) => 1f / x.baseCost, out var result);
			points = -5000f;
			researchManager.currentProj = result;
			researchManager.ResearchPerformed(points, null);
			researchManager.currentProj = currentProj;
			return result;
		}

		protected ResearchProjectDef AddResearchPoints5(float points)
		{
			ResearchManager researchManager = Find.ResearchManager;
			if (!researchManager.AnyProjectIsAvailable)
			{
				return null;
			}
			IEnumerable<ResearchProjectDef> source = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where((ResearchProjectDef x) => x.CanStartNow);
			ResearchProjectDef currentProj = researchManager.currentProj;
			source.TryRandomElementByWeight((ResearchProjectDef x) => 1f / x.baseCost, out var result);
			points = -500f;
			researchManager.currentProj = result;
			researchManager.ResearchPerformed(points, null);
			researchManager.currentProj = currentProj;
			return result;
		}

		protected ResearchProjectDef AddResearchPoints6(float points)
		{
			ResearchManager researchManager = Find.ResearchManager;
			if (!researchManager.AnyProjectIsAvailable)
			{
				return null;
			}
			IEnumerable<ResearchProjectDef> source = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where((ResearchProjectDef x) => x.CanStartNow);
			ResearchProjectDef currentProj = researchManager.currentProj;
			source.TryRandomElementByWeight((ResearchProjectDef x) => 1f / x.baseCost, out var result);
			points = 100f;
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
			if (RandomNumber(1, 5) != 2)
			{
				bool flag = RandomNumber(1, 3) == 2;
				if (flag)
				{
					ResearchProjectDef researchProjectDef = AddResearchPoints3(RandomResearchPointAmount.RandomInRange);
					string text = "Witty response for when there is no research";
					if (researchProjectDef != null)
					{
						text = researchProjectDef.LabelCap;
					}
				}
				if (flag)
				{
					return;
				}
				bool flag2 = RandomNumber(1, 5) != 2;
				if (flag2)
				{
					ResearchProjectDef researchProjectDef2 = AddResearchPoints2(RandomResearchPointAmount.RandomInRange);
					string text2 = "Witty response for when there is no research";
					if (researchProjectDef2 != null)
					{
						text2 = researchProjectDef2.LabelCap;
					}
				}
				if (!flag2)
				{
					ResearchProjectDef researchProjectDef3 = AddResearchPoints1(RandomResearchPointAmount.RandomInRange);
					string text3 = "Witty response for when there is no research";
					if (researchProjectDef3 != null)
					{
						text3 = researchProjectDef3.LabelCap;
					}
				}
				return;
			}
			bool flag3 = RandomNumber(1, 3) == 2;
			if (flag3)
			{
				bool flag4 = RandomNumber(1, 3) == 2;
				if (flag4)
				{
					ResearchProjectDef researchProjectDef4 = AddResearchPoints5(RandomResearchPointAmount.RandomInRange);
					string text4 = "Witty response for when there is no research";
					if (researchProjectDef4 != null)
					{
						text4 = researchProjectDef4.LabelCap;
					}
				}
				if (!flag4)
				{
					ResearchProjectDef researchProjectDef5 = AddResearchPoints4(RandomResearchPointAmount.RandomInRange);
					string text5 = "Witty response for when there is no research";
					if (researchProjectDef5 != null)
					{
						text5 = researchProjectDef5.LabelCap;
					}
				}
			}
			if (flag3)
			{
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
