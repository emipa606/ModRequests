using System;
using RimWorld;
using Verse;

namespace TamingArtifact
{
	// Token: 0x02000003 RID: 3
	public class CompTargetEffect_Tame : CompTargetEffect
	{
		// Token: 0x06000006 RID: 6 RVA: 0x000020F4 File Offset: 0x000002F4
		public int RandomNumber(int min, int max)
		{
			Random random = new Random();
			return random.Next(min, max);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002114 File Offset: 0x00000314
		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			bool flag = pawn.AnimalOrWildMan() && pawn.Faction == null && pawn.RaceProps.wildness <= 1f && !pawn.IsPrisonerInPrisonCell();
			if (flag)
			{
				bool dead = pawn.Dead;
				if (!dead)
				{
					bool flag2 = this.RandomNumber(1, 10) == 1;
					if (flag2)
					{
						bool flag3 = this.RandomNumber(1, 2) == 1;
						if (flag3)
						{
							pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, true, false, false, null, false, false);
							string str = "LetterArtifactTameFail".Translate(pawn).CapitalizeFirst();
							Find.LetterStack.ReceiveLetter("LetterArtifactTameFailBerserk".Translate(pawn.KindLabel, pawn).CapitalizeFirst(), str, LetterDefOf.NegativeEvent, pawn, null, null, null, null);
						}
						else
						{
							string str2 = "LetterArtifactTameFail".Translate(pawn).CapitalizeFirst();
							Find.LetterStack.ReceiveLetter("LetterLabelForcedTameFail".Translate(pawn.KindLabel, pawn).CapitalizeFirst(), str2, LetterDefOf.NegativeEvent, pawn, null, null, null, null);
						}
					}
					else
					{
						pawn.SetFaction(Faction.OfPlayer, null);
						string str3 = "LetterArtifactTame".Translate(pawn).CapitalizeFirst();
						Find.LetterStack.ReceiveLetter("LetterLabelForcedTame".Translate(pawn.KindLabel, pawn).CapitalizeFirst(), str3, LetterDefOf.PositiveEvent, pawn, null, null, null, null);
					}
				}
			}
		}
	}
}
