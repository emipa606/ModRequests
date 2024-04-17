using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace StatueOfColonist {
    public class StatueOfColonistMod : Mod {
        public static StatueOfColonistSettings Settings {
            get {
                return LoadedModManager.GetMod<StatueOfColonistMod>().settings;
            }
        }
        
        public StatueOfColonistSettings settings;
        private List<string> buffers = new List<string>() { "64", "1", "1", "1", "1", "1", "0", "0", "0", "-0.1", "0", "0.05", "0", "0", "0", "-0.1", "0.2", "0.3", "0.3", "0.05", "0.05", "0" };
        private bool isInitBuffers = false;

        public StatueOfColonistMod(ModContentPack content) : base(content) {
            this.settings = GetSettings<StatueOfColonistSettings>();

            settings.Refresh();
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            float num = inRect.y;

            Text.Font = GameFont.Small;

            Rect rectAutoNameStatue = new Rect(inRect.x, num, 500f, 24f);
            Widgets.CheckboxLabeled(rectAutoNameStatue, "StatueOfColonist.AutoNameStatue".Translate(), ref settings.autoName);
            num += 32f;

            Rect rectShowClothForDisplay = new Rect(inRect.x, num, 500f, 24f);
            Widgets.CheckboxLabeled(rectShowClothForDisplay, "StatueOfColonist.ShowClothForDisplay".Translate(), ref settings.showClothForDisplay);
            num += 32f;

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
                Widgets.Label(rectLabel, "StatueOfColonist.ClothIconSize".Translate());
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
                Rect rectLabel = new Rect(inRect.x, num, 500f, Text.CalcHeight("StatueOfColonist.ScaleOfCloth".Translate(), 500f));
                Widgets.Label(rectLabel, "StatueOfColonist.ScaleOfCloth".Translate());
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
                Widgets.Label(rectLabel, "StatueOfColonist.StatueFromPreset".Translate());
                TooltipHandler.TipRegion(rectLabel, "StatueOfColonist.StatueFromPresetDesc".Translate());
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
                Rect rectLabel = new Rect(inRect.x, num, 500f, Text.CalcHeight("StatueOfColonist.ScaleOfCloth".Translate(), 500f));
                Widgets.Label(rectLabel, "StatueOfColonist.OffsetStatueBody".Translate());
                num += rectLabel.height + 2f;
            }

            {
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Rect rectLabel = new Rect(inRect.x + 100f, num, 100f, Text.CalcHeight("StatueOfColonist.OffsetStatueBodyNormal".Translate(), 500f));
                Widgets.Label(rectLabel, "StatueOfColonist.OffsetStatueBodyNormal".Translate());
                Rect rectLabel2 = new Rect(inRect.x + 205f, num, 100f, Text.CalcHeight("StatueOfColonist.OffsetStatueBodyLarge".Translate(), 500f));
                Widgets.Label(rectLabel2, "StatueOfColonist.OffsetStatueBodyLarge".Translate());
                Rect rectLabel3 = new Rect(inRect.x + 310f, num, 100f, Text.CalcHeight("StatueOfColonist.OffsetStatueBodyGrand".Translate(), 500f));
                Widgets.Label(rectLabel3, "StatueOfColonist.OffsetStatueBodyGrand".Translate());
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

            Text.Font = GameFont.Medium;
        }

        public override string SettingsCategory() {
            return "Statue Of Colonist";
        }
        
    }
}
