using RimWorld;

namespace RandomFactions.filters
{
    public class NaturalEnemyFactionFilter : FactionFilter
    {
        private bool naturalEnemy;
        public NaturalEnemyFactionFilter(bool isNaturalEnemy)
        {
            this.naturalEnemy = isNaturalEnemy;
        }

        public override bool Matches(Faction f)
        {
            return f.def.naturalEnemy == this.naturalEnemy;
        }
    }
}
