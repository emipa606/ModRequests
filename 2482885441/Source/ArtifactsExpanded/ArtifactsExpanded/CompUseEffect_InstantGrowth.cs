using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ArtifactsExpanded
{
    public class CompUseEffect_InstantGrowth : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            //calls base method first
            base.DoEffect(usedBy);
            //increases age and calls AgeTick to recalculate life stages if necessary
            long ageIncrease = ((long)(0.3f * usedBy.RaceProps.lifeExpectancy)) * 3600000L * (long)ArtifactsExpandedMod.modSettings.growerMechSerumGrowFactor;
            usedBy.ageTracker.DebugMakeOlder(ageIncrease);
            //gives scar to a random body part
            if (Rand.Value < 0.3f)
            {
                BodyPartRecord affectedPart = usedBy.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.SurgicalCut);
                Hediff hediffToAdd = HediffMaker.MakeHediff(HediffDefOf.MechaniteScar, usedBy, affectedPart);
                hediffToAdd.TryGetComp<HediffComp_GetsPermanent>().IsPermanent = true;
                usedBy.health.AddHediff(hediffToAdd, affectedPart, null, null);
            }
        }
    }
}