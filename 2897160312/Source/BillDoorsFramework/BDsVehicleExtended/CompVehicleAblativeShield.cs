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
    public class CompVehicleAblativeShield : VehicleComp, IVehicleDamageAbsorber
    {
        public CompProperties_VehicleAblativeShield Props => props as CompProperties_VehicleAblativeShield;

        CompVehicleCapacitor capacitor => Vehicle.TryGetComp<CompVehicleCapacitor>();

        List<VehicleComponent> components = new List<VehicleComponent>();

        private Material BubbleMat => MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent, Props.color);

        public int lastDamageTick = -9999;

        public bool switchedOn = true;

        public float rechargePct => Mathf.Clamp(shieldPower / Props.capacity, 0, 1);

        public float damagedPct => Mathf.Clamp((Props.capacity - MaxCapacity) / Props.capacity, 0, 1);

        public float MaxCapacity => (float)Math.Ceiling(Props.capacity * Efficiency);

        public float shieldPower;

        private Vector3 impactAngleVect;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastDamageTick, "rechargeTick");
            Scribe_Values.Look(ref switchedOn, "switchedOn");
            Scribe_Values.Look(ref shieldPower, "charges");
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
                if (shieldPower <= 0)
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
                shieldPower = 0;
                Break();
                return false;
            }
            if (!ShouldDisplay || !dinfo.Def.harmsHealth)
            {
                return false;
            }
            shieldPower -= dinfo.Amount;
            AbsorbedDamage(ref dinfo);
            if (shieldPower <= 0)
            {
                shieldPower = 0;
                Break();
            }
            return true;
        }

        public void Break()
        {
            float scale = Mathf.Lerp(Props.mindrawsize, Props.drawsize, rechargePct);
            RimWorld.EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale / 2);
            FleckMaker.Static(parent.TrueCenter(), parent.Map, FleckDefOf.ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                FleckMaker.ThrowDustPuff(parent.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), parent.Map, Rand.Range(0.8f, 1.2f));
            }

            lastDamageTick = Props.breakDelay;
        }

        public void AbsorbedDamage(ref DamageInfo dinfo)
        {
            lastDamageTick = Props.rechargeDelay;
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
                float num = Mathf.Lerp(Props.mindrawsize, Props.drawsize, rechargePct);
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
                if (!capacitor.TryDrain(Props.maintainPower))
                {
                    shieldPower = 0;
                }
                else
                {
                    lastDamageTick--;
                    if (shieldPower < MaxCapacity && lastDamageTick <= 0 && capacitor.TryDrain(Props.powerPerRecharge * Props.rechargePerTick))
                    {
                        if (shieldPower == 0)
                        {
                            shieldPower += Props.regenInitPct * MaxCapacity;
                        }
                        shieldPower += Props.rechargePerTick;
                    }
                }
                if (shieldPower > MaxCapacity)
                {
                    shieldPower = MaxCapacity;
                }
            }
            else
            {
                shieldPower = 0;
                lastDamageTick = Props.breakDelay;
            }
        }

        public string DescExtraString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("AblativeShieldDesc".Translate());
            stringBuilder.AppendLine("AblativeShieldCapacity".Translate(MaxCapacity, Props.capacity, Efficiency.ToStringPercent()));
            stringBuilder.AppendLine("AblativeShieldRecharge".Translate(Props.rechargePerTick * 60));
            if (Props.rechargeDelay > 0f)
            {
                stringBuilder.AppendLine("AblativeShieldRechargeDelay".Translate(Props.rechargeDelay.TicksToSeconds()));
            }
            if (Props.regenInitPct > 0f)
            {
                stringBuilder.AppendLine("AblativeShieldRegenInitPct".Translate(Props.regenInitPct.ToStringPercent()));
            }
            if (Props.breakDelay > 0f)
            {
                stringBuilder.AppendLine("AblativeShieldBreakDelay".Translate(Props.breakDelay.TicksToSeconds()));
            }
            if (Props.powerPerRecharge > 0f)
            {
                stringBuilder.AppendLine("AblativeShieldRechargePower".Translate(Props.powerPerRecharge));
            }
            if (Props.maintainPower > 0f)
            {
                stringBuilder.AppendLine("AblativeShieldMaintainPower".Translate(Props.maintainPower * 60));
            }
            if (Props.EMPResistance > 0f)
            {
                stringBuilder.AppendLine("BDFV_EMPresist".Translate(Props.EMPResistance));
            }
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Gizmo_VehicleAblativeShield gizmo = new Gizmo_VehicleAblativeShield();
            gizmo.shield = this;
            yield return gizmo;
        }
    }

    public class CompProperties_VehicleAblativeShield : CompProperties_VehicleShield
    {
        public CompProperties_VehicleAblativeShield()
        {
            this.compClass = typeof(CompVehicleAblativeShield);
        }

        public int capacity = 500;

        public int rechargePerTick = 1;

        public int rechargeDelay = 300;

        public int breakDelay = 300;

        public float regenInitPct = 0.5f;

        public float powerPerRecharge = 1;
    }

    [StaticConstructorOnStartup]
    public class Gizmo_VehicleAblativeShield : Gizmo
    {
        public CompVehicleAblativeShield shield;

        private static readonly Texture2D ShieldOffTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.3f, 0f, 0f));

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.5f, 0.75f, 0.93f));

        private static readonly Texture2D ShieldProgressBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_VehicleAblativeShield()
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
            Widgets.Label(rect3, "BDFV_AblativeShield".Translate().Resolve());
            Rect rect4 = rect2;
            rect4.yMin = rect2.y + rect2.height / 2f;

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            if (shield.Efficiency <= 0)
            {
                float fillPercent = 1;
                Widgets.FillableBar(rect4, fillPercent, ShieldOffTex, EmptyShieldBarTex, doBorder: false);
                Widgets.Label(rect4, "AblativeShieldDamaged".Translate());
            }
            else if (!shield.switchedOn)
            {
                float fillPercent = 1;
                Widgets.FillableBar(rect4, fillPercent, ShieldOffTex, EmptyShieldBarTex, doBorder: false);
                Widgets.Label(rect4, "AblativeShieldOff".Translate());
            }
            else
            {
                Widgets.FillableBar(rect4, shield.rechargePct, ShieldProgressBarTex, EmptyShieldBarTex, doBorder: false);

                if (shield.Efficiency < 1)
                {
                    Rect damagedBar = rect4;
                    damagedBar.width *= shield.damagedPct;
                    damagedBar.position = new Vector2(rect4.xMax - damagedBar.width, damagedBar.position.y);
                    GUI.DrawTexture(damagedBar, ShieldOffTex);
                }
                if (shield.shieldPower <= 0 && shield.lastDamageTick > 0)
                {
                    Widgets.Label(rect4, "BDFV_AblativeShieldDelay".Translate(shield.lastDamageTick.TicksToSeconds().ToString("F1")).Resolve());
                }
                else
                {
                    Widgets.Label(rect4, shield.shieldPower.ToString() + "/" + shield.rechargePct.ToStringPercent("F0"));
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect2, shield.DescExtraString());
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
