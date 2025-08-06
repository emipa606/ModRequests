using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MaliExtinguishRefuelables
{
	[StaticConstructorOnStartup]
	public class CompDarklightOverlayExtinguishable : CompFireOverlayExtinguishableBase
	{
		protected CompRefuelable refuelableComp;
		protected CompFlickable flickableComp;

        public static readonly Graphic DarklightGraphic = GraphicDatabase.Get<Graphic_FlickerExtinguishable>("Things/Special/Darklight", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);
        public new CompProperties_DarklightOverlayExtinguishable Props => (CompProperties_DarklightOverlayExtinguishable)props;

		public override void PostDraw()
		{
			base.PostDraw();
			if (refuelableComp == null || refuelableComp.HasFuel)
			{
				if (flickableComp == null || flickableComp.SwitchIsOn)
                {
                    Vector3 drawPos = parent.DrawPos;
                    drawPos.y += 1f / 26f;
                    DarklightGraphic.Draw(drawPos, Rot4.North, parent);
                }
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			refuelableComp = parent.GetComp<CompRefuelable>();
			flickableComp = parent.GetComp<CompFlickable>();
			if (!flickableComp.SwitchIsOn && flickableComp != null)
            {
				refuelableComp.Props.fuelConsumptionPerTickInRain = 0f;
			}
		}

		public override void CompTick()
		{
			if ((refuelableComp == null || refuelableComp.HasFuel) && startedGrowingAtTick < 0)
			{
				startedGrowingAtTick = GenTicks.TicksAbs;
			}
		}
	}
}