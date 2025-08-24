using RimWorld;
using System.Collections.Generic;
using Verse;

namespace StuffableCore.SCComps
{
    internal class HediffCompStuffable : HediffComp
    {
        public ThingDef stuff;

        public HediffCompProperties_Stuffable Props => (HediffCompProperties_Stuffable)this.props;

        public override string CompLabelPrefix
        {
            get
            {
                string s = "";
                if (stuff != null)
                    s = stuff.label;
                return s;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Defs.Look(ref stuff, "stuff");
        }
    }
}