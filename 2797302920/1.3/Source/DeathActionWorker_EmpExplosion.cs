using RimWorld;
using System;
using System.Reflection;
using Verse;
using Verse.AI;

namespace Renamon
{

    public class DeathActionWorker_EmpExplosion : DeathActionWorker
    {
        public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;
        public override bool DangerousInMelee => true;
        public override void PawnDied(Corpse corpse)
        {
            Log.Message("1");
            GenExplosion.DoExplosion(radius: 3, center: corpse.Position, map: corpse.Map, 
                damType: DamageDefOf.EMP, instigator: corpse.InnerPawn);
            var component = ThingMaker.MakeThing(ThingDefOf.ComponentSpacer);
            component.stackCount = Rand.RangeInclusive(1, 4);
            GenSpawn.Spawn(component, corpse.PositionHeld, corpse.Map);
            FleckMaker.ThrowDustPuff(corpse.PositionHeld, corpse.Map, 1f);
            corpse.Destroy();
        }
    }
}
