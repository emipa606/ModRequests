using System;
using UnityEngine;
using Verse;

namespace StevesDoors.Utils
{
    public class Dialogue_DoorAccentsColorPicker : Window
    {
        private float _colorWheelSize;
        private readonly float _acceptButtonWidth;
        private float _colorWheelPosX;
        private float _colorWheelPosY;
        private bool _hsvColorWheelDragging;
        private Color _selectedColor;
        private readonly Color _oldColor;
        private readonly Action<Color> _colorSelectedCallback;
        private float _iconSize;
        private float _iconSpacing;
        private string _labelCur;
        private string _labelOld;
        private float _colorBarWidth;

        public override Vector2 InitialSize => new(300f, 215f);
        
        public Dialogue_DoorAccentsColorPicker(Color oldColor, Action<Color> colorSelectedCallback)
        {
            closeOnClickedOutside = true;
            _oldColor = oldColor;
            _selectedColor = oldColor;
            _colorSelectedCallback = colorSelectedCallback;
            _acceptButtonWidth = 35f;
        }

        public override void DoWindowContents(Rect inRect)
        {
            _colorWheelSize = inRect.width / 2f;
            _colorWheelPosX = inRect.xMin;
            _colorWheelPosY = inRect.yMin;
            Widgets.HSVColorWheel(new Rect(_colorWheelPosX, _colorWheelPosY, _colorWheelSize, _colorWheelSize), ref _selectedColor, ref _hsvColorWheelDragging, 1f);
            ColorReadback(inRect, _selectedColor, _oldColor);

            // Accept button
            if (Widgets.ButtonText(new Rect(inRect.center.x - _acceptButtonWidth, inRect.yMax - _acceptButtonWidth, 70f, _acceptButtonWidth), "Accept"))
            {
                _colorSelectedCallback?.Invoke(_selectedColor);
                Close();
            }

            _iconSize = 20f;
            _iconSpacing = _acceptButtonWidth;
            var iconPosX = inRect.xMax - _colorBarWidth;
            var iconPosY = inRect.yMin + ((_colorBarWidth / 5f) * 5.5f);

            // Copy icon
            if (Widgets.ButtonImage(new Rect(iconPosX, iconPosY, _iconSize, _iconSize), ContentFinder<Texture2D>.Get("UI/Buttons/Copy"),
                false, tooltip: "Copy the current color."))
            {
                var colorString = ColorUtility.ToHtmlStringRGB(_selectedColor);
                GUIUtility.systemCopyBuffer = colorString;
            }

            // Paste icon
            if (Widgets.ButtonImage(new Rect(iconPosX + _iconSpacing, iconPosY, _iconSize, _iconSize), ContentFinder<Texture2D>.Get("UI/Buttons/Paste"),
                false, tooltip: "Paste the copied color."))
            {
                string colorString = GUIUtility.systemCopyBuffer;

                if (ColorUtility.TryParseHtmlString("#" + colorString, out Color parsedColor))
                {
                    _selectedColor = parsedColor;
                }
                else
                {
                    SDLog.Warning("Failed to parse color string from clipboard.");
                }
            }

            // Reset to default icon
            if (Widgets.ButtonImage(new Rect(iconPosX + _iconSpacing * 2f, iconPosY, _iconSize, _iconSize), ContentFinder<Texture2D>.Get("UI/Widgets/RotLeft"),
                false, tooltip: "Reset the color to white."))
            {
                _selectedColor = Color.white;
            }
        }

        private void ColorReadback(Rect rect, Color currentColor, Color oldColor)
        {
            _labelCur = "New Color";
            _labelOld = "Old Color";

            _colorBarWidth = Mathf.Max(100f, _labelCur.GetWidthCached());
            var colorBarPosX = rect.xMax - _colorBarWidth;
            var colorBarPosY = rect.yMin;

            Widgets.Label(new Rect(colorBarPosX, colorBarPosY, _colorBarWidth, _colorBarWidth / 5), _labelCur);
            Widgets.DrawBoxSolid(new Rect(colorBarPosX, colorBarPosY + (_colorBarWidth / 5f), _colorBarWidth, _colorBarWidth / 5f), currentColor);

            Widgets.Label(new Rect(colorBarPosX, colorBarPosY + ((_colorBarWidth / 5f) * 2.5f), _colorBarWidth, _colorBarWidth / 5), _labelOld);
            Widgets.DrawBoxSolid(new Rect(colorBarPosX, colorBarPosY + ((_colorBarWidth / 5f) * 3.5f), _colorBarWidth, _colorBarWidth / 5f), oldColor);
        }
    }
}
