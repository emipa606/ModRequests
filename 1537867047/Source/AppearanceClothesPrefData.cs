using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AppearanceClothes {
    public class AppearanceClothesPrefData : IExposable {
        public List<AppearanceClothesPreset> presets = new List<AppearanceClothesPreset>();

        public void ExposeData() {
            Scribe_Collections.Look<AppearanceClothesPreset>(ref this.presets, "presets", LookMode.Deep);
        }
    }
}
