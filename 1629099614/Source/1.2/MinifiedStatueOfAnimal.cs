using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using Harmony;

namespace StatueOfAnimal {
    class MinifiedStatueOfAnimal : MinifiedThing {
        Building_StatueOfAnimal Statue {
            get {
                return InnerThing as Building_StatueOfAnimal;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);

            Building_StatueOfAnimal statue = Statue;
            if (statue != null) {
                if (!statue.IsValid) {
                    statue.InitStatue();
                    statue.ResolveGraphics();
                } else {
                    //statue.ResolveGraphics();
                }
            }
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos() {
            foreach (Gizmo c in base.GetGizmos()) {
                yield return c;
            }

            Building_StatueOfAnimal statue = Statue;
            if (statue != null) {
                foreach (Gizmo gizmo in statue.GetStatueGizmos()) {
                    yield return gizmo;
                }
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false) {
            base.DrawAt(drawLoc, flip);

            Building_StatueOfAnimal statue = Statue;
            if (statue != null) {
                if (!statue.IsValid) {
                    statue.InitStatue();
                    statue.ResolveGraphics();
                }

                Vector3 pos = drawLoc + new Vector3(statue.Def.offsetXMinified, -0.024f, statue.Def.offsetZMinified);
                statue.Render(pos, Rot4.South, false, 1f, statue.Data.packed);
            }
        }
    }
}
