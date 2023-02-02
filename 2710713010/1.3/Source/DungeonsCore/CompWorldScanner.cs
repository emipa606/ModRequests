using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DungeonsCore
{
    public class CompProperties_WorldScanner : CompProperties
	{
		public StatDef scanSpeedStat;
		public float scanFindMtbDays;
		public float scanFindGuaranteedDays;
		public JobDef scanJob;
		public CompProperties_WorldScanner()
		{
			compClass = typeof(CompWorldScanner);
		}
	}

	public class CompWorldScanner : ThingComp
	{
		public CompProperties_WorldScanner Props => base.props as CompProperties_WorldScanner;

		public static HashSet<CompWorldScanner> worldScanners = new HashSet<CompWorldScanner>();

		public bool scanningToggled;
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			worldScanners.Add(this);
		}
		protected float daysWorkingSinceLastFinding;

		protected float lastUserSpeed = 1f;

		protected float lastScanTick = -1f;
		public override string CompInspectStringExtra()
		{
			string text = "";
			if (lastScanTick > (float)(Find.TickManager.TicksGame - 30))
			{
				text += "UserScanAbility".Translate() + ": " + lastUserSpeed.ToStringPercent() + "\n" + "ScanAverageInterval".Translate() + ": "
					+ "PeriodDays".Translate((Props.scanFindMtbDays / lastUserSpeed).ToString("F1")) + "\n";
			}
			return text + "ScanningProgressToGuaranteedFind".Translate() + ": " + (daysWorkingSinceLastFinding / Props.scanFindGuaranteedDays).ToStringPercent();
		}
		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			worldScanners.Remove(this);
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			worldScanners.Remove(this);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (this.parent.Faction == Faction.OfPlayer)
			{
				yield return new Command_Toggle
				{
					defaultLabel = "EF.ToggleScanning".Translate(scanningToggled ? "On".Translate() : "Off".Translate()),
					defaultDesc = "EF.ToggleScanningDesc".Translate(),
					icon = this.parent.def.uiIcon,
					isActive = () => scanningToggled,
					toggleAction = () =>
					{
						scanningToggled = !scanningToggled;
					}
				};
                if (Prefs.DevMode)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Dev: Force find",
                        action = () =>
                        {
							DoFind();
                        }
                    };
                }
			}
		}
		public void Used(Pawn worker)
		{
			lastScanTick = Find.TickManager.TicksGame;
			lastUserSpeed = 1f;
			if (Props.scanSpeedStat != null)
			{
				lastUserSpeed = worker.GetStatValue(Props.scanSpeedStat);
			}
			daysWorkingSinceLastFinding += lastUserSpeed / 60000f;
			if (TickDoesFind(lastUserSpeed))
			{
				DoFind();
				daysWorkingSinceLastFinding = 0f;
			}
		}
		public bool TickDoesFind(float scanSpeed)
		{
			if (Find.TickManager.TicksGame % 59 == 0 && (Rand.MTBEventOccurs(Props.scanFindMtbDays / scanSpeed, 60000f, 59f) || (Props.scanFindGuaranteedDays > 0f && daysWorkingSinceLastFinding >= Props.scanFindGuaranteedDays)))
			{
				return true;
			}
			return false;
		}
		public void DoFind()
		{
			var quests = DefDatabase<QuestScriptDef>.AllDefs.Where(x => x.defName.Contains("EncounterFramework_QuestPool_"));
			if (quests.Any())
			{
				var chosenQuest = quests.RandomElement();
				var quest = QuestUtility.GenerateQuestAndMakeAvailable(chosenQuest, 0);
				if (!quest.hidden)
				{
					QuestUtility.SendLetterQuestAvailable(quest);
				}
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref daysWorkingSinceLastFinding, "daysWorkingSinceLastFinding");
			Scribe_Values.Look(ref lastUserSpeed, "lastUserSpeed");
			Scribe_Values.Look(ref lastScanTick, "lastScanTick");
			Scribe_Values.Look(ref scanningToggled, "scanningToggled");
		}
	}
}
