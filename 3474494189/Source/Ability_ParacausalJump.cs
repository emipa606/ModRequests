using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace AddieSolarHunter
{
    public class Ability_ParacausalJump : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            var map = Caster.Map;
            var flyer = (JumpingPawn)PawnFlyer.MakeFlyer(VPESH_DefOf.VPESH_JumpingPawn, CasterPawn, targets[0].Cell, null, null);
            flyer.ability = this;
            GenSpawn.Spawn(flyer, Caster.Position, map);
            base.Cast(targets);
        }
    }
}