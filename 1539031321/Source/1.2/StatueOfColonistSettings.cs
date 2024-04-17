using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StatueOfColonist {
    public class StatueOfColonistSettings : ModSettings {
        public bool autoName = true;
        public bool showClothForDisplay = true;
        public float clothIconSize = 64f;
        private Dictionary<string, float> scaleOfCloth = new Dictionary<string, float>();
        public float possibilityOfStatueFromPreset = 0f;
        public List<float> offsetStatueBody = new List<float>();

        public StatueOfColonistSettings() {
            InitScaleOfCloth();
        }

        public float GetScaleOfCloth(BodyTypeDef bodyType) {
            if (!scaleOfCloth.ContainsKey(bodyType.defName)) {
                return 1f;
            }
            return scaleOfCloth[bodyType.defName];
        }

        public void SetScaleOfCloth(BodyTypeDef bodyType, float scale) {
            scaleOfCloth[bodyType.defName] = scale;
        }

        public float GetOffsetStatueBody(BodyTypeDef bodyType,int size) {
            int x = bodyType.ToInt();
            int y = size;
            int index = x * 3 + y;
            if (index >= offsetStatueBody.Count || index < 0) {
                return 0f;
            }
            return offsetStatueBody[index];
        }

        public void Refresh() {
            if (scaleOfCloth == null || scaleOfCloth.Count < 6) {
                InitScaleOfCloth();
            }
            if (offsetStatueBody == null || offsetStatueBody.Count < 15) {
                InitOffsetStatueBody();
            }
        }

        public override void ExposeData() {
            Scribe_Values.Look(ref autoName, "autoName");
            Scribe_Values.Look(ref showClothForDisplay, "showClothForDisplay");
            Scribe_Values.Look(ref clothIconSize, "clothIconSize", 64f);
            Scribe_Collections.Look(ref scaleOfCloth, "scaleOfCloth");
            Scribe_Values.Look(ref possibilityOfStatueFromPreset, "possibilityOfStatueFromPreset", 0f);
            Scribe_Collections.Look(ref offsetStatueBody, "offsetStatueBody");
            
            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                Refresh();
            }
        }

        public void InitScaleOfCloth() {
            scaleOfCloth = new Dictionary<string, float>();

            scaleOfCloth["Male"] = 1.25f;
            scaleOfCloth["Female"] = 1.5f;
            scaleOfCloth["Thin"] = 1.75f;
            scaleOfCloth["Hulk"] = 1.0f;
            scaleOfCloth["Fat"] = 1.0f;
        }

        public void InitOffsetStatueBody() {
            offsetStatueBody = new List<float>(15);

            offsetStatueBody.Add(0f);
            offsetStatueBody.Add(0f);
            offsetStatueBody.Add(-0.1f);

            offsetStatueBody.Add(0f);
            offsetStatueBody.Add(0.05f);
            offsetStatueBody.Add(0f);

            offsetStatueBody.Add(0f);
            offsetStatueBody.Add(0f);
            offsetStatueBody.Add(-0.1f);

            offsetStatueBody.Add(0.2f);
            offsetStatueBody.Add(0.3f);
            offsetStatueBody.Add(0.3f);

            offsetStatueBody.Add(0.05f);
            offsetStatueBody.Add(0.05f);
            offsetStatueBody.Add(0f);
        }
    }
}
