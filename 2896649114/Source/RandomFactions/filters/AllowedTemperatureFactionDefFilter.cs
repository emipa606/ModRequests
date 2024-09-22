using RimWorld;

namespace RandomFactions.filters
{
    public class AllowedTemperatureFactionDefFilter : FactionDefFilter
    {
        private float temperature;
        public AllowedTemperatureFactionDefFilter(float temperature)
        {
            this.temperature = temperature;
        }

        public override bool Matches(FactionDef f)
        {
            return f.allowedArrivalTemperatureRange.Includes(temperature);
        }
    }
}
