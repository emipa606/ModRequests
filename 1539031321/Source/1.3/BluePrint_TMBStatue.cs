using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace StatueOfColonist {
    public class BluePrint_TMBStatue : Blueprint_Install {
        private static Color BlueprintColor = new Color(0.5f, 0.5f, 1f, 0.35f);

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!map.dynamicDrawManager.DrawThingsForReading.Contains(this)) {
                map.dynamicDrawManager.RegisterDrawable(this);
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish) {
            if (this.Map.dynamicDrawManager.DrawThingsForReading.Contains(this)) {
                this.Map.dynamicDrawManager.DeRegisterDrawable(this);
            }
            base.DeSpawn(mode);
        }

        public override void Draw() {
            base.Draw();
            Building_StatueOfColonist statue = this.MiniToInstallOrBuildingToReinstall.GetInnerIfMinified() as Building_StatueOfColonist;
            if (statue != null) {
                if (!statue.IsValid) {
                    Pawn p = this.Map.mapPawns.FreeColonistsAndPrisoners.RandomElement();
                    statue.ResolveGraphics(p);
                }
                Vector3 pos = this.DrawPos + new Vector3(statue.Def.offsetX, 0f, statue.Def.offsetZ);
                statue.Render(pos, Quaternion.identity, true, this.Rotation, this.Rotation, RotDrawMode.Fresh, false, false, statue.Def.scale, Building_StatueOfColonist.RenderMode.Blueprint);
            }
        }
    }
}
