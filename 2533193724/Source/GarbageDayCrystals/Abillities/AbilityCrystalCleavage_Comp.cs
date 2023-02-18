using System;
using RimWorld;
using Verse;

namespace Crystosentient.Abillities
{
	// Token: 0x02000024 RID: 36
	public class AbilityCrystalCleavage_Comp : CompAbilityEffect
	{
		// Token: 0x060000A6 RID: 166 RVA: 0x000052EC File Offset: 0x000034EC
		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			PawnKindDef pawnKindDef = this.Props.spawnPawnRandom.RandomElement<PawnKindDef>();
			float randomInRange = this.Props.pawnCountRange.RandomInRange;
			float randomInRange2 = this.Props.morePawnsCountRange.RandomInRange;
			float randomInRange3 = this.Props.morePawnsCountRangeTwo.RandomInRange;
			bool flag = this.Props.morePawnsTerm != null && pawnKindDef.defName.Contains(this.Props.morePawnsTerm);
			if (flag)
			{
				int num = 0;
				while ((float)num < randomInRange2)
				{
					PawnKindDef pawnKindDef2 = this.Props.spawnPawnRandom.RandomElement<PawnKindDef>();
					bool flag2 = pawnKindDef2.defName.Contains(this.Props.morePawnsTerm);
					if (flag2)
					{
						GenSpawn.Spawn(PawnGenerator.GeneratePawn(pawnKindDef2, Faction.OfPlayer), target.Cell, this.parent.pawn.Map, WipeMode.Vanish);
						num++;
					}
				}
			}
			bool flag3 = this.Props.morePawnsTermTwo != null && pawnKindDef.defName.Contains(this.Props.morePawnsTermTwo);
			if (flag3)
			{
				int num2 = 0;
				while ((float)num2 < randomInRange3)
				{
					PawnKindDef pawnKindDef3 = this.Props.spawnPawnRandom.RandomElement<PawnKindDef>();
					bool flag4 = pawnKindDef3.defName.Contains(this.Props.morePawnsTermTwo);
					if (flag4)
					{
						GenSpawn.Spawn(PawnGenerator.GeneratePawn(pawnKindDef3, Faction.OfPlayer), target.Cell, this.parent.pawn.Map, WipeMode.Vanish);
						num2++;
					}
				}
			}
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x0000548C File Offset: 0x0000368C
		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			bool flag = target.Cell.Filled(this.parent.pawn.Map) || (target.Cell.GetFirstBuilding(this.parent.pawn.Map) != null && !this.Props.allowOnBuildings);
			bool result;
			if (flag)
			{
				if (throwMessages)
				{
					Messages.Message("AbilityOccupiedCells".Translate(this.parent.def.LabelCap), target.ToTargetInfo(this.parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
				}
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x060000A8 RID: 168 RVA: 0x00004CCC File Offset: 0x00002ECC
		public new CrystalSpawners_CompProperties Props
		{
			get
			{
				return (CrystalSpawners_CompProperties)this.props;
			}
		}
	}
}
