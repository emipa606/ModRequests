using RimWorld;

namespace RandomFactions.filters
{
    public class PlayerFactionDefFilter : FactionDefFilter
    {
        private bool isPlayer;
        public PlayerFactionDefFilter(bool isPlayer)
        {
            this.isPlayer = isPlayer;
        }

        public override bool Matches(FactionDef f)
        {
            return f.isPlayer == this.isPlayer;
        }
    }
}
