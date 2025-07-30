using RimWorld;
using System.Linq;
using System.Text;
using Verse;

namespace ImperialFunctionality
{
    public class CompDisplayBank : ThingComp
    {
        public CompFacility compFacility;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compFacility = this.parent.GetComp<CompFacility>();
        }

        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();
            var building = compFacility.LinkedBuildings.FirstOrDefault();
            if (building is null)
            {
                sb.AppendLine("IF.InactiveMissingStation".Translate());
            }
            else
            {
                var stationComp = building.TryGetComp<CompSurveillanceScanner>();
                if (this.parent.GetComp<CompPowerTrader>().PowerOn is false)
                {
                    sb.AppendLine("IF.InactiveNoPower".Translate());
                }
                else if (stationComp.WorkingDisplayBanks.Contains(this.parent) is false)
                {
                    sb.AppendLine("IF.InactiveNoMatchingSurvelliancePillar".Translate());
                }
                else
                {
                    sb.AppendLine("IF.Active".Translate());
                }
            }
            return sb.ToString().TrimEndNewlines();
        }
    }
}
