using RimWorld;
using Verse;

namespace HLVRMonsters
{
    public class Projectile_HoundeyeBlast : Bullet
    {
        public ThingDef_HoundeyeBlast Def
        {
            get
            {
                return this.def as ThingDef_HoundeyeBlast;
            }
        }

        public override void Tick()
        {
            GenExplosion.DoExplosion(this.Position, this.Map, 3.3f, HLVRDefOf.HLVR_SonicBlast, this, -1, -1f, null, null, null, null, null, 0f, 1, GasType.Unused, false, null, 0f, 1, 0f, false, null, null);
            this.Destroy();
        }

        
        
    }

    public class ThingDef_HoundeyeBlast : ThingDef
    {

    }

}