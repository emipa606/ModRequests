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
        private Rect listRect;
        public Rect canvasRect;
        public Rect viewRect;
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight;
        public string translateName="";
        public bool useTranslate = true;

        public float curHeight;

        public float top;
        public float bottom;

        public static TextAnchor anchorBk;
        public static GameFont fontBk;
        public static Color colorBk;

        public static bool fontBackuped = false;

        public static float DefaultGap = 24f;
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
                canvasRect = new Rect(listRect);
                listRect.height -= 20f; //これが無いとスクロールバー下部が埋もれる謎
                viewRect = new Rect(0f, 0f, listRect.width - 20f, scrollViewHeight);
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
            curHeight = 0;

            GUI.BeginGroup(canvasRect);
            Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect);
            //Widgets.BeginScrollView(new Rect(0, 0, listRect.width, listRect.height), ref scrollPosition, viewRect);
        }
        public void EndWithScroll()
        {
            if (Event.current.type == EventType.Layout)
            {
                scrollViewHeight = curHeight;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        public Rect NowRect
        {
            get
            {
                top = curHeight;
                bottom = curHeight + DefaultGap;
                return new Rect(viewRect.x, top, viewRect.width, bottom-top);
            }
        }
        public Rect NowRectGap
        {
            get
            {
                Rect tmp = NowRect;
                curHeight += DefaultGap;
                return tmp;
            }
        }

        public Rect ColRect(float leftMag = 0f, float widthMag = 1f,float height=-1f, float leftOffset = 0f)
        {
            if (height == -1f) height = DefaultGap;
            top = curHeight;    bottom = curHeight + height;
            return OneColumnRect(curHeight, curHeight + DefaultGap, leftMag, widthMag, leftOffset);
        }
        public Rect ColRectGap(float leftMag = 0f, float widthMag = 1f, float height = -1f, float leftOffset = 0f)
        {
            if (height == -1f) height = DefaultGap;
            Rect tmp = OneColumnRect(curHeight, curHeight + height, leftMag, widthMag, leftOffset);
            top = curHeight;    curHeight += height;    bottom = curHeight;
            return tmp;
        }
        public Rect PrevColRect(float leftMag = 0f, float widthMag = 1f,float leftOffset=0f)
        {
            return OneColumnRect(top, bottom, leftMag, widthMag, leftOffset);
        }
        public Rect OneColumnRect(float top, float bottom, float leftMag = 0f, float widthMag = 1f,float leftOffset=0f)
        {
            return new Rect((float)(viewRect.x + leftOffset + (viewRect.width - leftOffset) * leftMag), top, (float)((viewRect.width - leftOffset) * widthMag), bottom - top);
        }


        public void CheckBoxSized(string paramName, ref bool flg, string tip = null, float leftMag = 0f, float widthMag = 1f)
        {
            Tooltip(ColRect(leftMag,widthMag), tip);
            Widgets.CheckboxLabeled(ColRectGap(leftMag, widthMag), trans(paramName), ref flg);
        }
        public void Label(string leftString, string tip = null)
        {
            Tooltip(NowRect, tip);
            Widgets.Label(NowRectGap, trans(leftString));
        }

        public void LabelDouble(string leftString, string rightString, string tip = null, float leftMag = 0f, float middleMag = 0.5f,float widthMag = 0.5f)
        {
            Tooltip(NowRect, tip);
            Widgets.Label(ColRect(leftMag, middleMag-leftMag), trans(leftString));
            Widgets.Label(ColRectGap(middleMag, widthMag), trans(rightString));
        }

        public void LabelSized(string leftString, string tip = null, float leftMag = 0f, float widthMag = 1f,float leftOffset=0f)
        {
            Tooltip(PrevColRect(leftMag, widthMag,leftOffset), tip);
            Widgets.Label(PrevColRect(leftMag, widthMag, leftOffset), trans(leftString));
        }

        public void InputLabeled(string label, ref int value, int min = 0, int max = 8)
        {
            string tmp = value.ToString();
            Widgets.TextFieldNumericLabeled(NowRectGap, trans(label), ref value,ref tmp, min, max);
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
            Tooltip(ColRect(leftMag,inputLeftMag+widthMag),tooltip);
            Label(label);
            string tmp = value.ToString();
            Widgets.TextFieldNumeric(PrevColRect(inputLeftMag, widthMag), ref value, ref tmp, min, max);

            Text.Anchor = anc;
        }

        public int InputLabeledSized(string label, int value, int min = 0, int max = 8, float leftMag = 0, float inputLeftMag = 0.5f, float widthMag = 0.5f, TextAnchor labelAnchor = TextAnchor.MiddleLeft, string tooltip = "")
        {
            InputLabeledSized(label, ref value, min, max, leftMag, inputLeftMag, widthMag, labelAnchor,tooltip);
            return value;
        }

        public void InputStringLabeledSized(string label, ref string value, float leftMag = 0, float inputLeftMag = 0.5f, float widthMag = 0.5f, TextAnchor labelAnchor = TextAnchor.MiddleLeft, string tooltip = "")
        {
            TextAnchor anc = Text.Anchor;
            Text.Anchor = labelAnchor;
            Tooltip(ColRect(leftMag, inputLeftMag + widthMag), tooltip);
            Label(label);
            string tmp = value.ToString();
            value = Widgets.TextField(PrevColRect(inputLeftMag, widthMag), value);

            Text.Anchor = anc;
        }



        public void Input(ref int value, int min = 0, int max = 8)
        {
            string tmp = value.ToString();
            Widgets.TextFieldNumeric<int>(NowRectGap,ref value, ref tmp, min, max);
        }
        public int Input(int value, int min = 0, int max = 8)
        {
            string tmp = value.ToString();
            Widgets.TextFieldNumeric<int>(NowRectGap, ref value, ref tmp, min, max);
            return value;
        }


        public void Slider(ref int value, int min = 0, int max = 8, string tip = null, float leftMag = 0f, float widthMag = 1f)
        {
            if (tip != null) Tooltip(ColRect(leftMag, widthMag), trans(tip));
            value = (int)Widgets.HorizontalSlider(ColRectGap(leftMag, widthMag), value, min, max);
        }
        public int SliderLabeled(string label, int value, int leftValue, int rightValue, float rectLeftMag = 0.3f, float rectWidthMag = 0.7f, string _tooltip = null,float leftOffset=0f)
        {
            Tooltip(ColRect(0, rectLeftMag + rectWidthMag,leftOffset: leftOffset), _tooltip);
            Label(label);

            return (int)Widgets.HorizontalSlider(PrevColRect(rectLeftMag, rectWidthMag, leftOffset: leftOffset), value, leftValue, rightValue, true, null, leftValue.ToString(), rightValue.ToString());
        }

        public void SliderWithUpDown(string label, ref float param, int min = int.MinValue, int max = int.MaxValue, int defaultValue = 0, string tooltip = null, float defaultButtonLeftMag = 0.4f, float defaultButtonWidth = 0.1f, float upDownButtonLeftMag = 0.5f, float upDownButtonWidth = 0.1f, float inputLeftMag = 0.7f, float inputWidthMag = 0.2f, TextAnchor labelAnchor = TextAnchor.MiddleLeft)
        {

            TextAnchor anc = Text.Anchor;
            Text.Anchor = labelAnchor;

            float xmax=0f;
            if(upDownButtonLeftMag> defaultButtonLeftMag)
            {
                xmax = upDownButtonLeftMag + upDownButtonWidth * 2;
            }
            else
            {
                xmax = defaultButtonLeftMag + defaultButtonWidth;
            }

            Widgets.Label(ColRectGap(0f, Mathf.Min(Mathf.Min(upDownButtonLeftMag, inputLeftMag), defaultButtonLeftMag)), trans(label));
            Text.Anchor = anc;
            param=Widgets.HorizontalSlider(OneColumnRect(top+7, bottom, inputLeftMag, inputWidthMag), param, min, max);
            if (Widgets.ButtonText(PrevColRect(defaultButtonLeftMag, defaultButtonWidth), "default".Translate()))
            {
                param = defaultValue;
            }
            if (Widgets.ButtonText(PrevColRect(upDownButtonLeftMag, upDownButtonWidth), "▲"))
            {
                param++;
            }
            if (Widgets.ButtonText(PrevColRect(upDownButtonLeftMag + upDownButtonWidth, upDownButtonWidth), "▼"))
            {
                param--;
            }

            Tooltip(PrevColRect(0, xmax), tooltip);
        }

        public void SliderWithParamUpDown(string label, ref float param, int min = int.MinValue, int max = int.MaxValue, int defaultValue = 0, string tooltip = null, float defaultButtonLeftMag = 0.4f, float defaultButtonWidth = 0.1f, float upDownButtonLeftMag = 0.5f, float upDownButtonWidth = 0.1f, float inputLeftMag = 0.7f, float inputWidthMag = 0.2f, TextAnchor labelAnchor = TextAnchor.MiddleLeft, float paramWidth = 0.05f,string format="")
        {
            TextAnchor anc = Text.Anchor;
            Text.Anchor = labelAnchor;

            float xmax = 0f;
            if (upDownButtonLeftMag > defaultButtonLeftMag)
            {
                xmax = upDownButtonLeftMag + upDownButtonWidth * 2;
            }
            else
            {
                xmax = defaultButtonLeftMag + defaultButtonWidth;
            }

            float labelRight = Mathf.Min(Mathf.Min(upDownButtonLeftMag, inputLeftMag), defaultButtonLeftMag) - paramWidth;

            Widgets.Label(ColRectGap(0f, Mathf.Min(Mathf.Min(upDownButtonLeftMag, inputLeftMag), defaultButtonLeftMag)), trans(label));
            Widgets.Label(PrevColRect(labelRight, paramWidth), param.ToString(format));
            Text.Anchor = anc;
            param = Widgets.HorizontalSlider(OneColumnRect(top + 7, bottom, inputLeftMag, inputWidthMag), param, min, max);
            if (Widgets.ButtonText(PrevColRect(defaultButtonLeftMag, defaultButtonWidth), "default".Translate()))
            {
                param = defaultValue;
            }
            if (Widgets.ButtonText(PrevColRect(upDownButtonLeftMag, upDownButtonWidth), "▲"))
            {
                param++;
            }
            if (Widgets.ButtonText(PrevColRect(upDownButtonLeftMag + upDownButtonWidth, upDownButtonWidth), "▼"))
            {
                param--;
            }

            Tooltip(PrevColRect(0, xmax), tooltip);
        }

        public int SliderWithParamUpDown(string label, int param, int min = int.MinValue, int max = int.MaxValue, int defaultValue = 0, string tooltip = null, float defaultButtonLeftMag = 0.4f, float defaultButtonWidth = 0.1f, float upDownButtonLeftMag = 0.5f, float upDownButtonWidth = 0.1f, float inputLeftMag = 0.7f, float inputWidthMag = 0.2f, TextAnchor labelAnchor = TextAnchor.MiddleLeft, float paramWidth = 0.05f, string format = "")
        {
            float tmp = param;
            SliderWithParamUpDown(label, ref tmp, min, max, defaultValue, tooltip, defaultButtonLeftMag, defaultButtonWidth, upDownButtonLeftMag, upDownButtonWidth, inputLeftMag, inputWidthMag, labelAnchor, paramWidth, format);
            return (int)tmp;
        }

            public void InputWithUpDown(string label, ref float param, int min = int.MinValue, int max = int.MaxValue, int defaultValue = 0, string tooltip = null,float defaultButtonLeftMag=0.4f,float defaultButtonWidth=0.1f, float upDownButtonLeftMag=0.5f,float upDownButtonWidth = 0.1f,float inputLeftMag=0.7f,float inputWidthMag=0.2f,TextAnchor labelAnchor=TextAnchor.MiddleLeft,float labelLeftMag=0)
        {

            float xmax = 0f;
            if (upDownButtonLeftMag > defaultButtonLeftMag)
            {
                xmax = upDownButtonLeftMag + upDownButtonWidth * 2;
            }
            else
            {
                xmax = defaultButtonLeftMag + defaultButtonWidth;
            }

            TextAnchor anc = Text.Anchor;
            Text.Anchor = labelAnchor;
            AllocOneCulumn();
            LabelSized(label, null, labelLeftMag, Mathf.Min(Mathf.Min(upDownButtonLeftMag, inputLeftMag), defaultButtonLeftMag)- labelLeftMag);
            float u = top, d = bottom;
            string buf = param.ToString();
            Text.Anchor = anc;
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
            Tooltip(PrevColRect(0, xmax), tooltip);
        }

        public int InputWithUpDown(string label, int param, int min = int.MinValue, int max = int.MaxValue, int defaultValue = 0, string tooltip = null, float defaultButtonLeftMag = 0.4f, float defaultButtonWidth = 0.1f, float upDownButtonLeftMag = 0.5f, float upDownButtonWidth = 0.1f, float inputLeftMag = 0.7f, float inputWidthMag = 0.2f, TextAnchor labelAnchor = TextAnchor.MiddleLeft)
        {
            float tmp = param;
            InputWithUpDown(label, ref tmp, min, max, defaultValue, tooltip, defaultButtonLeftMag, defaultButtonWidth, upDownButtonLeftMag, upDownButtonWidth, inputLeftMag, inputWidthMag, labelAnchor);
            return (int)tmp;
        }

        public void InputWithUpDownDouble(string label, string label1, string label2, ref float param1, ref float param2, int min = int.MinValue, int max = int.MaxValue, int defaultValue = 0, string tooltip = "")
        {
            AllocOneCulumn();
            LabelSized(label, null, 0, 1);
            float u = top, d = bottom;

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

            Tooltip(PrevColRect(), tooltip);
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
        
        public void AllocOneCulumn(int height=-1)
        {
            if (height == -1) height = (int)DefaultGap;
            Gap(height);
        }
        public bool ButtonTextSized(string label,float leftMag=0f,float widthMag=1f,float leftOffset=0f)
        {
            return Widgets.ButtonText(PrevColRect(leftMag, widthMag, leftOffset), trans(label));
        }
        public void CheckBoxLabeledSized(string label, ref bool checkOn,float left=0.7f,float width = 0.2f, string tooltip = "",TextAnchor labelAnchor=TextAnchor.MiddleLeft)
        {
            TextAnchor anc = Text.Anchor;
            Text.Anchor = labelAnchor;
            Widgets.CheckboxLabeled(PrevColRect(0,left + width), trans(label), ref checkOn);
            Text.Anchor = anc;
        }

        public void RadioButtonLabeled(ref string nowString,string targetString)
        {
            string label = targetString;
            if (useTranslate) label = label.TranslateW(label);
            AllocOneCulumn();
            Rect rect = OneColumnRect(top, bottom);
            if (Widgets.RadioButton(rect.x, top - 2, nowString == targetString))
            {
                nowString = targetString;
            }
            rect.x += 26;
            rect.width -= 26;

            Widgets.Label(rect, label);
        }

        public void RadioButtonLabeledHorizontal(ref string nowString, string targetString,float leftMag=0f,float widthMag=1f)
        {
            string label = targetString;
            if (useTranslate) label = label.TranslateW(label);
            Rect rect = OneColumnRect(top, bottom,leftMag,widthMag);
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
                return curHeight;
            }
        }
        public void Gap(int height=-1)
        {
            if (height == -1) height = (int)DefaultGap;
            top = curHeight;
            bottom = (curHeight += height);
        }
        public void GapLine(int height = -1,float width=1f)
        {
            if (height == -1) height = (int)DefaultGap;
            Rect tmp = ColRectGap(height: height);
            Widgets.DrawLine(new Vector2( tmp.x, tmp.y + tmp.height / 2), new Vector2(tmp.x+ tmp.width, tmp.y + tmp.height / 2), GUI.color,1);
        }

        public void ListSeparater(string label,float width=-1f)
        {
            if (width == -1f)
            {
                width = viewRect.width;
            }
            float dummy = curHeight;
            Widgets.ListSeparator(ref dummy, width, label);

            Gap((int)Widgets.ListSeparatorHeight);
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
            if (tooltip != null && tooltip != "")
            {
                if (Mouse.IsOver(rect))
                {
                    TooltipHandler.TipRegion(rect, trans(tooltip));
                }
            }
        }
        public static bool TooltipS(Rect rect, string tooltip)
        {
            if (tooltip != null && tooltip != "")
            {
                if (Mouse.IsOver(rect))
                {
                    TooltipHandler.TipRegion(rect, tooltip);
                    return true;
                }
            }
            return false;
        }
        public bool Tooltip(string text,float leftMag=0,float widthMag=1)
        {
            if(text!=null && text != "")
            {
                Rect rect = PrevColRect(leftMag, widthMag);
                if (Mouse.IsOver(rect))
                {
                    TooltipHandler.TipRegion(rect, trans(text));
                    return true;
                }
            }
            return false;
        }

        public void DrawTexture(Rect rect, Texture tex, float scale = 1f)
        {
            Widgets.DrawTextureFitted(rect, tex, scale);
            top = rect.yMin;
            bottom = rect.yMax;
            if (curHeight < rect.yMax)
            {
                curHeight = rect.yMax;
            }
        }

        public string TextAreaSized(string text, string tooltip="",float leftMag=0,float widthMag=1f,bool readOnly=false)
        {
            Tooltip(tooltip, leftMag, widthMag);
            return Widgets.TextArea(PrevColRect(leftMag, widthMag), text, readOnly);
        }

        public bool OnMouse(float leftMag = 0, float widthMag = 1f)
        {
            return Mouse.IsOver(PrevColRect(leftMag, widthMag));
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
