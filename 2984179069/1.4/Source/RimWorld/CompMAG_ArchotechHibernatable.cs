using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompMAG_ArchotechHibernatable : ThingComp
	{
		private HibernatableStateDef state = HibernatableStateDefOf.Hibernating;

		private int endStartupTick;

		private Sustainer sustainer;

		public CompProperties_MAG_ArchotechHibernatable Props => (CompProperties_MAG_ArchotechHibernatable)props;

		public HibernatableStateDef State
		{
			get
			{
				return state;
			}
			set
			{
				if (state != value)
				{
					state = value;
					parent.Map.info.parent.Notify_HibernatableChanged();
				}
			}
		}

		public bool Running => State == HibernatableStateDefOf.Running;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				parent.Map.info.parent.Notify_HibernatableChanged();
			}
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			map.info.parent.Notify_HibernatableChanged();
			if (sustainer != null && !sustainer.Ended)
			{
				sustainer.End();
			}
		}

		public void Startup()
		{
			if (State != HibernatableStateDefOf.Hibernating)
			{
				Log.ErrorOnce("Attempted to start a non-hibernating object", 34361223);
				return;
			}
			State = HibernatableStateDefOf.Starting;
			endStartupTick = Mathf.RoundToInt((float)Find.TickManager.TicksGame + Props.startupDays * 60000f);
		}

		public override string CompInspectStringExtra()
		{
			if (State == HibernatableStateDefOf.Hibernating)
			{
				return "HibernatableHibernating".Translate();
			}
			if (State == HibernatableStateDefOf.Starting)
			{
				return string.Format("{0}: {1}", "HibernatableStartingUp".Translate(), GenDate.ToStringTicksToPeriod(endStartupTick - Find.TickManager.TicksGame, true, false, true, true));
			}
			return null;
		}

		public override void CompTick()
		{
			if (State == HibernatableStateDefOf.Starting && Find.TickManager.TicksGame > endStartupTick)
			{
				State = HibernatableStateDefOf.Running;
				endStartupTick = 0;
				string text = ((parent.Map.Parent.GetComponent<EscapeShipComp>() == null) ? ((string)"MAG_LetterLabelHibernateComplete".Translate()) : ((string)"LetterHibernateComplete".Translate()));
				Find.LetterStack.ReceiveLetter("MAG_LetterLabelHibernateComplete".Translate(), text, LetterDefOf.PositiveEvent, new GlobalTargetInfo(parent));
			}
			if (State != HibernatableStateDefOf.Hibernating)
			{
				if (sustainer == null || sustainer.Ended)
				{
					sustainer = Props.sustainerActive.TrySpawnSustainer(SoundInfo.InMap(parent));
				}
				sustainer.Maintain();
			}
			else if (sustainer != null && !sustainer.Ended)
			{
				sustainer.End();
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Defs.Look(ref state, "hibernateState");
			Scribe_Values.Look(ref endStartupTick, "hibernateendStartupTick", 0);
		}
	}
}
