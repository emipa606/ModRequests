using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace UniqueItems
{
	[StaticConstructorOnStartup]
    public class UniqueItemsLocator : Window
    {
		private Vector2 scrollPosition;
		public static readonly Texture2D Locator = ContentFinder<Texture2D>.Get("UI/Buttons/UISpyglass");
		public static readonly Texture2D CloseXSmall = ContentFinder<Texture2D>.Get("UI/Widgets/CloseXSmall");
		public static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete");

		[TweakValue("0TRADING", 0, 1000)] private static float maxWidth = 800;
		[TweakValue("0TRADING", 0, 1000)] private static float maxHeight = 600;

		[TweakValue("0TRADING", 0, 100)] private static float cellHeight = 35f;

		[TweakValue("0TRADING", 0, 1000)] private static float thingIconXOffset = 30;
		[TweakValue("0TRADING", 0, 1000)] private static float thingIconWidth = 30;

		[TweakValue("0TRADING", 0, 1000)] private static float infoCardXOffset = 5;
		[TweakValue("0TRADING", 0, 1000)] private static float infoCardYOffset = 5;

		[TweakValue("0TRADING", 0, 1000)] private static float thingLabelXOffset = 35;
		[TweakValue("0TRADING", 0, 1000)] private static float thingLabelWidth = 580;

		[TweakValue("0TRADING", 0, 1000)] private static float buttonDistance = 5f;
		[TweakValue("0TRADING", 0, 1000)] private static float buttonMargin = 5f;

		[TweakValue("0TRADING", 0, 1000)] private static float closeButtonSize = 15f;
		public override Vector2 InitialSize => new Vector2(maxWidth, Mathf.Min(maxHeight, UI.screenHeight));
		public override void DoWindowContents(Rect inRect)
        {
			var closeButtonRect = new Rect(inRect.width - closeButtonSize, inRect.y, closeButtonSize, closeButtonSize);
			if (Widgets.ButtonImage(closeButtonRect, CloseXSmall))
            {
				this.Close();
				return;
            }
			var cachedItemsHash = new HashSet<Thing>();
			foreach (var list in CompUniqueItem.Instances.Values)
            {
				cachedItemsHash.AddRange(list);
			}
			var cachedItems = cachedItemsHash.ToList();
			Vector2 tablePos = Vector2.zero;
			tablePos.y += closeButtonSize + 5f;
			float listHeight = cachedItems.Count * cellHeight;
			Rect viewRect = new Rect(tablePos.x, tablePos.y, inRect.width, (inRect.height - tablePos.y) - 20);
			Rect scrollRect = new Rect(tablePos.x, tablePos.y, inRect.width - 16f, listHeight);
			Widgets.BeginScrollView(viewRect, ref scrollPosition, scrollRect);
			GUI.color = new Color(1f, 1f, 1f, 0.2f);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.Font = GameFont.Small;
			float num = 0f;

			for (int j = 0; j < cachedItems.Count; j++)
			{
				if (num > scrollPosition.y - cellHeight && num < scrollPosition.y + viewRect.height)
				{
					var thing = cachedItems[j];
					Rect rect2 = new Rect(0f, tablePos.y, viewRect.width, cellHeight);

					if (j % 2 == 1)
					{
						Widgets.DrawLightHighlight(rect2);
					}

					var thingIconRect = new Rect(rect2.x + thingIconXOffset, rect2.y, thingIconWidth, rect2.height);
					Widgets.ThingIcon(thingIconRect, thing);
					Widgets.InfoCardButton(thingIconRect.xMax + infoCardXOffset, rect2.y + infoCardYOffset, thing);
					var thingLabelRect = new Rect(thingIconRect.xMax + thingLabelXOffset, rect2.y, thingLabelWidth, rect2.height);
					var thingLabel = thing.LabelCap;
					if (thing.def.modContentPack?.Name != null)
                    {
						thingLabel += " (" + thing.def.modContentPack.Name + ")";
					}
					Widgets.Label(thingLabelRect, thingLabel);

					var deleteRect = new Rect(thingLabelRect.xMax + buttonDistance, rect2.y, rect2.height, rect2.height).ContractedBy(buttonMargin);
					if (Widgets.ButtonImage(deleteRect, DeleteX))
					{
						Find.WindowStack.Add(new Dialog_MessageBox("DM.DeleteThing".Translate(ColoredText.Colorize(thing.LabelCap, Color.red)), "Yes".Translate(), delegate ()
						{
							thing.ParentHolder?.GetDirectlyHeldThings()?.Remove(thing);
							thing.Destroy();
						}, "No".Translate()));
					}
					var locatorRect = new Rect(deleteRect.xMax + buttonDistance, rect2.y, rect2.height, rect2.height).ContractedBy(buttonMargin);
					if (Widgets.ButtonImage(locatorRect, Locator))
                    {
						CameraJumper.TryJumpAndSelect(thing);
					}
				}
				tablePos.y += cellHeight;
				num += cellHeight;
			}
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			Widgets.EndScrollView();
		}
    }
}
