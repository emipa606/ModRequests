using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Necro;

public class CompReloadable_Weapon : CompEquippable
{
    private int _maxCharges;
    private int _remainingCharges;
    private int _replenishInTicks = -1;
    private bool _shouldDestroy;

    private CompProperties_ReloadableWeapon Props => props as CompProperties_ReloadableWeapon;

    public Thing ReloadableThing => parent;

    public ThingDef AmmoDef => Props.ammoDef;

    public int MaxCharges => Props.maxCharges;

    public int BaseReloadTicks => Props.baseReloadTicks;

    public string LabelRemaining => string.Format("{0} / {1}", RemainingCharges, MaxCharges);

    public int RemainingCharges
    {
        get => _remainingCharges;
        set => _remainingCharges = Math.Max(0, Math.Min(_maxCharges, value));
    }


    public override void CompTick()
    {
        if (_shouldDestroy && Props.destroyOnEmpty)
        {
            parent?.Destroy();
        }
        else
        {
            base.CompTick();
            if (Props.replenishAfterCooldown && RemainingCharges == 0)
            {
                if (_replenishInTicks > 0)
                {
                    _replenishInTicks--;
                    return;
                }

                RemainingCharges = MaxCharges;
            }
        }
    }

    public override void PostPostMake()
    {
        base.PostPostMake();
        _maxCharges = MaxCharges;
        RemainingCharges = MaxCharges;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref _replenishInTicks, "replenishInTicks", -1);
        Scribe_Values.Look(ref _remainingCharges, "chargesRemaining");
        Scribe_Values.Look(ref _maxCharges, "maxCharges");
    }

    public void NotifyShot()
    {
        RemainingCharges = Math.Max(0, RemainingCharges - 1);
        if (RemainingCharges <= 0)
        {
            if (Props.destroyOnEmpty)
                _shouldDestroy = true;
            else if (Props.replenishAfterCooldown)
                _replenishInTicks = Props.baseReloadTicks;
        }
    }

    public override IEnumerable<Gizmo> CompGetEquippedGizmosExtra()
    {
        if (Holder == null) yield break;

        if (Props.displayGizmoWhileUndrafted && !Holder.Drafted) yield break;
        if (Props.displayGizmoWhileDrafted && Holder.Drafted) yield break;
        foreach (var gizmo in base.CompGetEquippedGizmosExtra())
            yield return gizmo;

        ThingWithComps gear = this.parent;
        foreach (Verb verb in this.VerbTracker.AllVerbs)
        {
            if (verb.verbProps.hasStandardCommand)
                yield return this.CreateVerbTargetCommand(gear, verb);
        }

        if (DebugSettings.ShowDevGizmos && NeedsReload(false))
            yield return new Command_Action
            {
                defaultLabel = "DEV: Reload to full",
                action = () => RemainingCharges = MaxCharges
            };
    }

    public virtual string GizmoExtraLabel
    {
        get { return this.LabelRemaining; }
    }

    private Command_VerbTarget CreateVerbTargetCommand(Thing gear, Verb verb)
    {
        Command_VerbTarget command_VerbOwner = new Command_VerbReloadable(this);
        command_VerbOwner.defaultDesc = gear.def.description;
        command_VerbOwner.hotKey = null;
        command_VerbOwner.defaultLabel = verb.verbProps.label;
        command_VerbOwner.verb = verb;
        if (verb.verbProps.defaultProjectile != null && verb.verbProps.commandIcon == null)
        {
            command_VerbOwner.icon = verb.verbProps.defaultProjectile.uiIcon;
            command_VerbOwner.iconAngle = verb.verbProps.defaultProjectile.uiIconAngle;
            command_VerbOwner.iconOffset = verb.verbProps.defaultProjectile.uiIconOffset;
            command_VerbOwner.defaultIconColor = verb.verbProps.defaultProjectile.graphicData.color;
        }
        else
        {
            command_VerbOwner.icon = ((verb.UIIcon != BaseContent.BadTex) ? verb.UIIcon : gear.def.uiIcon);
            command_VerbOwner.iconAngle = gear.def.uiIconAngle;
            command_VerbOwner.iconOffset = gear.def.uiIconOffset;
            command_VerbOwner.defaultIconColor = gear.DrawColor;
        }

        if (!this.Holder.IsColonistPlayerControlled)
        {
            command_VerbOwner.Disable("CannotOrderNonControlled".Translate());
        }
        else if (verb.verbProps.violent && this.Holder.WorkTagIsDisabled(WorkTags.Violent))
        {
            command_VerbOwner.Disable("IsIncapableOfViolenceLower".Translate(this.Holder.LabelShort, this.Holder).CapitalizeFirst() + ".");
        }
        else if (!this.CanBeUsed(out var reason))
        {
            command_VerbOwner.Disable(reason);
        }

        return command_VerbOwner;
    }

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
        var enumerable = base.SpecialDisplayStats();
        if (enumerable != null)
            foreach (var statDrawEntry in enumerable)
                yield return statDrawEntry;
        yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "Stat_Thing_ReloadChargesRemaining_Name".Translate(Props.ChargeNounArgument), LabelRemaining, "Stat_Thing_ReloadChargesRemaining_Desc".Translate(Props.ChargeNounArgument), 5440);
    }

    public override string CompInspectStringExtra()
    {
        return "ChargesRemaining".Translate(Props.ChargeNounArgument) + ": " + LabelRemaining;
    }

    public bool NeedsReload(bool allowForcedReload)
    {
        if (Props.ammoDef == null) return false;
        if (Props.ammoCountToRefill == 0) return RemainingCharges != MaxCharges;
        if (!allowForcedReload) return RemainingCharges == 0;
        return RemainingCharges != MaxCharges;
    }

    public void ReloadFrom(Thing ammo)
    {
        if (!NeedsReload(true)) return;
        if (Props.ammoCountToRefill != 0)
        {
            if (ammo.stackCount < Props.ammoCountToRefill) return;
            ammo.SplitOff(Props.ammoCountToRefill).Destroy();
            RemainingCharges = MaxCharges;
        }
        else
        {
            if (ammo.stackCount < Props.ammoCountPerCharge) return;
            var num = Mathf.Clamp(ammo.stackCount / Props.ammoCountPerCharge, 0, MaxCharges - RemainingCharges);
            ammo.SplitOff(num * Props.ammoCountPerCharge).Destroy();
            RemainingCharges += num;
        }

        if (Props.soundReload != null) Props.soundReload.PlayOneShot(new TargetInfo(parent.PositionHeld, parent.MapHeld));
    }


    public int MinAmmoNeeded(bool allowForcedReload)
    {
        if (!NeedsReload(allowForcedReload)) return 0;
        if (Props.ammoCountToRefill != 0) return Props.ammoCountToRefill;
        return Props.ammoCountPerCharge;
    }

    public int MaxAmmoNeeded(bool allowForcedReload)
    {
        if (!NeedsReload(allowForcedReload)) return 0;
        if (Props.ammoCountToRefill != 0) return Props.ammoCountToRefill;
        return Props.ammoCountPerCharge * (MaxCharges - RemainingCharges);
    }

    public int MaxAmmoAmount()
    {
        if (Props.ammoDef == null) return 0;
        if (Props.ammoCountToRefill == 0) return Props.ammoCountPerCharge * MaxCharges;
        return Props.ammoCountToRefill;
    }


    public bool CanBeUsed(out string reason)
    {
        reason = "";
        if (RemainingCharges <= 0)
        {
            reason = DisabledReason(MinAmmoNeeded(false), MaxAmmoNeeded(false));
            return false;
        }

        return true;
    }

    public string DisabledReason(int minNeeded, int maxNeeded)
    {
        if (Props.replenishAfterCooldown) return "CommandReload_Cooldown".Translate(Props.CooldownVerbArgument, _replenishInTicks.ToStringTicksToPeriod().Named("TIME"));
        if (Props.ammoDef == null) return "CommandReload_NoCharges".Translate(Props.ChargeNounArgument);
        string arg;
        if (Props.ammoCountToRefill != 0)
            arg = Props.ammoCountToRefill.ToString();
        else
            arg = minNeeded == maxNeeded ? minNeeded.ToString() : string.Format("{0}-{1}", minNeeded, maxNeeded);
        return "CommandReload_NoAmmo".Translate(Props.ChargeNounArgument, Props.ammoDef.Named("AMMO"), arg.Named("COUNT"));
    }
}
