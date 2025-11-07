using CombatExtended;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BillDoorsFramework
{
    public class CompSecondaryAmmo : CompRangedGizmoGiver
    {
        public CompProperties_SecondaryAmmo Props
        {
            get
            {
                return (CompProperties_SecondaryAmmo)props;
            }
        }

        public Verb_LaunchProjectileCE AttackVerb
        {
            get
            {
                if (CompEquippable.PrimaryVerb == null)
                {
                    return null;
                }

                Verb_LaunchProjectileCE verb = CompEquippable.PrimaryVerb as Verb_LaunchProjectileCE;
                return verb;
            }
        }


        //----------------------------- Comp cache ------------------

        //get current CompAmmoUser
        public CompAmmoUser CompAmmo
        {
            get
            {
                if (compAmmo == null && base.parent != null)
                {
                    compAmmo = base.parent.TryGetComp<CompAmmoUser>();
                }
                return compAmmo;
            }
        }

        public CompEquippable CompEquippable
        {
            get
            {
                if (compEquippable == null && base.parent != null)
                {
                    compEquippable = base.parent.TryGetComp<CompEquippable>();
                }
                if (compEquippable == null)
                {
                    Log.Error("Can not get CompEquippable.");
                }
                return compEquippable;
            }
        }

        public CompInventory CompInventory
        {
            get
            {
                if (compInventory == null && Holder != null)
                {
                    compInventory = Holder.TryGetComp<CompInventory>();
                }
                if (compInventory == null)
                {
                    Log.Error("Can not get CompInventory.");
                }
                return compInventory;
            }
        }

        //Pawn cache
        public Pawn Holder
        {
            get
            {
                if (CompEquippable == null
                    || CompEquippable.PrimaryVerb == null
                    || CompEquippable.PrimaryVerb.caster == null
                    || ((CompEquippable?.parent?.ParentHolder as Pawn_InventoryTracker)?.pawn is Pawn holderPawn && holderPawn != CompEquippable?.PrimaryVerb?.CasterPawn))
                {
                    return null;
                }
                return CompEquippable.PrimaryVerb.CasterPawn ?? (CompEquippable.parent.ParentHolder as Pawn_InventoryTracker)?.pawn;
            }
        }

        //------------------------ CompProperties_AmmoUser Cache -----------------------

        public CompProperties_AmmoUser OriginAmmoSetData
        {
            get
            {
                if (originAmmoProps == null && HasAmmoUser)
                {

                    originAmmoProps = CompAmmo.Props;
                }
                return originAmmoProps;
            }
        }

        public CompProperties_AmmoUser SecondaryAmmoSetData
        {
            get
            {
                if (secondaryAmmoProps == null && HasAmmoUser)
                {

                    secondaryAmmoProps = Props.secondaryAmmoProps;
                }
                return secondaryAmmoProps;
            }
        }

        //---------------- public status-------------------

        public bool IsSecondaryAmmoSelected => useSecondaryAmmo;

        public bool IsSharedAmmo => isSharedAmmo;

        public bool HasAmmoUser => CompAmmo != null;

        public string MainAmmoName => OriginAmmoSetData.ammoSet.label;

        public string SecondaryAmmoName => SecondaryAmmoSetData.ammoSet.label;

        public int ScondaryMagSize => IsSecondaryAmmoSelected ? OriginAmmoSetData.magazineSize : SecondaryAmmoSetData.magazineSize;

        public bool useChargeSpeed = false;

        public AmmoData MainAmmoData
        {
            get
            {
                if (mainAmmo == null)
                {
                    mainAmmo = new AmmoData(0, CompAmmo.Props.ammoSet.ammoTypes[0].ammo);
                }
                return mainAmmo;
            }
        }

        public AmmoData SecondaryAmmoData
        {
            get
            {
                if (secondaryAmmo == null)
                {
                    secondaryAmmo = new AmmoData(0, Props.secondaryAmmoProps.ammoSet.ammoTypes[0].ammo);
                }
                return secondaryAmmo;
            }
        }

        public AmmoDef CurrentSecondaryAmmo => IsSecondaryAmmoSelected ? MainAmmoData.selectedAmmo : SecondaryAmmoData.selectedAmmo;

        public int ScondaryMagCount => IsSecondaryAmmoSelected ? MainAmmoData.curMagCount : SecondaryAmmoData.curMagCount;

        //----------------- private vars -----------------

        private CompAmmoUser compAmmo;
        private CompEquippable compEquippable;
        private CompInventory compInventory;

        private AmmoData mainAmmo;//exposed
        private AmmoData secondaryAmmo;//exposed

        private CompProperties_AmmoUser originAmmoProps = null;
        private CompProperties_AmmoUser secondaryAmmoProps = null;

        private VerbProperties mainVerbProps;
        private VerbProperties secondaryVerbProps;

        private bool useSecondaryAmmo;
        private bool isSharedAmmo;

        private CompCharges secondaryAmmoCharges = null;

        //----------------------- Methods ---------------------------------

        public void SwitchLauncher()
        {
            if (!useSecondaryAmmo)
            {
                SaveAmmo(MainAmmoData);
                LoadAmmo(SecondaryAmmoData);
                LoadAmmoProps(SecondaryAmmoSetData);
                CompEquippable.PrimaryVerb.verbProps = secondaryVerbProps;
                useSecondaryAmmo = true;
            }
            else
            {
                SaveAmmo(SecondaryAmmoData);
                LoadAmmo(MainAmmoData);
                LoadAmmoProps(OriginAmmoSetData);
                CompEquippable.PrimaryVerb.verbProps = mainVerbProps;
                useSecondaryAmmo = false;
            }
            if (CompEquippable.PrimaryVerb.verbProps.defaultProjectile.projectile.flyOverhead)
            {
                EnableChargeSpeed();
            }
            else
            {
                DisableChargeSpeed();
            }
            if (compAmmo.CurMagCount == 0 && compAmmo.IsEquippedGun)
            {
                TryReload();
            }
            if (Holder != null)
            {
                CompInventory.UpdateInventory();
            }
            return;
        }

        private void LoadAmmo(AmmoData data)
        {
            if (!HasAmmoUser || isSharedAmmo)
            {
                return;
            }
            CompAmmo.ResetAmmoCount(data.selectedAmmo);
            CompAmmo.CurMagCount = data.curMagCount;
            CompAmmo.SelectedAmmo = data.selectedAmmo;
        }

        private void SaveAmmo(AmmoData data)
        {
            if (!HasAmmoUser)
            {
                return;
            }
            data.curMagCount = CompAmmo.CurMagCount;
            data.selectedAmmo = CompAmmo.SelectedAmmo;
        }

        private void LoadAmmoProps(CompProperties_AmmoUser ammoProps)
        {
            if (!HasAmmoUser)
            {
                return;
            }
            CompAmmo.props = ammoProps;
        }


        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                PreSaveData();
            }

            Scribe_Values.Look<bool>(ref useSecondaryAmmo, "CE_useSecondaryAmmo", false);
            Scribe_Values.Look<bool>(ref useChargeSpeed, "useChargeSpeed", false);
            Scribe_Deep.Look<AmmoData>(ref mainAmmo, "CE_mainAmmoData", null);
            Scribe_Deep.Look<AmmoData>(ref secondaryAmmo, "CE_secondaryAmmoProps", null);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                PostAmmoDataLoaded();
            }
        }

        private void PreSaveData()
        {
            bool check = CompAmmo.Props == secondaryAmmoProps;

            if (!check)
            {
                MainAmmoData.curMagCount = CompAmmo.CurMagCount;
                MainAmmoData.selectedAmmo = CompAmmo.SelectedAmmo;

                return;
            }

            SecondaryAmmoData.curMagCount = CompAmmo.CurMagCount;
            SecondaryAmmoData.selectedAmmo = CompAmmo.SelectedAmmo;

            return;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (Props.secondaryAmmoProps == null)
            {
                Log.Error("Prop secondaryAmmoProps not initialized");
            }

            if (Props.secondaryVerb == null)
            {
                Log.Error("Prop secondaryVerb not initialized");
            }

            InitData();
        }

        private void PostAmmoDataLoaded()
        {

            InitData();

            if (useSecondaryAmmo)
            {
                LoadAmmo(SecondaryAmmoData);
                LoadAmmoProps(secondaryAmmoProps);
                CompEquippable.PrimaryVerb.verbProps = secondaryVerbProps;
            }
        }

        public void InitData()
        {
            mainVerbProps = CompEquippable.PrimaryVerb.verbProps;
            secondaryVerbProps = Props.secondaryVerb;
            if (secondaryAmmoCharges == null && Props.secondaryWeaponChargeSpeeds != null)
            {
                CompProperties_Charges chargesProps = new CompProperties_Charges();
                chargesProps.chargeSpeeds = Props.secondaryWeaponChargeSpeeds;
                secondaryAmmoCharges = new CompCharges();
                secondaryAmmoCharges.props = chargesProps;
            }

            if (HasAmmoUser)
            {
                if (originAmmoProps == null)
                {
                    originAmmoProps = CompAmmo.Props;
                }
                if (secondaryAmmoProps == null)
                {
                    secondaryAmmoProps = Props.secondaryAmmoProps;
                }
            }

            if (useChargeSpeed)
            {
                EnableChargeSpeed();
            }
            else
            {
                DisableChargeSpeed();
            }

            //Command Icon

            if (Props.mainCommandIcon == "")
            {
                Props.mainCommandIcon = "UI/Buttons/Reload";
            }

            if (Props.secondaryCommandIcon == "")
            {
                Props.secondaryCommandIcon = "UI/Buttons/Reload";
            }

            //Is shared ammo

            CompProperties_AmmoUser compProps = null;
            HashSet<AmmoDef> ammoCheck = new HashSet<AmmoDef>();
            isSharedAmmo = true;
            foreach (var comp in parent.def.comps)
            {
                compProps = comp as CompProperties_AmmoUser;
                if (compProps != null)
                {
                    break;
                }
            }

            if (compProps == null || Props.secondaryAmmoProps.ammoSet.ammoTypes.Count != compProps.ammoSet.ammoTypes.Count)
            {
                isSharedAmmo = false;
                return;
            }

            foreach (var ammo in Props.secondaryAmmoProps.ammoSet.ammoTypes)
            {
                ammoCheck.Add(ammo.ammo);
            }

            foreach (var ammo in compProps.ammoSet.ammoTypes)
            {
                if (!ammoCheck.Contains(ammo.ammo))
                {
                    isSharedAmmo = false;
                }
            }
        }

        public void SwitchToSecondary()
        {
            SaveAmmo(MainAmmoData);
            LoadAmmo(SecondaryAmmoData);
            LoadAmmoProps(SecondaryAmmoSetData);
            CompEquippable.PrimaryVerb.verbProps = secondaryVerbProps;
            useSecondaryAmmo = true;
            if (CompEquippable.PrimaryVerb.verbProps.defaultProjectile.projectile.flyOverhead)
            {
                EnableChargeSpeed();
            }
            else
            {
                DisableChargeSpeed();
            }
        }

        private void TryReload()
        {
            Job job = compAmmo.TryMakeReloadJob();
            if (job == null)
            {
                return;
            }
            job.playerForced = true;
            Pawn_JobTracker jobs = compAmmo.Holder.jobs;
            Job newJob = job;
            JobCondition lastJobEndCondition = JobCondition.InterruptForced;
            ThinkNode jobGiver = null;
            Job curJob = compAmmo.Holder.CurJob;
            jobs.StartJob(newJob, lastJobEndCondition, jobGiver, ((curJob != null) ? curJob.def : null) != job.def, true, null, null, false, false);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (CompAmmo != null && !CompAmmo.UseAmmo)
            {
                yield break;
            }

            if (Holder != null && (Holder.Faction == null || !Holder.Faction.Equals(Faction.OfPlayer)))
            {
                yield break;
            }
            yield return new Command_Action
            {
                action = new Action(SwitchLauncher),
                defaultDesc = Props.description,
                icon = ContentFinder<Texture2D>.Get(IsSecondaryAmmoSelected ? Props.secondaryCommandIcon : Props.mainCommandIcon, false),
                defaultLabel = (IsSecondaryAmmoSelected ? Props.secondaryWeaponLabel : Props.mainWeaponLabel),
            };
            if (IsSharedAmmo)
            {
                yield break;
            }
            yield return new GizmoSecondaryAmmoStatus
            {
                compAmmo = this
            };
            yield break;
        }

        private void EnableChargeSpeed()
        {
            if (secondaryAmmoCharges == null || AttackVerb == null)
            {
                return;
            }

            parent.AllComps.Add(secondaryAmmoCharges);
            AttackVerb.compCharges = secondaryAmmoCharges;
            useChargeSpeed = true;
        }

        private void DisableChargeSpeed()
        {
            if (secondaryAmmoCharges == null || AttackVerb == null)
            {
                return;
            }
            parent.AllComps.Remove(secondaryAmmoCharges);
            AttackVerb.compCharges = null;
            useChargeSpeed = false;
        }
    }

    public class CompProperties_SecondaryAmmo : CompProperties
    {
        public CompProperties_SecondaryAmmo()
        {
            compClass = typeof(CompSecondaryAmmo);
        }

        public CompProperties_AmmoUser secondaryAmmoProps;
        public VerbPropertiesCE secondaryVerb;
        public float loadedAmmoBulkFactor = 0f;
        public bool showSecondaryAmmoStat = true;

        public string mainCommandIcon = "";
        public string secondaryCommandIcon = "";

        [MustTranslate]
        public string mainWeaponLabel = "";
        [MustTranslate]
        public string secondaryWeaponLabel = "";
        [MustTranslate]
        public string description = "";

        public List<int> secondaryWeaponChargeSpeeds = new List<int>();
    }
}
