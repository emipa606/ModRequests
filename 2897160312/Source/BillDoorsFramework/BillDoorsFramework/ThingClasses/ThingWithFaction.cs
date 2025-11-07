using RimWorld;
using Verse;

namespace BillDoorsFramework
{
    public class ThingWithFaction : ThingWithComps
    {
        public override void SetFaction(Faction newFaction, Pawn recruiter = null)
        {
            this.factionInt = newFaction;
        }
    }
}
