using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Fireflies
{
    public class Comp_Firefly : ThingComp
    {
        private SimpleCurve xCurve;
        private SimpleCurve yCurve;

        private int xTick;
        private int yTick;

        private int xGraphTicks;
        private int yGraphTicks;

        private int ticksUntilNextBlink = 0;
        private int curBlinkDuration = 0;

        public CompProperties_Firefly Props => (CompProperties_Firefly) base.props;

        private Graphic graphicInt;
        private Graphic GlowGraphic => graphicInt ??= Props.glowGraphicData.Graphic;

        private float XGraphPercent => xTick / (float) xGraphTicks;
        private float YGraphPercent => yTick / (float) yGraphTicks;

        public Vector3 FireflyPos => new Vector3(xCurve.Evaluate(XGraphPercent), 0, yCurve.Evaluate(YGraphPercent));

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            //GlowGraphic = Props.glowGraphicData?.Graphic;
            xCurve = new SimpleCurve(Props.xPoints);
            yCurve = new SimpleCurve(Props.yPoints);
            xGraphTicks = Props.xMoveDuration;
            yGraphTicks = Props.yMoveDuration;
            xTick = Rand.Range(0, xGraphTicks);
            yTick = Rand.Range(0, yGraphTicks);
        }

        public override void CompTick()
        {
            base.CompTick();

            if (curBlinkDuration > 0)
            {
                curBlinkDuration--;
            }

            if (curBlinkDuration <= 0)
            {
                if (ticksUntilNextBlink > 0)
                    ticksUntilNextBlink--;
                if (ticksUntilNextBlink <= 0)
                {
                    curBlinkDuration = Rand.Chance(Props.chanceToStayOn)
                        ? Props.blinkDuration.RandomInRange
                        : Props.shortBlinkDuration;
                    ticksUntilNextBlink = Props.blinkIntervalRange.RandomInRange;
                }
            }

            //animation ticks
            if (xTick > xGraphTicks)
            {
                xTick = 0;
            }

            xTick++;

            if (yTick > yGraphTicks)
            {
                yTick = 0;
            }

            yTick++;
        }

        public override void PostDraw()
        {
            if (((Firefly) parent).IsSleeping) return;
            if (GlowGraphic != null && curBlinkDuration > 0)
            {
                var matrix = new Matrix4x4();
                matrix.SetTRS(parent.DrawPos, Quaternion.identity, Vector3.one);
                Graphics.DrawMesh(GlowGraphic.MeshAt(Rot4.North), matrix, GlowGraphic.MatSingle, 0);
            }
        }
    }

    public class CompProperties_Firefly : CompProperties
    {
        public int shortBlinkDuration = 5;
        public IntRange blinkIntervalRange = new IntRange(50, 250);
        public IntRange blinkDuration = new IntRange(250, 2000);
        public float chanceToStayOn = 0.5f;

        public GraphicData glowGraphicData;

        //Movement
        public int xMoveDuration = 100;
        public int yMoveDuration = 250;
        public List<CurvePoint> xPoints;
        public List<CurvePoint> yPoints;

        public CompProperties_Firefly()
        {
            compClass = typeof(Comp_Firefly);
        }
    }
}
