using System;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x0200000A RID: 10
	public class CompMainComponent : ThingComp
	{
		// Token: 0x06000034 RID: 52 RVA: 0x000054AC File Offset: 0x000036AC
		private void performSetup()
		{
			if (!this.setup)
			{
				this.setup = true;

				ThingCategory category = this.parent.def.category;

				this.compComponentsPositionTracker = new CompComponentsPositionTracker();				
				this.compComponentsPositionTracker.parent = this.parent;
				this.compComponentsPositionTracker.mainComponent = this;

				this.compHiddenable = new CompHiddenable();
				this.compHiddenable.parent = this.parent;
				this.compHiddenable.mainComponent = this;

				this.compHideFromPlayer = new CompHideFromPlayer();
				this.compHideFromPlayer.parent = this.parent;
				this.compHideFromPlayer.mainComponent = this;

				if (category == ThingCategory.Building)
				{
					this.compViewBlockerWatcher = new CompViewBlockerWatcher();
					this.compViewBlockerWatcher.parent = this.parent;
					this.compViewBlockerWatcher.mainComponent = this;
				}
				if (
				category == ThingCategory.Pawn 
				||category == ThingCategory.Building
				//||category == ThingCategory.Projectile
				)
				{
					this.compFieldOfViewWatcher = new CompFieldOfViewWatcher();
					this.compFieldOfViewWatcher.parent = this.parent;
					this.compFieldOfViewWatcher.mainComponent = this;
				}
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!this.setup)
			{
				this.performSetup();
			}
			this.compComponentsPositionTracker.PostSpawnSetup(respawningAfterLoad);

			this.compHiddenable.PostSpawnSetup(respawningAfterLoad);

			this.compHideFromPlayer.PostSpawnSetup(respawningAfterLoad);

			if (this.compViewBlockerWatcher != null)
			{
				this.compViewBlockerWatcher.PostSpawnSetup(respawningAfterLoad);
			}
			if (this.compFieldOfViewWatcher != null)
			{
				this.compFieldOfViewWatcher.PostSpawnSetup(respawningAfterLoad);
			}
		}

		// Token: 0x06000036 RID: 54 RVA: 0x0000564C File Offset: 0x0000384C
		public override void CompTick()
		{
			if (!this.setup)
			{
				this.performSetup();
			}
			this.compComponentsPositionTracker.CompTick();
			this.compHiddenable.CompTick();
			this.compHideFromPlayer.CompTick();
			if (this.compViewBlockerWatcher != null)
			{
				this.compViewBlockerWatcher.CompTick();
			}
			if (this.compFieldOfViewWatcher != null)
			{
				this.compFieldOfViewWatcher.CompTick();
			}
		}

		// Token: 0x06000037 RID: 55 RVA: 0x000056CC File Offset: 0x000038CC
		public override void CompTickRare()
		{
			if (!this.setup)
			{
				this.performSetup();
			}
			this.compComponentsPositionTracker.CompTickRare();
			this.compHiddenable.CompTickRare();
			this.compHideFromPlayer.CompTickRare();
			if (this.compViewBlockerWatcher != null)
			{
				this.compViewBlockerWatcher.CompTickRare();
			}
			if (this.compFieldOfViewWatcher != null)
			{
				this.compFieldOfViewWatcher.CompTickRare();
			}
		}

		// Token: 0x06000038 RID: 56 RVA: 0x0000574C File Offset: 0x0000394C
		public override void ReceiveCompSignal(string signal)
		{
			if (!this.setup)
			{
				this.performSetup();
			}
			this.compComponentsPositionTracker.ReceiveCompSignal(signal);
			this.compHiddenable.ReceiveCompSignal(signal);
			this.compHideFromPlayer.ReceiveCompSignal(signal);
			if (this.compViewBlockerWatcher != null)
			{
				this.compViewBlockerWatcher.ReceiveCompSignal(signal);
			}
			if (this.compFieldOfViewWatcher != null)
			{
				this.compFieldOfViewWatcher.ReceiveCompSignal(signal);
			}
		}

		// Token: 0x06000039 RID: 57 RVA: 0x000057D0 File Offset: 0x000039D0
		public override void PostDeSpawn(Map map)
		{
			if (!this.setup)
			{
				this.performSetup();
			}
			this.compComponentsPositionTracker.PostDeSpawn(map);
			this.compHiddenable.PostDeSpawn(map);
			this.compHideFromPlayer.PostDeSpawn(map);
			if (this.compViewBlockerWatcher != null)
			{
				this.compViewBlockerWatcher.PostDeSpawn(map);
			}
			if (this.compFieldOfViewWatcher != null)
			{
				this.compFieldOfViewWatcher.PostDeSpawn(map);
			}
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00005854 File Offset: 0x00003A54
		public override void PostExposeData()
		{
			if (!this.setup)
			{
				this.performSetup();
			}
			this.compComponentsPositionTracker.PostExposeData();
			this.compHiddenable.PostExposeData();
			this.compHideFromPlayer.PostExposeData();

			if (this.compViewBlockerWatcher != null)
			{
				this.compViewBlockerWatcher.PostExposeData();
			}
			if (this.compFieldOfViewWatcher != null)
			{
				this.compFieldOfViewWatcher.PostExposeData();
			}
			if (Scribe.saver.savingForDebug)
			{
				bool flag4 = this.compComponentsPositionTracker != null;
				bool flag5 = this.compHiddenable != null;
				bool flag6 = this.compHideFromPlayer != null;
				bool flag7 = this.compViewBlockerWatcher != null;
				bool flag8 = this.compFieldOfViewWatcher != null;
				Scribe_Values.Look<bool>(ref flag4, "hasCompComponentsPositionTracker", false, false);
				Scribe_Values.Look<bool>(ref flag5, "hasCompHiddenable", false, false);
				Scribe_Values.Look<bool>(ref flag6, "hasCompHideFromPlayer", false, false);
				Scribe_Values.Look<bool>(ref flag7, "hasCompViewBlockerWatcher", false, false);
				Scribe_Values.Look<bool>(ref flag8, "hasCompFieldOfViewWatcher", false, false);
			}
		}

		// Token: 0x0400002A RID: 42
		public static readonly CompProperties COMP_DEF = new CompProperties(typeof(CompMainComponent));
		private bool setup = false;
		public CompComponentsPositionTracker compComponentsPositionTracker = null;
		public CompFieldOfViewWatcher compFieldOfViewWatcher = null;
		public CompHiddenable compHiddenable = null;
		public CompHideFromPlayer compHideFromPlayer = null;
		public CompViewBlockerWatcher compViewBlockerWatcher = null;
	}
}
