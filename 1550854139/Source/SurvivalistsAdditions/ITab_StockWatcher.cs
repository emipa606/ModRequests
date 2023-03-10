using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace SurvivalistsAdditions {

	public class ITab_StockWatcher : ITab {

		private int minimumToAdd = 1;
		public int MinimumToAdd => minimumToAdd;


		public ITab_StockWatcher() {
			size = new Vector2(300f, 100f);
			labelKey = Static.Label_ITabCharPit;
		}


		protected override void FillTab() {
			Text.Font = GameFont.Small;
			Rect fullRect = GenUI.GetInnerRect(new Rect(10f, 10f, size.x - 10, size.y - 10));
			Rect leftRect = fullRect.LeftHalf().Rounded();
			Rect rightRect = fullRect.RightHalf().Rounded();
			GUI.BeginGroup(fullRect);

			Widgets.Label(leftRect, "Pause at: ");
			minimumToAdd = (int)Widgets.HorizontalSlider(rightRect, minimumToAdd, 1, 50);

			GenUI.AbsorbClicksInRect(fullRect);
			GUI.EndGroup();
		}
	}
}
