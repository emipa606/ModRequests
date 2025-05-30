using Verse;

namespace IndustrialArmory
{
    public class Verb_Shoot_RocketLance : Verb_Shoot
    {
        public override bool Available()
        {
            return base.Available() && this.EquipmentSource.GetComp<CompRocketLance>().rocketsEnabled;
        }
    }
}
