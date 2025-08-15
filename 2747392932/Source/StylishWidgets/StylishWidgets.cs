using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace StylishWidgets
{
	// スタイリッシュなウィジェットをチェスト
    public class StylishWidgets
	{
		static int j = 0;
		public static bool addTextField = true;
		private static bool pressCtrl = false;
		private static bool pressShift = false;
		private static bool pushTab = false;
		public static int focusTarg = -1;
		static readonly List<string> textFieldList = new List<string>();

		// 数値入力フィールドを設定時にコントロール名を返すように
		public static string TextFieldNumeric<T>(Rect rect, ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
		{
			if (buffer == null)
			{
				buffer = val.ToString();
			}
			string text = string.Format("{0:0000}", textFieldList.Count) + "TextField" + GUIUtility.GetControlID(FocusType.Keyboard);
			GUI.SetNextControlName(text);
			string text2 = GUI.TextField(rect, buffer, Text.CurTextFieldStyle);
			if (GUI.GetNameOfFocusedControl() != text)
			{
				ResolveParseNow(buffer, ref val, ref buffer, min, max, force: true);
			}
			else if (text2 != buffer && IsPartiallyOrFullyTypedNumber(ref val, text2, min, max))
			{
				buffer = text2;
				if (IsFullyTypedNumber<T>(text2))
				{
					ResolveParseNow(text2, ref val, ref buffer, min, max, force: false);
				}
			}
			return text;
		}

		// 幅を二つに分けてラベル、数値入力エリア、スライダー、調整ボタンを２セット設置する
		// 最小最大値やボタンクリック時の加算値が同じもの用
		public static void SetTextAndSliderDouble(Listing_Standard l, float height, ref float value1, ref float value2, ref int i1, ref int i2, string label1, string label2, float min, float max, int round = 0, float inputMin = -9999, float inputMax = 9999, float add = 10, bool displayPer = false, bool loop = false, string toolTip = null)
		{
			SetTextAndSliderDouble(l, height, ref value1, ref value2, ref i1, ref i2, label1, label2, min, max, round, inputMin, inputMax, add, min, max, round, inputMin, inputMax, add, displayPer, displayPer, loop, loop, toolTip, toolTip);
		}
		// 幅を二つに分けてラベル、数値入力エリア、スライダー、調整ボタンを２セット設置する
		// 最小最大値やボタンクリック時の加算値が別のもの用
		public static void SetTextAndSliderDouble(Listing_Standard l, float height, ref float value1, ref float value2, ref int i1, ref int i2, string label1, string label2, float min1, float max1, int round1, float inputMin1, float inputMax1, float add1, float min2, float max2, int round2, float inputMin2, float inputMax2, float add2, bool displayPer1 = false, bool displayPer2 = false, bool loop1 = false, bool loop2 = false, string toolTip1 = null, string toolTip2 = null)
        {
			SetTextAndSliderDouble(l.GetRect(height), ref value1, ref value2, ref i1, ref i2, label1, label2, min1, max1, round1, inputMin1, inputMax1, add1, min2, max2, round2, inputMin2, inputMax2, add2, displayPer1, displayPer2, loop1, loop2, toolTip1, toolTip2);
			l.Gap(-1);
		}
		public static void SetTextAndSliderDouble(Rect r, ref float value1, ref float value2, ref int i1, ref int i2, string label1, string label2, float min1, float max1, int round1, float inputMin1, float inputMax1, float add1, float min2, float max2, int round2, float inputMin2, float inputMax2, float add2, bool displayPer1 = false, bool displayPer2 = false, bool loop1 = false, bool loop2 = false, string toolTip1 = null, string toolTip2 = null)
		{
			SetTextAndSliderRect(r.LeftPart(0.485f), ref value1, ref i1, label1, min1, max1, round1, inputMin1, inputMax1, add1, displayPer1, false, loop1, toolTip1);
			SetTextAndSliderRect(r.RightPart(0.485f), ref value2, ref i2, label2, min2, max2, round2, inputMin2, inputMax2, add2, displayPer2, false, loop2, toolTip2);
		}
		// そのままラベル、数値入力エリア、スライダー、調整ボタンを設置する
		public static void SetTextAndSlider(Listing_Standard l, float height, ref float value, ref int i, string label, float min, float max, int round = 0, float inputMin = -9999, float inputMax = 9999, float add = 10, bool displayPer = false, bool labelHalf = false, bool loop = false, string toolTip = null)
		{
			SetTextAndSliderRect(l.GetRect(height), ref value, ref i, label, min, max, round, inputMin, inputMax, add, displayPer, labelHalf, loop, toolTip);
			l.Gap(-1);
		}
		// そのままラベル、数値入力エリア、スライダー、調整ボタンを設置する
		public static void SetTextAndSliderRect(Rect r, ref float value, ref int i, string label, float min, float max, int round = 0, float inputMin = -9999, float inputMax = 9999, float add = 10, bool displayPer = false, bool labelHalf = false, bool loop = false, string toolTip = null)
		{
			float w = r.width * (labelHalf ? 0.485f : 1);
			float h = r.TopHalf().height;
			Rect r1 = new Rect(r.x, r.y, (float)Math.Round(w * 0.72f), h);
			Rect r2 = new Rect(r.x + r1.width, r.y, (float)Math.Round(w * 0.25f), h);
			Rect r3 = new Rect(r.x + r1.width + r2.width, r.y, (float)Math.Round(w * 0.05f), h);
			Rect bottomLeft = r.BottomHalf().LeftPart(labelHalf ? 0.925f : 0.85f);
			Rect bottomRight = r.BottomHalf().RightPart(labelHalf ? 0.075f : 0.15f);

			Widgets.Label(r1, label.Translate());
			string buffer = Math.Round(value, round, MidpointRounding.AwayFromZero).ToString();
			string field = TextFieldNumeric(r2, ref value, ref buffer, inputMin, inputMax);
			if (addTextField)
			{
				if (textFieldList.Contains(field))
				{
					EndRecordTextField();
                }
                else
				{
					textFieldList.Add(field);
				}
			}
			if (displayPer) Widgets.Label(r3, "%");
			value = (float)Math.Round(Widgets.HorizontalSlider(bottomLeft, value, min, max, true, null, null, null, 0), round, MidpointRounding.AwayFromZero);
			Widgets.ButtonText(bottomRight.LeftHalf(), "<", true, true, false);
			Widgets.ButtonText(bottomRight.RightHalf(), ">", true, true, false);
			ChangeValueByKey(ref add);
			if (i <= 0)
			{
				if ((i < 0) || (Mouse.IsOver(bottomRight.LeftHalf()) && Input.GetMouseButtonDown(0)))
				{
					i -= 1;
					if (i < j)
					{
						value -= add;
						value = (inputMin > value) ? (loop ? inputMax : inputMin) : value;
						if (j > -50)
						{
							j -= 50;
						}
						else if (j > -150)
						{
							j -= 10;
						}
						else if (j > -250)
						{
							j -= 5;
						}
						else
						{
							j -= 1;
						}
					}
					if (!Mouse.IsOver(bottomRight.LeftHalf()) || Input.GetMouseButtonUp(0))
					{
						i = j = 0;
					}
				}
				else
                {
					i = 0;
				}
			}

			if (i >= 0)
			{
				if ((i > 0) || (Mouse.IsOver(bottomRight.RightHalf()) && Input.GetMouseButtonDown(0)))
				{
					i += 1;
					if (i > j)
					{
						value += add;
						value = (inputMax < value) ? (loop ? inputMin : inputMax) : value;
						if (j < 50)
						{
							j += 50;
						}
						else if (j < 150)
						{
							j += 10;
						}
						else if (j < 250)
						{
							j += 5;
						}
						else
						{
							j += 1;
						}
					}
					if (!Mouse.IsOver(bottomRight.RightHalf()) || Input.GetMouseButtonUp(0))
					{
						i = j = 0;
					}
				}
				else
				{
					i = 0;
				}
			}

			TooltipHandler.TipRegion(bottomRight, "Ctrl * 10, Shift * 0.1");

			if (toolTip != null) TooltipHandler.TipRegion(r, toolTip.Translate());
		}
		private static void ChangeValueByKey(ref float value)
		{
			if (PressCtrl) value *= 10;
			if (PressShift) value *= 0.1f;
		}
		// リストの数だけ横に分割してボタンを追加。
		// hasMenuはSay Yes
		public static void SetButtons(Listing_Standard l, float height, List<Action> a, List<string> labels = null, List<bool> hasMenu = null) => SetButtons(l.GetRect(height), a, labels, hasMenu);
		public static void SetButtons(Rect r, List<Action> a, List<string> labels = null, List<bool> hasMenu = null)
		{
			if (hasMenu == null)
            {
				hasMenu = new List<bool>();
				for (int i = 0; i < a.Count; i++)
                {
					hasMenu.Add(false);
                }
            }
			if (labels == null)
            {
				labels = new List<string>();
				for (int i = 0; i < a.Count; i++)
				{
					labels.Add(string.Empty);
				}
			}
			int count = labels.Count();
			float width = r.width / count;
			for (int i = 0; i < count; i++)
			{
				if (Widgets.ButtonText(new Rect(r.x + width * i, r.y, width * 0.9f, r.height), labels[i].Translate()))
				{
					if (hasMenu[i])
					{
						Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>() { new FloatMenuOption("Yes", a[i]) }));
					}
					else
					{
						a[i]();
					}
				}
			}
		}

		// リストの数だけ横に分割してラジオボタンを追加。
		// 戻り値がint
		public static int SetRadioButtons(Listing_Standard l, float height, List<string> labels, int now)
        {
			return SetRadioButtons(l.GetRect(height), labels, now);
        }
		public static int SetRadioButtons(Rect r, List<string> labels, int now)
		{
			int count = labels.Count();
			float width = r.width / count;
			for (int i = 0; i < count; i++)
			{
				if (Widgets.RadioButtonLabeled(new Rect(r.x + width * i, r.y, width * 0.9f, r.height), labels.ElementAt(i), now == i))
				{
					if (now != i)
					{
						if (!GUI.GetNameOfFocusedControl().NullOrEmpty())
                        {
							GUI.FocusControl(textFieldList[0]);
						}
						ResetTabDown();
					}
					return i;
				}
			}
			return now;
		}
		private static bool CtrlDown => Event.current.type == EventType.KeyDown && (Event.current.keyCode == (KeyCode.LeftControl) || Event.current.keyCode == (KeyCode.RightControl));
		private static bool ShiftDown => Event.current.type == EventType.KeyDown && (Event.current.keyCode == (KeyCode.LeftShift) || Event.current.keyCode == (KeyCode.RightShift));
		private static bool CtrlUp => Event.current.type == EventType.KeyUp && (Event.current.keyCode == (KeyCode.LeftControl) || Event.current.keyCode == (KeyCode.RightControl));
		private static bool ShiftUp => Event.current.type == EventType.KeyUp && (Event.current.keyCode == (KeyCode.LeftShift) || Event.current.keyCode == (KeyCode.RightShift));
		private static bool CtrlKeyIs()
		{
			if (CtrlDown) return true;
			if (CtrlUp) return false;
			return PressCtrl;
		}
		private static bool ShiftKeyIs()
		{
			if (ShiftDown) return true;
			if (ShiftUp) return false;
			return PressShift;
		}
		private static bool TabDown => Event.current.keyCode == KeyCode.Tab && Event.current.type == EventType.KeyDown;

		// 数値入力エリアの数が変わるときに一旦リセットしたい
		public static void ResetTabDown()
        {
			pushTab = false;
			focusTarg = -1;
		}
		// Ctrlが押されているか
		public static bool PressCtrl => pressCtrl;
		// Shiftが押されているか
		public static bool PressShift => pressShift;
		// DoSettingsWindowContents 内で走らせてCtrlとShiftの入力状況を監視する
		public static void HoldKeys()
		{
			pressCtrl = CtrlKeyIs();
			pressShift = ShiftKeyIs();
		}
		// DoSettingsWindowContents の最後に走らせてTabキーの移動をチェストする
		public static void MoveFocusNext()
		{
			if (!GUI.GetNameOfFocusedControl().NullOrEmpty())
			{
				if (TabDown)
				{
					if (!pushTab)
					{
						if (!pushTab) focusTarg = textFieldList.IndexOf(GUI.GetNameOfFocusedControl());
						pushTab = true;
						GUI.FocusControl("");
					}
				}
			}
			if (pushTab && focusTarg != -1)
			{
				if (!TabDown && !Input.GetKey(KeyCode.Tab))
				{
					if (PressShift)
                    {
						if (--focusTarg < 0) focusTarg = textFieldList.Count - 1;
					}
                    else
                    {
						if (++focusTarg > textFieldList.Count - 1) focusTarg = 0;
                    }
					GUI.FocusControl(textFieldList[focusTarg]);
					ResetTabDown();
				}
			}
		}
		// DoSettingsWindowContents の最初に走らせて数値入力エリアをリスト化する
		public static void BeginRecordTextField()
		{
			addTextField = true;
			textFieldList.Clear();
		}
		// なんかの弾みに止めたい場合
		public static void EndRecordTextField()
		{
			addTextField = false;
		}
		// CtrlShiftの監視と数値入力エリアのリスト化をRunするわかりやすい名前の
		public static void Begin()
        {
			HoldKeys();
			BeginRecordTextField();
        }
		// 現状はタブ移動のみを走らせるわかりやすい名前
		public static void End()
        {
			MoveFocusNext();
        }





		//////////////////以下数値入力エリア用のメソ///////////////////////
		private static bool IsPartiallyOrFullyTypedNumber<T>(ref T val, string s, float min, float max)
		{
			if (s == "")
			{
				return true;
			}

			if (s[0] == '-' && min >= 0f)
			{
				return false;
			}

			if (s.Length > 1 && s[s.Length - 1] == '-')
			{
				return false;
			}

			if (s == "00")
			{
				return false;
			}

			if (s.Length > 12)
			{
				return false;
			}

			if (typeof(T) == typeof(float) && CharacterCount(s, '.') <= 1 && ContainsOnlyCharacters(s, "-.0123456789"))
			{
				return true;
			}

			if (IsFullyTypedNumber<T>(s))
			{
				return true;
			}

			return false;
		}
		private static bool IsFullyTypedNumber<T>(string s)
		{
			if (s == "")
			{
				return false;
			}

			if (typeof(T) == typeof(float))
			{
				string[] array = s.Split('.');
				if (array.Length > 2 || array.Length < 1)
				{
					return false;
				}

				if (!ContainsOnlyCharacters(array[0], "-0123456789"))
				{
					return false;
				}

				if (array.Length == 2 && (array[1].Length == 0 || !ContainsOnlyCharacters(array[1], "0123456789")))
				{
					return false;
				}
			}

			if (typeof(T) == typeof(int) && !ContainsOnlyCharacters(s, "-0123456789"))
			{
				return false;
			}

			return true;
		}
		private static void ResolveParseNow<T>(string edited, ref T val, ref string buffer, float min, float max, bool force)
		{
			if (typeof(T) == typeof(int))
			{
				int result;
				if (edited.NullOrEmpty())
				{
					ResetValue(edited, ref val, ref buffer, min, max);
				}
				else if (int.TryParse(edited, out result))
				{
					val = (T)(object)Mathf.RoundToInt(Mathf.Clamp(result, min, max));
					buffer = ToStringTypedIn(val);
				}
				else if (force)
				{
					ResetValue(edited, ref val, ref buffer, min, max);
				}
			}
			else if (typeof(T) == typeof(float))
			{
				if (float.TryParse(edited, out float result2))
				{
					val = (T)(object)Mathf.Clamp(result2, min, max);
					buffer = ToStringTypedIn(val);
				}
				else if (force)
				{
					ResetValue(edited, ref val, ref buffer, min, max);
				}
			}
			else
			{
				Log.Error("TextField<T> does not support " + typeof(T));
			}
		}
		private static void ResetValue<T>(string edited, ref T val, ref string buffer, float min, float max)
		{
			val = default(T);
			if (min > 0f)
			{
				val = (T)(object)Mathf.RoundToInt(min);
			}

			if (max < 0f)
			{
				val = (T)(object)Mathf.RoundToInt(max);
			}

			buffer = ToStringTypedIn(val);
		}
		private static bool ContainsOnlyCharacters(string s, string allowedChars)
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (!allowedChars.Contains(s[i]))
				{
					return false;
				}
			}

			return true;
		}
		private static int CharacterCount(string s, char c)
		{
			int num = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == c)
				{
					num++;
				}
			}

			return num;
		}
		private static string ToStringTypedIn<T>(T val)
		{
			if (typeof(T) == typeof(float))
			{
				return ((float)(object)val).ToString("0.##########");
			}

			return val.ToString();
		}
	}
}
