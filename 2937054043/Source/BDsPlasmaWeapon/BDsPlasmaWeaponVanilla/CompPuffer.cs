using Verse;
using RimWorld;

namespace BDsPlasmaWeaponVanilla
{
    public class CompPuffer : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            FleckMaker.ThrowSmoke(parent.DrawPos, parent.Map, parent.stackCount);
            parent.DeSpawn();
        }
    }
}
