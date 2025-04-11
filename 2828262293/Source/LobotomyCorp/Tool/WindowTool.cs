using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;


namespace LobotomyCorp.Tool
{
    [StaticConstructorOnStartup]
    public static class WindowTool
    {
        public static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);

        public static readonly Texture2D Plus = ContentFinder<Texture2D>.Get("UI/Buttons/Plus", true);
        public static readonly Texture2D Minus = ContentFinder<Texture2D>.Get("UI/Buttons/Minus", true);


        public static Rect Square(this Rect r)
        {
            return new Rect(r.x, r.y, Math.Min(r.width, r.height), Math.Min(r.width, r.height));
        }

        public static Rect Square(this Rect r, float f)
        {
            return new Rect(r.x, r.y, f, f);
        }

        public static void StringCheckBox(Rect rect, string label, ref bool flag, bool drawDesc = true)
        {
            Rect labelRect = rect;
            labelRect.width = rect.width - 35f;
            Widgets.Label(labelRect, label.Translate());

            Rect checkRect = rect;
            checkRect.xMin += labelRect.width - 5f;
            checkRect.width = 30f;
            bool before = flag;
            Widgets.Checkbox(checkRect.x, checkRect.y, ref flag);

            if (Mouse.IsOver(rect) && !drawDesc)
            {
                Widgets.DrawHighlight(rect);
            }
            if (drawDesc) TooltipDescription(rect, (label + ".text").Translate(), true);
        }

        public static float TextSlider(Rect rect, string label, ref float now, bool showVal = true)
        {
            TooltipDescription(rect, (label + ".text").Translate(), true);

            Rect numRect = rect;
            numRect.height = Text.LineHeight * 1.1f;
            Widgets.Label(numRect, label.Translate());

            numRect.x += rect.width - 30f;
            numRect.width = 30f;
            if (showVal) Widgets.Label(numRect, (now * 100).ToString("F0") + "%");


            Rect sliderRect = rect;
            sliderRect.y += numRect.height;
            sliderRect.height = Text.LineHeight * 1.1f;
            now = Widgets.HorizontalSlider(sliderRect, now, 0, 1, roundTo: 0.01f);
            return now;
        }

        public static void RightNumField(Rect rect, string label, ref int num, ref string str, int min = 0, int max = 1)
        {
            Widgets.Label(rect, label.Translate().Truncate(rect.width - 100f));

            Rect butRect = rect.Square();
            butRect.x += rect.width - 100f;

            if (Widgets.ButtonImage(butRect, WindowTool.Plus))
            {
                num = Math.Min(max, num + 1);
                str = num.ToString();
            }
            butRect.x += butRect.height;
            if (Widgets.ButtonImage(butRect, WindowTool.Minus))
            {
                num = Math.Max(min, num - 1);
                str = num.ToString();
            }

            Rect valRect = butRect;
            valRect.x += butRect.width;
            valRect.width = 50f;
            Widgets.TextFieldNumeric(valRect, ref num, ref str, min, max);

            TooltipDescription(rect,
                (Description(label)).Translate(), true);
        }

        public static void LabelButton(Rect rect, string label, Action act, string buttonLabel = null)
        {
            Widgets.Label(rect, label);

            Rect buttonRect = rect;
            buttonRect.xMin += rect.width - 50f;
            /*if (Widgets.ButtonText(buttonRect, buttonLabel ?? " ")) // idk how to fix this
            {
                act.Invoke();
            }*/
        }

        public static void IconLabel(Rect rect, Texture2D icon, string text)
        {
            Rect iconRect = rect.Square();
            Widgets.DrawTextureFitted(iconRect, icon, 1f);

            Rect labelRect = rect;
            labelRect.xMin += iconRect.width;
            Widgets.Label(labelRect, text);
        }

        public static void TextScrollView(Rect rect,ref Vector2 pos, string text)
        {
            Rect view = rect.ContractedBy(5f);
            view.width  -= 7f;
            view.height = Text.CalcHeight(text,view.width);

            if(rect.height > view.height)
            {
                rect.x += 5f;
                Widgets.Label(rect , text);
                return;
            }
            else
            {

            }

            Widgets.BeginScrollView(rect, ref pos, view);
            Widgets.Label(view,text);
            Widgets.EndScrollView();
        }

        public static void TooltipDescription(Rect rect, string str, bool high = false)
        {
            if (Mouse.IsOver(rect))
            {
                if (high) Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, () => str, (int)rect.x ^ 810);
            }
        }

        public static string Description(string str) => str + ".text";

        public static string DescriptionBuilder(string label)
        {
            return label.Translate() + ":\n" + (label + ".text").Translate();
        }

        public static string colorString(string color, string str)
        {
            return "<color=" + color + ">" + str + "</color>";
        }


        public static void ColorAction(Color c, Action act)
        {
            Color tmp = GUI.color;
            GUI.color = c;
            act.Invoke();
            GUI.color = tmp;
        }
    }

}
