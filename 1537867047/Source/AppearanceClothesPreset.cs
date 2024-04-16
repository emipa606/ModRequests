using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AppearanceClothes {
    public class AppearanceClothesPreset : IExposable {
        public string name;
        public List<AppearanceClothData> appearanceClothes = new List<AppearanceClothData>();

        public bool IsValid {
            get {
                return appearanceClothes.All(ap => ap.IsValid);
            }
        }

        public AppearanceClothesPreset() {
            this.appearanceClothes = new List<AppearanceClothData>();
        }

        public AppearanceClothesPreset(string name,CompAppearanceClothes comp) {
            this.name = name;
            SetAppearanceClothes(comp);
        }

        public void SetAppearanceClothes(CompAppearanceClothes comp) {
            this.appearanceClothes = new List<AppearanceClothData>();
            foreach (Apparel apparel in comp.AppearanceClothes) {
                appearanceClothes.Add(new AppearanceClothData(apparel));
            }
        }

        public void ExposeData() {
            Scribe_Values.Look<string>(ref this.name, "name");
            Scribe_Collections.Look<AppearanceClothData>(ref this.appearanceClothes, "appearanceClothes", LookMode.Deep);
        }

        public string GetToolTip() {
            StringBuilder sb = new StringBuilder();

            foreach (AppearanceClothData ap in appearanceClothes) {
                if (ap.IsValid) {
                    if (ap.apparelDef.HasComp(typeof(CompColorable))) {
                        string colorStr = string.Format("({0},{1},{2})", new object[] { (int)(ap.apparelColor.r * 255), (int)(ap.apparelColor.g * 255), (int)(ap.apparelColor.b * 255) });
                        sb.AppendLine("AppearanceClothes.ColorableApparelData".Translate( ap.apparelDef.LabelCap, colorStr ));
                    } else {
                        sb.AppendLine("AppearanceClothes.ApparelData".Translate( ap.apparelDef.LabelCap ));
                    }
                }
            }

            if (appearanceClothes.Any(ap => !ap.IsValid)) {
                if (sb.Length > 0) {
                    sb.AppendLine();
                }
                foreach (AppearanceClothData ap in appearanceClothes) {
                    if (!ap.IsValid) {
                        sb.AppendLine("AppearanceClothes.ApparelIsNotFound".Translate( ap.apparelDefName ));
                    }
                }
            }

            return sb.ToString();
        }
    }
}
