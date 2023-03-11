using System;
using Verse;
using RimWorld;

namespace Marro
{
    // Token: 0x02000CE2 RID: 3298
    public class CompProperties_MarroHatcher : CompProperties
    {
        // Token: 0x06004FCB RID: 20427 RVA: 0x001AA272 File Offset: 0x001A8472
        public CompProperties_MarroHatcher()
        {
            this.compClass = typeof(CompMarroLarvae);
        }

        // Token: 0x04002C3B RID: 11323
        public float hatcherDaystoHatch = 1f;

        // Token: 0x04002C3C RID: 11324
        public PawnKindDef hatcherPawn;
    }
}
