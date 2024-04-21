using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace StatueOfAnimal {
    public class BluePrint_TMBStatue : Blueprint_Install {
        private static Color BlueprintColor = new Color(0.5f, 0.5f, 1f, 0.35f);

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!map.dynamicDrawManager.DrawThingsForReading.Contains(this)) {
                map.dynamicDrawManager.RegisterDrawable(this);
            }
        }

        public override void Draw() {
            base.Draw();
            Building_StatueOfAnimal statue = this.MiniToInstallOrBuildingToReinstall.GetInnerIfMinified() as Building_StatueOfAnimal;
            if (statue != null) {
                if (!statue.IsValid) {
                    statue.Data = new StatueOfAnimalData(statue.DrawColor);
                    statue.ResolveGraphics();
                }
                //Log.Message("Blueprint.Draw");

                statue.DrawBase(this.DrawPos, Building_StatueOfAnimal.RenderMode.Blueprint);
                Vector3 pos = this.DrawPos + new Vector3(statue.Def.offsetX, 0f, statue.Def.offsetZ);
                statue.Render(pos, this.Rotation, false, statue.Def.scale, statue.Data.packed, Building_StatueOfAnimal.RenderMode.Blueprint);
            }
        }
    }
}
