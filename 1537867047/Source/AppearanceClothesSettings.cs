using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AppearanceClothes {
    public class AppearanceClothesSettings : ModSettings {
        public bool showClothForDisplay = false;
        public float clothIconSize = 64f;
        private Dictionary<string, float> scaleOfCloth = new Dictionary<string, float>();
        public bool allowDuplicationOfClothes = false;

        public float GetScaleOfCloth(BodyTypeDef bodyType) {
            if (bodyType == null) {
                bodyType = BodyTypeDefOf.Male;
            }
            if (!scaleOfCloth.ContainsKey(bodyType.defName)) {
                InitScaleOfCloth();
            }
            if (!scaleOfCloth.ContainsKey(bodyType.defName)) {
                return 1f;
            }
            return scaleOfCloth[bodyType.defName];
        }

        public void SetScaleOfCloth(BodyTypeDef bodyType, float scale) {
            scaleOfCloth[bodyType.defName] = scale;
        }

        public override void ExposeData() {
            Scribe_Values.Look(ref showClothForDisplay, "showClothForDisplay");
            Scribe_Values.Look(ref clothIconSize, "clothIconSize", 64f);
            Scribe_Collections.Look(ref scaleOfCloth, "scaleOfCloth",LookMode.Value,LookMode.Value);
            Scribe_Values.Look(ref allowDuplicationOfClothes, "allowDuplicationOfClothes");

            if (Scribe.mode == LoadSaveMode.LoadingVars && scaleOfCloth.Count < 5) {
                InitScaleOfCloth();
            }
        }

        public void InitScaleOfCloth() {
            scaleOfCloth["Male"] = 1.25f;
            scaleOfCloth["Female"] = 1.5f;
            scaleOfCloth["Thin"] = 1.75f;
            scaleOfCloth["Hulk"] = 1.0f;
            scaleOfCloth["Fat"] = 1.0f;
            foreach (BodyTypeDef bodyTypeDef in DefDatabase<BodyTypeDef>.AllDefsListForReading) {
                if (!scaleOfCloth.ContainsKey(bodyTypeDef.defName)) {
                    scaleOfCloth[bodyTypeDef.defName] = 1.0f;
                }
            }
        }
    }
}
