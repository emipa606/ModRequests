using Verse;
using RimWorld;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Drawing;
using Verse.AI;
using BillDoorsFramework;

namespace BDsPlasmaWeaponVanilla
{
    public class CompTankFeedWeapon : CompEquippedGizmo
    {
        public CompProperties_TankFeedWeapon Props
        {
            get
            {
                return (CompProperties_TankFeedWeapon)props;
            }
        }

        public CompReloadableFromFiller compReloadableFromFiller;

        CompReloadableFromFiller compWeaponTank;

        public override void PostPostMake()
        {
            base.PostPostMake();
            searchTank(1, false);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            searchTank(1, false);
            compWeaponTank = parent.TryGetComp<CompReloadableFromFiller>();
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            searchTank(1, false);
        }

        public Pawn CasterPawn
        {
            get
            {
                return Verb.caster as Pawn;
            }
        }

        private Verb Verb
        {
            get
            {
                return EquipmentSource.PrimaryVerb;
            }
        }


        private CompEquippable EquipmentSource
        {
            get
            {
                return parent.TryGetComp<CompEquippable>();
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            compReloadableFromFiller = null;
        }

        public bool searchTank(int t = 1, bool notify = true)
        {

            if (CasterPawn != null)
            {
                List<Apparel> apparels = CasterPawn.apparel.WornApparel;
                foreach (Apparel apparel in apparels)
                {
                    CompReloadableFromFiller a = apparel.TryGetComp<CompReloadableFromFiller>();
                    if (a != null && a.RemainingCharges >= t)
                    {
                        compReloadableFromFiller = a;
                        return true;
                    }
                }
            }
            if (notify)
            {
                if (CasterPawn != null)
                {
                    Messages.Message(string.Format("BDP_LizionTankSearchFailedWithPawn".Translate(), parent.LabelCap, CasterPawn), parent, MessageTypeDefOf.RejectInput, historical: false);
                }
                else
                {
                    Messages.Message(string.Format("BDP_LizionTankSearchFailed".Translate(), parent.LabelCap), parent, MessageTypeDefOf.RejectInput, historical: false);
                }
            }
            return false;
        }

        private void siphonTank()
        {
            if (compReloadableFromFiller != null && compWeaponTank != null)
            {
                int gasNeeded = compWeaponTank.emptySpace;
                int gasAvailable = compReloadableFromFiller.RemainingCharges;
                if (gasNeeded > 0)
                {
                    if (gasNeeded <= gasAvailable)
                    {
                        compWeaponTank.RefillGas(gasNeeded);
                        compReloadableFromFiller.DrawGas(gasNeeded);
                    }
                    else
                    {
                        compWeaponTank.RefillGas(gasAvailable);
                        compReloadableFromFiller.DrawGas(gasAvailable);
                    }
                }
            }
        }

        public void OverchargedDamage(ThingWithComps weapon)
        {
            if (Rand.Chance(Props.overchargeDamageChance))
            {
                float HPcache = (float)weapon.HitPoints / weapon.MaxHitPoints;
                weapon.HitPoints -= (int)Math.Round(Rand.Value * Props.overchargeDamageMultiplier);
                float HPnow = (float)weapon.HitPoints / weapon.MaxHitPoints;
                if (parent.ParentHolder is Pawn_EquipmentTracker equipmentTracker)
                {
                    Pawn pawn = equipmentTracker.pawn;
                    if (pawn.Faction == Faction.OfPlayer)
                    {
                        if (HPcache > 0.5 && HPnow <= 0.5)
                        {
                            Messages.Message(string.Format("BDP_WeaponFailingPawn".Translate(), pawn, parent.LabelCap), parent, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else if (HPcache > 0.25 && HPnow <= 0.25)
                        {
                            Messages.Message(string.Format("BDP_WeaponFailingUrgentPawn".Translate(), pawn, parent.LabelCap), parent, MessageTypeDefOf.ThreatSmall, historical: false);
                        }
                    }
                }
                else if (parent.ParentHolder is Thing thing && thing.Faction == Faction.OfPlayer)
                {
                    if (HPcache > 0.5 && HPnow <= 0.5)
                    {
                        Messages.Message(string.Format("BDP_WeaponFailing".Translate(), parent.LabelCap), parent, MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else if (HPcache > 0.25 && HPnow <= 0.25)
                    {
                        Messages.Message(string.Format("BDP_WeaponFailingUrgent".Translate(), parent.LabelCap), parent, MessageTypeDefOf.ThreatSmall, historical: false);
                    }
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosEquipped()
        {

            if (CasterPawn != null && !CasterPawn.Faction.Equals(Faction.OfPlayer))
            {
                yield break;
            }

            if (compReloadableFromFiller != null && compWeaponTank != null)
            {
                Command_Action command_Siphon = new Command_Action
                {
                    defaultLabel = Props.siphon.Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/FireModes/PlasmaWeaponTankMode_on", false),
                    defaultDesc = Props.siphondescription.Translate(),
                    action = delegate
                    {
                        siphonTank();
                    }
                };
                yield return command_Siphon;
            }

            Command_Action command_Action = new Command_Action
            {
                defaultLabel = Props.reconnect.Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/PlasmaBackpack_Reconnect", false),
                defaultDesc = Props.reconnectdescription.Translate(),
                action = delegate
                {
                    searchTank();
                }
            };
            yield return command_Action;

            yield break;
        }
    }

    public class CompProperties_TankFeedWeapon : CompProperties
    {
        public string reconnect = "BDP_SiphonReconnect";
        public string reconnectdescription = "BDP_SiphonReconnectDesc";
        public string siphon = "BDP_SiphonVanilla";
        public string siphondescription = "BDP_SiphonDiscVanilla";
        public int ammoConsumption = 1;
        public float overchargeDamageChance = 0;
        public float overchargeDamageMultiplier = 1;
        public CompProperties_TankFeedWeapon()
        {
            compClass = typeof(CompTankFeedWeapon);
        }
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, "BDP_GunAmmoConsumption".Translate(), ammoConsumption.ToString(), "BDP_GunAmmoConsumptionDesc".Translate(), 0);
            if (overchargeDamageChance > 0)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, "BDP_GunOverchargeDamageChance".Translate(), overchargeDamageChance.ToStringPercent(), "BDP_GunOverchargeDamageChanceDesc".Translate(), 0);
                yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, "BDP_GunoverchargeDamageMultiplier".Translate(), overchargeDamageMultiplier.ToString(), "BDP_GunOverchargeDamageMultiplierDesc".Translate(), 0);
            }
        }
    }
}
