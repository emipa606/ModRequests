using RimWorld;
using Verse;
using HLVR;

namespace HLVR
{
    public class Projectile_ThrownSnark : Bullet
    {
        public ThingDef_ThrownSnark Def
        {
            get
            {
                return this.def as ThingDef_ThrownSnark;
            }
        }


        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            IntVec3 strikeLocation = base.Position;

            Pawn pawn = PawnGenerator.GeneratePawn(HLVRDefOf.Monster_Snark, null);
            GenSpawn.Spawn(pawn, strikeLocation, this.Map, this.Rotation, WipeMode.Vanish, false);
            this.Destroy();
        }
    }

    public class ThingDef_ThrownSnark : ThingDef
    {

    }

    

}