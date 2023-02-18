using System;
using RimWorld;
using Verse;

namespace Crystosentient.Abillities
{
	// Token: 0x02000026 RID: 38
	public class AbilityCrystalMunitions_Comp : CompAbilityEffect
	{
		// Token: 0x060000AE RID: 174 RVA: 0x0000570C File Offset: 0x0000390C
		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			ThingDef thingDef = this.Props.spawnThingRandom.RandomElement<ThingDef>();
			float randomInRange = this.Props.moreThingsCountRange.RandomInRange;
			float randomInRange2 = this.Props.moreThingsCountRangeTwo.RandomInRange;
			bool flag = this.Props.moreThingsTerm != null && thingDef.defName.Contains(this.Props.moreThingsTerm);
			if (flag)
			{
				int num = 0;
				while ((float)num < randomInRange)
				{
					ThingDef thingDef2 = this.Props.spawnThingRandom.RandomElement<ThingDef>();
					bool flag2 = thingDef2.defName.Contains(this.Props.moreThingsTerm);
					if (flag2)
					{
						Thing thing = ThingMaker.MakeThing(thingDef2, null);
						ThingSetMakerUtility.AssignQuality(thing, new QualityGenerator?(QualityGenerator.BaseGen));
						GenPlace.TryPlaceThing(thing, target.Cell, this.parent.pawn.Map, ThingPlaceMode.Near, null, null, default(Rot4));
						num++;
					}
				}
			}
			else
			{
				bool flag3 = this.Props.moreThingsTermTwo != null && thingDef.defName.Contains(this.Props.moreThingsTermTwo);
				if (flag3)
				{
					int num2 = 0;
					while ((float)num2 < randomInRange2)
					{
						ThingDef thingDef3 = this.Props.spawnThingRandom.RandomElement<ThingDef>();
						bool flag4 = thingDef3.defName.Contains(this.Props.moreThingsTermTwo);
						if (flag4)
						{
							Thing thing2 = ThingMaker.MakeThing(thingDef3, null);
							ThingSetMakerUtility.AssignQuality(thing2, new QualityGenerator?(QualityGenerator.BaseGen));
							GenPlace.TryPlaceThing(thing2, target.Cell, this.parent.pawn.Map, ThingPlaceMode.Near, null, null, default(Rot4));
							num2++;
						}
					}
				}
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060000AF RID: 175 RVA: 0x00004CCC File Offset: 0x00002ECC
		public new CrystalSpawners_CompProperties Props
		{
			get
			{
				return (CrystalSpawners_CompProperties)this.props;
			}
		}
	}
}
