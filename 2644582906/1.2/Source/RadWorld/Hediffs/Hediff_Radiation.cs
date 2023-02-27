using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RadWorld
{
    public class Hediff_Radiation : HediffWithComps // was containing methods, moved to HediffCompRadiation instead, left so saves won't be broken
    {

    }
    public class HediffCompProperties_RadiationMutation : HediffCompProperties
    {
        public HediffCompProperties_RadiationMutation()
        {
            this.compClass = typeof(HediffCompRadiationMutation);
        }
    }

    public class HediffCompRadiationMutation : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Find.TickManager.TicksGame % 30000 == 0)
            {
                var chance = MutationChances.Evaluate(this.parent.Severity);
                if (Rand.Chance(chance))
                {
                    var hediffCandidates = RW_Utils.GetFreeHediffCandidatesFor(Pawn, RW_Utils.hediffsPerBodyParts);
                    if (hediffCandidates.Any())
                    {
                        var randomHediff = hediffCandidates.RandomElement();
                        var part = randomHediff.Value;
                        if (randomHediff.Key == RW_DefOf.RW_RadiationBurnScar)
                        {
                            part = Pawn.RaceProps.body.AllParts.Where(x => x.depth == BodyPartDepth.Outside && x.coverageAbs > 0).RandomElement();
                        }
                        var hediff = HediffMaker.MakeHediff(randomHediff.Key, Pawn, part);
                        Pawn.health.AddHediff(hediff, part);
                        Find.LetterStack.ReceiveLetter("RW.NewMutation".Translate(randomHediff.Key.LabelCap, Pawn.Named("PAWN")), "RW.NewMutationDesc".Translate(randomHediff.Key.LabelCap, Pawn.Named("PAWN")),
                            LetterDefOf.NeutralEvent, Pawn);
                    }
                }
            }
        }

        private static readonly SimpleCurve MutationChances = new SimpleCurve
        {
            new CurvePoint(0.6f, 0f),
            new CurvePoint(0.7f, 0.1f),
            new CurvePoint(0.8f, 0.2f),
            new CurvePoint(0.9f, 0.35f),
        };
    }
}
