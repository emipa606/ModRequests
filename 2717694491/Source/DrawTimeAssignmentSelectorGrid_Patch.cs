using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomSchedules
{
    [HarmonyPatch(typeof(TimeAssignmentSelector), "DrawTimeAssignmentSelectorGrid")]
	public static class DrawTimeAssignmentSelectorGrid_Patch
	{
        /// <summary>
        /// A reference to our settings.
        /// </summary>
        public static CSModSettings settings;

        public static void Postfix(Rect rect)
		{
            if (settings == null)
            {
                settings = CSModStarter.settings;
            }

            rect.yMax -= 2f;
			Rect rect2 = rect;
			rect2.xMax = rect2.center.x;
			rect2.yMax = rect2.center.y;
			rect2.x += 4f * rect2.width;

			if (ModsConfig.RoyaltyActive)
			{
				rect2.x += rect2.width;
			}

            float rectHeight = rect2.height;
            float rectHeightB = rectHeight / 3.0f * 2.0f; ;

            DrawTimeAssignmentSelectorFor(rect2, CSDefOf.CS_SchedA);
            rect2.y += rectHeight;
            rect2.height = rectHeightB;
            DrawTimeAssignmentSelectorFor(rect2, CSDefOf.CS_SchedB);
            rect2.height = rectHeight;
            rect2.x += rect2.width;
            rect2.y -= rectHeight;
            DrawTimeAssignmentSelectorFor(rect2, CSDefOf.CS_SchedC);
            rect2.y += rectHeight;
            rect2.height = rectHeightB;
            DrawTimeAssignmentSelectorFor(rect2, CSDefOf.CS_SchedD);
            rect2.height = rectHeight;
            rect2.x += rect2.width;
            rect2.y -= rectHeight;
            DrawTimeAssignmentSelectorFor(rect2, CSDefOf.CS_SchedE);
            rect2.y += rectHeight;
            rect2.height = rectHeightB;
            DrawTimeAssignmentSelectorFor(rect2, CSDefOf.CS_SchedF);
        }

		public static void DrawTimeAssignmentSelectorFor(Rect rect, TimeAssignmentDef ta)
		{
            if (!ButtonEnabled(ta))
                return;

            rect = rect.ContractedBy(2f);
			GUI.DrawTexture(rect, ta.ColorTexture);
			if (Widgets.ButtonInvisible(rect))
			{
				TimeAssignmentSelector.selectedAssignment = ta;
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			GUI.color = Color.white;
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}

			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			GUI.color = Color.white;
            
			Widgets.Label(rect, ta.label);
			Text.Anchor = TextAnchor.UpperLeft;
			if (TimeAssignmentSelector.selectedAssignment == ta)
			{
				Widgets.DrawBox(rect, 2);
			}
			else
			{
				UIHighlighter.HighlightOpportunity(rect, ta.cachedHighlightNotSelectedTag);
			}
		}

        public static bool ButtonEnabled(TimeAssignmentDef ta)
        {
            if (ta == CSDefOf.CS_SchedA)
                return settings.enableScheduleA;
            if (ta == CSDefOf.CS_SchedB)
                return settings.enableScheduleB;
            if (ta == CSDefOf.CS_SchedC)
                return settings.enableScheduleC;
            if (ta == CSDefOf.CS_SchedD)
                return settings.enableScheduleD;
            if (ta == CSDefOf.CS_SchedE)
                return settings.enableScheduleE;
            if (ta == CSDefOf.CS_SchedF)
                return settings.enableScheduleF;

            return false;
        }
    }
}
