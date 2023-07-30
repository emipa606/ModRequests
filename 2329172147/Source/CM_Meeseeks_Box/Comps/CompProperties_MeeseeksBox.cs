using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    public class CompProperties_MeeseeksBox : CompProperties
    {
        [NoTranslate]
        public string useCompSignal = "CM_Meeseeks_Box_Use_Meeseeks_Box";

        public int cooldownTicksBase = 60000;

        public CompProperties_MeeseeksBox()
        {
            compClass = typeof(CompMeeseeksBox);
        }
    }
}
