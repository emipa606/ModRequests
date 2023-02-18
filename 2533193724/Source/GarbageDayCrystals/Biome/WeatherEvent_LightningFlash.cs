using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace Crystosentient.Biome
{
	// Token: 0x02000D30 RID: 3376
	public class WeatherEvent_LightningFlash : WeatherEvent
	{
		// Token: 0x17000DC3 RID: 3523
		// (get) Token: 0x06004F67 RID: 20327 RVA: 0x001AC1F1 File Offset: 0x001AA3F1
		public override bool Expired
		{
			get
			{
				return this.age > this.duration;
			}
		}

		// Token: 0x17000DC4 RID: 3524
		// (get) Token: 0x06004F68 RID: 20328 RVA: 0x001AC201 File Offset: 0x001AA401
		public override SkyTarget SkyTarget
		{
			get
			{
				return new SkyTarget(1f, WeatherEvent_LightningFlash.LightningFlashColors, 1f, 1f);
			}
		}

		// Token: 0x17000DC5 RID: 3525
		// (get) Token: 0x06004F69 RID: 20329 RVA: 0x001AC21C File Offset: 0x001AA41C
		public override Vector2? OverrideShadowVector
		{
			get
			{
				return new Vector2?(this.shadowVector);
			}
		}

		// Token: 0x17000DC6 RID: 3526
		// (get) Token: 0x06004F6A RID: 20330 RVA: 0x001AC229 File Offset: 0x001AA429
		public override float SkyTargetLerpFactor
		{
			get
			{
				return this.LightningBrightness;
			}
		}

		// Token: 0x17000DC7 RID: 3527
		// (get) Token: 0x06004F6B RID: 20331 RVA: 0x001AC231 File Offset: 0x001AA431
		protected float LightningBrightness
		{
			get
			{
				if (this.age <= 3)
				{
					return (float)this.age / 3f;
				}
				return 1f - (float)this.age / (float)this.duration;
			}
		}

		// Token: 0x06004F6C RID: 20332 RVA: 0x001AC260 File Offset: 0x001AA460
		public WeatherEvent_LightningFlash(Map map) : base(map)
		{
			this.duration = Rand.Range(15, 60);
			this.shadowVector = new Vector2(Rand.Range(-5f, 5f), Rand.Range(-5f, 0f));
		}

		// Token: 0x06004F6D RID: 20333 RVA: 0x001AC2AC File Offset: 0x001AA4AC
		public override void FireEvent()
		{
			SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(this.map);
		}

		// Token: 0x06004F6E RID: 20334 RVA: 0x001AC2BE File Offset: 0x001AA4BE
		public override void WeatherEventTick()
		{
			this.age++;
		}

		// Token: 0x04002FD6 RID: 12246
		private int duration;

		// Token: 0x04002FD7 RID: 12247
		private Vector2 shadowVector;

		// Token: 0x04002FD8 RID: 12248
		private int age;

		// Token: 0x04002FD9 RID: 12249
		private const int FlashFadeInTicks = 3;

		// Token: 0x04002FDA RID: 12250
		private const int MinFlashDuration = 15;

		// Token: 0x04002FDB RID: 12251
		private const int MaxFlashDuration = 60;

		// Token: 0x04002FDC RID: 12252
		private const float FlashShadowDistance = 5f;

		// Token: 0x04002FDD RID: 12253
		private static readonly SkyColorSet LightningFlashColors = new SkyColorSet(new Color(0.9f, 0.95f, 1f), new Color(0.784313738f, 0.8235294f, 0.847058833f), new Color(0.9f, 0.95f, 1f), 1.15f);
	}
}
