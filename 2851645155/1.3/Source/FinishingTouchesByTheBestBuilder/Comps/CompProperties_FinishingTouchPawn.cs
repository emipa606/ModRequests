using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FinishingTouchesByTheBestBuilder
{
    public class CompProperties_FinishingTouchPawn : CompProperties
    {
        public bool isFinishingTouchTarget;
        public CompProperties_FinishingTouchPawn()
        {
            this.compClass = typeof(CompFinishingTouchPawn);
        }
    }
}
