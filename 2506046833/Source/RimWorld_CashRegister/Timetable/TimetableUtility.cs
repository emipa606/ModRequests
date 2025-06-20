using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CashRegister.Timetable
{
	public static class TimetableUtility
	{
		private static bool lastAssignmentValue;

		public static void DoCell(Rect rect, TimetableBool timetable, Map map)
		{
			//if (pawn.timetable != null)
			{
				float num = rect.x;
				float num2 = rect.width / 24f;
				for (int i = 0; i < 24; i++)
				{
					Rect rect2 = new Rect(num, rect.y, num2, rect.height);
					DoTimeAssignment(rect2, timetable, i, map);
					num += num2;
				}

				//GUI.color = Color.white;
				//if (TimeAssignmentSelector.selectedAssignment != null)
				//{
				//	UIHighlighter.HighlightOpportunity(rect, "TimeAssignmentTableRow-If" + TimeAssignmentSelector.selectedAssignment.defName + "Selected");
				//}
			}
		}

		public static void DoHeader(Rect rect)
		{
			float num = rect.x;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.LowerCenter;
			float num2 = rect.width / 24f;
			for (int i = 0; i < 24; i++)
			{
				Widgets.Label(new Rect(num, rect.y, num2, rect.height + 3f), i.ToString());
				num += num2;
			}

			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private static void DoTimeAssignment(Rect rect, TimetableBool table, int hour, Map map)
		{
			if(table == null) 
			{
				Log.Error("Table must not be null.");
				return;
			}
			rect = rect.ContractedBy(1f);
			bool mouseButton = Input.GetMouseButton(0);
			bool assignment = table.GetAssignment(hour);
			GUI.DrawTexture(rect, table.GetTexture(assignment, GenLocalDate.HourOfDay(map) == hour));
			if (!mouseButton)
			{
				MouseoverSounds.DoRegion(rect);
			}

			if (!Mouse.IsOver(rect))
			{
				return;
			}

			if (!mouseButton)
			{
				lastAssignmentValue = !assignment;
			}

			Widgets.DrawBox(rect, 2);
			if (mouseButton && assignment != lastAssignmentValue)
			{
				SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera();
				table.SetAssignment(hour, lastAssignmentValue);
			}
		}
	}
}
