using Rimatomics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
    [StaticConstructorOnStartup]
    class Building_ShipCapacitorPPC : Building_PPC
    {
        public override void Tick()
        {
            base.Tick();
            this.BarTo = 0;
        }

        private static Graphic barGraphic = GraphicDatabase.Get(typeof(Graphic_Multi), "Things/Building/Ship/CapacitorBar", ShaderDatabase.Cutout, new Vector2(3, 5), Color.white, Color.white);

        public override void Draw()
        {
            base.Draw();
            Color barColor;
            if (this.TryGetComp<CompPowerBattery>().StoredEnergyPct < 0.25f)
                barColor = new Color(0.25f + this.TryGetComp<CompPowerBattery>().StoredEnergyPct * 3, 0, 0);
            else
            {
                float angle = (this.TryGetComp<CompPowerBattery>().StoredEnergyPct - 0.25f) * 2 * Mathf.PI / 3;
                barColor = new Color(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            }
            barGraphic.GetColoredVersion(ShaderDatabase.Cutout, barColor, barColor).Draw(new Vector3(this.DrawPos.x, this.DrawPos.y + 1f, this.DrawPos.z), this.Rotation, this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }
    }
}
