using RimWorld;

namespace RandomFactions.filters
{
    public class PermanentEnemyFactionDefFilter : FactionDefFilter
    {
        private bool permanentEnemy;
        public PermanentEnemyFactionDefFilter(bool isPermanentEnemy)
        {
            this.permanentEnemy = isPermanentEnemy;
        }

        public override bool Matches(FactionDef f)
        {
            return f.permanentEnemy == this.permanentEnemy;
        }
    }
}
