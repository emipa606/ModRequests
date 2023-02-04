using Verse;

namespace CeilingLights
{
  public class CompProperties_PowerConsume : CompProperties
  {
    // Add your XML changeable variables, such as
    public int basePowerConsumption = 2000;

    public CompProperties_PowerConsume()
    {
      compClass = typeof(CompPowerConsume);
    }
  }
}
