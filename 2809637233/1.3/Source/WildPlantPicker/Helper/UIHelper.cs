using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace WildPlantPicker.Helper
{
    [StaticConstructorOnStartup]
    internal static class UIHelper
    {
        public const float TOP_SECTION_HEIGHT = 50f;
        public const float BARRIER_SECTION_HEIGHT = 65f;
        public const float BOTTOM_SECTION_HEIGHT = 65f;
        public const float WINDOW_PADDING_RIGHT = 45;
        public const float WINDOW_PADDING_BOTTOM = 45;

        public const float MARGIN_TOP = 5f;
        public const float MARGIN_LEFT = 5f;
        public const float MARGIN_BOTTOM = 5f;
        public const float MARGIN_HEADER = 10f;
        public const float MARGIN_TAB = 32f;
        public const float MARGIN_GRID_COLUMN = 15f;
        public const float MARGIN_SCROLL_BAR = 25f;
        public const float MARGIN_RADIO = 200f;
        public const float SIZE_IMAGE = 256f;
        public const float SIZE_BUTTON = 30f;
        public const float SIZE_ICON = 30f;
        public const float SIZE_ICON_TINY = 24f;

        public const float HEIGHT_ROW = 30f;
        public const float HEIGHT_ROW_GRID = 30f;
        public const float HEIGHT_ROW_GRID_WIDE = 45f;
        public const float HEIGHT_SECTION_TITLE = 30f;
        public const float HEIGHT_HEADER = 40f;
        public const float WIDTH_CHECKBOX = 30f;
        public const float WIDTH_BUTTON = 30f;

        public const float LABEL_WIDTH_MIN = 125;
        public const float INPUT_WIDTH_MIN = 125;


        public const float WIDTH_QUARTER = 187.5f;
        public const float WIDTH_THREE_QUARTERS = 562.5f;
        public const float WIDTH_THREE_QUARTERS_HALF = 281.25f;
        public const float WIDTH_TWO_QUARTERS = 375f;


        public const float LABEL_WIDTH_HALF = 375f;
        public const float INPUT_WIDTH_HALF = 375f;
        public static readonly Color SUB_SECTION_BACKGROUND_ACTIVE_COLOR = Color.HSVToRGB(0.675f, 0.75f, 0.05f);
        public static readonly Color SUB_SECTION_BACKGROUND_DEACTIVE_COLOR = Color.HSVToRGB(0f, 0f, 0.25f);

        public static void ScrollBlock(Rect rect, ref Vector2 scrollPosition, ref Rect viewRect, ref float viewHeight, ref float viewHeightCache, Action inBlock)
        {
            Rect baseRect = rect;
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect, true);

            inBlock();

            viewRect = new Rect(baseRect.x, baseRect.y, viewRect.width, viewHeight);
            Widgets.EndScrollView();
            viewHeightCache = viewHeight;
        }

        public static void HighlightRect(Rect rect, params Rect[] linkRect)
        {
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                if (linkRect != null)
                {
                    foreach (Rect link in linkRect)
                    {
                        Widgets.DrawHighlight(link);
                    }
                }
            }
        }

        public static void ToolTipRect(Rect rect, string tooltip)
        {
            if (Mouse.IsOver(rect))
            {
                if (tooltip != null)
                {
                    TooltipHandler.TipRegion(rect, tooltip);
                }
            }
        }

        public static float WriteFixedSectionTitle(Rect inRect, string title, GameFont orgFont, GameFont newFont = GameFont.Medium, bool titleBg = true)
        {
            if (titleBg)
            {
                Widgets.DrawTitleBG(inRect);
            }
            Text.Font = newFont;
            Widgets.Label(inRect, title);
            Text.Font = orgFont;
            Widgets.DrawLineHorizontal(inRect.x, inRect.y + UIHelper.HEIGHT_SECTION_TITLE, inRect.width);
            return UIHelper.HEIGHT_SECTION_TITLE;
        }

        public static float WriteFixedSectionTitleBox(Rect inRect, string title, ref bool boxOpened, Rect innerRect, Func<Rect, float> innerAction, GameFont orgFont, GameFont newFont = GameFont.Medium, bool titleBg = true)
        {
            if (titleBg)
            {
                Widgets.DrawTitleBG(inRect);
            }
            Rect buttonRect = new Rect(inRect.x, inRect.y, UIHelper.SIZE_BUTTON, inRect.height);
            Rect labelRect = new Rect(inRect.x + buttonRect.width + UIHelper.MARGIN_LEFT, inRect.y, inRect.width - (buttonRect.width + UIHelper.MARGIN_LEFT), inRect.height);
            if (Widgets.ButtonImageFitted(buttonRect, boxOpened ? TexButton.Collapse : TexButton.Reveal))
            {
                boxOpened = !boxOpened;
            }
            Text.Font = newFont;
            Widgets.Label(labelRect, title);
            Text.Font = orgFont;
            float innerHeight = 0;
            if (boxOpened)
            {
                innerHeight = innerAction(innerRect);
                Rect boxRect = new Rect(inRect.x, inRect.y + inRect.height, inRect.width, innerHeight);
                Widgets.DrawBox(boxRect);
            }
            return UIHelper.HEIGHT_SECTION_TITLE + innerHeight;
        }

        public static float WriteFloatedSectionTitle(Rect inRect, string title, ref bool checkOn, ref bool valueChanged, GameFont orgFont, GameFont newFont = GameFont.Medium)
        {
            Text.Font = newFont;
            Vector2 buttonVector = new Vector2(inRect.x, inRect.y);
            LabeledCheckBoxLeft(inRect, title, ref checkOn, ref valueChanged, UIHelper.SIZE_BUTTON, TexButton.Plus, TexButton.Minus);
            Text.Font = orgFont;
            Widgets.DrawLineHorizontal(inRect.x + MARGIN_LEFT, inRect.y + UIHelper.HEIGHT_SECTION_TITLE, inRect.width - MARGIN_LEFT);
            return UIHelper.HEIGHT_SECTION_TITLE;
        }

        public static float TitledBox(Rect inRect, float marginLeft, float marginTop, float viewHeightSum, string title, bool open, out bool openChanged, Func<float, float, float, float, float> innerFunction)
        {
            float boxY = inRect.y + viewHeightSum;
            GameFont bkFont = Text.Font;
            TextAnchor bkAnchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            float x = inRect.x + marginLeft;
            float y = inRect.y + viewHeightSum + marginTop;
            float boxWidth = inRect.width - (marginLeft * 2);
            float labelX = x + SIZE_BUTTON + MARGIN_LEFT;
            float labelWidth = boxWidth - (SIZE_BUTTON + (MARGIN_LEFT * 2));

            bool checkValue = open;
            Vector2 buttonVector = new Vector2(x, y);
            Widgets.Checkbox(buttonVector, ref checkValue, SIZE_BUTTON, false, false, TexButton.Minus, TexButton.Plus);
            openChanged = checkValue;
            Rect labelRect = new Rect(labelX, y, labelWidth, HEIGHT_ROW);
            Widgets.Label(labelRect, title);
            Text.Font = bkFont;
            Text.Anchor = bkAnchor;
            viewHeightSum += marginTop + labelRect.height + MARGIN_BOTTOM;
            if (open)
            {
                viewHeightSum += innerFunction(labelX, inRect.y + viewHeightSum, labelWidth, viewHeightSum);
            }
            float boxHeight = inRect.y + (viewHeightSum - boxY) + marginTop;
            Rect box = new Rect(inRect.x, boxY, inRect.width, boxHeight);
            return viewHeightSum;
        }

        public static bool LabeledRadioButtonLeft(Rect inRect, string text, bool chosen, out bool valueChanged)
        {
            Vector2 radioVec = new Vector2(inRect.x, inRect.y);
            float labelLeftMargin = Widgets.RadioButtonSize + MARGIN_LEFT;
            Rect labelRect = new Rect(inRect.x + labelLeftMargin, inRect.y, inRect.width - labelLeftMargin, inRect.height);
            bool radio = Widgets.RadioButton(radioVec, chosen);
            Widgets.Label(labelRect, text);
            valueChanged = chosen != radio;
            return radio;
        }

        public static void LabeledCheckButtonLeftFixed(Rect inRect, string text, bool flagEnabled)
        {
            Rect checkImageRect = new Rect(inRect.x, inRect.y, Widgets.RadioButtonSize, Widgets.RadioButtonSize);
            float labelLeftMargin = Widgets.RadioButtonSize + MARGIN_LEFT;
            Rect labelRect = new Rect(inRect.x + labelLeftMargin, inRect.y, inRect.width - labelLeftMargin, inRect.height);
            Widgets.DrawTextureFitted(checkImageRect, flagEnabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex, 1f);
            Widgets.Label(labelRect, text);
        }

        private static bool SingleCheckBox(float x, float y, ref bool checkOn, ref bool valueChanged, float imageSize, Texture2D texChecked, Texture2D texUnchecked)
        {
            bool bk = checkOn;

            Vector2 checkVec = new Vector2(x, y);
            Widgets.Checkbox(checkVec, ref checkOn);

            valueChanged |= bk != checkOn;
            return bk != checkOn;
        }

        public static bool SingleCheckBox(Rect inRect, ref bool checkOn, ref bool valueChanged, float imageSize, Texture2D texChecked, Texture2D texUnchecked)
        {
            float x = inRect.x + ((WIDTH_CHECKBOX - Widgets.CheckboxSize) * 0.5f);
            float y = inRect.y + ((HEIGHT_ROW - Widgets.CheckboxSize) * 0.5f);
            return SingleCheckBox(x, y, ref checkOn, ref valueChanged, imageSize, texChecked, texUnchecked);
        }

        public static bool LabeledCheckBoxLeft(Rect inRect, string text, ref bool checkOn, ref bool valueChanged, float imageSize = Widgets.CheckboxSize, Texture2D texChecked = null, Texture2D texUnchecked = null)
        {
            bool nowChanged = SingleCheckBox(inRect, ref checkOn, ref valueChanged, imageSize, texChecked, texUnchecked);
            Rect labelRect = new Rect(inRect.x + WIDTH_CHECKBOX, inRect.y, inRect.width - WIDTH_CHECKBOX, inRect.height);
            Widgets.Label(labelRect, text);
            return nowChanged;
        }

        public static bool LabeledCheckBoxRight(Rect inRect, float labelWidth, string text, ref bool checkOn, ref bool valueChanged, float imageSize = Widgets.CheckboxSize, Texture2D texChecked = null, Texture2D texUnchecked = null)
        {
            Rect labelRect = new Rect(inRect.x, inRect.y, labelWidth, inRect.height);
            Widgets.Label(labelRect, text);
            return SingleCheckBox(inRect.x + labelWidth, inRect.y, ref checkOn, ref valueChanged, imageSize, texChecked, texUnchecked);
        }

        public static bool LabeledTextBoxPercentage(Rect inRect, float labelWidth, float textWidth, string text, ref float val, ref string buf, ref bool valueChanged, float min, float max, Func<float, string> labelFormatter, TextAnchor labelAnchor = TextAnchor.MiddleLeft)
        {
            float bk = val;
            TextAnchor bkAnchor = Text.Anchor;

            Text.Anchor = labelAnchor;
            Rect labelRect = new Rect(inRect.x, inRect.y, labelWidth, HEIGHT_ROW);
            Widgets.Label(labelRect, text);
            Text.Anchor = bkAnchor;

            Rect textRect = new Rect(inRect.x + labelWidth, inRect.y, textWidth, HEIGHT_ROW);
            Widgets.TextFieldNumericLabeled<float>(textRect, labelFormatter == null ? String.Format("[{0:F2}%]", val * 100f) : labelFormatter(val), ref val, ref buf, min, max);
            if (float.TryParse(buf, out float result))
            {
                val = result;
            }

            valueChanged |= bk != val;
            return bk != val;
        }
        public static bool LabeledTextBoxPercentage(Rect inRect, float labelWidth, float textWidth, string text, ref float val, ref string buf, ref bool valueChanged, float min, float max, TextAnchor labelAnchor = TextAnchor.MiddleLeft)
        {
            return LabeledTextBoxPercentage(inRect, labelWidth, textWidth, text, ref val, ref buf, ref valueChanged, min, max, null, labelAnchor);
        }

        public static bool LabeledTextBoxInt(Rect inRect, float labelWidth, float textWidth, string text, ref int val, ref string buf, ref bool valueChanged, int min, int max, Func<int, string> labelFormatter, TextAnchor labelAnchor = TextAnchor.MiddleLeft)
        {
            int bk = val;
            TextAnchor bkAnchor = Text.Anchor;

            Text.Anchor = labelAnchor;
            Rect labelRect = new Rect(inRect.x, inRect.y, labelWidth, HEIGHT_ROW);
            Widgets.Label(labelRect, text);
            Text.Anchor = bkAnchor;

            Rect textRect = new Rect(inRect.x + labelWidth, inRect.y, textWidth, HEIGHT_ROW);
            Widgets.TextFieldNumericLabeled<int>(textRect, labelFormatter == null ? null : labelFormatter(val), ref val, ref buf, min, max);
            if (int.TryParse(buf, out int result))
            {
                val = result;
            }

            valueChanged |= bk != val;
            return bk != val;
        }
        public static bool LabeledTextBoxInt(Rect inRect, float labelWidth, float textWidth, string text, ref int val, ref string buf, ref bool valueChanged, int min, int max, TextAnchor labelAnchor = TextAnchor.MiddleLeft)
        {
            return LabeledTextBoxInt(inRect, labelWidth, textWidth, text, ref val, ref buf, ref valueChanged, min, max, null, labelAnchor);
        }

        public static bool LabeledSliderPercentage(Rect inRect, float labelWidth, float sliderWidth, string text, ref float val, ref bool valueChanged, float min, float max, Func<float, string> labelFormatter, TextAnchor labelAnchor = TextAnchor.MiddleLeft)
        {
            float bk = val;
            TextAnchor bkAnchor = Text.Anchor;

            Text.Anchor = labelAnchor;
            Rect labelRect = new Rect(inRect.x, inRect.y, labelWidth, HEIGHT_ROW);
            Widgets.Label(labelRect, text);
            Text.Anchor = bkAnchor;

            Rect sliderRect = new Rect(inRect.x + labelWidth, inRect.y, sliderWidth, HEIGHT_ROW);
            val = Widgets.HorizontalSlider(sliderRect, val, min, max, false, labelFormatter == null ? String.Format("[{0:F2} : {1:F2}%]", val, val * 100f) : labelFormatter(val), min.ToString(), max.ToString(), 0.01f);

            valueChanged |= bk != val;
            return bk != val;
        }
        public static bool LabeledSliderPercentage(Rect inRect, float labelWidth, float sliderWidth, string text, ref float val, ref bool valueChanged, float min, float max, TextAnchor labelAnchor = TextAnchor.MiddleLeft)
        {
            return LabeledSliderPercentage(inRect, labelWidth, sliderWidth, text, ref val, ref valueChanged, min, max, null, labelAnchor);
        }

        public static bool LabeledSliderInt(Rect inRect, float labelWidth, float sliderWidth, string text, ref int val, ref bool valueChanged, float min, float max, Func<int, string> labelFormatter, TextAnchor labelAnchor = TextAnchor.MiddleLeft)
        {
            int bk = val;
            TextAnchor bkAnchor = Text.Anchor;

            Text.Anchor = labelAnchor;
            Rect labelRect = new Rect(inRect.x, inRect.y, labelWidth, HEIGHT_ROW);
            Widgets.Label(labelRect, text);
            Text.Anchor = bkAnchor;

            Rect sliderRect = new Rect(inRect.x + labelWidth, inRect.y, sliderWidth, HEIGHT_ROW);
            val = (int)Widgets.HorizontalSlider(sliderRect, val * 1f, min, max, false, labelFormatter == null ? val.ToString() : labelFormatter(val), min.ToString(), max.ToString(), 1f);

            valueChanged |= bk != val;
            return bk != val;
        }
        public static bool LabeledSliderInt(Rect inRect, float labelWidth, float sliderWidth, string text, ref int val, ref bool valueChanged, float min, float max, TextAnchor labelAnchor = TextAnchor.MiddleLeft)
        {
            return LabeledSliderInt(inRect, labelWidth, sliderWidth, text, ref val, ref valueChanged, min, max, null, labelAnchor);
        }

        public static float GetMaxTextWidth(IEnumerable<string> texts) => texts.Max(x => Text.CalcSize(x).x);
    }
}
