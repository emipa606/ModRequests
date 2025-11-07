using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.Sound;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    public class CompVehicleECSShield : VehicleComp, IVehicleDamageAbsorber
    {
        public CompProperties_VehicleECSShield Props => props as CompProperties_VehicleECSShield;

        CompVehicleCapacitor capacitor => Vehicle.TryGetComp<CompVehicleCapacitor>();

        List<VehicleComponent> components = new List<VehicleComponent>();
        private Material BubbleMat => MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent, Props.color);

        public int regenerateTicks = 9999;

        public bool switchedOn = true;

        public bool broken = true;

        public float rechargePct => Mathf.Clamp(1 - ((float)regenerateTicks / Props.regenerateTicks), 0, 1);

        public float stressCapacity => Mathf.Lerp(0, Props.stressCapacity, Efficiency);

        public float stress;

        public float stressPct => stress / stressCapacity;

        public float threshold => Props.thresholdRange.LerpThroughRange(1 - stressPct);

        private Vector3 impactAngleVect;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref regenerateTicks, "rechargeTick");
            Scribe_Values.Look(ref switchedOn, "switchedOn");
            Scribe_Values.Look(ref stress, "charges");
            Scribe_Values.Look(ref broken, "broken");
            Scribe_Values.Look(ref impactAngleVect, "impactAngleVect");
        }

        public float Efficiency
        {
            get
            {
                if (components.NullOrEmpty())
                {
                    return 1;
                }
                float hitpoint = 0;
                float hitpointtotal = 0;
                foreach (VehicleComponent component in components)
                {
                    hitpointtotal += component.props.health;
                    hitpoint += component.health;
                }
                return hitpoint / hitpointtotal;
            }
        }

        protected bool ShouldBeOn
        {
            get
            {
                if (!Vehicle.Spawned || !switchedOn || Efficiency <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        protected bool ShouldDisplay
        {
            get
            {
                if (!ShouldBeOn || broken)
                {
                    return false;
                }
                return true;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            foreach (VehicleComponent component in Vehicle.statHandler.components)
            {
                if (Props.shieldComponentKeys.Contains(component.props.key))
                {
                    components.Add(component);
                }
            }
            if (regenerateTicks > Props.regenerateTicks) regenerateTicks = Props.regenerateTicks;
        }

        public bool AbsorbDamageExplosive(ref DamageInfo dinfo)
        {
            if (dinfo.Def == DamageDefOf.EMP && dinfo.Amount > Props.EMPResistance)
            {
                Break();
                return false;
            }
            if (!ShouldDisplay || !dinfo.Def.harmsHealth)
            {
                return false;
            }
            stress += dinfo.Amount / Props.explosiveProtectionMultiplier;
            if (dinfo.Amount <= threshold * Props.explosiveProtectionMultiplier)
            {
                AbsorbedDamage(ref dinfo);
                return true;
            }
            Break();
            return false;
        }

        public bool AbsorbDamage(ref DamageInfo dinfo)
        {
            if (dinfo.Def.isExplosive)
            {
                return AbsorbDamageExplosive(ref dinfo);
            }
            if (dinfo.Def == DamageDefOf.EMP && dinfo.Amount > Props.EMPResistance)
            {
                Break();
                return false;
            }
            if (!ShouldDisplay || !dinfo.Def.harmsHealth)
            {
                return false;
            }
            stress += dinfo.Amount;
            if (dinfo.Amount <= threshold)
            {
                AbsorbedDamage(ref dinfo);
                return true;
            }
            Break();
            return false;
        }

        public void AbsorbedDamage(ref DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(Vehicle.Position, Vehicle.Map));
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = Vehicle.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f * Props.drawsize;
            float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            FleckMaker.Static(loc, Vehicle.Map, FleckDefOf.ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                FleckMaker.ThrowDustPuff(loc, Vehicle.Map, Rand.Range(0.8f, 1.2f));
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (ShouldDisplay)
            {
                float num = Mathf.Lerp(Props.drawsize, Props.mindrawsize, stressPct);
                Vector3 drawPos = Vehicle.Drawer.DrawPos;
                drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                float angle = Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
            }
        }

        public override void CompTick()
        {
            if (stress < 0) stress = 0;
            if (stress > stressCapacity) stress = stressCapacity;
            if (ShouldBeOn)
            {
                if (!capacitor.TryDrain(Props.maintainPower))
                {
                    regenerateTicks = Props.regenerateTicks;
                    broken = true;
                }
                else
                {
                    if (stress > 0)
                    {
                        stress -= Props.stressReliefRate;
                        if (stress < 0) stress = 0;
                    }
                    if (broken)
                    {
                        if (regenerateTicks == 0)
                        {
                            broken = false;
                        }
                        if (capacitor.TryDrain(Props.regeneratePower / Props.regenerateTicks))
                        {
                            regenerateTicks--;
                        }
                    }
                }
            }
            else
            {
                broken = true;
                regenerateTicks = Props.regenerateTicks;
            }
        }

        public void Break()
        {
            float scale = Mathf.Lerp(Props.drawsize, Props.mindrawsize, stressPct);
            RimWorld.EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale / 2);
            FleckMaker.Static(parent.TrueCenter(), parent.Map, FleckDefOf.ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                FleckMaker.ThrowDustPuff(parent.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), parent.Map, Rand.Range(0.8f, 1.2f));
            }
            regenerateTicks = Props.regenerateTicks;
            broken = true;
        }

        public string DescExtraString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("ECSShieldDesc".Translate());
            stringBuilder.AppendLine("ECSShieldDamageThreshold".Translate(Props.thresholdRange.max, Props.thresholdRange.min));
            stringBuilder.AppendLine("ECSShieldStressCapacity".Translate(stressCapacity, Props.stressCapacity, Efficiency.ToStringPercent()));
            stringBuilder.AppendLine("ECSShieldStressRelief".Translate(Props.stressReliefRate * 60));
            stringBuilder.AppendLine("ECSShieldRechargeTime".Translate(Props.regenerateTicks.TicksToSeconds()));
            stringBuilder.AppendLine("ECSShieldRechargePower".Translate(Props.regeneratePower));
            if (Props.maintainPower > 0f)
            {
                stringBuilder.AppendLine("ECSShieldMaintainPower".Translate(Props.maintainPower * 60));
            }
            if (Props.EMPResistance > 0f)
            {
                stringBuilder.AppendLine("BDFV_EMPresist".Translate(Props.EMPResistance));
            }
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Gizmo_VehicleECSShield gizmo = new Gizmo_VehicleECSShield();
            gizmo.shield = this;
            yield return gizmo;
        }
    }

    public class CompProperties_VehicleECSShield : CompProperties_VehicleShield
    {
        public CompProperties_VehicleECSShield()
        {
            this.compClass = typeof(CompVehicleECSShield);
        }

        public FloatRange thresholdRange = new FloatRange(10, 30);

        public float stressCapacity = 200;

        public int stressReliefRate = 1;

        public int regenerateTicks = 600;

        public float explosiveProtectionMultiplier = 10;

        public float regeneratePower = 1;
    }

    [StaticConstructorOnStartup]
    public class Gizmo_VehicleECSShield : Gizmo
    {
        public CompVehicleECSShield shield;

        private static readonly Texture2D ShieldOffTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.3f, 0f, 0f));

        private static readonly Texture2D ShieldProgressBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_VehicleECSShield()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            if (Widgets.ButtonInvisible(rect2))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                shield.switchedOn = !shield.switchedOn;
            }
            Rect rect3 = rect2;
            rect3.height = rect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect3, "BDFV_ECSShield".Translate().Resolve());
            Rect rect4 = rect2;
            rect4.yMin = rect2.y + rect2.height / 2f;

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            if (shield.Efficiency <= 0)
            {
                float fillPercent = 1;
                Widgets.FillableBar(rect4, fillPercent, ShieldOffTex, EmptyShieldBarTex, doBorder: false);
                Widgets.Label(rect4, "ECSShieldDamaged".Translate());
            }
            else if (!shield.switchedOn)
            {
                float fillPercent = 1;
                Widgets.FillableBar(rect4, fillPercent, ShieldOffTex, EmptyShieldBarTex, doBorder: false);
                Widgets.Label(rect4, "ECSShieldOff".Translate());
            }
            else
            {
                if (shield.broken)
                {
                    float fillPercent = shield.rechargePct;
                    Widgets.FillableBar(rect4, fillPercent, ShieldOffTex, EmptyShieldBarTex, doBorder: false);
                    Widgets.Label(rect4, "BDFV_ECSShieldDelay".Translate(shield.regenerateTicks.TicksToSeconds().ToString("F1")).Resolve());
                }
                else
                {
                    float fillPercent = shield.stressPct;
                    Widgets.FillableBar(rect4, fillPercent, ShieldProgressBarTex, EmptyShieldBarTex, doBorder: false);
                    Widgets.Label(rect4, shield.threshold.ToString() + "ECSShieldThreshold".Translate() + "/" + shield.stressPct.ToStringPercent("F0"));
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect2, shield.DescExtraString());
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
