using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    public class CompShieldSpawner : ThingComp
    {
        public CompProperties_ShieldSpawner Props
        {
            get
            {
                return (CompProperties_ShieldSpawner)this.props;
            }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compPower = parent.GetComp<CompPowerTrader>();
        }

        public virtual void addCEprojectile(ThingWithComps thing, List<ThingWithComps> projectiles)
        { }

        public virtual float getDamageCE(ThingWithComps thing)
        {
            return -1;
        }
        public override void CompTick()
        {
            base.CompTick();
            if (compPower.PowerOn)
            {
                if (restartTiming > 0)
                {
                    restartTiming--;
                }
                else
                {
                    List<ThingWithComps> projectiles = new List<ThingWithComps>();
                    foreach (ThingWithComps projectile in ProjectilesStore.projectiles.Where(projectile => projectile.Spawned && !projectile.Destroyed))
                    {
                        if (projectile is Projectile a && IsUnfriendly(a, parent) && CollisionDetermination(a.ExactPosition, parent.TrueCenter(), Props.radius, Props.invalidRadius))
                        {
                            projectiles.Add(a);
                        }
                        addCEprojectile(projectile, projectiles);
                    }
                    while (projectiles.Count > 0)
                    {
                        float amount = 0;
                        if (projectiles[0] is Projectile projectile)
                        {
                            amount = projectile.DamageAmount;
                        }
                        float CEdamage = getDamageCE(projectiles[0]);
                        if (CEdamage > 0)
                        {
                            amount = CEdamage;
                        }
                        foreach (DamageMultiplier multiplier in Props.damageMultipliers)
                        {
                            if (multiplier.damageDef == projectiles[0].def.projectile.damageDef)
                            {
                                amount *= multiplier.multiplier;
                                ImpectEffect(projectiles[0].TrueCenter(), amount);
                                projectiles[0].Destroy();
                                projectiles.Remove(projectiles[0]);
                            }
                        }
                    }
                }
            }
        }
        public virtual void ImpectEffect(Vector3 drawPos, float amout)
        {
            RimWorld.SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(drawPos.ToIntVec3(), parent.Map, false));
            float num = Math.Max(Rand.Range(amout / 40f, 1f + amout / 30f), 1.5f);
            FleckMaker.Static(drawPos, parent.Map, RimWorld.FleckDefOf.ExplosionFlash, num);
            if (amout >= Props.damageLimit)
            {
                restartTiming = Props.restartTiming;
                RimWorld.EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, 12);
            }
        }
        public override void PostDraw()
        {
            base.PostDraw();
            if (compPower.PowerOn && restartTiming <= 0)
            {
                Matrix4x4 matrix = default;
                Vector3 vector3 = parent.DrawPos;
                vector3.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                matrix.SetTRS(vector3, Quaternion.identity, new Vector3(Props.radius * 2f * 1.16015625f, 1f, Props.radius * 2f * 1.16015625f));
                Material m = ForceFieldMat;
                m.color = new Color(0.17f, 1f, 1f, 0.4f);
                Graphics.DrawMesh(MeshPool.plane10, matrix, m, 0, null, 0);
            }
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Format(Props.textDamageLimit, Props.damageLimit.ToString("0.##")));
            stringBuilder.Append(string.Format(Props.textRestartTiming, (Props.restartTiming / 60f).ToString("0.##")));
            if (restartTiming > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append(string.Format(Props.textRestartTime, (restartTiming / 60f).ToString("0.##")));
            }
            return stringBuilder.ToString();
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref restartTiming, "BDsGlitterworldFaction3HST_restartTiming", 0);
        }
        public CompPowerTrader compPower;
        private static readonly Material ForceFieldMat = MaterialPool.MatFrom("Other/ForceField", ShaderDatabase.MoteGlow);
        public int restartTiming = 0;
        public static bool IsUnfriendly(Projectile a, Thing b)
        {
            return a.Launcher == null || b == null || a.Launcher.Faction == null || b.Faction == null || a.Launcher.Faction.HostileTo(b.Faction);
        }
        public static bool CollisionDetermination(Vector3 a, Vector3 b, float range)
        {
            return ((a.x - b.x) * (a.x - b.x)) + ((a.z - b.z) * (a.z - b.z)) <= range * range;
        }
        public static bool CollisionDetermination(Vector3 a, Vector3 b, float range, float rangeLimit)
        {
            return CollisionDetermination(a, b, range) && !CollisionDetermination(a, b, rangeLimit);
        }
    }

    public static class ProjectilesStore
    {
        public static List<ThingWithComps> projectiles = new List<ThingWithComps>();
        public static bool IsProjectile(ThingWithComps thing)
        {
            if (thing is Projectile) return true;
            return false;
        }
    }
    public class CompProperties_ShieldSpawner : CompProperties
    {
        public CompProperties_ShieldSpawner()
        {
            this.compClass = typeof(CompShieldSpawner);
        }
        public float radius;
        public float invalidRadius;
        public float damageLimit;
        public int restartTiming;
        public List<DamageMultiplier> damageMultipliers;
        [MustTranslate]
        public string textDamageLimit = "伤害阈值{0}";
        [MustTranslate]
        public string textRestartTiming = "破损后重启时间:{0}秒";
        [MustTranslate]
        public string textRestartTime = "{0}秒后重启";
    }
}
