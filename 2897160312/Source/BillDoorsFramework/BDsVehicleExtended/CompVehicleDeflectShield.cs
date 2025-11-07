using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.Sound;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    public class CompVehicleDeflectShield : VehicleComp, IVehicleDamageAbsorber
    {
        public CompProperties_VehicleDeflectShield Props => props as CompProperties_VehicleDeflectShield;

        CompVehicleCapacitor capacitor => Vehicle.TryGetComp<CompVehicleCapacitor>();

        List<VehicleComponent> components = new List<VehicleComponent>();

        private Material BubbleMat => MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent, Props.color);

        public int regenerateTicks = 9999;

        public int remainingDurationTicks = 0;

        public bool switchedOn => remainingDurationTicks > 0;

        public float rechargePct => Mathf.Clamp(1 - ((float)regenerateTicks / Props.regenerateTicks), 0, 1);

        public int maxDurationTicks => (int)Math.Ceiling(Props.durationTick * Efficiency);

        private Vector3 impactAngleVect;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref regenerateTicks, "rechargeTick");
            Scribe_Values.Look(ref remainingDurationTicks, "remainingDurationTicks");
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
                if (!Vehicle.Spawned || Efficiency <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        protected bool ShouldDisplay => ShouldBeOn && remainingDurationTicks > 0;

        public bool ready => regenerateTicks <= 0 && ShouldBeOn && !switchedOn;

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
            if (ShouldDisplay && dinfo.Def.harmsHealth)
            {
                AbsorbedDamage(ref dinfo);
                return true;
            }
            return false;
        }

        public bool AbsorbDamage(ref DamageInfo dinfo)
        {
            if (dinfo.Def == DamageDefOf.EMP && dinfo.Amount > Props.EMPResistance)
            {
                Break();
                return false;
            }
            if (ShouldDisplay && dinfo.Def.harmsHealth)
            {
                AbsorbedDamage(ref dinfo);
                return true;
            }
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

        public void Activate()
        {
            remainingDurationTicks = maxDurationTicks;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (ShouldDisplay)
            {
                float scale = Mathf.Lerp(Props.mindrawsize, Props.drawsize, (float)remainingDurationTicks / maxDurationTicks);
                Vector3 drawPos = Vehicle.Drawer.DrawPos;
                drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                float angle = Rand.Range(0, 360);
                Vector3 s = new Vector3(scale, 1f, scale);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
            }
        }

        public override void CompTick()
        {
            if (ShouldBeOn)
            {
                if (switchedOn)
                {
                    if (!capacitor.TryDrain(Props.maintainPower))
                    {
                        regenerateTicks = Props.regenerateTicks;
                        remainingDurationTicks = 0;
                    }
                    else
                    {
                        remainingDurationTicks--;
                        if (!switchedOn)
                        {
                            Break();
                        }
                    }
                }
                else if (regenerateTicks > 0 && capacitor.TryDrain(Props.regeneratePower / Props.regenerateTicks))
                {
                    regenerateTicks--;
                }
            }
            else if (Props.resetWhenOff)
            {
                remainingDurationTicks = 0;
                regenerateTicks = Props.regenerateTicks;
            }
        }

        public void Break()
        {
            float scale = Mathf.Lerp(Props.mindrawsize, Props.drawsize, (float)remainingDurationTicks / maxDurationTicks);
            RimWorld.EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale / 2);
            FleckMaker.Static(parent.TrueCenter(), parent.Map, FleckDefOf.ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                FleckMaker.ThrowDustPuff(parent.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), parent.Map, Rand.Range(0.8f, 1.2f));
            }
            regenerateTicks = Props.regenerateTicks;
        }

        public string DescExtraString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("DeflectShieldDesc".Translate());
            stringBuilder.AppendLine("DeflectShieldDuration".Translate(maxDurationTicks.TicksToSeconds(), Props.durationTick.TicksToSeconds(), Efficiency.ToStringPercent()));
            stringBuilder.AppendLine("DeflectShieldRechargeTime".Translate(Props.regenerateTicks.TicksToSeconds()));
            stringBuilder.AppendLine("DeflectShieldRechargePower".Translate(Props.regeneratePower));
            if (Props.maintainPower > 0f)
            {
                stringBuilder.AppendLine("DeflectShieldMaintainPower".Translate(Props.maintainPower * 60));
            }
            if (Props.EMPResistance > 0f)
            {
                stringBuilder.AppendLine("BDFV_EMPresist".Translate(Props.EMPResistance));
            }
            if (!Props.resetWhenOff)
            {
                stringBuilder.AppendLine("DeflectShieldKeepCharge".Translate());
            }
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Gizmo_VehicleDeflectShield gizmo = new Gizmo_VehicleDeflectShield();
            gizmo.shield = this;
            yield return gizmo;
        }
    }

    public class CompProperties_VehicleDeflectShield : CompProperties_VehicleShield
    {
        public CompProperties_VehicleDeflectShield()
        {
            this.compClass = typeof(CompVehicleDeflectShield);
        }

        public int durationTick = 600;

        public int regenerateTicks = 600;

        public float regeneratePower = 2000;

        public bool resetWhenOff = true;
    }

    [StaticConstructorOnStartup]
    public class Gizmo_VehicleDeflectShield : Gizmo
    {
        public CompVehicleDeflectShield shield;

        private static readonly Texture2D ShieldOffTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.3f, 0f, 0f));

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.5f, 0.75f, 0.93f));

        private static readonly Texture2D ShieldProgressBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_VehicleDeflectShield()
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
            if (Widgets.ButtonInvisible(rect2) && shield.ready)
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                shield.Activate();
            }
            Rect rect3 = rect2;
            rect3.height = rect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect3, "BDFV_DeflectShield".Translate().Resolve());
            Rect rect4 = rect2;
            rect4.yMin = rect2.y + rect2.height / 2f;

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            if (shield.Efficiency <= 0)
            {
                float fillPercent = 1;
                Widgets.FillableBar(rect4, fillPercent, ShieldOffTex, EmptyShieldBarTex, doBorder: false);
                Widgets.Label(rect4, "DeflectShieldDamaged".Translate());
            }
            else if (shield.ready)
            {
                float fillPercent = 1;
                Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: false);
                Widgets.Label(rect4, "BDFV_DeflectShieldReady".Translate());
            }
            else if (shield.switchedOn)
            {
                float fillPercent = (float)shield.remainingDurationTicks / shield.maxDurationTicks;
                Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: false);
                Widgets.Label(rect4, "BDFV_DeflectShieldDuration".Translate(shield.remainingDurationTicks.TicksToSeconds().ToString("F1")));
            }
            else if (shield.regenerateTicks > 0)
            {
                float fillPercent = shield.rechargePct;
                Widgets.FillableBar(rect4, fillPercent, ShieldProgressBarTex, EmptyShieldBarTex, doBorder: false);
                Widgets.Label(rect4, "BDFV_DeflectShieldRecharge".Translate(shield.regenerateTicks.TicksToSeconds().ToString("F1")));
            }
            else
            {
                float fillPercent = 1;
                Widgets.FillableBar(rect4, fillPercent, ShieldProgressBarTex, EmptyShieldBarTex, doBorder: false);
                Widgets.Label(rect4, "BDFV_DeflectShieldStandby".Translate());
            }

            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect2, shield.DescExtraString());
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
