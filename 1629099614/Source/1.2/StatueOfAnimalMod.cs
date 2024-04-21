using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace StatueOfAnimal {
    public class StatueOfAnimalMod : Mod {
        public static StatueOfAnimalSettings Settings {
            get {
                return LoadedModManager.GetMod<StatueOfAnimalMod>().settings;
            }
        }
        
        public StatueOfAnimalSettings settings;
        private List<string> buffers = new List<string>() { "0" };
        private bool isInitBuffers = false;

        public StatueOfAnimalMod(ModContentPack content) : base(content) {
            this.settings = GetSettings<StatueOfAnimalSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            float num = inRect.y;

            Text.Font = GameFont.Small;

            if (!isInitBuffers) {
                buffers[0] = settings.possibilityOfStatueFromPreset.ToString();
                isInitBuffers = true;
            }

            {
                Rect rectCheckbox = new Rect(inRect.x, num, 500f, 24f);
                Widgets.CheckboxLabeled(rectCheckbox, "StatueOfAnimal.AutoNameStatue".Translate(), ref settings.autoName);
                num += 32f;
            }

            {
                string buffer = buffers[0];
                Rect rectLabel = new Rect(0f, num, 400f, 24f);
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rectLabel, "StatueOfAnimal.StatueFromPreset".Translate());
                TooltipHandler.TipRegion(rectLabel, "StatueOfAnimal.StatueFromPresetDesc".Translate());
                Text.Anchor = anchor;
                Rect rectTextField = new Rect(400f, num, 100f, 24f);
                buffer = Widgets.TextField(rectTextField, buffer);
                buffers[0] = buffer;

                if (buffer.IsFullyTypedFloat()) {
                    float value;
                    if (float.TryParse(buffer, out value)) {
                        settings.possibilityOfStatueFromPreset = value;
                    }
                }

                num += 36f;
            }

            /*
            if (!isInitBuffers) {
                buffers[0] = settings.clothIconSize.ToString();
                for (int i = 0; i < 5; i++) {
                    BodyTypeDef bodyType = i.ToBodyTypeDef();
                    buffers[i + 1] = settings.GetScaleOfCloth(bodyType).ToString();
                }
                buffers[6] = settings.possibilityOfStatueFromPreset.ToString();
                for (int i=0;i<15;i++) {
                    buffers[i + 7] = settings.offsetStatueBody[i].ToString();
                }
                isInitBuffers = true;
            }

            {
                string buffer = buffers[0];
                Rect rectLabel = new Rect(0f, num, 400f, 24f);
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rectLabel, "StatueOfAnimal.ClothIconSize".Translate());
                Text.Anchor = anchor;
                Rect rectTextField = new Rect(400f, num, 100f, 24f);
                buffer = Widgets.TextField(rectTextField, buffer);
                buffers[0] = buffer;

                if (buffer.IsFullyTypedFloat()) {
                    float value;
                    if (float.TryParse(buffer, out value)) {
                        settings.clothIconSize = value;
                    }
                }

                num += 36f;
            }

            {
                Rect rectLabel = new Rect(inRect.x, num, 500f, Text.CalcHeight("StatueOfAnimal.ScaleOfCloth".Translate(), 500f));
                Widgets.Label(rectLabel, "StatueOfAnimal.ScaleOfCloth".Translate());
                num += rectLabel.height + 4f;
            }

            for (int i = 0; i < 5; i++) {
                BodyTypeDef bodyType = i.ToBodyTypeDef();

                string buffer = buffers[i + 1];
                Rect rectLabel = new Rect(0f, num, 100f, 24f);
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rectLabel, bodyType.GetName());
                Text.Anchor = anchor;
                Rect rectTextField = new Rect(100f, num, 100f, 24f);
                buffer = Widgets.TextField(rectTextField, buffer);
                num += 24f;
                buffers[i + 1] = buffer;

                if (buffer.IsFullyTypedFloat()) {
                    float value;
                    if (float.TryParse(buffer, out value)) {
                        settings.SetScaleOfCloth(bodyType, value);
                    }
                }
            }

            num += 12f;

            {
                string buffer = buffers[6];
                Rect rectLabel = new Rect(0f, num, 400f, 24f);
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rectLabel, "StatueOfAnimal.StatueFromPreset".Translate());
                TooltipHandler.TipRegion(rectLabel, "StatueOfAnimal.StatueFromPresetDesc".Translate());
                Text.Anchor = anchor;
                Rect rectTextField = new Rect(400f, num, 100f, 24f);
                buffer = Widgets.TextField(rectTextField, buffer);
                buffers[6] = buffer;

                if (buffer.IsFullyTypedFloat()) {
                    float value;
                    if (float.TryParse(buffer, out value)) {
                        settings.possibilityOfStatueFromPreset = value;
                    }
                }

                num += 36f;
            }

            {
                Rect rectLabel = new Rect(inRect.x, num, 500f, Text.CalcHeight("StatueOfAnimal.ScaleOfCloth".Translate(), 500f));
                Widgets.Label(rectLabel, "StatueOfAnimal.OffsetStatueBody".Translate());
                num += rectLabel.height + 2f;
            }

            {
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Rect rectLabel = new Rect(inRect.x + 100f, num, 100f, Text.CalcHeight("StatueOfAnimal.OffsetStatueBodyNormal".Translate(), 500f));
                Widgets.Label(rectLabel, "StatueOfAnimal.OffsetStatueBodyNormal".Translate());
                Rect rectLabel2 = new Rect(inRect.x + 205f, num, 100f, Text.CalcHeight("StatueOfAnimal.OffsetStatueBodyLarge".Translate(), 500f));
                Widgets.Label(rectLabel2, "StatueOfAnimal.OffsetStatueBodyLarge".Translate());
                Rect rectLabel3 = new Rect(inRect.x + 310f, num, 100f, Text.CalcHeight("StatueOfAnimal.OffsetStatueBodyGrand".Translate(), 500f));
                Widgets.Label(rectLabel3, "StatueOfAnimal.OffsetStatueBodyGrand".Translate());
                Text.Anchor = anchor;
                num += rectLabel.height;
            }

            for (int i = 0; i < 5; i++) {
                BodyTypeDef bodyType = i.ToBodyTypeDef();
                Rect rectLabel = new Rect(0f, num, 100f, 24f);
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rectLabel, bodyType.GetName());
                Text.Anchor = anchor;

                for (int j = 0; j < 3; j++) {
                    int index = i * 3 + j;
                    string buffer = buffers[index + 7];
                    Rect rectTextField = new Rect(100f + j * 105f, num, 100f, 24f);
                    buffer = Widgets.TextField(rectTextField, buffer);
                    buffers[index + 7] = buffer;

                    if (buffer.IsFullyTypedFloat()) {
                        float value;
                        if (float.TryParse(buffer, out value)) {
                            settings.offsetStatueBody[index] = value;
                        }
                    }
                }

                num += 24f;
            }
            */

            Text.Font = GameFont.Medium;
        }

        public override string SettingsCategory() {
            return "Statue Of Animal";
        }
        
    }
}
