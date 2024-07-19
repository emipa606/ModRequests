using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FinishingTouchesByTheBestBuilder
{
    public class CompFinishingTouchFrame : ThingComp
    {
        bool finishing;
        public bool Finishing
        {
            get => finishing;
            set
            {
                finishing = ((CompProperties_FinishingTouchFrame)this.props).isFinishingTouchTarget && value;
                //Log.Message($"@@@{this.parent.LabelCap}: Finishing={Finishing}");
            }
        }
        public string forciblePawnID;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref finishing, "finishing", false);
            Scribe_Values.Look<string>(ref forciblePawnID, "forciblePawnID", null);
        }
    }
}
