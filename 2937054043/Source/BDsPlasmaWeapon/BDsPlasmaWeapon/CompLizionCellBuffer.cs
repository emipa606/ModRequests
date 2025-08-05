using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace BDsPlasmaWeapon
{
    public class CompLizionCellBuffer : CompEggContainer
    {
        private int forcedUnloadTick;

        public new bool CanEmpty
        {
            get
            {
                if (base.Empty)
                {
                    return false;
                }
                if (forcedUnloadTick < Find.TickManager.TicksGame)
                {
                    return true;
                }
                return base.ContainedThing.stackCount >= base.Props.minCountToEmpty;
            }
        }

        public override bool Accepts(ThingDef thingDef)
        {
            return true;
        }

        public void NotifyOperational()
        {
            forcedUnloadTick = Find.TickManager.TicksGame + 3000;
        }

        public override string CompInspectStringExtra()
        {
            string text = "";
            if (!base.Empty)
            {
                foreach (Thing thing in innerContainer)
                {
                    text += "\n" + thing.LabelCap;
                }
            }
            return "Contents".Translate() + ": " + (Empty ? ((string)"Nothing".Translate()) : text);
        }
    }
}
