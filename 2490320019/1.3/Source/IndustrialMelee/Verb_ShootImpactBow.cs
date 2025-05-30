using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace IndustrialMelee
{
    public class Verb_ShootImpactBow : Verb_Shoot
    {
        public override ThingDef Projectile
        {
            get
            {
                var pawn = this.CasterPawn;
                if (!Pawn_GetGizmos_Patch.cachedComps.TryGetValue(pawn, out var comp))
                {
                    comp = pawn.TryGetComp<CompAttackCooldown>();
                    Pawn_GetGizmos_Patch.cachedComps[pawn] = comp;
                }
                if (comp.AttackIsEnabled(AttackType.ExplosiveArrows))
                {
                    return IM_DefOf.IM_ArrowExplosive;
                }
                return verbProps.defaultProjectile;
            }
        }
    }
}
