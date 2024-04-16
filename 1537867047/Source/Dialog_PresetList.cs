using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;
using RimWorld;

namespace AppearanceClothes {
    [StaticConstructorOnStartup]
    public class Dialog_PresetList : Window {
        public static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);

        private const float BoxMargin = 20f;

        private const float EntrySpacing = 3f;

        private const float EntryMargin = 1f;

        private const float NameExtraLeftMargin = 15f;

        private const float InfoExtraLeftMargin = 270f;

        private const float DeleteButtonSpace = 5f;

        private const float EntryHeight = 36f;

        private const float NameTextFieldWidth = 400f;

        private const float NameTextFieldHeight = 35f;

        private const float NameTextFieldButtonSpace = 20f;

        private float bottomAreaHeight = 85f;

        private List<AppearanceClothesPreset> presets = new List<AppearanceClothesPreset>();

        private Vector2 scrollPosition = Vector2.zero;

        private string typingName = string.Empty;

        private bool focusedNameArea;

        private CompAppearanceClothes compAppearanceClothes;

        private static readonly Color DefaultPresetTextColor = new Color(1f, 1f, 0.6f);

        private static readonly Color InvalidPresetTextColor = new Color(1f, 0f, 0f);

        public override Vector2 InitialSize {
            get {
                return new Vector2(600f, 700f);
            }
        }

        public Dialog_PresetList(CompAppearanceClothes compAppearanceClothes) {
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.compAppearanceClothes = compAppearanceClothes;
            this.LoadSettings();
        }

        public override void DoWindowContents(Rect inRect) {
            Vector2 vector = new Vector2(inRect.width - 16f, 36f);
            Vector2 vector2 = new Vector2(100f, vector.y - 2f);
            inRect.height -= 45f;
            float num = vector.y + 3f;
            float height = (float)this.presets.Count * num;
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, height);
            Rect outRect = new Rect(inRect.AtZero());
            outRect.height -= this.bottomAreaHeight;
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
            float num2 = 0f;
            int num3 = 0;
            foreach (AppearanceClothesPreset current in this.presets) {
                Rect rect = new Rect(0f, num2, vector.x, vector.y);
                if (num3 % 2 == 0) {
                    Widgets.DrawAltRect(rect);
                }
                Rect position = rect.ContractedBy(1f);
                GUI.BeginGroup(position);
                GUI.color = this.PresetNameColor(current);
                Rect rect2 = new Rect(15f, 0f, position.width, position.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Small;
                Widgets.Label(rect2, current.name);
                TooltipHandler.TipRegion(rect2, current.GetToolTip());
                GUI.color = Color.white;
                Rect rect3 = new Rect(270f, 0f, 200f, position.height);
                //Dialog_FileList.DrawDateAndVersion(current, rect3);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
                float num4 = vector.x - 2f - vector2.x - vector2.y;
                Rect rect4 = new Rect(num4, 0f, vector2.x, vector2.y);
                if (Widgets.ButtonText(rect4, "AppearanceClothes.LoadPreset".Translate(), true, false, true)) {
                    Load(current);
                }
                Rect rect5 = new Rect(num4 + vector2.x + 5f, 0f, vector2.y, vector2.y);
                if (Widgets.ButtonImage(rect5, Dialog_PresetList.DeleteX)) {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(current.name), delegate {
                        Remove(current);
                    }, true, null));
                }
                TooltipHandler.TipRegion(rect5, "AppearanceClothes.DeleteThisPreset".Translate());
                GUI.EndGroup();
                num2 += vector.y + 3f;
                num3++;
            }
            Widgets.EndScrollView();
            this.DoTypeInField(inRect.AtZero());
        }

        public void Load(AppearanceClothesPreset preset) {
            this.compAppearanceClothes.CopyAppearanceClothesFromPreset(preset);
            this.Close(true);
        }

        public void Save(AppearanceClothesPreset preset) {
            this.presets.Add(preset);
            SaveSettings();
        }

        public void Remove(AppearanceClothesPreset preset) {
            this.presets.Remove(preset);
            SaveSettings();
        }

        public void LoadSettings() {
            this.presets = AppearanceClothesPref.presets;
            if (this.presets == null) {
                Log.Message("AppearanceClothesPreset is null");
                this.presets = new List<AppearanceClothesPreset>();
            }
        }

        public void SaveSettings() {
            AppearanceClothesPref.presets = this.presets;
            AppearanceClothesPref.SavePref();
        }

        public void DoTypeInField(Rect rect) {
            GUI.BeginGroup(rect);
            bool flag = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
            float y = rect.height - 52f;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.SetNextControlName("MapNameField");
            Rect rect2 = new Rect(5f, y, 400f, 35f);
            string str = Widgets.TextField(rect2, this.typingName);
            if (GenText.IsValidFilename(str)) {
                this.typingName = str;
            }
            if (!this.focusedNameArea) {
                UI.FocusControl("MapNameField", this);
                this.focusedNameArea = true;
            }
            Rect rect3 = new Rect(420f, y, rect.width - 400f - 20f, 35f);
            if (Widgets.ButtonText(rect3, "AppearanceClothes.SavePresetButton".Translate(), true, false, true) || flag) {
                if (this.typingName.NullOrEmpty()) {
                    Messages.Message("NeedAName".Translate(), MessageTypeDefOf.RejectInput);
                } else {
                    this.Save(new AppearanceClothesPreset(this.typingName, compAppearanceClothes));
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        public Color PresetNameColor(AppearanceClothesPreset preset) {
            if (preset.IsValid) {
                return Dialog_PresetList.DefaultPresetTextColor;
            }
            return Dialog_PresetList.InvalidPresetTextColor;
        }

        public static void DrawDateAndVersion(SaveFileInfo sfi, Rect rect) {
            GUI.BeginGroup(rect);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect2 = new Rect(0f, 2f, rect.width, rect.height / 2f);
            GUI.color = SaveFileInfo.UnimportantTextColor;
            Widgets.Label(rect2, sfi.FileInfo.LastWriteTime.ToString("g"));
            Rect rect3 = new Rect(0f, rect2.yMax, rect.width, rect.height / 2f);
            GUI.color = sfi.VersionColor;
            Widgets.Label(rect3, sfi.GameVersion);
            TooltipHandler.TipRegion(rect3, sfi.CompatibilityTip);
            GUI.EndGroup();
        }
    }
}
