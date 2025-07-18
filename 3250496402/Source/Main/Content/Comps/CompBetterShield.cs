using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Kraltech_Industries;

[StaticConstructorOnStartup]
public class CompBetterShield : ThingComp
{
    private const float MaxDamagedJitterDist = 0.05f;

    private const int JitterDurationTicks = 8;

    private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble_Strong2", ShaderDatabase.Transparent);

    private readonly float ApparelScorePerEnergyMax = 0.25f;

    protected float energy;

    private Vector3 impactAngleVect;

    private readonly int KeepDisplayingTicks = 1000;

    private int lastAbsorbDamageTick = -9999;

    protected int lastKeepDisplayTick = -9999;

    protected int ticksToReset = -1;

    public CompProperties_BetterShield Props => (CompProperties_BetterShield)props;

    private float EnergyMax => parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax);

    private float EnergyGainPerTick => parent.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;

    public float Energy => energy;

    public ShieldState ShieldState
    {
        get
        {
            Pawn p;
            ShieldState result;
            if ((p = parent as Pawn) != null && (p.IsCharging() || p.IsSelfShutdown()))
            {
                result = ShieldState.Disabled;
            }
            else
            {
                var comp = parent.GetComp<CompCanBeDormant>();
                if (comp != null && !comp.Awake)
                    result = ShieldState.Disabled;
                else if (ticksToReset <= 0)
                    result = ShieldState.Active;
                else
                    result = ShieldState.Resetting;
            }

            return result;
        }
    }

    protected bool ShouldDisplay
    {
        get
        {
            var pawnOwner = PawnOwner;
            return pawnOwner.Spawned && !pawnOwner.Dead && !pawnOwner.Downed && (pawnOwner.InAggroMentalState || pawnOwner.Drafted || (pawnOwner.Faction.HostileTo(Faction.OfPlayer) && !pawnOwner.IsPrisoner) || Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks || (ModsConfig.BiotechActive && pawnOwner.IsColonyMech && Find.Selector.SingleSelectedThing == pawnOwner));
        }
    }

    protected Pawn PawnOwner
    {
        get
        {
            Apparel apparel;
            Pawn result;
            Pawn pawn;
            if ((apparel = parent as Apparel) != null)
                result = apparel.Wearer;
            else if ((pawn = parent as Pawn) != null)
                result = pawn;
            else
                result = null;
            return result;
        }
    }

    public bool IsApparel => parent is Apparel;

    private bool IsBuiltIn => !IsApparel;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref energy, "energy");
        Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
        Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick");
    }

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
    {
        foreach (var gizmo in base.CompGetWornGizmosExtra()) yield return gizmo;
        IEnumerator<Gizmo> enumerator = null;
        if (IsApparel)
        {
            foreach (var gizmo2 in GetGizmos()) yield return gizmo2;
            enumerator = null;
        }

        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action
            {
                defaultLabel = "DEV: Break",
                action = Break
            };
            if (ticksToReset > 0)
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Clear reset",
                    action = delegate { ticksToReset = 0; }
                };
        }

        yield break;
        yield break;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra()) yield return gizmo;
        IEnumerator<Gizmo> enumerator = null;
        if (IsBuiltIn)
        {
            foreach (var gizmo2 in GetGizmos()) yield return gizmo2;
            enumerator = null;
        }

        yield break;
        yield break;
    }

    private IEnumerable<Gizmo> GetGizmos()
    {
        Pawn pawn;
        if ((PawnOwner.Faction == Faction.OfPlayer || ((pawn = parent as Pawn) != null && pawn.RaceProps.IsMechanoid)) && Find.Selector.SingleSelectedThing == PawnOwner)
            yield return new Gizmo_BossEnergyShieldStatus
            {
                shield = this
            };
    }

    public override float CompGetSpecialApparelScoreOffset()
    {
        return EnergyMax * ApparelScorePerEnergyMax;
    }

    public override void CompTick()
    {
        base.CompTick();
        if (PawnOwner == null)
        {
            energy = 0f;
            return;
        }

        if (ShieldState == ShieldState.Resetting)
        {
            ticksToReset--;
            if (ticksToReset <= 0) Reset();
        }
        else if (ShieldState == ShieldState.Active)
        {
            energy += EnergyGainPerTick;
            if (energy > EnergyMax) energy = EnergyMax;
        }
    }

    public void KeepDisplaying()
    {
        lastKeepDisplayTick = Find.TickManager.TicksGame;
    }

    private void AbsorbedDamage(DamageInfo dinfo)
    {
        SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(PawnOwner.Position, PawnOwner.Map));
        impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
        var loc = PawnOwner.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
        var num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
        FleckMaker.Static(loc, PawnOwner.Map, FleckDefOf.ExplosionFlash, num);
        var num2 = (int)num;
        for (var i = 0; i < num2; i++) FleckMaker.ThrowDustPuff(loc, PawnOwner.Map, Rand.Range(0.8f, 1.2f));
        lastAbsorbDamageTick = Find.TickManager.TicksGame;
        KeepDisplaying();
    }

    private void Break()
    {
        var scale = Mathf.Lerp(Props.minDrawSize, Props.maxDrawSize, energy);
        EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale);
        FleckMaker.Static(PawnOwner.TrueCenter(), PawnOwner.Map, FleckDefOf.ExplosionFlash, 12f);
        for (var i = 0; i < 6; i++) FleckMaker.ThrowDustPuff(PawnOwner.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), PawnOwner.Map, Rand.Range(0.8f, 1.2f));
        energy = 0f;
        ticksToReset = Props.startingTicksToReset;
    }

    private void Reset()
    {
        if (PawnOwner.Spawned)
        {
            SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(PawnOwner.Position, PawnOwner.Map));
            FleckMaker.ThrowLightningGlow(PawnOwner.TrueCenter(), PawnOwner.Map, 3f);
        }

        ticksToReset = -1;
        energy = Props.energyOnReset;
    }

    public override void CompDrawWornExtras()
    {
        base.CompDrawWornExtras();
        if (IsApparel) Draw();
    }

    public override void PostDraw()
    {
        base.PostDraw();
        if (IsBuiltIn) Draw();
    }

    private void Draw()
    {
        if (ShieldState == ShieldState.Active && ShouldDisplay)
        {
            var num = Mathf.Lerp(Props.minDrawSize, Props.maxDrawSize, energy);
            var vector = PawnOwner.Drawer.DrawPos;
            vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            var num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
            if (num2 < 8)
            {
                var num3 = (8 - num2) / 8f * 0.05f;
                vector += impactAngleVect * num3;
                num -= num3;
            }

            float angle = Rand.Range(0, 360);
            var s = new Vector3(num, 1f, num);
            var matrix = default(Matrix4x4);
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
        }
    }

    public override bool CompAllowVerbCast(Verb verb)
    {
        return !Props.blocksRangedWeapons || !(verb is Verb_LaunchProjectile);
    }


    public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        absorbed = false;
        if (ShieldState <= ShieldState.Active && (dinfo.Def.isRanged || dinfo.Def.isExplosive))
        {
            energy -= dinfo.Amount * Props.energyLossPerDamage;
            if (energy < 0f)
                Break();
            else
                AbsorbedDamage(dinfo);
            absorbed = true;
        }
    }
}
