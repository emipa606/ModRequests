﻿using Verse;
using RimWorld;

namespace RimArchive;

#pragma warning disable CS1591
[DefOf]
[StaticConstructorOnStartup]
public static class HediffDefOf
{
    public static HediffDef BA_ArmorReduction;
    public static HediffDef BA_PillarSuppression;
    static HediffDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
    }
}
