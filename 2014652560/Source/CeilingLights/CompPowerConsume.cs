using RimWorld;

namespace CeilingLights
{
  public class CompPowerConsume : CompPowerPlant
  {
    public CompProperties_PowerConsume Props => (CompProperties_PowerConsume)props;

    protected override float DesiredPowerOutput
    {
      get
      {
        return -CeilingLightsSettings.GrowLightPowerConsumption;
      }
    }
  }
}
