using Verse;

namespace UINotIncluded.Widget
{
    public class ExtendedWidgetDef : Def
    {
        public float minWidth = 0f;
        public bool multipleInstances = false;

        public override TaggedString LabelCap
        {
            get
            {
                if (label == null) label = defName;

                return base.LabelCap + " (widget)";
            }
        }
    }
}