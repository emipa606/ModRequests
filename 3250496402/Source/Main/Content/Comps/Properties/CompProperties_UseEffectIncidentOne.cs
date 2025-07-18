using RimWorld;

namespace Kraltech_Industries;

public class CompProperties_UseEffectIncidentOne : CompProperties_Usable
{
    public bool? discovered;

    public bool sendLetterQuestAvailable = true;

    public CompProperties_UseEffectIncidentOne()
    {
        compClass = typeof(CompUseEffect_GiveIncidentOne);
    }
}