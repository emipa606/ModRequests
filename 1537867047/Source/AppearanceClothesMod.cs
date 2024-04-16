using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AppearanceClothes {
    public class AppearanceClothesMod : Mod {
        public static AppearanceClothesSettings Settings {
            get {
                return LoadedModManager.GetMod<AppearanceClothesMod>().settings;
            }
        }
        
        public AppearanceClothesSettings settings;
        private List<string> buffers = new List<string>() { "64", "1", "1", "1", "1", "1" };
        private List<string> bodyTypes = new List<string>(){"Male","Female","Thin","Hulk","Fat"};
        private bool isInitBuffers = false;

        public AppearanceClothesMod(ModContentPack content) : base(content) {
            this.settings = GetSettings<AppearanceClothesSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            float num = inRect.y;

            Text.Font = GameFont.Small;

            {
                Rect checkBoxRect = new Rect(inRect.x, num, 500f, 24f);
                Widgets.CheckboxLabeled(checkBoxRect, "AppearanceClothes.ShowClothForDisplay".Translate(), ref settings.showClothForDisplay);
                num += 32f;
            }

            if (!isInitBuffers) {
                buffers[0] = settings.clothIconSize.ToString();
                int index = 0;
                foreach (BodyTypeDef bodyType in DefDatabase<BodyTypeDef>.AllDefsListForReading) {
                    if (index + 1 >= buffers.Count) {
                        buffers.Add("1");
                    }
                    buffers[index + 1] = settings.GetScaleOfCloth(bodyType).ToString();
                    index++;
                }
                isInitBuffers = true;
            }

            {
                string buffer = buffers[0];
                Rect rectLabel = new Rect(0f, num, 400f, 24f);
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rectLabel, "AppearanceClothes.ClothIconSize".Translate());
                Text.Anchor = anchor;
                Rect rectTextField = new Rect(400f, num, 100f, 24f);
                buffer = Widgets.TextField(rectTextField, buffer);
                buffers[0] = buffer;

                if (IsFullyTypedFloat(buffer)) {
                    float value;
                    if (float.TryParse(buffer, out value)) {
                        settings.clothIconSize = value;
                    }
                }

                num += 36f;
            }

            {
                Rect rectLabel = new Rect(inRect.x, num, 500f, Text.CalcHeight("AppearanceClothes.ScaleOfCloth".Translate(), 500f));
                Widgets.Label(rectLabel, "AppearanceClothes.ScaleOfCloth".Translate());
                num += rectLabel.height + 4f;
            }

            int indexBodyType = 0;
            foreach (BodyTypeDef bodyType in DefDatabase<BodyTypeDef>.AllDefsListForReading) {
                string buffer = buffers[indexBodyType + 1];
                Rect rectLabel = new Rect(0f, num, 300f, 24f);
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rectLabel, bodyType.defName);
                Text.Anchor = anchor;
                Rect rectTextField = new Rect(300f, num, 100f, 24f);
                buffer = Widgets.TextField(rectTextField, buffer);
                num += 24f;
                buffers[indexBodyType + 1] = buffer;

                if (IsFullyTypedFloat(buffer)) {
                    float value;
                    if (float.TryParse(buffer, out value)) {
                        settings.SetScaleOfCloth(bodyType, value);
                    }
                }
                indexBodyType++;
            }

            num += 8f;

            {
                Rect checkBoxRect = new Rect(inRect.x, num, 500f, 24f);
                Widgets.CheckboxLabeled(checkBoxRect, "AppearanceClothes.AllowDuplicationOfClothes".Translate(), ref settings.allowDuplicationOfClothes);
                num += 32f;
            }

            Text.Font = GameFont.Medium;
        }

        public override string SettingsCategory() {
            return "Appearance clothes";
        }

        private static bool IsFullyTypedFloat(string str) {
            if (str == string.Empty) {
                return false;
            }
            string[] array = str.Split(new char[]
                {
            '.'
                });
            if (array.Length > 2 || array.Length < 1) {
                return false;
            }
            if (!ContainsOnlyCharacters(array[0], "-0123456789")) {
                return false;
            }
            if (array.Length == 2 && !ContainsOnlyCharacters(array[1], "0123456789")) {
                return false;
            }
            return true;
        }

        private static bool ContainsOnlyCharacters(string str, string allowedChars) {
            for (int i = 0; i < str.Length; i++) {
                if (!allowedChars.Contains(str[i])) {
                    return false;
                }
            }
            return true;
        }
    }
}
