using RimWorld;

namespace Kraltech_Industries;

[DefOf]
internal class DutyDefOf : DefOf
{
    static DutyDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(DutyDefOf));
    }
}