using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace StatueOfColonist {
    public class StatueOfColonistApparelData : IExposable {
        public ThingDef apparelDef;
        public string apparelDefName;

        public bool IsValid {
            get {
                return (apparelDef != null);
            }
        }

        public StatueOfColonistApparelData() {}

        public StatueOfColonistApparelData(ThingDef apparel) {
            this.apparelDef = apparel;
        }

        public void ExposeData() {
            if (Scribe.mode == LoadSaveMode.Saving) {
                if (this.apparelDef != null) {
                    this.apparelDefName = this.apparelDef.defName;
                }
            }

            Scribe_Values.Look<string>(ref this.apparelDefName, "apparelDef");

            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                this.apparelDef = DefDatabase<ThingDef>.GetNamed(this.apparelDefName, false);
                if (this.apparelDef == null) {
                    Log.Warning("apparelDef \"" + apparelDefName + "\" is not found.");
                }
            }
        }
    }
}
