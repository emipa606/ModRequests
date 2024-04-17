using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;
using RimWorld;

namespace StatueOfColonist {
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

        private List<StatueOfColonistPreset> presets = new List<StatueOfColonistPreset>();

        private Vector2 scrollPosition = Vector2.zero;

        private string typingName = string.Empty;

        private bool focusedNameArea;

        private Building_StatueOfColonist statue;

        private Window_EditStatue parent;

        private static readonly Color DefaultPresetTextColor = new Color(1f, 1f, 0.6f);

        private static readonly Color InvalidPresetTextColor = new Color(1f, 0f, 0f);

        public override Vector2 InitialSize {
            get {
                return new Vector2(600f, 700f);
            }
        }

        public Dialog_PresetList(Building_StatueOfColonist statue, Window_EditStatue parent) {
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.resizeable = true;
            this.statue = statue;
            this.parent = parent;
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
            foreach (StatueOfColonistPreset current in this.presets) {
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
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
                float num4 = vector.x - 2f - vector2.x - vector2.y;
                Rect rect4 = new Rect(num4, 0f, vector2.x, vector2.y);
                if (Widgets.ButtonText(rect4, "StatueOfColonist.LoadPreset".Translate(), true, false, current.IsValid)) {
                    Load(current);
                }

                {
                    string buffer = current.bufferWeight;
                    Rect rectTextField = new Rect(num4 - 90f, 6f, 80f, 24f);
                    TooltipHandler.TipRegion(rectTextField, "StatueOfColonist.WeightInputFormDesc".Translate());
                    buffer = Widgets.TextField(rectTextField, buffer);
                    current.bufferWeight = buffer;

                    if (buffer.IsFullyTypedFloat()) {
                        float value;
                        if (float.TryParse(buffer, out value)) {
                            current.weight = value;
                        }
                    }
                }

                Rect rect5 = new Rect(num4 + vector2.x + 5f, 0f, vector2.y, vector2.y);
                if (Widgets.ButtonImage(rect5, Dialog_PresetList.DeleteX)) {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(current.name), delegate {
                        Remove(current);
                    }, true, null));
                }
                TooltipHandler.TipRegion(rect5, "StatueOfColonist.DeleteThisPreset".Translate());
                GUI.EndGroup();
                num2 += vector.y + 3f;
                num3++;
            }
            Widgets.EndScrollView();
            this.DoTypeInField(inRect.AtZero());
        }

        public void Load(StatueOfColonistPreset preset) {
            this.statue.CopyStatueOfColonistFromPreset(preset);
            this.parent.UpdateButtonLabel();
            this.Close(true);
        }

        public void Save(StatueOfColonistPreset preset) {
            this.presets.Add(preset);
            SaveSettings();
        }

        public void Remove(StatueOfColonistPreset preset) {
            this.presets.Remove(preset);
            SaveSettings();
        }

        public void LoadSettings() {
            this.presets = StatueOfColonistPref.presets;
            if (this.presets == null) {
                Log.Message("StatueOfColonistPreset is null");
                this.presets = new List<StatueOfColonistPreset>();
            }
        }

        public void SaveSettings() {
            StatueOfColonistPref.presets = this.presets;
            StatueOfColonistPref.SavePref();
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
            if (Widgets.ButtonText(rect3, "StatueOfColonist.SavePresetButton".Translate(), true, false, true) || flag) {
                if (this.typingName.NullOrEmpty()) {
                    Messages.Message("NeedAName".Translate(), MessageTypeDefOf.RejectInput);
                } else {
                    this.Save(new StatueOfColonistPreset(this.typingName, statue));
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        public Color PresetNameColor(StatueOfColonistPreset preset) {
            if (preset.IsValid) {
                return Dialog_PresetList.DefaultPresetTextColor;
            }
            return Dialog_PresetList.InvalidPresetTextColor;
        }
    }
}
