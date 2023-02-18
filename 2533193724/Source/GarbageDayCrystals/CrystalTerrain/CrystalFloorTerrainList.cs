using System;
using System.Collections.Generic;
using Verse;

namespace Crystosentient.CrystalTerrain
{
	// Token: 0x02000010 RID: 16
	public class CrystalFloorTerrainList : MapComponent
	{
		// Token: 0x06000053 RID: 83 RVA: 0x00002394 File Offset: 0x00000594
		public CrystalFloorTerrainList(Map map) : base(map)
		{
		}

		// Token: 0x06000054 RID: 84 RVA: 0x000023AA File Offset: 0x000005AA
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look<IntVec3, CrystalFloor>(ref this.terrains, "terrains", LookMode.Value, LookMode.Deep);
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00003C6C File Offset: 0x00001E6C
		public override void MapComponentTick()
		{
			base.MapComponentTick();
			foreach (KeyValuePair<IntVec3, CrystalFloor> keyValuePair in this.terrains)
			{
				keyValuePair.Value.Tick();
			}
		}

		// Token: 0x06000056 RID: 86 RVA: 0x000023C7 File Offset: 0x000005C7
		public override void FinalizeInit()
		{
			base.FinalizeInit();
			this.RefreshAllCurrentTerrain();
			this.CallPostLoad();
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00003CD4 File Offset: 0x00001ED4
		public void CallPostLoad()
		{
			foreach (IntVec3 key in this.terrains.Keys)
			{
				this.terrains[key].PostLoad();
			}
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00003D3C File Offset: 0x00001F3C
		public void RefreshAllCurrentTerrain()
		{
			foreach (IntVec3 intVec in this.map)
			{
				TerrainDef terrainDef = this.map.terrainGrid.TerrainAt(intVec);
				CrystalFloorDef special;
				bool flag = (special = (terrainDef as CrystalFloorDef)) != null;
				bool flag2 = flag;
				if (flag2)
				{
					this.RegisterAt(special, intVec);
				}
			}
		}

		// Token: 0x06000059 RID: 89 RVA: 0x000023DF File Offset: 0x000005DF
		public void RegisterAt(CrystalFloorDef special, int i)
		{
			this.RegisterAt(special, this.map.cellIndices.IndexToCell(i));
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00003DBC File Offset: 0x00001FBC
		public void RegisterAt(CrystalFloorDef special, IntVec3 cell)
		{
			bool flag = !this.terrains.ContainsKey(cell);
			bool flag2 = flag;
			if (flag2)
			{
				CrystalFloor crystalFloor = special.MakeTerrainInstance(this.map, cell);
				crystalFloor.Init();
				this.terrains.Add(cell, crystalFloor);
			}
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00003E04 File Offset: 0x00002004
		public override void MapComponentUpdate()
		{
			base.MapComponentUpdate();
			foreach (KeyValuePair<IntVec3, CrystalFloor> keyValuePair in this.terrains)
			{
				keyValuePair.Value.Update();
			}
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00003E6C File Offset: 0x0000206C
		public void Notify_RemovedTerrainAt(IntVec3 c)
		{
			CrystalFloor reefFloor = this.terrains[c];
			this.terrains.Remove(c);
			reefFloor.PostRemove();
		}

		// Token: 0x0400002C RID: 44
		public Dictionary<IntVec3, CrystalFloor> terrains = new Dictionary<IntVec3, CrystalFloor>();
	}
}
