using System;
using RimWorld;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x02000012 RID: 18
	public class CompHideFromPlayer : ThingSubComp
	{
		// Token: 0x06000059 RID: 89 RVA: 0x00006428 File Offset: 0x00004628
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			this.setupDone = true;
			this.calculated = false;
			this.lastPosition = CompHideFromPlayer.iv3Invalid;
			this.lastRotation = CompHideFromPlayer.r4Invalid;
			this.isPawn = (this.parent.def.category == ThingCategory.Pawn);
			this.size = this.parent.def.size;
			this.isOneCell = (this.size.z == 1 && this.size.x == 1);
			this.isSaveable = this.parent.def.isSaveable;
			this.saveCompressible = this.parent.def.saveCompressible;
			this.compHiddenable = this.mainComponent.compHiddenable;
			lastUpdateTick = Find.TickManager.TicksGame;
			this.updateVisibility(false, false);
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000064FC File Offset: 0x000046FC
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<bool>(ref this.seenByPlayer, "seenByPlayer", false, false);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00006519 File Offset: 0x00004719
		public override void ReceiveCompSignal(string signal)
		{
			base.ReceiveCompSignal(signal);
			this.updateVisibility(false, false);
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00006530 File Offset: 0x00004730
		public override void CompTick()
		{
			base.CompTick();
			int tickGame = Find.TickManager.TicksGame;
			if (tickGame-lastUpdateTick==12)
			{
				lastUpdateTick = tickGame;
				this.updateVisibility(false, false);
			}
		}
		private int lastUpdateTick;

		// Token: 0x0600005D RID: 93 RVA: 0x00006564 File Offset: 0x00004764
		public void forceSeen()
		{
			this.seenByPlayer = true;
			this.updateVisibility(true, true);
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00006578 File Offset: 0x00004778
		public void updateVisibility(bool forceCheck, bool forceUpdate = false)
		{
			bool flag = !this.setupDone || Current.ProgramState == ProgramState.MapInitializing;
			if (!flag)
			{
				Thing parent = this.parent;
				IntVec3 position = parent.Position;
				Rot4 rotation = parent.Rotation;
				bool flag2 = parent != null && parent.Spawned && parent.Map != null && position != CompHideFromPlayer.iv3Invalid && (this.isOneCell || rotation != CompHideFromPlayer.r4Invalid);
				if (flag2)
				{
					bool flag3 = this.map != parent.Map;
					if (flag3)
					{
						this.map = parent.Map;
						this.fogGrid = this.map.fogGrid;
						this.mapCompSeenFog = parent.Map.getMapComponentSeenFog();
					}
					else
					{
						bool flag4 = this.mapCompSeenFog == null;
						if (flag4)
						{
							this.mapCompSeenFog = parent.Map.getMapComponentSeenFog();
						}
					}
					bool flag5 = this.mapCompSeenFog == null;
					if (!flag5)
					{
						bool flag6 = forceCheck || !this.calculated || position != this.lastPosition || (!this.isOneCell && rotation != this.lastRotation);
						if (flag6)
						{
							this.calculated = true;
							this.lastPosition = position;
							this.lastRotation = rotation;
							bool flag7 = parent.Faction != null && parent.Faction.IsPlayer;
							bool flag8 = this.mapCompSeenFog != null && !this.fogGrid.IsFogged(this.lastPosition);
							if (flag8)
							{
								bool flag9 = this.isSaveable && !this.saveCompressible;
								if (flag9)
								{
									bool flag10 = !flag7;
									if (flag10)
									{
										bool flag11 = this.isPawn && !this.hasPartShownToPlayer();
										if (flag11)
										{
											this.compHiddenable.hide();
										}
										else
										{
											bool flag12 = !this.isPawn && !this.seenByPlayer && !this.hasPartShownToPlayer();
											if (flag12)
											{
												this.compHiddenable.hide();
											}
											else
											{
												this.seenByPlayer = true;
												this.compHiddenable.show();
											}
										}
									}
									else
									{
										this.seenByPlayer = true;
										this.compHiddenable.show();
									}
								}
								else
								{
									bool flag13 = (forceUpdate || !this.seenByPlayer) && this.hasPartShownToPlayer();
									if (flag13)
									{
										this.seenByPlayer = true;
										this.compHiddenable.show();
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000067F8 File Offset: 0x000049F8
		private bool hasPartShownToPlayer()
		{
			Faction ofPlayer = Faction.OfPlayer;
			bool flag = this.isOneCell;
			bool result;
			if (flag)
			{
				result = this.mapCompSeenFog.isShown(ofPlayer, this.lastPosition.x, this.lastPosition.z);
			}
			else
			{
				CellRect cellRect = GenAdj.OccupiedRect(this.lastPosition, this.lastRotation, this.size);
				for (int i = cellRect.minX; i <= cellRect.maxX; i++)
				{
					for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
					{
						bool flag2 = this.mapCompSeenFog.isShown(ofPlayer, i, j);
						if (flag2)
						{
							return true;
						}
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x0400004A RID: 74
		private static readonly IntVec3 iv3Invalid = IntVec3.Invalid;

		// Token: 0x0400004B RID: 75
		private static readonly Rot4 r4Invalid = Rot4.Invalid;

		// Token: 0x0400004C RID: 76
		private bool calculated;

		// Token: 0x0400004D RID: 77
		private IntVec3 lastPosition;

		// Token: 0x0400004E RID: 78
		private Rot4 lastRotation;

		// Token: 0x0400004F RID: 79
		private bool isOneCell;

		// Token: 0x04000050 RID: 80
		private Map map;

		// Token: 0x04000051 RID: 81
		private FogGrid fogGrid;

		// Token: 0x04000052 RID: 82
		private MapComponentSeenFog mapCompSeenFog;

		// Token: 0x04000053 RID: 83
		private CompHiddenable compHiddenable;

		// Token: 0x04000054 RID: 84
		private bool setupDone = false;

		// Token: 0x04000055 RID: 85
		private bool seenByPlayer;

		// Token: 0x04000056 RID: 86
		private bool isPawn;

		// Token: 0x04000057 RID: 87
		private IntVec2 size;

		// Token: 0x04000058 RID: 88
		private bool isSaveable;

		// Token: 0x04000059 RID: 89
		private bool saveCompressible;
	}
}
