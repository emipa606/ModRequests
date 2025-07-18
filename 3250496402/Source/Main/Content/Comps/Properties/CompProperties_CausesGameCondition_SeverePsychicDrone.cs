using RimWorld;

namespace Kraltech_Industries;

public class CompProperties_CausesGameCondition_SeverePsychicDrone : CompProperties_CausesGameCondition
{
    public PsychicDroneLevel droneLevel = PsychicDroneLevel.BadMedium;

    public int droneLevelIncreaseInterval = int.MinValue;

    public CompProperties_CausesGameCondition_SeverePsychicDrone()
    {
        compClass = typeof(CompCauseGameCondition_SeverePsychicDrone);
    }
}