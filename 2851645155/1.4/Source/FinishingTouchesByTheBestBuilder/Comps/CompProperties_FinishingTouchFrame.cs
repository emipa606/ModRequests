using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FinishingTouchesByTheBestBuilder
{
    public class CompProperties_FinishingTouchFrame : CompProperties
    {
        public bool isFinishingTouchTarget;
        public CompProperties_FinishingTouchFrame()
        {
            this.compClass = typeof(CompFinishingTouchFrame);
        }
    }
}
