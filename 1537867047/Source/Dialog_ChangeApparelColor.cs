using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace AppearanceClothes {
    public class Dialog_ChangeApparelColor : Window {
        private Pawn pawn;
        private Apparel apparel;
        private Color color;

        private float colorHue;
        private float colorSaturation;
        private float colorValue;

        private Color oldColor;

        private String bufferColorCode;

        public override Vector2 InitialSize {
            get {
                return new Vector2(500f, 380f);
            }
        }

        public Dialog_ChangeApparelColor(Pawn pawn, Apparel apparel) {
            this.pawn = pawn;
            this.apparel = apparel;
            this.color = this.apparel.DrawColor;
            Color.RGBToHSV(this.color, out colorHue, out colorSaturation, out colorValue);
            UpdateBufferColorCode();

            this.optionalTitle = "AppearanceClothes.DialogChangeApparelColorTitle".Translate();
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect) {
            this.oldColor = this.color;

            Text.Font = GameFont.Medium;

            if (KeyBindingDef.Named("TMB_PickColor").JustPressed) {
                Texture2D texture = new Texture2D(Find.Camera.pixelWidth, Find.Camera.pixelHeight);
                texture.ReadPixels(new Rect(0, 0, Find.Camera.pixelWidth, Find.Camera.pixelHeight), 0, 0);
                texture.Apply();
                Vector2 position = UI.MousePositionOnUI;
                Color color = texture.GetPixel((int)position.x, (int)position.y);
                this.color = color;

            }

            //RGB
            this.color.r = Widgets.HorizontalSlider(new Rect(160f, 0f, 200f, 30f), this.color.r, 0f, 1f, false, "R");
            this.color.g = Widgets.HorizontalSlider(new Rect(160f, 30f, 200f, 30f), this.color.g, 0f, 1f, false, "G");
            this.color.b = Widgets.HorizontalSlider(new Rect(160f, 60f, 200f, 30f), this.color.b, 0f, 1f, false, "B");
            if (this.color != this.oldColor) {
                Color.RGBToHSV(this.color, out colorHue, out colorSaturation, out colorValue);
            }

            //HSV
            this.colorHue = Widgets.HorizontalSlider(new Rect(160f, 110f, 200f, 30f), this.colorHue, 0f, 1f, false, "H");
            this.colorSaturation = Widgets.HorizontalSlider(new Rect(160f, 140f, 200f, 30f), this.colorSaturation, 0f, 1f, false, "S");
            this.colorValue = Widgets.HorizontalSlider(new Rect(160f, 170f, 200f, 30f), this.colorValue, 0f, 1f, false, "V");
            this.color = Color.HSVToRGB(colorHue, colorSaturation, colorValue);

            //Hex color
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(160f,220f,120f,25f), "AppearanceClothes.HexColorCode".Translate());
            HexColorCodeField(new Rect(280f,218f,100f,25f));

            //Preview
            DrawApparelIcon(new Rect(13f, 36f, 128f, 128f));


            if (Widgets.ButtonText(new Rect(inRect.width / 2f - 50f, inRect.height - 40f, 100f, 40f), "OK", true, false, true) || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)) {
                this.apparel.DrawColor = this.color;
                this.pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
                PortraitsCache.SetDirty(this.pawn);
                GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(this.pawn);
                Find.WindowStack.TryRemove(this, true);
            }

            Text.Font = GameFont.Medium;
        }

        private void DrawApparelIcon(Rect rect) {
            GUI.color = this.color;
            float num = GenUI.IconDrawScale(this.apparel.def);
            if (num != 1f) {
                Vector2 center = rect.center;
                rect.width *= num;
                rect.height *= num;
                rect.center = center;
            }
            Texture2D tex = null;
            float scale = 1f;
            BodyTypeDef bodyType = pawn.GetAppearanceBodyTypeDef();
            bool flipped = false;
            if (!AppearanceClothesMod.Settings.showClothForDisplay || !this.apparel.def.TryGetApparelTexture(bodyType, out tex, Rot4.South, out flipped)) {
                tex = this.apparel.def.uiIcon;
            } else {
                scale = AppearanceClothesMod.Settings.GetScaleOfCloth(bodyType);
            }
            GUI.DrawTexture(rect, tex);
            GUI.color = Color.white;
        }

        private void HexColorCodeField(Rect rect) {
            if (oldColor != color) {
                UpdateBufferColorCode();
            }
            
            bufferColorCode = Widgets.TextField(rect,bufferColorCode);
            Color outColor = Color.white;
            bool isColorFormat = ColorUtility.TryParseHtmlString(bufferColorCode, out outColor);
            Color textColor = isColorFormat ? Widgets.NormalOptionColor : new Color(0.5f, 0.5f, 0.5f);
            if (Widgets.ButtonText(new Rect(rect.xMax + 15, rect.y, 70f, rect.height), "OK", false, false, textColor, true)) {
                if (isColorFormat) {
                    this.color = outColor;
                    Color.RGBToHSV(this.color, out colorHue, out colorSaturation, out colorValue);
                } else {
                    Messages.Message("AppearanceClothes.MessageHexColorCodeIsIllFormed".Translate(),MessageTypeDefOf.CautionInput);
                }
            }
            
        }

        private void UpdateBufferColorCode() {
            int r = (int)(color.r * 255);
            int g = (int)(color.g * 255);
            int b = (int)(color.b * 255);
            int code = r * 65536 + g * 256 + b;
            bufferColorCode = "#" + code.ToString("X6");
        }
    }
}
