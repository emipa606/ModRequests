using RimWorld;

namespace RandomFactions.filters
{
    public class AllowedTemperatureFactionFilter : FactionFilter
    {
        private float temperature;
        public AllowedTemperatureFactionFilter(float temperature)
        {
            this.temperature = temperature;
        }

        public override bool Matches(Faction f)
        {
            return f.def.allowedArrivalTemperatureRange.Includes(temperature);
        }
    }
}
