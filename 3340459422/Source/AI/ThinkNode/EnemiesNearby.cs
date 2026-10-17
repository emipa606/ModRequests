// Modified by llunak, l.lunak@centrum.cz .

using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace SR.ModRimWorld.RaidExtension
{

public class ThinkNode_EnemiesNearby : ThinkNode_Conditional
{
    protected override bool Satisfied(Pawn pawn)
    {
        // This is the same condition that Job.expireRequiresEnemiesNearby uses.
        return PawnUtility.EnemiesAreNearby(pawn, 25);
    }
}

}
