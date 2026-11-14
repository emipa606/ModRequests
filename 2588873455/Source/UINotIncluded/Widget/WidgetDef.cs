using System;
using UINotIncluded.Widget.Configs;
using Verse;

namespace UINotIncluded.Widget
{
    public class WidgetDef : Def
    {
        public Type configType;

        public override TaggedString LabelCap
        {
            get
            {
                if (label == null) label = defName;

                return base.LabelCap + " (widget)";
            }
        }

        public ElementConfig GetNewConfig()
        {
            return (ElementConfig)Activator.CreateInstance(configType);
        }
    }
}