// AnalystJobDefOf.cs
using Analyst;
using RimWorld;
using Verse;

[DefOf]
public static class AnalystJobDefOf
{
    public static JobDef MechResearch;
    public static JobDef MechOperateScanner;

    static AnalystJobDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(AnalystJobDefOf));
    }
}
