using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Clockwork
{
    class ThingDef_FancyApparel : ThingDef
    {
        public float fanciness = 0;
    }
    public class ThoughtWorker_FancyClothing : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.apparel != null && !ThoughtUtility.ThoughtNullified(p, def))
            {
                float fanciness = 0;
                foreach (var app in p.apparel.WornApparel)
                {
                    var gear_def = app.def as ThingDef_FancyApparel;
                    if (gear_def != null)
                    {
                        fanciness += gear_def.fanciness;
                    }
                }

                if (fanciness >= 1)
                    return ThoughtState.ActiveAtStage(4);
                else if (fanciness >= 0.75)
                    return ThoughtState.ActiveAtStage(3);
                else if (fanciness >= 0.50)
                    return ThoughtState.ActiveAtStage(2);
                else if (fanciness >= 0.25)
                    return ThoughtState.ActiveAtStage(1);
                else if (fanciness > 0)
                    return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
        }
    }
}

