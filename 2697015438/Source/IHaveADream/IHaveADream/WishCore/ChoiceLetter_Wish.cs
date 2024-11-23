using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class ChoiceLetter_Wish : ChoiceLetter
	{
		public Wish wish;
		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				yield return base.Option_Close;
				if (wish != null && wish.ageTicks < WishUtility.Def.dayToDismiss * GenDate.TicksPerDay)
				{
					yield return DismissWish();
				}
			}
		}

		private DiaOption DismissWish()
		{
			DiaOption diaOption = new DiaOption(TranslationKey.WISH_DISMISS_OPTION.Translate());
			diaOption.action = delegate
			{
				wish.pawn.wishes().DismissWish(wish);
				Find.LetterStack.RemoveLetter(this);
			};
			diaOption.resolveTree = true;
			if (wish == null || wish.ageTicks > WishUtility.Def.dayToDismiss * GenDate.TicksPerDay)
			{
				diaOption.Disable(TranslationKey.WISH_DISMISS_TO_OLD.Translate());
			}
			return diaOption;
		}
	}
}
