using Verse;

namespace ArchotechWeaponry.Comps
{
    public class HediffCompProperties_FilthOnDeath : HediffCompProperties
    {
        public ThingDef filthToSpawn;

        public HediffCompProperties_FilthOnDeath()
        {
            this.compClass = typeof(HediffComp_FilthOnDeath);
        }
    }

    public class HediffComp_FilthOnDeath : HediffComp
    {
        private HediffCompProperties_FilthOnDeath Props => (HediffCompProperties_FilthOnDeath)this.props;

        public override void Notify_PawnDied()
        {
            if (this.parent.Severity >= 0.99) //Handles case where severity reducing tick happens at same time as death
            {
                GenSpawn.Spawn(Props.filthToSpawn, this.Pawn.Corpse.Position, this.Pawn.Corpse.Map);
                this.Pawn.Corpse.Destroy(DestroyMode.Vanish);
            }
        }
    }
}