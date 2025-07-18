using RimWorld;

namespace Kraltech_Industries;

[DefOf]
public static class IncidentDefOf
{
    public static IncidentDef MechanitorCargoShipCrash;

    public static IncidentDef MechanitorCargoShipCrashOne;

    public static IncidentDef MechanitorCargoShipCrashTwo;

    public static IncidentDef RaidEnemy;

    static IncidentDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
    }
}