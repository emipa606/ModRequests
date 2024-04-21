using RimWorld;
using System.Collections.Generic;
using Verse;

namespace StatueOfAnimal {
    public class StatueOfAnimalPrefData : IExposable {
        public List<StatueOfAnimalPreset> presets = new List<StatueOfAnimalPreset>();

        public void ExposeData() {
            Scribe_Collections.Look<StatueOfAnimalPreset>(ref this.presets, "presets", LookMode.Deep);
        }
    }
}
