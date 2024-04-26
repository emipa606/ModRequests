using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace WallStuff
{
	[StaticConstructorOnStartup]
	public class WallMounted_Battery : WallThingBase
    {

		private static readonly Vector2 BarSize = new Vector2(0.3f, 0.6f);

		private static readonly Material BatteryBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.0f, 0.6f, 0.6f));

		private static readonly Material BatteryBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

		private GenDraw.FillableBarRequest r;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            r = default(GenDraw.FillableBarRequest);
            r.size = BarSize;
            r.filledMat = BatteryBarFilledMat;
            r.unfilledMat = BatteryBarUnfilledMat;
            r.margin = 0.05f;
            r.center = DrawPos + Vector3.up * 0.1f;
            
            if (Rot4.North.Equals(base.Rotation)) //Actualy Facing North
            {
                r.center += Vector3.forward * 0.18f;
            }
            else if (Rot4.South.Equals(base.Rotation)) //Actualy Facing South, But forward from the Vector moves the bar north...
            {
                r.center += Vector3.forward * 0.18f;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

		public override void Draw()
        {
            base.Draw();
            CompPowerBattery comp = GetComp<CompPowerBattery>();
            r.fillPercent = comp.StoredEnergy / comp.Props.storedEnergyMax;
            SetRotationAndMoveBar();
            GenDraw.DrawFillableBar(r);
        }

        private void SetRotationAndMoveBar()
        {
            Rot4 rotation = base.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            
        }

        public override void Tick()
		{
			base.Tick();
		}

	}
}
