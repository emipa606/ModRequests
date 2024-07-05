using System;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x0200000F RID: 15
	public class CompComponentsPositionTracker : ThingSubComp
	{
		// Token: 0x06000044 RID: 68 RVA: 0x00005A3C File Offset: 0x00003C3C
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			this.setupDone = true;
			ThingDef def = this.parent.def;
			this.size = def.size;
			this.isOneCell = (this.size.z == 1 && this.size.x == 1);
			this.compHideFromPlayer = this.mainComponent.compHideFromPlayer;
			this.compAffectVision = this.parent.TryGetComp<CompAffectVision>();
			this.lastPosition = CompComponentsPositionTracker.iv3Invalid;
			this.lastRotation = CompComponentsPositionTracker.r4Invalid;
			lastPositionUpdateTick = Find.TickManager.TicksGame;
			this.updatePosition();
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00005ACD File Offset: 0x00003CCD
		public override void ReceiveCompSignal(string signal)
		{
			base.ReceiveCompSignal(signal);
			this.updatePosition();
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00005AE0 File Offset: 0x00003CE0
		public override void CompTick()
		{
			base.CompTick();
			int ticksGame = Find.TickManager.TicksGame;
			if (ticksGame - lastPositionUpdateTick == 12)
			{
				lastPositionUpdateTick = ticksGame;
				this.updatePosition();
			}
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00005B14 File Offset: 0x00003D14
		public void updatePosition()
		{
			bool flag = !this.setupDone;
			if (!flag)
			{
				Thing parent = this.parent;
				IntVec3 position = parent.Position;
				Rot4 rotation = parent.Rotation;
				bool flag2 = parent != null && parent.Spawned && parent.Map != null && position != CompComponentsPositionTracker.iv3Invalid && (this.isOneCell || rotation != CompComponentsPositionTracker.r4Invalid) && (this.compHideFromPlayer != null || this.compAffectVision != null);
				if (flag2)
				{
					bool flag3 = this.map != parent.Map;
					if (flag3)
					{
						this.map = parent.Map;
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
						bool flag6 = !this.calculated || position != this.lastPosition || (!this.isOneCell && rotation != this.lastRotation);
						if (flag6)
						{
							this.calculated = true;
							bool flag7 = this.isOneCell;
							if (flag7)
							{
								bool flag8 = this.compHideFromPlayer != null;
								if (flag8)
								{
									this.mapCompSeenFog.DeregisterCompHideFromPlayerPosition(this.compHideFromPlayer, this.lastPosition.x, this.lastPosition.z);
									this.mapCompSeenFog.RegisterCompHideFromPlayerPosition(this.compHideFromPlayer, position.x, position.z);
								}
								bool flag9 = this.compAffectVision != null;
								if (flag9)
								{
									this.mapCompSeenFog.DeregisterCompAffectVisionPosition(this.compAffectVision, this.lastPosition.x, this.lastPosition.z);
									this.mapCompSeenFog.RegisterCompAffectVisionPosition(this.compAffectVision, position.x, position.z);
								}
							}
							else
							{
								bool flag10 = this.lastPosition != CompComponentsPositionTracker.iv3Invalid && this.lastRotation != CompComponentsPositionTracker.r4Invalid;
								if (flag10)
								{
									CellRect cellRect = GenAdj.OccupiedRect(this.lastPosition, this.lastRotation, this.size);
									for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
									{
										for (int j = cellRect.minX; j <= cellRect.maxX; j++)
										{
											bool flag11 = this.compHideFromPlayer != null;
											if (flag11)
											{
												this.mapCompSeenFog.DeregisterCompHideFromPlayerPosition(this.compHideFromPlayer, j, i);
											}
											bool flag12 = this.compAffectVision != null;
											if (flag12)
											{
												this.mapCompSeenFog.DeregisterCompAffectVisionPosition(this.compAffectVision, j, i);
											}
										}
									}
								}
								bool flag13 = position != CompComponentsPositionTracker.iv3Invalid && rotation != CompComponentsPositionTracker.r4Invalid;
								if (flag13)
								{
									CellRect cellRect2 = GenAdj.OccupiedRect(position, rotation, this.size);
									for (int i = cellRect2.minZ; i <= cellRect2.maxZ; i++)
									{
										for (int j = cellRect2.minX; j <= cellRect2.maxX; j++)
										{
											bool flag14 = this.compHideFromPlayer != null;
											if (flag14)
											{
												this.mapCompSeenFog.RegisterCompHideFromPlayerPosition(this.compHideFromPlayer, j, i);
											}
											bool flag15 = this.compAffectVision != null;
											if (flag15)
											{
												this.mapCompSeenFog.RegisterCompAffectVisionPosition(this.compAffectVision, j, i);
											}
										}
									}
								}
							}
							this.lastPosition = position;
							bool flag16 = this.size.x != 1 || this.size.z != 1;
							if (flag16)
							{
								this.lastRotation = rotation;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00005F0C File Offset: 0x0000410C
		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			bool flag = this.mapCompSeenFog != null && (this.compHideFromPlayer != null || this.compAffectVision != null);
			if (flag)
			{
				bool flag2 = this.isOneCell;
				if (flag2)
				{
					this.mapCompSeenFog.DeregisterCompHideFromPlayerPosition(this.compHideFromPlayer, this.lastPosition.x, this.lastPosition.z);
					this.mapCompSeenFog.DeregisterCompAffectVisionPosition(this.compAffectVision, this.lastPosition.x, this.lastPosition.z);
				}
				else
				{
					bool flag3 = this.lastPosition != CompComponentsPositionTracker.iv3Invalid && this.lastRotation != CompComponentsPositionTracker.r4Invalid;
					if (flag3)
					{
						CellRect cellRect = GenAdj.OccupiedRect(this.lastPosition, this.lastRotation, this.size);
						for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
						{
							for (int j = cellRect.minX; j <= cellRect.maxX; j++)
							{
								bool flag4 = this.compHideFromPlayer != null;
								if (flag4)
								{
									this.mapCompSeenFog.DeregisterCompHideFromPlayerPosition(this.compHideFromPlayer, j, i);
								}
								bool flag5 = this.compAffectVision != null;
								if (flag5)
								{
									this.mapCompSeenFog.DeregisterCompAffectVisionPosition(this.compAffectVision, j, i);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x04000037 RID: 55
		private static readonly IntVec3 iv3Invalid = IntVec3.Invalid;

		// Token: 0x04000038 RID: 56
		private static readonly Rot4 r4Invalid = Rot4.Invalid;

		// Token: 0x04000039 RID: 57
		private IntVec2 size;

		// Token: 0x0400003A RID: 58
		private IntVec3 lastPosition;

		// Token: 0x0400003B RID: 59
		private Rot4 lastRotation;

		// Token: 0x0400003C RID: 60
		private bool isOneCell;

		// Token: 0x0400003D RID: 61
		private Map map;

		// Token: 0x0400003E RID: 62
		private MapComponentSeenFog mapCompSeenFog;

		// Token: 0x0400003F RID: 63
		private CompHideFromPlayer compHideFromPlayer;

		// Token: 0x04000040 RID: 64
		private CompAffectVision compAffectVision;
		private int lastPositionUpdateTick;

		// Token: 0x04000041 RID: 65
		private bool setupDone = false;

		// Token: 0x04000042 RID: 66
		private bool calculated = false;
	}
}
