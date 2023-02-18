using System;
using System.Collections.Generic;
using Verse;

namespace Crystosentient.CrystalTerrain
{
	// Token: 0x0200001C RID: 28
	public class CrystalFloor : IExposable
	{
		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000098 RID: 152 RVA: 0x000025A3 File Offset: 0x000007A3
		// (set) Token: 0x06000099 RID: 153 RVA: 0x000025AB File Offset: 0x000007AB
		public Map Map
		{
			get
			{
				return this.mapInt;
			}
			set
			{
				this.mapInt = value;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600009A RID: 154 RVA: 0x000025B4 File Offset: 0x000007B4
		// (set) Token: 0x0600009B RID: 155 RVA: 0x000025BC File Offset: 0x000007BC
		public IntVec3 Position
		{
			get
			{
				return this.positionInt;
			}
			set
			{
				this.positionInt = value;
			}
		}

		// Token: 0x0600009C RID: 156 RVA: 0x000025C5 File Offset: 0x000007C5
		public virtual void Init()
		{
			this.InitializeComps();
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x0600009D RID: 157 RVA: 0x000045E0 File Offset: 0x000027E0
		public virtual string Label
		{
			get
			{
				string text = this.def.LabelCap;
				for (int i = 0; i < this.comps.Count; i++)
				{
					text = this.comps[i].TransformLabel(text);
				}
				return text;
			}
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00004634 File Offset: 0x00002834
		public T GetComp<T>() where T : CompCrystalFloor
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				T result;
				bool flag = (result = (this.comps[i] as T)) != null;
				bool flag2 = flag;
				if (flag2)
				{
					return result;
				}
			}
			return default(T);
		}

		// Token: 0x0600009F RID: 159 RVA: 0x0000469C File Offset: 0x0000289C
		public void InitializeComps()
		{
			foreach (CompPropertiesCrystalFloor compPropertiesReefFloor in this.def.terrainComps)
			{
				CompCrystalFloor compCrystalFloor = (CompCrystalFloor)Activator.CreateInstance(compPropertiesReefFloor.compClass);
				compCrystalFloor.parent = this;
				this.comps.Add(compCrystalFloor);
				compCrystalFloor.Initialize(compPropertiesReefFloor);
			}
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00004720 File Offset: 0x00002920
		public virtual void Tick()
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].CompTick();
			}
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x0000475C File Offset: 0x0000295C
		public virtual void Update()
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].CompUpdate();
			}
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00004798 File Offset: 0x00002998
		public virtual void PostPlacedDown()
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PlaceSetup();
			}
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x000047D4 File Offset: 0x000029D4
		public virtual void PostRemove()
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostRemove();
			}
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x00004810 File Offset: 0x00002A10
		public virtual void PostLoad()
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostPostLoad();
			}
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x0000484C File Offset: 0x00002A4C
		public virtual void BroadcastCompSignal(string sig)
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].ReceiveCompSignal(sig);
			}
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x0000488C File Offset: 0x00002A8C
		public virtual void ExposeData()
		{
			Scribe_References.Look<Map>(ref this.mapInt, "map", false);
			Scribe_Values.Look<IntVec3>(ref this.positionInt, "pos", default(IntVec3), false);
			Scribe_Defs.Look<CrystalFloorDef>(ref this.def, "def");
			bool flag = Scribe.mode == LoadSaveMode.LoadingVars;
			bool flag2 = flag;
			if (flag2)
			{
				this.InitializeComps();
			}
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostExposeData();
			}
		}

		// Token: 0x04000045 RID: 69
		public CrystalFloorDef def;

		// Token: 0x04000046 RID: 70
		public List<CompCrystalFloor> comps = new List<CompCrystalFloor>();

		// Token: 0x04000047 RID: 71
		private Map mapInt;

		// Token: 0x04000048 RID: 72
		private IntVec3 positionInt;
	}
}
