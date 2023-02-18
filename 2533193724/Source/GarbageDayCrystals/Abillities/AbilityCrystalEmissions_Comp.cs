using System;
using RimWorld;
using Verse;

namespace Crystosentient.Abillities
{
	// Token: 0x02000025 RID: 37
	public class AbilityCrystalEmissions_Comp : CompAbilityEffect
	{
		// Token: 0x060000AA RID: 170 RVA: 0x0000554C File Offset: 0x0000374C
		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			GenTemperature.PushHeat(target.Cell, this.parent.pawn.MapHeld, this.Props.temperatureRange.RandomInRange);
			FleckMaker.ThrowDustPuff(target.Cell, this.parent.pawn.MapHeld, Rand.Range(1.5f, 3f));
			bool doExplosion = this.Props.doExplosion;
			if (doExplosion)
			{
				GenExplosion.DoExplosion(target.Cell, this.parent.pawn.MapHeld, this.Props.abilityRadius, this.Props.damageDef, null, this.Props.damageAmount, -1f, this.Props.soundExplosion, null, null, null, this.Props.postExplosionSpawnThingDef, 1f, 1, false, this.Props.preExplosionSpawnThingDef, 1f, 1, 0f, false, null, null);
			}
			bool hasThing = target.HasThing;
			Effecter effecter;
			if (hasThing)
			{
				effecter = this.Props.effecterDef.Spawn(target.Thing, this.parent.pawn.Map, this.Props.scale);
			}
			else
			{
				effecter = this.Props.effecterDef.Spawn(target.Cell, this.parent.pawn.Map, this.Props.scale);
			}
			bool flag = this.Props.maintainForTicks > 0;
			if (flag)
			{
				this.parent.AddEffecterToMaintain(effecter, target.Cell, this.Props.maintainForTicks, null);
			}
			else
			{
				effecter.Cleanup();
			}
		}

		// Token: 0x060000AB RID: 171 RVA: 0x00002714 File Offset: 0x00000914
		public override void DrawEffectPreview(LocalTargetInfo target)
		{
			GenDraw.DrawRadiusRing(target.Cell, this.Props.abilityRadius);
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x060000AC RID: 172 RVA: 0x00004CCC File Offset: 0x00002ECC
		public new CrystalSpawners_CompProperties Props
		{
			get
			{
				return (CrystalSpawners_CompProperties)this.props;
			}
		}
	}
}
