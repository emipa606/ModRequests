using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class Building_TrapExplosiveWithFaction : Building_Trap
{
    protected override void SpringSub(Pawn p)
    {
        GetComp<CompExplosive>().StartWick();
    }

    public override void SetFaction(Faction newFaction, Pawn recruiter = null)
    {
        factionInt = newFaction;
    }
}