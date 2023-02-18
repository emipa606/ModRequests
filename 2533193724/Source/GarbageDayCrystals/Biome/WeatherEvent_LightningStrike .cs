using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace Crystosentient.Biome
{
	// Token: 0x02000D31 RID: 3377
	[StaticConstructorOnStartup]
	public class WeatherEvent_LightningStrike : WeatherEvent_LightningFlash
	{
		// Token: 0x06004F70 RID: 20336 RVA: 0x001AC328 File Offset: 0x001AA528
		public WeatherEvent_LightningStrike(Map map) : base(map)
		{
		}

		// Token: 0x06004F71 RID: 20337 RVA: 0x001AC33C File Offset: 0x001AA53C
		public WeatherEvent_LightningStrike(Map map, IntVec3 forcedStrikeLoc) : base(map)
		{
			this.strikeLoc = forcedStrikeLoc;
		}

		// Token: 0x06004F72 RID: 20338 RVA: 0x001AC358 File Offset: 0x001AA558
		public override void FireEvent()
		{
			base.FireEvent();
			if (!this.strikeLoc.IsValid)
			{
				this.strikeLoc = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(this.map) && !this.map.roofGrid.Roofed(sq), this.map, 1000);
			}
			this.boltMesh = LightningBoltMeshPool.RandomBoltMesh;
			if (!this.strikeLoc.Fogged(this.map))
			{
				GenExplosion.DoExplosion(this.strikeLoc, this.map, 1.9f, DamageDefOf.Flame, null, -1, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, null);
				Vector3 loc = this.strikeLoc.ToVector3Shifted();
				for (int i = 0; i < 4; i++)
				{
					FleckMaker.ThrowSmoke(loc, this.map, 1.5f);
					FleckMaker.ThrowMicroSparks(loc, this.map);
					FleckMaker.ThrowLightningGlow(loc, this.map, 1.5f);
				}
			}
			SoundInfo info = SoundInfo.InMap(new TargetInfo(this.strikeLoc, this.map, false), MaintenanceType.None);
			SoundDefOf.Thunder_OnMap.PlayOneShot(info);
		}

		// Token: 0x06004F73 RID: 20339 RVA: 0x001AC46A File Offset: 0x001AA66A
		public override void WeatherEventDraw()
		{
			Graphics.DrawMesh(this.boltMesh, this.strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity, FadedMaterialPool.FadedVersionOf(WeatherEvent_LightningStrike.LightningMat, base.LightningBrightness), 0);
			Color color = Color.green;
		}

		// Token: 0x04002FDE RID: 12254
		private IntVec3 strikeLoc = IntVec3.Invalid;

		// Token: 0x04002FDF RID: 12255
		private Mesh boltMesh;

		// Token: 0x04002FE0 RID: 12256
		private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt", -1);
	}
}
