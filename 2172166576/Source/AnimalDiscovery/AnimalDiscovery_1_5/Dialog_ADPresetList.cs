using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnimalDiscovery
{
    [StaticConstructorOnStartup]
    public class Dialog_ADPresetList : Window
    {
		public override Vector2 InitialSize => new Vector2(600f, 700f);

        public Dialog_ADPresetList()
        {
            this.draggable = true;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.resizeable = true;
            AnimalDiscoveryPref.LoadPref();
            this.LoadSettings();
        }

        public override void DoWindowContents(Rect inRect)
        {
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
            foreach (AnimalDiscoveryPreset preset in presets)
            {
                Rect rect = new Rect(0f, num2, vector.x, vector.y);
                if ((num3 & 1) == 0)
                {
                    Widgets.DrawAltRect(rect);
                }
                Rect position = rect.ContractedBy(1f);
                GUI.BeginGroup(position);
                GUI.color = this.PresetNameColor(preset);
                Rect rect2 = new Rect(15f, 0f, position.width, position.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Small;
                Widgets.Label(rect2, preset.name);
                TooltipHandler.TipRegion(rect2, preset.GetToolTip());
                GUI.color = Color.white;
                new Rect(270f, 0f, 200f, position.height);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
                float num4 = vector.x - 2f - vector2.x - vector2.y;
                if (Widgets.ButtonText(new Rect(num4, 0f, vector2.x, vector2.y), "AnimalDiscovery.LoadPreset".Translate(), true, false, preset.IsValid))
                {
                    this.Load(preset);
                }
                Rect rect3 = new Rect(num4 + vector2.x + 5f, 0f, vector2.y, vector2.y);
                if (Widgets.ButtonImage(rect3, Dialog_ADPresetList.DeleteX, true))
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(preset.name), delegate
                    {
                        this.Remove(preset);
                    }, true, null, WindowLayer.Dialog));
                }
                TooltipHandler.TipRegion(rect3, "AnimalDiscovery.DeleteThisPreset".Translate());
                GUI.EndGroup();
                num2 += vector.y + 3f;
                num3++;
            }
            Widgets.EndScrollView();
            this.DoTypeInField(inRect.AtZero());
        }

        public void Load(AnimalDiscoveryPreset preset)
        {
            foreach (AnimalAlertSettingItem item in preset.animalList)
            {
                item.ResolveDef();
                bool flag = false;
                if (item.def != null)
                {
                    foreach (AnimalAlertSettingItem item2 in AnimalDiscoveryMod.Settings.settingItems)
                    {
                        if (item2.def != null && item.def == item2.def)
                        {
                            flag = true;
                            break;
                        }
                    }
                    // def一致するものがあった場合
                    if (flag)
                    {
                        continue;
                    }
                }
                foreach (AnimalAlertSettingItem item2 in AnimalDiscoveryMod.Settings.settingItems)
                {
                    if (item.defName == item2.defName)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    continue;
                }
                AnimalDiscoveryMod.Settings.settingItems.Add(item);
            }
            Close();
        }

        public void Save(AnimalDiscoveryPreset preset)
        {
            presets.Add(preset);
            SavePreset();
        }

        public void Remove(AnimalDiscoveryPreset preset)
        {
            presets.Remove(preset);
            SavePreset();
        }

        public void SavePreset()
        {
            AnimalDiscoveryPref.presets = presets;
            AnimalDiscoveryPref.SavePref();
        }

        public void LoadSettings()
        {
            this.presets = AnimalDiscoveryPref.presets;
            if (this.presets == null)
            {
                Log.Message("AnimalDiscoveryPreset is null");
                this.presets = new List<AnimalDiscoveryPreset>();
            }
        }

        public void DoTypeInField(Rect rect)
        {
            GUI.BeginGroup(rect);
            bool flag = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
            float y = rect.height - 52f;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.SetNextControlName("MapNameField");
            string str = Widgets.TextField(new Rect(5f, y, 400f, 35f), this.typingName);
            if (GenText.IsValidFilename(str))
            {
                this.typingName = str;
            }
            if (!this.focusedNameArea)
            {
                UI.FocusControl("MapNameField", this);
                this.focusedNameArea = true;
            }
            if (Widgets.ButtonText(new Rect(420f, y, rect.width - 400f - 20f, 35f), "AnimalDiscovery.SavePreset".Translate(), true, false, true) || flag)
            {
                if (this.typingName.NullOrEmpty())
                {
                    Messages.Message("NeedAName".Translate(), MessageTypeDefOf.RejectInput, true);
                }
                else
                {
                    this.Save(new AnimalDiscoveryPreset(this.typingName));
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        public Color PresetNameColor(AnimalDiscoveryPreset preset)
        {
            if (preset != null && preset.IsValid)
            {
                return Dialog_ADPresetList.DefaultPresetTextColor;
            }
            return Dialog_ADPresetList.InvalidPresetTextColor;
        }

        public static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);

        private readonly float bottomAreaHeight = 85f;

        private List<AnimalDiscoveryPreset> presets = new List<AnimalDiscoveryPreset>();

        private Vector2 scrollPosition = Vector2.zero;

        private string typingName = string.Empty;

        private bool focusedNameArea;

        private static readonly Color DefaultPresetTextColor = new Color(1f, 1f, 0.6f);

        private static readonly Color InvalidPresetTextColor = new Color(1f, 0f, 0f);
    }
}
