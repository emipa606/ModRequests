using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class CompSecondaryVerbCE : CompRangedGizmoGiver
    {
        public CompProperties_SecondaryVerbCE Props
        {
            get
            {
                return (CompProperties_SecondaryVerbCE)props;
            }
        }

        public bool IsSecondaryVerbSelected
        {
            get
            {
                return isSecondaryVerbSelected;
            }
        }

        public Verb_LaunchProjectileCE AttackVerb
        {
            get
            {
                if (EquipmentSource.PrimaryVerb == null)
                {
                    return null;
                }

                Verb_LaunchProjectileCE verb = EquipmentSource.PrimaryVerb as Verb_LaunchProjectileCE;
                return verb;
            }
        }

        private CompEquippable EquipmentSource
        {
            get
            {
                if (compEquippableInt != null)
                {
                    return compEquippableInt;
                }
                compEquippableInt = parent.TryGetComp<CompEquippable>();
                if (compEquippableInt == null)
                {
                    Log.ErrorOnce(parent.LabelCap + " has CompSecondaryVerb but no CompEquippable", 50020);

                }
                return compEquippableInt;
            }
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
                if (verbInt == null)
                {
                    verbInt = EquipmentSource.PrimaryVerb;
                }
                return verbInt;
            }
        }

        private Verb mainVerb;
        private Verb secondaryVerb;

        private CompCharges secondaryAmmoCharges = null;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            InitData();
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            InitData();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (CasterPawn != null && (CasterPawn.Faction == null || !CasterPawn.Faction.Equals(Faction.OfPlayer)))
            {
                yield break;
            }


            string commandIcon = IsSecondaryVerbSelected ? Props.secondaryCommandIcon : Props.mainCommandIcon;

            if (commandIcon == "")
            {
                commandIcon = "UI/Buttons/Reload";
            }

            Command_Action switchSecondaryLauncher = new Command_Action
            {
                action = new Action(SwitchVerb),
                defaultLabel = (IsSecondaryVerbSelected ? Props.secondaryWeaponLabel : Props.mainWeaponLabel),
                defaultDesc = Props.description,
                icon = ContentFinder<Texture2D>.Get(commandIcon, false),
                //tutorTag = "Switch between rifle and grenade launcher"
            };
            yield return switchSecondaryLauncher;

            yield break;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look<bool>(ref isSecondaryVerbSelected, "PLA_useSecondaryVerb", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                PostAmmoDataLoaded();
            }
        }
        public void SwitchToSecondary()
        {
            EquipmentSource.PrimaryVerb.verbProps = Props.verbProps;
            isSecondaryVerbSelected = true;
            if (EquipmentSource.PrimaryVerb.verbProps.defaultProjectile.projectile.flyOverhead)
            {
                EnableChargeSpeed();
            }
            else
            {
                DisableChargeSpeed();
            }
        }
        public void SwitchVerb()
        {
            if (!IsSecondaryVerbSelected)
            {

                EquipmentSource.PrimaryVerb.verbProps = Props.verbProps;
                isSecondaryVerbSelected = true;
            }
            else
            {
                EquipmentSource.PrimaryVerb.verbProps = parent.def.Verbs[0];
                isSecondaryVerbSelected = false;
            }
            if (EquipmentSource.PrimaryVerb.verbProps.defaultProjectile.projectile.flyOverhead)
            {
                EnableChargeSpeed();
            }
            else
            {
                DisableChargeSpeed();
            }
        }

        private void PostAmmoDataLoaded()
        {

            InitData();

            if (isSecondaryVerbSelected)
            {
                EquipmentSource.verbTracker.AllVerbs.Replace(EquipmentSource.PrimaryVerb, secondaryVerb);
            }
        }

        public void InitData()
        {
            mainVerb = EquipmentSource.PrimaryVerb;
            secondaryVerb = (Verb)Activator.CreateInstance(Props.verbProps.verbClass);
            secondaryVerb.verbProps = Props.verbProps;
            secondaryVerb.verbTracker = new VerbTracker(EquipmentSource);
            secondaryVerb.caster = mainVerb.Caster;
            secondaryVerb.castCompleteCallback = mainVerb.castCompleteCallback;
            if (this.secondaryAmmoCharges == null && this.Props.secondaryWeaponChargeSpeeds != null)
            {
                CompProperties_Charges chargesProps = new CompProperties_Charges();
                chargesProps.chargeSpeeds = this.Props.secondaryWeaponChargeSpeeds;
                this.secondaryAmmoCharges = new CompCharges();
                this.secondaryAmmoCharges.props = chargesProps;
            }
        }

        private void EnableChargeSpeed()
        {
            if (this.secondaryAmmoCharges == null || this.AttackVerb == null)
            {
                return;
            }

            this.parent.AllComps.Add(this.secondaryAmmoCharges);
            this.AttackVerb.compCharges = this.secondaryAmmoCharges;
        }

        private void DisableChargeSpeed()
        {
            if (this.secondaryAmmoCharges == null || this.AttackVerb == null)
            {
                return;
            }
            this.parent.AllComps.Remove(this.secondaryAmmoCharges);
            this.AttackVerb.compCharges = null;
        }

        private Verb verbInt = null;
        private CompEquippable compEquippableInt;
        private bool isSecondaryVerbSelected;
    }

    public class CompProperties_SecondaryVerbCE : CompProperties
    {
        public CompProperties_SecondaryVerbCE()
        {
            compClass = typeof(CompSecondaryVerbCE);
        }

        public VerbPropertiesCE verbProps = new VerbPropertiesCE();

        public string mainCommandIcon = "";

        [MustTranslate]
        public string mainWeaponLabel = "";
        public string secondaryCommandIcon = "";

        [MustTranslate]
        public string secondaryWeaponLabel = "";
        [MustTranslate]
        public string description = "";

        public List<int> secondaryWeaponChargeSpeeds = new List<int>();
    }
}
