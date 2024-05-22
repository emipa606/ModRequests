using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace TakeOffIndoor
{
    public class ListingStandardWrapper
    {
        public Listing_Standard listingStandard=new Listing_Standard();
        private Rect listRect;
        public Rect viewRect;
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight;
        public string translateName="";
        public bool useTranslate = true;

        public float top;
        public float bottom;

        public static TextAnchor anchorBk;
        public static GameFont fontBk;
        public static Color colorBk;

        public static bool fontBackuped = false;
        public void BackupGUI()
        {
            if (fontBackuped) return;
            anchorBk = Text.Anchor;
            fontBk = Text.Font;
            colorBk = GUI.color;
            fontBackuped = true;
        }
        public void RecoverGUI()
        {
            Text.Anchor = anchorBk;
            Text.Font = fontBk;
            GUI.color = colorBk;
            fontBackuped = false;
        }
        public ListingStandardWrapper(string _translateName)
        {
            if (_translateName != "")
            {
                translateName = _translateName+".";
            }
        }

        public Rect ListRect
        {
            set
            {
                listRect = value;
                viewRect = new Rect(0f, 0f, listRect.width - 40f, scrollViewHeight);
            }
            get
            {
                return listRect;
            }
        }

        public string trans(string str)
        {
            if (useTranslate)
            {
                return (translateName + str).TranslateW(str);
            }
            else
            {
                return str;
            }
        }

        public void BeginWithScroll()
        {
            listingStandard.Begin(listRect);
            listingStandard.BeginScrollView(new Rect(0,0,listRect.width,listRect.height),ref scrollPosition,ref viewRect);
        }
        public void EndWithScroll()
        {
            if (Event.current.type == EventType.Layout)
            {
                scrollViewHeight = listingStandard.CurHeight + 30f;
            }
            listingStandard.EndScrollView(ref viewRect);

            listingStandard.End();
        }

        public void CheckBox(string paramName, ref bool flg, string tip = null)
        {
            listingStandard.CheckboxLabeled(trans(paramName), ref flg, (tip != null) ? trans(tip) : null);
        }
        public void Label(string leftString, string tip = null)
        {
            top = listingStandard.CurHeight;
            listingStandard.Label(trans(leftString), -1, (tip != null) ? trans(tip) : null);
            bottom = listingStandard.CurHeight;
        }

        public void LabelDouble(string leftString, string rightString, string tip = null)
        {
            top = listingStandard.CurHeight;
            listingStandard.LabelDouble(trans(leftString), trans(rightString), (tip != null) ? trans(tip) : null);
            bottom = listingStandard.CurHeight;
        }
        public void Slider(ref int value, int min = 0, int max = 8)
        {
            top = listingStandard.CurHeight;
            value = (int)listingStandard.Slider((float)value, min, max);
            bottom = listingStandard.CurHeight;
        }

        public void InputLabeled(string label, ref int value, int min = 0, int max = 8)
        {
            top = listingStandard.CurHeight;
            string tmp = value.ToString();
            listingStandard.TextFieldNumericLabeled<int>(trans(label), ref value, ref tmp, min, max);
            bottom = listingStandard.CurHeight;
        }

        public int InputLabeled(string label, int value, int min = 0, int max = 8)
        {
            InputLabeled(label, ref value, min, max);
            return value;
        }

        public void InputLabeledSized(string label, ref int value,int min = 0, int max = 8,float leftMag=0,float inputLeftMag=0.5f,float widthMag=0.5f,TextAnchor labelAnchor=TextAnchor.MiddleLeft,string tooltip="")
        {
            TextAnchor anc = Text.Anchor;
            Text.Anchor = labelAnchor;
            Label(" ");
            Widgets.Label(OneColumnRect(top, bottom, leftMag, inputLeftMag), label);
            string tmp = value.ToString();
            Widgets.TextFieldNumeric(OneColumnRect(top, bottom, inputLeftMag, widthMag), ref value, ref tmp, min, max);
            Tooltip(tooltip);

            bottom = listingStandard.CurHeight;
            Text.Anchor = anc;
        }

        public int InputLabeledSized(string label, int value, int min = 0, int max = 8, float leftMag = 0, float inputLeftMag = 0.5f, float widthMag = 0.5f, TextAnchor labelAnchor = TextAnchor.MiddleLeft, string tooltip = "")
        {
            InputLabeledSized(label, ref value, min, max, leftMag, inputLeftMag, widthMag, labelAnchor,tooltip);
            return value;
        }


        public void Input(ref int value, int min = 0, int max = 8)
        {
            top = listingStandard.CurHeight;
            string tmp = value.ToString();
            listingStandard.TextFieldNumeric<int>(ref value, ref tmp, min, max);
            bottom = listingStandard.CurHeight;
        }
        public int Input(int value, int min = 0, int max = 8)
        {
            top = listingStandard.CurHeight;
            string tmp = value.ToString();
            listingStandard.TextFieldNumeric<int>(ref value, ref tmp, min, max);
            bottom = listingStandard.CurHeight;
            return value;
        }

        public int SliderLabeled(string label,int value,int leftValue,int rightValue, float rectLeftMag=0.3f,float rectWidthMag=0.7f,string _tooltip=null)
        {
            listingStandard.Label(label,tooltip:_tooltip);

            return (int)Widgets.HorizontalSlider(OneColumnRect(top, bottom, rectLeftMag, rectWidthMag),value,leftValue,rightValue,true,null,leftValue.ToString(),rightValue.ToString());
        }

        public void SliderWithUpDown(string label, ref float param, int min = int.MinValue, int max = int.MaxValue, int defaultValue = 0, string tooltip = null, float defaultButtonLeftMag = 0.4f, float defaultButtonWidth = 0.1f, float upDownButtonLeftMag = 0.5f, float upDownButtonWidth = 0.1f, float inputLeftMag = 0.7f, float inputWidthMag = 0.2f, TextAnchor labelAnchor = TextAnchor.MiddleLeft)
        {
            top = listingStandard.CurHeight;
            float u = listingStandard.CurHeight;
            listingStandard.Label(" ", tooltip: tooltip);
            float d = listingStandard.CurHeight;

            TextAnchor anc = Text.Anchor;
            Text.Anchor = labelAnchor;
            Widgets.Label(OneColumnRect(u, d, 0f, Mathf.Min(Mathf.Min(upDownButtonLeftMag, inputLeftMag), defaultButtonLeftMag)), trans(label));
            Text.Anchor = anc;
            param=Widgets.HorizontalSlider(OneColumnRect(u+7, d, inputLeftMag, inputWidthMag), param, min, max);
            if (Widgets.ButtonText(OneColumnRect(u, d, defaultButtonLeftMag, defaultButtonWidth), "default".Translate()))
            {
                param = defaultValue;
            }
            if (Widgets.ButtonText(OneColumnRect(u, d, upDownButtonLeftMag, upDownButtonWidth), "▲"))
            {
                param++;
            }
            if (Widgets.ButtonText(OneColumnRect(u, d, upDownButtonLeftMag + upDownButtonWidth, upDownButtonWidth), "▼"))
            {
                param--;
            }
            bottom = listingStandard.CurHeight;

            if (tooltip != null)
            {
                Tooltip(OneColumnRect(u, d, inputLeftMag, defaultButtonLeftMag + defaultButtonWidth), tooltip);
            }
        }

        public void SliderWithParamUpDown(string label, ref float param, int min = int.MinValue, int max = int.MaxValue, int defaultValue = 0, string tooltip = null, float defaultButtonLeftMag = 0.4f, float defaultButtonWidth = 0.1f, float upDownButtonLeftMag = 0.5f, float upDownButtonWidth = 0.1f, float inputLeftMag = 0.7f, float inputWidthMag = 0.2f, TextAnchor labelAnchor = TextAnchor.MiddleLeft, float paramWidth = 0.05f,string format="")
        {
            top = listingStandard.CurHeight;
            float u = listingStandard.CurHeight;
            listingStandard.Label(" ", tooltip: tooltip);
            float d = listingStandard.CurHeight;

            TextAnchor anc = Text.Anchor;
            Text.Anchor = labelAnchor;
            float labelRight= Mathf.Min(Mathf.Min(upDownButtonLeftMag, inputLeftMag), defaultButtonLeftMag) - paramWidth;
            Widgets.Label(OneColumnRect(u, d, 0f, labelRight), trans(label));
            Widgets.Label(OneColumnRect(u, d, labelRight, paramWidth), param.ToString(format));

            Text.Anchor = anc;
            param = Widgets.HorizontalSlider(OneColumnRect(u + 7, d, inputLeftMag, inputWidthMag), param, min, max);
            if (Widgets.ButtonText(OneColumnRect(u, d, defaultButtonLeftMag, defaultButtonWidth), "default".Translate()))
            {
                param = defaultValue;
            }
            if (Widgets.ButtonText(OneColumnRect(u, d, upDownButtonLeftMag, upDownButtonWidth), "▲"))
            {
                param++;
            }
            if (Widgets.ButtonText(OneColumnRect(u, d, upDownButtonLeftMag + upDownButtonWidth, upDownButtonWidth), "▼"))
            {
                param--;
            }
            bottom = listingStandard.CurHeight;

            if (tooltip != null)
            {
                Tooltip(OneColumnRect(u, d, inputLeftMag, defaultButtonLeftMag + defaultButtonWidth), tooltip);
            }
        }

        public int SliderWithParamUpDown(string label, int param, int min = int.MinValue, int max = int.MaxValue, int defaultValue = 0, string tooltip = null, float defaultButtonLeftMag = 0.4f, float defaultButtonWidth = 0.1f, float upDownButtonLeftMag = 0.5f, float upDownButtonWidth = 0.1f, float inputLeftMag = 0.7f, float inputWidthMag = 0.2f, TextAnchor labelAnchor = TextAnchor.MiddleLeft, float paramWidth = 0.05f, string format = "")
        {
            float tmp = param;
            SliderWithParamUpDown(label, ref tmp, min, max, defaultValue, tooltip, defaultButtonLeftMag, defaultButtonWidth, upDownButtonLeftMag, upDownButtonWidth, inputLeftMag, inputWidthMag, labelAnchor, paramWidth, format);
            return (int)tmp;
        }

            public void InputWithUpDown(string label, ref float param, int min = int.MinValue, int max = int.MaxValue, int defaultValue = 0, string tooltip = null,float defaultButtonLeftMag=0.4f,float defaultButtonWidth=0.1f, float upDownButtonLeftMag=0.5f,float upDownButtonWidth = 0.1f,float inputLeftMag=0.7f,float inputWidthMag=0.2f,TextAnchor labelAnchor=TextAnchor.MiddleLeft)
        {
            top = listingStandard.CurHeight;
            float u = listingStandard.CurHeight;
            listingStandard.Label("                                   ", tooltip: tooltip);
            float d = listingStandard.CurHeight;
            TextAnchor anc = Text.Anchor;
            Text.Anchor = labelAnchor;
            Widgets.Label(OneColumnRect(u,d,0f, Mathf.Min(Mathf.Min(upDownButtonLeftMag, inputLeftMag),defaultButtonLeftMag)),trans(label));
            Text.Anchor = anc;
            string buf = param.ToString();
            Widgets.TextFieldNumeric(OneColumnRect(u, d, inputLeftMag, inputWidthMag), ref param, ref buf, min, max);
            if (Widgets.ButtonText(OneColumnRect(u, d, defaultButtonLeftMag, defaultButtonWidth), "default".Translate()))
            {
                param = defaultValue;
            }
            if (Widgets.ButtonText(OneColumnRect(u, d, upDownButtonLeftMag, upDownButtonWidth), "▲"))
            {
                param++;
            }
            if (Widgets.ButtonText(OneColumnRect(u, d, upDownButtonLeftMag + upDownButtonWidth, upDownButtonWidth), "▼"))
            {
                param--;
            }
            bottom = listingStandard.CurHeight;
        }

        public int InputWithUpDown(string label, int param, int min = int.MinValue, int max = int.MaxValue, int defaultValue = 0, string tooltip = null, float defaultButtonLeftMag = 0.4f, float defaultButtonWidth = 0.1f, float upDownButtonLeftMag = 0.5f, float upDownButtonWidth = 0.1f, float inputLeftMag = 0.7f, float inputWidthMag = 0.2f, TextAnchor labelAnchor = TextAnchor.MiddleLeft)
        {
            float tmp = param;
            InputWithUpDown(label, ref tmp, min, max, defaultValue, tooltip, defaultButtonLeftMag, defaultButtonWidth, upDownButtonLeftMag, upDownButtonWidth, inputLeftMag, inputWidthMag, labelAnchor);
            return (int)tmp;
        }

        public void InputWithUpDownDouble(string label, string label1, string label2, ref float param1, ref float param2, int min = int.MinValue, int max = int.MaxValue, int defaultValue = 0, string tooltip = "")
        {
            top = listingStandard.CurHeight;
            float u = listingStandard.CurHeight;
            listingStandard.Label(trans(label), -1, tooltip);
            float d = listingStandard.CurHeight;
            string buf1 = param1.ToString();
            string buf2 = param2.ToString();
            float left = 0.25f;

            if (Widgets.ButtonText(OneColumnRect(u, d, left, 0.09f), "Def"))
            {
                param1 = defaultValue;
                param2 = defaultValue;
            }
            left += 0.15f;

            Widgets.Label(OneColumnRect(u, d, left, 0.03f), label1);
            left += 0.03f;

            left += 0.1f;
            Widgets.TextFieldNumeric(OneColumnRect(u, d, left, 0.1f), ref param1, ref buf1, min, max);
            left -=0.1f;

            if (Widgets.ButtonText(OneColumnRect(u, d, left, 0.05f), "▲"))
            {
                param1++;
            }
            left += 0.05f;
            if (Widgets.ButtonText(OneColumnRect(u, d, left, 0.05f), "▼"))
            {
                param1--;
            }
            left += 0.05f;
            left += 0.14f;

            Widgets.Label(OneColumnRect(u, d, left, 0.03f), label2);
            left += 0.03f;

            left += 0.1f;
            Widgets.TextFieldNumeric(OneColumnRect(u, d, left, 0.1f), ref param2, ref buf2, min, max);
            left -= 0.1f;
            if (Widgets.ButtonText(OneColumnRect(u, d, left, 0.05f), "▲"))
            {
                param2++;
            }
            left += 0.05f;
            if (Widgets.ButtonText(OneColumnRect(u, d, left, 0.05f), "▼"))
            {
                param2--;
            }
            left += 0.05f;

            bottom = listingStandard.CurHeight;
        }

        public void ColorSlider(ref Color color, ref float intensity, float leftMag = 0f, float rightMag = 1f)
        {
            float max = Color.white.r;
            Gap();
            GUI.color = Color.red;
            color.r = Widgets.HorizontalSlider(OneColumnRect(top, bottom, leftMag, rightMag), color.r, 0f, max);

            Gap();
            GUI.color = Color.green;
            color.g = Widgets.HorizontalSlider(OneColumnRect(top, bottom, leftMag, rightMag), color.g, 0f, max);

            Gap();
            GUI.color = Color.blue;
            color.b = Widgets.HorizontalSlider(OneColumnRect(top, bottom, leftMag, rightMag), color.b, 0f, max);

            Gap();
            GUI.color = Color.gray;
            color.a = Widgets.HorizontalSlider(OneColumnRect(top, bottom, leftMag, rightMag), color.a, 0f, max);

            Gap();
            GUI.color = Color.white;
            intensity = Widgets.HorizontalSlider(OneColumnRect(top, bottom, leftMag, rightMag), intensity, 0f, 1f);

        }

        //h:360度を0～1fに正規化する前提で0～360の範囲で受け取る
        //s:0～1f
        //v:0～1f
        //戻り値は変更の有無
        public bool HSVSlider(ref float h,ref float s,ref float v,float leftMag=0f,float rightMag=1f)
        {
            bool ret = false;
            float prev;

            Gap();
            prev = h;

            h = Widgets.HorizontalSlider(OneColumnRect(top, bottom, leftMag, rightMag), h, 0f, 360f);
            if (prev != h) ret = true;

            Gap();
            prev = s;

            s = Widgets.HorizontalSlider(OneColumnRect(top, bottom, leftMag, rightMag), s, -1f, 1f);
            if (prev != s) ret = true;

            Gap();
            prev = v;
            
            v = Widgets.HorizontalSlider(OneColumnRect(top, bottom, leftMag, rightMag), v, -1f, 1f);
            if (prev != s) ret = true;

            return ret;
        }

        public Rect OneColumnRect(float top, float bottom, float leftMag = 0f, float widthMag = 1f)
        {
            return new Rect((float)(listRect.x + viewRect.width * leftMag), top, (float)(viewRect.width * widthMag), bottom - top);
        }

        public void AllocOneCulumn(int height=12)
        {
            top = listingStandard.CurHeight;
            listingStandard.Gap(height);
            bottom = listingStandard.CurHeight;
        }
        public bool ButtonTextSized(string label,float leftMag=0f,float widthMag=1f)
        {
            return Widgets.ButtonText(OneColumnRect(top, bottom, leftMag, widthMag), trans(label));
        }
        public void CheckBoxLabeledSized(string label, ref bool checkOn,float left=0.7f,float width = 0.2f, string tooltip = "",TextAnchor labelAnchor=TextAnchor.MiddleLeft)
        {
            TextAnchor anc = Text.Anchor;
            Text.Anchor = labelAnchor;
            Widgets.CheckboxLabeled(OneColumnRect(top, bottom, left, width), trans(label), ref checkOn);
            Text.Anchor = anc;
        }

        public void RadioButtonLabeled(ref string nowString,string targetString)
        {
            string label = targetString;
            if (useTranslate) label = label.Translate();
            top = listingStandard.CurHeight;
            this.Label(" ");
            bottom = listingStandard.CurHeight;
            Rect rect = OneColumnRect(top, bottom);
            if (Widgets.RadioButton(rect.x, top - 2, nowString == targetString))
            {
                nowString = targetString;
            }
            rect.x += 26;
            rect.width -= 26;

            Widgets.Label(rect, label);
        }

        public float CurHeight
        {
            get
            {
                return listingStandard.CurHeight;
            }
        }
        public void Gap(int height=12)
        {
            top = CurHeight;
            listingStandard.Gap(height);
            bottom = CurHeight;
        }
        public void GapLine(int height = 12)
        {
            top = CurHeight;
            listingStandard.GapLine(height);
            bottom = CurHeight;
        }

        public void ListSeparater(string label,float width=-1f)
        {
            if (width == -1f)
            {
                width = viewRect.width;
            }
            top = CurHeight;
            float dummy = CurHeight;
            Widgets.ListSeparator(ref dummy, width, label);

            listingStandard.Gap(Widgets.ListSeparatorHeight);
            bottom = CurHeight;
        }

        public bool DrawTextureWithHSVSlider(string label, Texture2D texture, float height, ref float h, ref float s, ref float v,float sliderWidthMag=0.8f)
        {
            float d = CurHeight;
            Label(label);
            bool ret = HSVSlider(ref h, ref s, ref v, 0, sliderWidthMag);


            float width = texture.width * height / texture.height;

            Widgets.DrawTextureFitted(new Rect(listRect.x + viewRect.width - width, d, width, height),texture, 1f);

            float nowHeight = CurHeight - d;
            if (height > nowHeight)
            {
                Gap((int)(height - nowHeight));
            }

            return ret;
        }


        public void Tooltip(Rect rect, string tooltip)
        {
            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
        }
        public bool Tooltip(string text,float leftMag=0,float rightMag=1)
        {
            Rect rect = OneColumnRect(top, bottom, leftMag, rightMag);
            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, trans(text));
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static class TranslateExtension
    {
        public static string TranslateW(this string key, string defaultString, params object[] args)
        {
            if (key.CanTranslate())
            {
                return key.Translate(args);
            }
            else
            {
                return defaultString;
            }
        }

        public static string TranslateW2(this string key, string arg1)
        {
            bool flg = arg1.CanTranslate();

            if (key.CanTranslate())
            {
                string ret = key.Translate(arg1);
                if (flg)
                {
                    ret.Replace(arg1.Translate(), arg1);
                }

                return ret;
            }
            else
            {
                return key;
            }
        }
    }
}
