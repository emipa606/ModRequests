﻿using RimWorld;
using Verse;

namespace StagzMerfolk;

public class ThoughtWorker_Charming : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
    {
        return other?.genes != null && other.genes.HasGene(StagzDefOf.Stagz_Charming);
    }
}