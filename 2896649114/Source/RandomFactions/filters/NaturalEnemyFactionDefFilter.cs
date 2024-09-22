using RimWorld;

namespace RandomFactions.filters
{
    public class NaturalEnemyFactionDefFilter : FactionDefFilter
    {
        private bool naturalEnemy;
        public NaturalEnemyFactionDefFilter(bool isNaturalEnemy)
        {
            this.naturalEnemy = isNaturalEnemy;
        }

        public override bool Matches(FactionDef f)
        {
            return f.naturalEnemy == this.naturalEnemy;
        }
    }
}
