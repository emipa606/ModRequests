using RimWorld;

namespace RandomFactions.filters
{
    public class PlayerFactionFilter : FactionFilter
    {
        private bool isPlayer;
        public PlayerFactionFilter(bool isPlayer)
        {
            this.isPlayer = isPlayer;
        }

        public override bool Matches(Faction f)
        {
            return f.IsPlayer == this.isPlayer;
        }
    }
}
