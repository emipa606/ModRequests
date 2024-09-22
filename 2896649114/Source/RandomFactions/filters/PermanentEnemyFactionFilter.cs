using RimWorld;

namespace RandomFactions.filters
{
    public class PermanentEnemyFactionFilter : FactionFilter
    {
        private bool permanentEnemy;
        public PermanentEnemyFactionFilter(bool isPermanentEnemy)
        {
            this.permanentEnemy = isPermanentEnemy;
        }

        public override bool Matches(Faction f)
        {
            return f.def.permanentEnemy == this.permanentEnemy;
        }
    }
}
