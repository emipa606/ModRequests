// AnalystThingDefOf.cs
using Analyst;
using RimWorld;
using Verse;

[DefOf]
public static class AnalystThingDefOf
{
    public static ThingDef Mech_Analyst;

    static AnalystThingDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(AnalystThingDefOf));
    }
}
