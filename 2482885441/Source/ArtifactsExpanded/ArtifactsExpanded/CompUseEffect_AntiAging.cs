using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace ArtifactsExpanded
{
    public class CompUseEffect_AntiAging : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            //calls base method first
            base.DoEffect(usedBy);
            //decreases age and calls AgeTick to recalculate life stages if necessary
            long ageDecrease = ((long)(0.3f * usedBy.RaceProps.lifeExpectancy)) * 3600000L * (long)ArtifactsExpandedMod.modSettings.revitalizerMechSerumFactor;
            ageDecrease = (long)Mathf.Min(ageDecrease - 1L, usedBy.ageTracker.AgeBiologicalTicks);
            usedBy.ageTracker.AgeBiologicalTicks -= ageDecrease;
            usedBy.ageTracker.BirthAbsTicks -= ageDecrease;
            usedBy.ageTracker.AgeTick();
        }
    }
}
