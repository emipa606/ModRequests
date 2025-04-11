using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace LobotomyCorp.Weapon
{
    
    public class Verb_MagicBullet :  Verb_Shoot
    {
        public override bool IsMeleeAttack => false;

        public VerbProperties_LCMagicBullet Props 
            => verbProps as VerbProperties_LCMagicBullet;

        public bool IsWithIn(IntVec3 root, LocalTargetInfo targ)
        {
            return root.DistanceTo(targ.Cell) < Props.range;
        }
        

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (targ.Thing != null && targ.Thing == caster) return targetParams.canTargetSelf;
            return !ApparelPreventsShooting() && (root.DistanceTo(targ.Cell) < Props.range);
        }
        

        protected override bool TryCastShot()
        {
            if (CurrentTarget.HasThing && currentTarget.Thing.Map != caster.Map) return false;

            ThingDef bullet = Projectile;
            if (bullet == null) return false;
            //if (!verbProps.stopBurstWithoutLos) return false;

            if (base.EquipmentSource != null)
            {
                CompChangeableProjectile ch = base.EquipmentSource.GetComp<CompChangeableProjectile>();
                if (ch != null) ch.Notify_ProjectileLaunched();
                CompReloadable reload = base.EquipmentSource.GetComp<CompReloadable>();
                if (reload != null) reload.UsedOnce();
            }
            lastShotTick = Find.TickManager.TicksGame;

            Thing launcher = this.caster;
            Thing equipment = base.EquipmentSource;
            CompMannable compMannable = this.caster.TryGetComp<CompMannable>();
            if (compMannable != null && compMannable.ManningPawn != null)
            {
                launcher = compMannable.ManningPawn;
                equipment = this.caster;
            }
            Projectile projectile2 = (Projectile)GenSpawn.Spawn(bullet, caster.Position, caster.Map, WipeMode.Vanish);

            projectile2.Launch(launcher, caster.DrawPos, MaxCellIntVec3(Props.range),
                MaxCellIntVec3(Props.range), 
                ProjectileHitFlags.None,
                preventFriendlyFire, equipment);
            return true;
        }

        private IntVec3 MaxCellIntVec3(float range)
        {
            return MaxCell(range).ToIntVec3();
        }

        private Vector3 MaxCell(float range)
        {
            float angle = (float)((currentTarget.Cell - caster.Position).AngleFlat 
                * (Math.PI / 180));

            Vector3 vec = new Vector3()
            {
                x = (float)(caster.Position.x + Math.Sin(angle) * range),
                y = caster.Position.y,
                z = (float)(caster.Position.z + Math.Cos(angle) * range),
            };

            return vec;
        }


        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            if (caster == null){
                Log.Error("Verb " + this.GetUniqueLoadID() + " needs caster to work (possibly lost during saving/loading).");
                return false;
            }

            if (!caster.Spawned) return false;
            if (state == VerbState.Bursting || !CanHitTarget(castTarg)) return false;
            if (CausesTimeSlowdown(castTarg)) Find.TickManager.slower.SignalForceNormalSpeed();

            this.surpriseAttack = surpriseAttack;
            this.canHitNonTargetPawnsNow = canHitNonTargetPawns;
            this.preventFriendlyFire = preventFriendlyFire;
            this.currentTarget = castTarg;
            this.currentDestination = destTarg;

            if (CasterIsPawn && verbProps.warmupTime > 0f)
            {
                ShootLine newShootLine = new ShootLine(caster.Position, castTarg.Cell);
                CasterPawn.Drawer.Notify_WarmingCastAlongLine(newShootLine, caster.Position);

                float statValue = CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor);
                int ticks = (verbProps.warmupTime * statValue).SecondsToTicks();

                CasterPawn.stances.SetStance(new Stance_Warmup(ticks, castTarg, this));
                if (verbProps.stunTargetOnCastStart && castTarg.Pawn != null)
                {
                    castTarg.Pawn.stances.stunner.StunFor(ticks, null);
                }

                MoteMaker(0.5f);
                MoteMaker(1.2f, 0.7f);
            }
            else
            {
                WarmupComplete();
            }
            return true;
        }

        private void MoteMaker(float range, float size = 1f)
        {
            if (Props.circle == null) return;
            Mote circle = (Mote)ThingMaker.MakeThing(Props.circle);
            circle.Scale = size;
            circle.exactRotation = (currentTarget.Cell - caster.Position).AngleFlat;
            circle.exactPosition = caster.DrawPos + MaxCell(range) - caster.Position.ToVector3();
            GenSpawn.Spawn( circle, caster.Position, caster.MapHeld);
        }

        private bool CausesTimeSlowdown(LocalTargetInfo castTarg)
        {
            if (!verbProps.CausesTimeSlowdown) return false;
            if (!castTarg.HasThing) return false;
            
            Thing thing = castTarg.Thing;
            if ( thing.def.category != ThingCategory.Pawn 
                 && (thing.def.building == null || !thing.def.building.IsTurret)) return false;


            return (thing.Faction == Faction.OfPlayer && caster.HostileTo(Faction.OfPlayer)) 
                || (caster.Faction == Faction.OfPlayer && thing.HostileTo(Faction.OfPlayer) 
                && !(thing as Pawn != null && (thing as Pawn).Downed));
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            if (target == null) return;

            float angle = (target.Cell - caster.Position).AngleFlat;
            GenDraw.DrawFieldEdges(
                Props.AffectCells.Select(x =>
                    (x.RotatedBy(angle) + caster.DrawPos).ToIntVec3()
                    ).ToList()
                    ,
                Color.white
            );
            if (target.IsValid) GenDraw.DrawTargetHighlight(target);
        }
    }
    
}
