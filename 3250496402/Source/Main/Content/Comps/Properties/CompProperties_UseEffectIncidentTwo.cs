using RimWorld;

namespace Kraltech_Industries;

public class CompProperties_UseEffectIncidentTwo : CompProperties_Usable
{
    public bool? discovered;

    public bool sendLetterQuestAvailable = true;

    public CompProperties_UseEffectIncidentTwo()
    {
        compClass = typeof(CompUseEffect_GiveIncidentTwo);
    }
}