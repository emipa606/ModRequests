using Verse;
using RimWorld;
using RimWorld.Planet;
using System.Linq;
using System.Collections.Generic;
using Ability = VFECore.Abilities.Ability;
using System;
using Verse.Sound;

namespace AddieSolarHunter
{
    public class Ability_DeadshotGoldenGun : Ability
    {
        private int ticksUntilNextShot = 0;
        private int shotsRemaining = 0;
        private List<Pawn> enemyPawns = null;
        private ThingDef projectileDef = null;

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (targets.NullOrEmpty())
                return;

            Pawn caster = this.pawn;
            Map map = caster.Map;
            if (map == null)
                return;

            GlobalTargetInfo target = targets[0];
            IntVec3 targetCell = target.Cell;

            float radius = this?.def.GetModExtension<VPESH_RadiusExtension>()?.targetRadius ?? 5.9f;
            enemyPawns = GenRadial
                .RadialDistinctThingsAround(targetCell, map, radius, true)
                .OfType<Pawn>()
                .Where(p =>
                    p.Spawned &&
                    ((p.Faction != null && p.Faction.HostileTo(caster.Faction)) ||
                     (p.MentalState != null && (p.MentalStateDef == MentalStateDefOf.Manhunter || p.MentalStateDef == MentalStateDefOf.Berserk))
                    )
                ).ToList();

            if (enemyPawns.NullOrEmpty())
                return;

            float psySensitivity = caster.GetStatValue(StatDefOf.PsychicSensitivity);
            shotsRemaining = Math.Max(10, (int)(12 * psySensitivity));

            projectileDef = VPESH_DefOf.Addie_GoldenGunBullet_Deadshot;

            ticksUntilNextShot = 0;
        }

        public override void Tick()
        {
            base.Tick();

            if (shotsRemaining <= 0 || enemyPawns.NullOrEmpty() || pawn.DestroyedOrNull() || pawn.Map == null)
                return;

            if (ticksUntilNextShot > 0)
            {
                ticksUntilNextShot--;
                return;
            }

            Pawn caster = this.pawn;
            Map map = caster.Map;
            if (map == null)
                return;

            Pawn targetPawn = enemyPawns.RandomElement();
            if (targetPawn != null && targetPawn.Spawned)
            {
                if (projectileDef != null)
                {
                    Projectile projectile = (Projectile)GenSpawn.Spawn(projectileDef, caster.Position, map);
                    projectile.Launch(caster, targetPawn, targetPawn, ProjectileHitFlags.IntendedTarget, false, null);
                    VPESH_DefOf.Addie_GoldenGunSoundDeadshot.PlayOneShot(new TargetInfo(caster.Position, map));
                }
            }

            shotsRemaining--;
            ticksUntilNextShot = (int)(0.25f * 60f); //0.45
        }
    }
}
