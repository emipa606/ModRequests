using CombatExtended;
using CombatExtended.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class Projectile_PretendToBeCE : Projectile_Explosive
    {
        private float suppressionAmount;
        protected DangerTracker _dangerTracker = null;
        protected DangerTracker DangerTracker
        {
            get
            {
                return _dangerTracker ?? (_dangerTracker = Map.GetDangerTracker());
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing, blockedByShield);
            var projectilePropsCE = (def.projectile as ProjectilePropertiesCE);
            float dangerFactor = projectilePropsCE.dangerFactor;
            var ignoredThings = new List<Thing>();
            if (def.projectile.explosionRadius > 0f)
            {
                float explosionSuppressionRadius = 3 + (def.projectile.applyDamageToExplosionCellsNeighbors ? 1.5f : 0f);
                //Handle anything explosive
                if (hitThing is Pawn pawn && pawn.Dead)
                {
                    ignoredThings.Add(pawn.Corpse);
                }

                var suppressThings = new List<Pawn>();
                float dangerAmount = 0f;

                // Apply suppression around impact area
                if (Position.y < 3)
                {
                    explosionSuppressionRadius += def.projectile.explosionRadius;

                    if (projectilePropsCE.suppressionFactor > 0f)
                    {
                        suppressThings.AddRange(Position.PawnsInRange(Map, explosionSuppressionRadius));
                    }
                }

                foreach (var thing in suppressThings)
                {
                    ApplySuppression(thing);
                }

                if (projectilePropsCE.dangerFactor > 0f)
                {
                    DangerTracker.Notify_DangerRadiusAt(Position,
                                                        explosionSuppressionRadius - 3,
                                                        dangerAmount * projectilePropsCE.dangerFactor);
                }
            }
            if (dangerFactor > 0f)
            {
                DangerTracker.Notify_DangerRadiusAt(Position,
                        def.projectile.explosionRadius +
                            (def.projectile.applyDamageToExplosionCellsNeighbors ? 1.5f : 0f),
                        def.projectile.GetDamageAmount(1) * dangerFactor);

                GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, this.def.projectile.damageDef,
                        this.launcher?.Faction);
            }
        }
        protected override void Explode()
        {
            this.TryGetComp<CompFragments>()?.Throw(DrawPos, Map, launcher);
            base.Explode();
        }

        protected void ApplySuppression(Pawn pawn, float suppressionMultiplier = 1f)
        {
            var propsCE = def.projectile as ProjectilePropertiesCE;

            if (propsCE.suppressionFactor <= 0f || (!landed && propsCE.airborneSuppressionFactor <= 0f))
            {
                return;
            }

            CompShield shield = pawn.TryGetComp<CompShield>();
            if (pawn.RaceProps.Humanlike)
            {
                // check for shield user

                var wornApparel = pawn.apparel.WornApparel;
                for (var i = 0; i < wornApparel.Count; i++)
                {
                    var personalShield = wornApparel[i].TryGetComp<CompShield>();
                    if (personalShield != null)
                    {
                        shield = personalShield;
                        break;
                    }
                }
            }
            //Add suppression
            var compSuppressable = pawn.TryGetComp<CompSuppressable>();
            if (compSuppressable != null
                    && pawn.Faction != launcher?.Faction
                    && (shield == null || shield.ShieldState == ShieldState.Resetting))
            {
                suppressionAmount = def.projectile.GetDamageAmount(1) * suppressionMultiplier;

                suppressionAmount *= propsCE.suppressionFactor;
                if (!landed)
                {
                    suppressionAmount *= propsCE.airborneSuppressionFactor;
                }

                var explodeRadius = propsCE.explosionRadius;
                if (explodeRadius == 0f)
                {
                    var comp = this.TryGetComp<CompExplosiveCE>()?.props as CompProperties_ExplosiveCE;
                    if (comp != null)
                    {
                        explodeRadius = comp.explosiveRadius;
                        suppressionAmount = comp.damageAmountBase;
                    }
                }

                if (explodeRadius == 0f)
                {
                    var penetrationAmount = propsCE?.armorPenetrationSharp ?? 0f;
                    var armorMod = penetrationAmount <= 0 ? 0 : 1 - Mathf.Clamp(pawn.GetStatValue(CE_StatDefOf.AverageSharpArmor) * 0.5f / penetrationAmount, 0, 1);
                    suppressionAmount *= armorMod;
                }
                else
                {
                    // Larger suppression amount at distances compared to linear interpolation.
                    var dPosX = ExactPosition.x - pawn.DrawPos.x;
                    var dPosZ = ExactPosition.z - pawn.DrawPos.z;
                    // Affected by the ratio of distance from the explosion/projectile to the max suppression radius raised to the power of two.
                    var totalRadius = explodeRadius + 3;
                    var distanceFactor = Mathf.Clamp01(1f - (dPosX * dPosX + dPosZ * dPosZ) / (totalRadius * totalRadius));
                    suppressionAmount *= distanceFactor;
                }
                compSuppressable.AddSuppression(suppressionAmount, origin.ToIntVec3());
            }
        }

    }
}
