using RimWorld;
using System.Collections.Generic;
using Verse;

namespace StatueOfColonist {
    public class StatueOfColonistPrefData : IExposable {
        public List<StatueOfColonistPreset> presets = new List<StatueOfColonistPreset>();

        public void ExposeData() {
            Scribe_Collections.Look<StatueOfColonistPreset>(ref this.presets, "presets", LookMode.Deep);
        }
    }
}
