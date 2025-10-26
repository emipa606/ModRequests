using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace HairStyling
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
        public class ColourPresets
        {
            private const float _minimumBoxSize = 10f;
            private static List<Color> _presets = new List<Color>();
            private int _size = 10;
            private Dialog_ColourPicker _parent;
            public List<Color> presets
            {
                get
                {
                    return Dialog_ColourPicker.ColourPresets._presets;
                }
            }
            public ColourPresets(Dialog_ColourPicker parent, int size = 10)
            {
                this._parent = parent;
                this._size = size;
            }
            public void Add(Color col)
            {
                Dialog_ColourPicker.ColourPresets._presets.Add(col);
                while (Dialog_ColourPicker.ColourPresets._presets.Count > this._size)
                {
                    Dialog_ColourPicker.ColourPresets._presets.RemoveAt(0);
                }
            }
            public void DrawPresetBoxes()
            {
            }
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
        public static Dialog_ColourPicker.ColourPresets presets;
        public int numPresets = 0;
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
                this.Notify_HSVUpdated();
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
                this.Notify_HSVUpdated();
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
                this.Notify_HSVUpdated();
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
                this.Notify_HSVUpdated();
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
            this.Notify_RGBUpdated();
        }
        public void Notify_HSVUpdated()
        {
            this.tempColour = ColourHelper.HSVtoRGB(this.H, this.S, this.V, 1f);
            this.tempColour.a = this.A;
            this._tempPreviewBG = this.CreatePreviewBG(this.tempColour);
            if (this._advControls)
            {
                this._hexOut = (this._hexIn = ColourHelper.RGBtoHex(this.tempColour));
            }
            if (this._autoApply)
            {
                this.Apply();
            }
        }
        public void Notify_RGBUpdated()
        {
            ColourHelper.RGBtoHSV(this.tempColour, out this._H, out this._S, out this._V);
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
                this._hexOut = (this._hexIn = ColourHelper.RGBtoHex(this.tempColour));
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
                    texture2D.SetPixel(i, j, ColourHelper.HSVtoRGB(this.H, s, v, this.A));
                }
            }
            texture2D.Apply();
            this._colourPickerBG = texture2D;
        }
        private void CreateHuePickerBG()
        {
            Texture2D texture2D = new Texture2D(1, this.pickerSize);
            int num = this.pickerSize;
            float unitsPerPixel = this.UnitsPerPixel;
            for (int i = 0; i < num; i++)
            {
                texture2D.SetPixel(0, i, ColourHelper.HSVtoRGB(unitsPerPixel * (float)i, 1f, 1f, 1f));
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
            this.Notify_HSVUpdated();
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
            this.Notify_RGBUpdated();
            this._alphaPosition = this.Colour.a / this.UnitsPerPixel;
        }
        public void SetWindowSize(Vector2 size)
        {
            this.currentWindowRect.width = size.x;
            this.currentWindowRect.height = size.y;
        }
        public void SetWindowLocation(Vector2 location)
        {
            this.currentWindowRect.xMin = location.x;
            this.currentWindowRect.yMin = location.y;
        }
        public void SetWindowRcet(Rect rect)
        {
            this.currentWindowRect = rect;
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
                    if (ColourHelper.TryHexToRGB(this._hexIn, ref this.tempColour))
                    {
                        this.Notify_RGBUpdated();
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
