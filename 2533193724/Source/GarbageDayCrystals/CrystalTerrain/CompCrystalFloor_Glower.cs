using System;
using RimWorld;
using Verse;

namespace Crystosentient.CrystalTerrain
{
	// Token: 0x02000017 RID: 23
	public class CompCrystalFloor_Glower : CompCrystalFloor
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600006C RID: 108 RVA: 0x00003F0C File Offset: 0x0000210C
		public CompGlower AsThingComp
		{
			get
			{
				CompGlower result;
				if ((result = this.instanceGlowerComp) == null)
				{
					result = (this.instanceGlowerComp = (CompGlower)this);
				}
				return result;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600006D RID: 109 RVA: 0x00002464 File Offset: 0x00000664
		public CompPropertiesCrystalFloor_Glower Props
		{
			get
			{
				return (CompPropertiesCrystalFloor_Glower)this.props;
			}
		}


		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600006F RID: 111 RVA: 0x00002471 File Offset: 0x00000671
		// (set) Token: 0x06000070 RID: 112 RVA: 0x00002479 File Offset: 0x00000679
		public float OverlightRadius { get; set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000071 RID: 113 RVA: 0x00002482 File Offset: 0x00000682
		// (set) Token: 0x06000072 RID: 114 RVA: 0x0000248A File Offset: 0x0000068A
		public float GlowRadius { get; set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000073 RID: 115 RVA: 0x00002493 File Offset: 0x00000693
		// (set) Token: 0x06000074 RID: 116 RVA: 0x0000249B File Offset: 0x0000069B
		public ColorInt Color
		{
			get
			{
				return this.colorInt;
			}
			set
			{
				this.colorInt = value;
			}
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00003F70 File Offset: 0x00002170
		public void UpdateLit()
		{
				this.parent.Map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.Things);
				(this.currentlyOn ? new Action<CompGlower>(this.parent.Map.glowGrid.RegisterGlower) : new Action<CompGlower>(this.parent.Map.glowGrid.DeRegisterGlower))(this.AsThingComp);
			
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00004050 File Offset: 0x00002250
		public override void PostPostLoad()
		{
			this.UpdateLit();
	        this.parent.Map.glowGrid.RegisterGlower(this.AsThingComp);
		
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00004090 File Offset: 0x00002290
		public override void Initialize(CompPropertiesCrystalFloor props)
		{
			base.Initialize(props);
			this.Color = this.Props.glowColor;
			this.GlowRadius = this.Props.glowRadius;
			this.OverlightRadius = this.Props.overlightRadius;
		}

		// Token: 0x06000079 RID: 121 RVA: 0x000040DC File Offset: 0x000022DC
		public static explicit operator CompGlower(CompCrystalFloor_Glower inst)
		{
			CompGlower compGlower = new CompGlower
			{
				parent = (ThingWithComps)ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Steel)
			};
			compGlower.parent.SetPositionDirect(inst.parent.Position);
			compGlower.Initialize(new CompProperties_Glower
			{
				glowColor = inst.Color,
				glowRadius = inst.GlowRadius,
				overlightRadius = inst.OverlightRadius
			});
			return compGlower;
		}

		// Token: 0x0400003A RID: 58
		[Unsaved(false)]
		protected bool currentlyOn;

		// Token: 0x0400003B RID: 59
		[Unsaved(false)]
		private CompGlower instanceGlowerComp;

		// Token: 0x0400003C RID: 60
		private ColorInt colorInt;
	}
}
