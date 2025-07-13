using RimWorld;
using Verse;
using UnityEngine;

namespace Fireplace
{
    [StaticConstructorOnStartup]
    public class CompDarklightOverlay : CompFireOverlayBase
    {
        public new CompProperties_DarklightOverlay Props
        {
            get
            {
                return (CompProperties_DarklightOverlay)props;
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (refuelableComp != null && !refuelableComp.HasFuel)
            {
                return;
            }
            Vector3 drawPos = parent.DrawPos;
            drawPos.y += 0.03846154f;
            DarklightGraphic.Draw(drawPos, parent.Rotation, parent, 0f);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            refuelableComp = parent.GetComp<CompRefuelable>();
        }

        public override void CompTick()
        {
            if (refuelableComp != null && !refuelableComp.HasFuel)
            {
                return;
            }
            if (startedGrowingAtTick < 0)
            {
                startedGrowingAtTick = GenTicks.TicksAbs;
            }
        }

        protected CompRefuelable refuelableComp;

        public static readonly Graphic DarklightGraphic = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Darklight", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);
    }
}
