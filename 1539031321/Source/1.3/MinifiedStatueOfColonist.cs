using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using HarmonyLib;

namespace StatueOfColonist {
    class MinifiedStatueOfColonist : MinifiedThing {
        Building_StatueOfColonist Statue {
            get {
                return InnerThing as Building_StatueOfColonist;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);

            Building_StatueOfColonist statue = Statue;
            if (statue != null) {
                if (statue.IsValid) {
                    statue.InitializeStatue();
                } else {
                    statue.ResolveGraphics();
                }
            }
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos() {
            foreach (Gizmo c in base.GetGizmos()) {
                yield return c;
            }

            Building_StatueOfColonist statue = Statue;
            if (statue != null) {
                foreach (Gizmo gizmo in statue.GetStatueGizmos()) {
                    yield return gizmo;
                }
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false) {
            base.DrawAt(drawLoc, flip);

            Building_StatueOfColonist statue = Statue;
            if (statue != null) {
                if (!statue.IsValid) {
                    Pawn p = Find.CurrentMap.mapPawns.FreeColonists.RandomElement();
                    statue.ResolveGraphics(p);
                }

                Vector3 pos = drawLoc + new Vector3(statue.Def.offsetXMinified, -0.024f, statue.Def.offsetZMinified);
                statue.Render(pos, Quaternion.identity, true, Rot4.South, Rot4.South, RotDrawMode.Fresh, false, false, statue.Def.scaleMinified);
            }
        }
    }
}
