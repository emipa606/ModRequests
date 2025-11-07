using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class CompRandomPositionPrint : ThingComp
    {
        public CompProperties_RandomPositionPrint Props => props as CompProperties_RandomPositionPrint;
        public override void PostPrintOnto(SectionLayer layer)
        {
            Rand.PushState();
            Vector3 drawPos = Vector3.zero;
            Rand.Seed = parent.DrawPos.GetHashCode();
            drawPos.x += Rand.Range(-100, 101) / 100f * Props.maxDeviation;
            Rand.Seed = parent.thingIDNumber.GetHashCode();
            drawPos.z += Rand.Range(-100, 101) / 100f * Props.maxDeviation;
            var graphic = Props.graphicData;
            graphic.drawOffset = drawPos;
            graphic.Graphic.Print(layer, parent, 0);
            Rand.PopState();
        }
    }

    public class CompProperties_RandomPositionPrint : CompProperties
    {
        public CompProperties_RandomPositionPrint()
        {
            compClass = typeof(CompRandomPositionPrint);
        }
        public GraphicData graphicData;
        public float maxDeviation;
    }
}
