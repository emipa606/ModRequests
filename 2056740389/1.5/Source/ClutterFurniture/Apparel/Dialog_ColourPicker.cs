using System;
using System.Globalization;
using UnityEngine;
using Verse;
namespace ApparelColorChange
{
    public class Dialog_ColourPicker : Window
    {
        private enum controls
        {
            colourPicker,
            huePicker,
            alphaPicker,
            none
        }
        private Dialog_ColourPicker.controls _activeControl = Dialog_ColourPicker.controls.none;
        private Texture2D _colourPickerBG;
        private Texture2D _huePickerBG;
        private Texture2D _alphaPickerBG;
        private Texture2D _tempPreviewBG;
        private Texture2D _previewBG;
        private Texture2D _pickerAlphaBG;
        private Texture2D _sliderAlphaBG;
        private Texture2D _previewAlphaBG;
        private Color _alphaBGColorA = Color.white;
        private Color _alphaBGColorB = new Color(0.85f, 0.85f, 0.85f);
        public int pickerSize = 300;
        public int sliderWidth = 15;
        public int alphaBGBlockSize = 10;
        public int previewSize = 90;
        public int handleSize = 10;
        private float _margin = 6f;
        private float _fieldHeight = 30f;
        private float _huePosition;
        private float _alphaPosition;
        private float _unitsPerPixel;
        private float _H = 0f;
        private float _S = 1f;
        private float _V = 1f;
        private float _A = 1f;
        private Vector2 _pickerPosition = Vector2.zero;
        public Vector2 initialPosition = Vector2.zero;
        public Vector2 windowSize = Vector2.zero;
        private string _hexOut;
        private string _hexIn;
        private Action _callback;
        private bool _advControls = true;
        private bool _autoApply = false;
        private ColourWrapper _wrapper;
        public Color Colour = Color.blue;
        public Color tempColour = Color.white;
        public float UnitsPerPixel
        {
            get
            {
                if (this._unitsPerPixel == 0f)
                {
                    this._unitsPerPixel = 1f / (float)this.pickerSize;
                }
                return this._unitsPerPixel;
            }
        }
        public float H
        {
            get
            {
                return this._H;
            }
            set
            {
                this._H = Mathf.Clamp(value, 0f, 1f);
                this.NotifyHSVUpdated();
                this.CreateColourPickerBG();
                this.CreateAlphaPickerBG();
            }
        }
        public float S
        {
            get
            {
                return this._S;
            }
            set
            {
                this._S = Mathf.Clamp(value, 0f, 1f);
                this.NotifyHSVUpdated();
                this.CreateAlphaPickerBG();
            }
        }
        public float V
        {
            get
            {
                return this._V;
            }
            set
            {
                this._V = Mathf.Clamp(value, 0f, 1f);
                this.NotifyHSVUpdated();
                this.CreateAlphaPickerBG();
            }
        }
        public float A
        {
            get
            {
                return this._A;
            }
            set
            {
                this._A = Mathf.Clamp(value, 0f, 1f);
                this.NotifyHSVUpdated();
                this.CreateColourPickerBG();
            }
        }
        public Texture2D ColourPickerBG
        {
            get
            {
                if (this._colourPickerBG == null)
                {
                    this.CreateColourPickerBG();
                }
                return this._colourPickerBG;
            }
        }
        public Texture2D HuePickerBG
        {
            get
            {
                if (this._huePickerBG == null)
                {
                    this.CreateHuePickerBG();
                }
                return this._huePickerBG;
            }
        }
        public Texture2D AlphaPickerBG
        {
            get
            {
                if (this._alphaPickerBG == null)
                {
                    this.CreateAlphaPickerBG();
                }
                return this._alphaPickerBG;
            }
        }
        public Texture2D TempPreviewBG
        {
            get
            {
                if (this._tempPreviewBG == null)
                {
                    this._tempPreviewBG = this.CreatePreviewBG(this.tempColour);
                }
                return this._tempPreviewBG;
            }
        }
        public Texture2D PreviewBG
        {
            get
            {
                if (this._previewBG == null)
                {
                    this._previewBG = this.CreatePreviewBG(this.Colour);
                }
                return this._previewBG;
            }
        }
        public Texture2D PickerAlphaBG
        {
            get
            {
                if (this._pickerAlphaBG == null)
                {
                    this._pickerAlphaBG = this.CreateAlphaBG(this.pickerSize, this.pickerSize);
                }
                return this._pickerAlphaBG;
            }
        }
        public Texture2D SliderAlphaBG
        {
            get
            {
                if (this._sliderAlphaBG == null)
                {
                    this._sliderAlphaBG = this.CreateAlphaBG(this.sliderWidth, this.pickerSize);
                }
                return this._sliderAlphaBG;
            }
        }
        public Texture2D PreviewAlphaBG
        {
            get
            {
                if (this._previewAlphaBG == null)
                {
                    this._previewAlphaBG = this.CreateAlphaBG(this.previewSize, this.previewSize);
                }
                return this._previewAlphaBG;
            }
        }
        public Vector2 WindowSize
        {
            get
            {
                return this.windowSize;
            }
            set
            {
                this.windowSize = value;
                this.SetWindowSize(this.windowSize);
            }
        }
        public Dialog_ColourPicker(ColourWrapper colour, Action callback = null, bool advancedControls = true, bool autoApply = false)
        {
            this._wrapper = colour;
            this._callback = callback;
            this._advControls = advancedControls;
            this._autoApply = autoApply;
            this.Colour = this._wrapper.Color;
            this.tempColour = this._wrapper.Color;
            this.NotifyRGBUpdated();
        }
        public void NotifyHSVUpdated()
        {
            this.tempColour = HSV.ToRGBA(this.H, this.S, this.V, 1f);
            this.tempColour.a = this.A;
            this._tempPreviewBG = this.CreatePreviewBG(this.tempColour);
            if (this._advControls)
            {
                this._hexOut = (this._hexIn = Dialog_ColourPicker.RGBtoHex(this.tempColour));
            }
            if (this._autoApply)
            {
                this.Apply();
            }
        }
        public void NotifyRGBUpdated()
        {
            HSV.ToHSV(this.tempColour, out this._H, out this._S, out this._V);
            this._A = this.tempColour.a;
            this.CreateColourPickerBG();
            this.CreateHuePickerBG();
            if (this._advControls)
            {
                this.CreateAlphaPickerBG();
            }
            this._huePosition = (1f - this._H) / this.UnitsPerPixel;
            this._pickerPosition.x = this._S / this.UnitsPerPixel;
            this._pickerPosition.y = (1f - this._V) / this.UnitsPerPixel;
            if (this._advControls)
            {
                this._alphaPosition = (1f - this._A) / this.UnitsPerPixel;
            }
            this._tempPreviewBG = this.CreatePreviewBG(this.tempColour);
            if (this._advControls)
            {
                this._hexOut = (this._hexIn = Dialog_ColourPicker.RGBtoHex(this.tempColour));
            }
            if (this._autoApply)
            {
                this.Apply();
            }
        }
        public void SetColor()
        {
            this.Colour = this.tempColour;
            this._previewBG = this.CreatePreviewBG(this.tempColour);
        }
        private void CreateColourPickerBG()
        {
            int num = this.pickerSize;
            int num2 = this.pickerSize;
            float unitsPerPixel = this.UnitsPerPixel;
            float unitsPerPixel2 = this.UnitsPerPixel;
            Texture2D texture2D = new Texture2D(num, num2);
            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < num2; j++)
                {
                    float s = (float)i * unitsPerPixel;
                    float v = (float)j * unitsPerPixel2;
                    texture2D.SetPixel(i, j, HSV.ToRGBA(this.H, s, v, this.A));
                }
            }
            texture2D.Apply();
            this._colourPickerBG = texture2D;
        }
        private void CreateHuePickerBG()
        {
            Texture2D texture2D = new Texture2D(1, this.pickerSize);
            int num = this.pickerSize;
            float num2 = 1f / (float)num;
            for (int i = 0; i < num; i++)
            {
                texture2D.SetPixel(0, i, HSV.ToRGBA(num2 * (float)i, 1f, 1f, 1f));
            }
            texture2D.Apply();
            this._huePickerBG = texture2D;
        }
        private void CreateAlphaPickerBG()
        {
            if (this._advControls)
            {
                Texture2D texture2D = new Texture2D(1, this.pickerSize);
                int num = this.pickerSize;
                float num2 = 1f / (float)num;
                for (int i = 0; i < num; i++)
                {
                    texture2D.SetPixel(0, i, new Color(this.tempColour.r, this.tempColour.g, this.tempColour.b, (float)i * num2));
                }
                texture2D.Apply();
                this._alphaPickerBG = texture2D;
            }
        }
        private Texture2D CreateAlphaBG(int width, int height)
        {
            Texture2D texture2D = new Texture2D(width, height);
            Color[] array = new Color[this.alphaBGBlockSize * this.alphaBGBlockSize];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this._alphaBGColorA;
            }
            Color[] array2 = new Color[this.alphaBGBlockSize * this.alphaBGBlockSize];
            for (int i = 0; i < array2.Length; i++)
            {
                array2[i] = this._alphaBGColorB;
            }
            int num = 0;
            for (int j = 0; j < width; j += this.alphaBGBlockSize)
            {
                int num2 = num;
                for (int k = 0; k < height; k += this.alphaBGBlockSize)
                {
                    texture2D.SetPixels(j, k, this.alphaBGBlockSize, this.alphaBGBlockSize, (num2 % 2 == 0) ? array : array2);
                    num2++;
                }
                num++;
            }
            texture2D.Apply();
            return texture2D;
        }
        public Texture2D CreatePreviewBG(Color col)
        {
            return SolidColorMaterials.NewSolidColorTexture(col);
        }
        public void PickerAction(Vector2 pos)
        {
            this._S = this.UnitsPerPixel * pos.x;
            this._V = 1f - this.UnitsPerPixel * pos.y;
            this.CreateAlphaPickerBG();
            this.NotifyHSVUpdated();
            this._pickerPosition = pos;
        }
        public void HueAction(float pos)
        {
            this.H = 1f - this.UnitsPerPixel * pos;
            this._huePosition = pos;
        }
        public void AlphaAction(float pos)
        {
            this.A = 1f - this.UnitsPerPixel * pos;
            this._alphaPosition = pos;
        }
        public override void PreOpen()
        {
            this.NotifyRGBUpdated();
            this._alphaPosition = this.Colour.a / this.UnitsPerPixel;
        }
        public override void PostOpen()
        {
            if (this.initialPosition != Vector2.zero)
            {
                this.currentWindowRect.x = this.initialPosition.x;
                this.currentWindowRect.y = this.initialPosition.y;
            }
            if (this.windowSize == Vector2.zero)
            {
                float num2;
                float num = num2 = (float)this.pickerSize + 36f;
                num2 += (float)this.sliderWidth + this._margin;
                if (this._advControls)
                {
                    num2 += (float)this.sliderWidth + this._margin * 3f + (float)(this.previewSize * 2);
                }
                else
                {
                    if (!this._autoApply)
                    {
                        num += this._fieldHeight + this._margin;
                    }
                }
                this.SetWindowSize(new Vector2(num2, num));
            }
            else
            {
                this.SetWindowSize(this.windowSize);
            }
        }
        public void SetWindowSize(Vector2 size)
        {
            this.currentWindowRect.width = size.x;
            this.currentWindowRect.height = size.y;
        }
        public static string RGBtoHex(Color col)
        {
            int num = (int)Mathf.Clamp(col.r * 256f, 0f, 255f);
            int num2 = (int)Mathf.Clamp(col.g * 256f, 0f, 255f);
            int num3 = (int)Mathf.Clamp(col.b * 256f, 0f, 255f);
            int num4 = (int)Mathf.Clamp(col.a * 256f, 0f, 255f);
            return string.Concat(new string[]
			{
				"#",
				num.ToString("X2"),
				num2.ToString("X2"),
				num3.ToString("X2"),
				num4.ToString("X2")
			});
        }
        public static bool TryGetColorFromHex(string hex, out Color col)
        {
            Color color = new Color(0f, 0f, 0f);
            bool result;
            if (hex != null && (hex.Length == 9 || hex.Length == 7))
            {
                try
                {
                    string text = hex.Substring(1, hex.Length - 1);
                    color.r = (float)int.Parse(text.Substring(0, 2), NumberStyles.AllowHexSpecifier) / 255f;
                    color.g = (float)int.Parse(text.Substring(2, 2), NumberStyles.AllowHexSpecifier) / 255f;
                    color.b = (float)int.Parse(text.Substring(4, 2), NumberStyles.AllowHexSpecifier) / 255f;
                    if (text.Length == 8)
                    {
                        color.a = (float)int.Parse(text.Substring(6, 2), NumberStyles.AllowHexSpecifier) / 255f;
                    }
                    else
                    {
                        color.a = 1f;
                    }
                }
                catch (Exception ex)
                {
                    Log.Message(string.Concat(new object[]
					{
						"Falied to convert from",
						hex,
						"\n",
						ex
					}));
                    col = Color.white;
                    result = false;
                    return result;
                }
                col = color;
                result = true;
            }
            else
            {
                col = Color.white;
                result = false;
            }
            return result;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.xMin, inRect.yMin, (float)this.pickerSize, (float)this.pickerSize);
            Rect rect2 = new Rect(rect.xMax + this._margin, inRect.yMin, (float)this.sliderWidth, (float)this.pickerSize);
            Rect rect3 = new Rect(rect2.xMax + this._margin, inRect.yMin, (float)this.sliderWidth, (float)this.pickerSize);
            Rect position = new Rect(rect3.xMax + this._margin, inRect.yMin, (float)this.previewSize, (float)this.previewSize);
            Rect position2 = new Rect(position.xMax, inRect.yMin, (float)this.previewSize, (float)this.previewSize);
            Rect rect4 = new Rect(rect3.xMax + this._margin, inRect.yMax - this._fieldHeight, (float)(this.previewSize * 2), this._fieldHeight);
            Rect rect5 = new Rect(rect3.xMax + this._margin, inRect.yMax - 2f * this._fieldHeight - this._margin, (float)this.previewSize - this._margin / 2f, this._fieldHeight);
            Rect rect6 = new Rect(rect5.xMax + this._margin, rect5.yMin, (float)this.previewSize - this._margin / 2f, this._fieldHeight);
            Rect rect7 = new Rect(rect3.xMax + this._margin, inRect.yMax - 3f * this._fieldHeight - 2f * this._margin, (float)(this.previewSize * 2), this._fieldHeight);
            if (!this._advControls && !this._autoApply)
            {
                rect6 = new Rect(inRect.xMin, rect.yMax + this._margin, ((float)this.pickerSize - this._margin) / 2f, this._fieldHeight);
                rect4 = rect6;
                rect4.x += ((float)this.pickerSize + this._margin) / 2f;
            }
            GUI.DrawTexture(rect, this.PickerAlphaBG);
            if (this._advControls)
            {
                GUI.DrawTexture(position, this.PreviewAlphaBG);
                GUI.DrawTexture(position2, this.PreviewAlphaBG);
                GUI.DrawTexture(rect3, this.SliderAlphaBG);
            }
            GUI.DrawTexture(rect, this.ColourPickerBG);
            GUI.DrawTexture(rect2, this.HuePickerBG);
            if (this._advControls)
            {
                GUI.DrawTexture(rect3, this.AlphaPickerBG);
                GUI.DrawTexture(position, this.TempPreviewBG);
                GUI.DrawTexture(position2, this.PreviewBG);
            }
            Rect rect8 = new Rect(rect2.xMin - 3f, rect2.yMin + this._huePosition - (float)(this.handleSize / 2), (float)this.sliderWidth + 6f, (float)this.handleSize);
            Rect rect9 = new Rect(rect.xMin + this._pickerPosition.x - (float)(this.handleSize / 2), rect.yMin + this._pickerPosition.y - (float)(this.handleSize / 2), (float)this.handleSize, (float)this.handleSize);
            GUI.DrawTexture(rect8, this.TempPreviewBG);
            GUI.DrawTexture(rect9, this.TempPreviewBG);
            GUI.color = Color.gray;
            Widgets.DrawBox(rect8, 1);
            Widgets.DrawBox(rect9, 1);
            GUI.color = Color.white;
            if (this._advControls)
            {
                Rect rect10 = new Rect(rect3.xMin - 3f, rect3.yMin + this._alphaPosition - (float)(this.handleSize / 2), (float)this.sliderWidth + 6f, (float)this.handleSize);
                GUI.DrawTexture(rect10, this.TempPreviewBG);
                GUI.color = Color.gray;
                Widgets.DrawBox(rect10, 1);
                GUI.color = Color.white;
            }
            if (Input.GetMouseButtonUp(0))
            {
                this._activeControl = Dialog_ColourPicker.controls.none;
            }
            if (Mouse.IsOver(rect))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    this._activeControl = Dialog_ColourPicker.controls.colourPicker;
                }
                if (this._activeControl == Dialog_ColourPicker.controls.colourPicker)
                {
                    Vector2 mousePosition = Event.current.mousePosition;
                    Vector2 pos = mousePosition - new Vector2(rect.xMin, rect.yMin);
                    this.PickerAction(pos);
                }
            }
            if (Mouse.IsOver(rect2))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    this._activeControl = Dialog_ColourPicker.controls.huePicker;
                }
                if (Event.current.type == EventType.ScrollWheel)
                {
                    this.H -= Event.current.delta.y * this.UnitsPerPixel;
                    this._huePosition = Mathf.Clamp(this._huePosition + Event.current.delta.y, 0f, (float)this.pickerSize);
                    Event.current.Use();
                }
                if (this._activeControl == Dialog_ColourPicker.controls.huePicker)
                {
                    float y = Event.current.mousePosition.y;
                    float pos2 = y - rect2.yMin;
                    this.HueAction(pos2);
                }
            }
            if (this._advControls)
            {
                if (Mouse.IsOver(rect3))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        this._activeControl = Dialog_ColourPicker.controls.alphaPicker;
                    }
                    if (Event.current.type == EventType.ScrollWheel)
                    {
                        this.A -= Event.current.delta.y * this.UnitsPerPixel;
                        this._alphaPosition = Mathf.Clamp(this._alphaPosition + Event.current.delta.y, 0f, (float)this.pickerSize);
                        Event.current.Use();
                    }
                    if (this._activeControl == Dialog_ColourPicker.controls.alphaPicker)
                    {
                        float y = Event.current.mousePosition.y;
                        float pos2 = y - rect3.yMin;
                        this.AlphaAction(pos2);
                    }
                }
            }
            if (!this._autoApply)
            {
                Text.Font = GameFont.Small;
                if (Widgets.TextButton(rect4, "ClutterColorPickerButtonOK".Translate(), true, false))
                {
                    this.Apply();
                    this.Close(true);
                }
                if (Widgets.TextButton(rect5, "ClutterColorPickerButtonApply".Translate(), true, false))
                {
                    this.Apply();
                    this.SetColor();
                }
                if (Widgets.TextButton(rect6, "ClutterColorPickerButtonCancel".Translate(), true, false))
                {
                    this.Close(true);
                }
            }
            if (this._advControls)
            {
                if (this._hexIn != this._hexOut)
                {
                    Color color = this.tempColour;
                    if (Dialog_ColourPicker.TryGetColorFromHex(this._hexIn, out color))
                    {
                        this.tempColour = color;
                        this.NotifyRGBUpdated();
                    }
                    else
                    {
                        GUI.color = Color.red;
                    }
                }
                this._hexIn = Widgets.TextField(rect7, this._hexIn);
            }
            GUI.color = Color.white;
        }
        public void Apply()
        {
            this._wrapper.Color = this.tempColour;
            if (this._callback != null)
            {
                this._callback();
            }
        }
    }
}
