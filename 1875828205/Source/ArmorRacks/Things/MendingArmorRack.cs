using RimWorld;
using Verse;

namespace ArmorRacks.Things
{
    public class MendingArmorRack : MechanizedArmorRack
    {
        public static int RareTicksPerMend = 40;
        public int TickCounter = 0;

        public override void TickRare()
        {
            var power = GetComp<CompPowerTrader>();
            if (power.PowerOn)
            {
                TickCounter++;
            }
            if (TickCounter == RareTicksPerMend)
            {
                MendContents();
                TickCounter = 0;
            }
            base.TickRare();
        }

        public void MendContents()
        {
            foreach (Thing thing in InnerContainer.InnerListForReading)
            {
                if (thing.HitPoints < thing.MaxHitPoints)
                {
                    thing.HitPoints++;
                }
            }
        }
    }
}