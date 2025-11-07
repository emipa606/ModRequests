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
    public interface IVehicleDamageAbsorber
    {
        bool AbsorbDamage(ref DamageInfo dinfo);

        bool AbsorbDamageExplosive(ref DamageInfo dinfo);
    }

    public class CompProperties_VehicleShield : CompProperties
    {
        public float drawsize = 1;

        public float mindrawsize = 1;

        public float maintainPower = 1;

        public List<string> shieldComponentKeys = new List<string>();

        public float EMPResistance = -1;

        public Color color = Color.white;
    }

    [StaticConstructorOnStartup]
    public class CompVehicleWhippleShield : VehicleComp, IVehicleDamageAbsorber
    {
        public CompProperties_VehicleWhippleShield Props => props as CompProperties_VehicleWhippleShield;

        CompVehicleCapacitor capacitor => Vehicle.TryGetComp<CompVehicleCapacitor>();

        List<VehicleComponent> components = new List<VehicleComponent>();
        private Material BubbleMat => MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent, Props.color);

        private int rechargeTick = 9999;

        public bool switchedOn = true;

        public float rechargePct => Mathf.Clamp(1 - ((float)rechargeTick / Props.rechargeTicks), 0, 1);

        public int MaxCharges => (int)Math.Ceiling(Props.charges * Efficiency);

        public int charges;

        private Vector3 impactAngleVect;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref rechargeTick, "rechargeTick");
            Scribe_Values.Look(ref switchedOn, "switchedOn");
            Scribe_Values.Look(ref charges, "charges");
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
                if (!ShouldBeOn)
                {
                    return false;
                }
                if (charges <= 0)
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
        }



        public bool AbsorbDamageExplosive(ref DamageInfo dinfo)
        {
            return AbsorbDamage(ref dinfo);
        }

        public bool AbsorbDamage(ref DamageInfo dinfo)
        {
            if (dinfo.Def == DamageDefOf.EMP && dinfo.Amount > Props.EMPResistance)
            {
                charges = 0;
                rechargeTick = Props.EMPticks;
                return false;
            }
            if (charges == 0 || !dinfo.Def.harmsHealth)
            {
                return false;
            }
            if (dinfo.Amount > Props.damageThreshold)
            {
                charges--;
                AbsorbedDamage(ref dinfo);
                return true;
            }
            return false;
        }

        public void AbsorbedDamage(ref DamageInfo dinfo)
        {
            float scale = Mathf.Lerp(Props.mindrawsize, Props.drawsize, (float)charges / Props.charges);
            RimWorld.EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale / 2);
            FleckMaker.Static(parent.TrueCenter(), parent.Map, FleckDefOf.ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                FleckMaker.ThrowDustPuff(parent.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), parent.Map, Rand.Range(0.8f, 1.2f));
            }

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
            if (charges == 0)
            {
                CompleteBreak();
            }
        }

        public virtual void CompleteBreak()
        {
            rechargeTick = Props.regenTicks;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (ShouldDisplay)
            {
                float num = Mathf.Lerp(Props.mindrawsize, Props.drawsize, (float)charges / Props.charges);
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
            if (ShouldBeOn)
            {
                if (charges > MaxCharges)
                {
                    charges = MaxCharges;
                }
                if (!capacitor.TryDrain(Props.maintainPower))
                {
                    rechargeTick = Props.rechargeTicks;
                    charges = 0;
                }
                else if (charges < MaxCharges)
                {
                    if (rechargeTick < 0)
                    {
                        rechargeTick = Props.rechargeTicks;
                    }
                    if (rechargeTick == 0)
                    {
                        charges++;
                    }
                    if (capacitor.TryDrain(Props.powerPerRecharge / Props.rechargeTicks))
                    {
                        rechargeTick--;
                    }
                }
            }
            else
            {
                rechargeTick = Props.rechargeTicks;
                charges = 0;
            }
        }

        public string DescExtraString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("WhippleShieldDesc".Translate());
            stringBuilder.AppendLine("WhippleShieldLayers".Translate(MaxCharges, Props.charges, Efficiency.ToStringPercent()));
            stringBuilder.AppendLine("WhippleShieldRechargeTime".Translate(Props.rechargeTicks.TicksToSeconds()));
            stringBuilder.AppendLine("WhippleShieldDamageThreshold".Translate(Props.damageThreshold));
            stringBuilder.AppendLine("WhippleShieldRechargePower".Translate(Props.powerPerRecharge));
            if (Props.maintainPower > 0f)
            {
                stringBuilder.AppendLine("WhippleShieldMaintainPower".Translate(Props.maintainPower * 60));
            }
            if (Props.EMPResistance > 0f)
            {
                stringBuilder.AppendLine("BDFV_EMPresist".Translate(Props.EMPResistance));
            }
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Gizmo_VehicleWhippleShield gizmo = new Gizmo_VehicleWhippleShield();
            gizmo.shield = this;
            yield return gizmo;
        }
    }

    public class CompProperties_VehicleWhippleShield : CompProperties_VehicleShield
    {
        public CompProperties_VehicleWhippleShield()
        {
            this.compClass = typeof(CompVehicleWhippleShield);
        }

        public int charges = 1;

        public float damageThreshold = 30;

        public int rechargeTicks = 300;

        public int regenTicks = 600;

        public int EMPticks = 1500;

        public float powerPerRecharge = 1000;

        public bool isAPS = false;
    }

    [StaticConstructorOnStartup]
    public class Gizmo_VehicleWhippleShield : Gizmo
    {
        public CompVehicleWhippleShield shield;

        private static readonly Texture2D ShieldOffTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.3f, 0f, 0f));

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.5f, 0.75f, 0.93f));

        private static readonly Texture2D ShieldProgressBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_VehicleWhippleShield()
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
            Widgets.Label(rect3, "BDFV_WhippleShield".Translate().Resolve());
            Rect rect4 = rect2;
            rect4.yMin = rect2.y + rect2.height / 2f;

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            if (shield.Efficiency <= 0)
            {
                float fillPercent = 1;
                Widgets.FillableBar(rect4, fillPercent, ShieldOffTex, EmptyShieldBarTex, doBorder: false);
                Widgets.Label(rect4, "WhippleShieldDamaged".Translate());
            }
            else if (!shield.switchedOn)
            {
                float fillPercent = 1;
                Widgets.FillableBar(rect4, fillPercent, ShieldOffTex, EmptyShieldBarTex, doBorder: false);
                Widgets.Label(rect4, "WhippleShieldOff".Translate());
            }
            else
            {
                float fillPercent = shield.rechargePct;
                Widgets.FillableBar(rect4, fillPercent, ShieldProgressBarTex, EmptyShieldBarTex, doBorder: false);

                float chargeRectWidth = rect4.width / shield.Props.charges;
                for (int i = 0; i < shield.charges; i++)
                {
                    Rect rect1 = new Rect(rect4.position + new Vector2((i * chargeRectWidth), 0), new Vector2(chargeRectWidth, rect4.height)).ContractedBy(12 / shield.Props.charges, 6);
                    GUI.DrawTexture(rect1, FullShieldBarTex);
                }

                Widgets.Label(rect4, shield.charges.ToString() + "WhippleShieldCharge".Translate() + "/" + shield.rechargePct.ToStringPercent("F0"));
            }

            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect2, shield.DescExtraString());
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
