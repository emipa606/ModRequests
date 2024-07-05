using System;
using RimWorld;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x02000013 RID: 19
	public class CompHiddenable : ThingSubComp
	{
		// Token: 0x06000062 RID: 98 RVA: 0x000068EC File Offset: 0x00004AEC
		public void hide()
		{
			bool flag = !this.hidden;
			if (flag)
			{
				this.hidden = true;
				bool flag2 = this.parent.def.drawerType != DrawerType.MapMeshOnly;
				if (flag2)
				{
					this.parent.Map.dynamicDrawManager.DeRegisterDrawable(this.parent);
				}
				bool hasTooltip = this.parent.def.hasTooltip;
				if (hasTooltip)
				{
					this.parent.Map.tooltipGiverList.Notify_ThingDespawned(this.parent);
				}
				Selector selector = Find.Selector;
				bool flag3 = selector.IsSelected(this.parent);
				if (flag3)
				{
					selector.Deselect(this.parent);
				}
				this.updateMeshes();
			}
		}

		// Token: 0x06000063 RID: 99 RVA: 0x000069AC File Offset: 0x00004BAC
		public void show()
		{
			bool flag = this.hidden;
			if (flag)
			{
				this.hidden = false;
				bool flag2 = this.parent.def.drawerType != DrawerType.MapMeshOnly;
				if (flag2)
				{
					this.parent.Map.dynamicDrawManager.RegisterDrawable(this.parent);
				}
				bool hasTooltip = this.parent.def.hasTooltip;
				if (hasTooltip)
				{
					this.parent.Map.tooltipGiverList.Notify_ThingSpawned(this.parent);
				}
				this.updateMeshes();
			}
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00006A40 File Offset: 0x00004C40
		private void updateMeshes()
		{
			bool flag = this.map != this.parent.Map;
			if (flag)
			{
				this.map = this.parent.Map;
				this.mapComp = this.map.getMapComponentSeenFog();
			}
			bool flag2 = this.mapComp != null && this.mapComp.initialized;
			if (flag2)
			{
				MapMeshFlag dirtyFlags = MapMeshFlag.Things | MapMeshFlag.Buildings | MapMeshFlag.GroundGlow | MapMeshFlag.Terrain | MapMeshFlag.Roofs | MapMeshFlag.Snow | MapMeshFlag.Zone | MapMeshFlag.PowerGrid | MapMeshFlag.BuildingsDamage;
				foreach (IntVec3 intVec in this.parent.OccupiedRect().Cells)
				{
					bool flag3 = intVec.InBounds(this.map);
					if (flag3)
					{
						this.map.mapDrawer.MapMeshDirty(intVec, dirtyFlags, false, false);
					}
				}
			}
		}

		// Token: 0x0400005A RID: 90
		private IntVec3 lastPosition = IntVec3.Invalid;

		// Token: 0x0400005B RID: 91
		public bool hidden = false;

		// Token: 0x0400005C RID: 92
		private Map map = null;

		// Token: 0x0400005D RID: 93
		private MapComponentSeenFog mapComp = null;
	}
}
