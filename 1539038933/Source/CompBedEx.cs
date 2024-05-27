using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace LyingFace {
    public class CompBedEx : ThingComp {
        public List<Pawn> owners = new List<Pawn>();

        Building_Bed Bed {
            get {
                return this.parent as Building_Bed;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            if (!owners.NullOrEmpty()) {
                Bed.owners = owners;
            }
        }

        public override void PostExposeData() {
            if (Scribe.mode == LoadSaveMode.Saving) {
                owners = Bed.owners;
            }
            Scribe_Collections.Look<Pawn>(ref this.owners, "owners", LookMode.Reference, new object[0]);
        }
    }
}
