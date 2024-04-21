using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StatueOfAnimal {
    public class StatueOfAnimalSettings : ModSettings {
        public bool autoName = true;
        public float possibilityOfStatueFromPreset = 0f;

        public override void ExposeData() {
            Scribe_Values.Look(ref autoName, "autoName",true);
            Scribe_Values.Look(ref possibilityOfStatueFromPreset, "possibilityOfStatueFromPreset", 0f);
        }
    }
}
