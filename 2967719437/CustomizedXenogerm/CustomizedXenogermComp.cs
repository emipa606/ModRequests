using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CustomizedXenogerm
{
    public class CustomizedXenogermComp : ThingComp
    {
        public Pawn customPawn;

        public override string CompInspectStringExtra()
        {
            if (customPawn != null)
            {
                return "Custom xenogerm for: " + customPawn.LabelShort;
            }
            return null;
        }

        public override void PostExposeData()
        {
            Scribe_References.Look(ref customPawn, "customPawn");
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref customPawn, "customPawn");

        }
    }
}
