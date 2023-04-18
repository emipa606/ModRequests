using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SimpleWarrants
{
    [HotSwapAll]
	public class Dialog_SelectAnimal : Window
	{
        public override Vector2 InitialSize => new Vector2(620f, 500f);
        public List<PawnKindDef> allAnimalDefs;
        private readonly MainTabWindow_Warrants parent;
        private Vector2 scrollPosition;

        string searchKey;

        public Dialog_SelectAnimal(MainTabWindow_Warrants parent)
		{
			doCloseButton = true;
			doCloseX = true;
			closeOnClickedOutside = false;
			absorbInputAroundWindow = false;
			allAnimalDefs = Utils.AllWorthAnimalDefs.ToList();
			this.parent = parent;
		}

        public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			var searchLabel = new Rect(inRect.x, inRect.y, 60, 24);
			Widgets.Label(searchLabel, "SW.Search".Translate());
			var searchRect = new Rect(searchLabel.xMax + 5, searchLabel.y, 200, 24f);
			searchKey = Widgets.TextField(searchRect, searchKey);
			Text.Anchor = TextAnchor.UpperLeft;

			Rect outRect = new Rect(inRect);
			outRect.y = searchRect.yMax + 5;
			outRect.yMax -= 70f;
			outRect.width -= 16f;

			var thingDefs = searchKey.NullOrEmpty() ? allAnimalDefs : allAnimalDefs.Where(x => x.label.ToLower().Contains(searchKey.ToLower())).ToList();

			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, thingDefs.Count() * 35f);
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			try
			{
				float num = 0f;
				foreach (PawnKindDef pawnkindDef in thingDefs.OrderBy(x => x.label))
				{
					Rect iconRect = new Rect(0f, num, 24, 32);
					Widgets.InfoCardButton(iconRect, pawnkindDef);
					iconRect.x += 24;
					Widgets.ThingIcon(iconRect, pawnkindDef.race);
					Rect rect = new Rect(iconRect.xMax + 5, num, viewRect.width * 0.7f, 32f);
					Text.Anchor = TextAnchor.MiddleLeft;
					Widgets.Label(rect, pawnkindDef.LabelCap);
					Text.Anchor = TextAnchor.UpperLeft;
					rect.x = rect.xMax + 10;
					rect.width = 100;
					if (Widgets.ButtonText(rect, "SW.Select".Translate()))
					{
						parent.AssignAnimal(PawnGenerator.GeneratePawn(pawnkindDef));
						SoundDefOf.Click.PlayOneShotOnCamera();
						Close();
					}
					num += 35f;
				}
			}
			finally
			{
				Widgets.EndScrollView();
			}
		}
    }
}
