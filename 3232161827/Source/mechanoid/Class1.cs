using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace mechanoid
{
    public class CompProperties_CauseHediff_AoE_WithFactionPikeman : CompProperties
    {
        public HediffDef hediff;

        public BodyPartDef part;

        public float range;

        public bool onlyTargetMechs;

        public int checkInterval = 10;

        public SoundDef activeSound;

        public bool affectMyFaction = true;

        public CompProperties_CauseHediff_AoE_WithFactionPikeman()
        {
            compClass = typeof(CompCauseHediff_AoE_WithFactionPikeman);
        }
    }
}