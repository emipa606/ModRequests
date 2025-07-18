using RimWorld;

namespace Kraltech_Industries;

public class CompProperties_UseEffectIncident : CompProperties_Usable
{
    public bool? discovered;

    public bool sendLetterQuestAvailable = true;

    public CompProperties_UseEffectIncident()
    {
        compClass = typeof(CompUseEffect_GiveIncident);
    }
}