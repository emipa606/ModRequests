using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class CompSecondaryVerb : CompEquippedGizmo
    {
        private Verb verbInt;

        private CompEquippable compEquippableInt;
        private bool isSecondaryVerbSelected;

        public CompProperties_SecondaryVerb Props => (CompProperties_SecondaryVerb)props;

        public bool IsSecondaryVerbSelected => isSecondaryVerbSelected;

        public Verb secondaryVerb;

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

        public Pawn CasterPawn => Verb.caster as Pawn;

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

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (CasterPawn == null || CasterPawn.Faction.Equals(Faction.OfPlayer))
            {
                string text = (IsSecondaryVerbSelected ? Props.secondaryCommandIcon : Props.mainCommandIcon);
                if (text == "")
                {
                    text = "UI/Buttons/Reload";
                }
                yield return new Command_Action
                {
                    action = SwitchVerb,
                    defaultLabel = (IsSecondaryVerbSelected ? Props.secondaryWeaponLabel : Props.mainWeaponLabel),
                    defaultDesc = Props.description,
                    icon = ContentFinder<Texture2D>.Get(text, reportFailure: false)
                };
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosEquipped()
        {
            if (CasterPawn == null || CasterPawn.Faction.Equals(Faction.OfPlayer))
            {
                string text = (IsSecondaryVerbSelected ? Props.secondaryCommandIcon : Props.mainCommandIcon);
                if (text == "")
                {
                    text = "UI/Buttons/Reload";
                }
                yield return new Command_Action
                {
                    action = SwitchVerb,
                    defaultLabel = (IsSecondaryVerbSelected ? Props.secondaryWeaponLabel : Props.mainWeaponLabel),
                    defaultDesc = Props.description,
                    icon = ContentFinder<Texture2D>.Get(text, reportFailure: false)
                };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isSecondaryVerbSelected, "PLA_useSecondaryVerb", defaultValue: false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                PostAmmoDataLoaded();
            }
        }

        public void SwitchToSecondary()
        {
            EquipmentSource.PrimaryVerb.verbProps = Props.verbProps;
            isSecondaryVerbSelected = true;
        }

        private void SwitchVerb()
        {
            /*
            Verb verb = (Verb)Activator.CreateInstance(Props.verbProps.verbClass);
            string text = Verb.CalculateUniqueLoadID(EquipmentSource, 114514);
            verb.loadID = text;
            verb.verbProps = Props.verbProps;
            verb.verbTracker = EquipmentSource.verbTracker;
            verb.caster = Verb.caster;
            verb.tool = null;
            verb.maneuver = null;
            Log.Message(verb.ToString());
            secondaryVerb = verb;
            */
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
        }

        private void PostAmmoDataLoaded()
        {
            if (isSecondaryVerbSelected)
            {
                EquipmentSource.PrimaryVerb.verbProps = Props.verbProps;
            }
        }
    }

    public class CompProperties_SecondaryVerb : CompProperties
    {
        public VerbProperties verbProps = new VerbProperties();

        public string mainCommandIcon = "UI/Icons/FireModes/BDP_Overcharge_Off";

        public string mainWeaponLabel = "BDP_MainWeaponLabelVanilla";

        public string secondaryCommandIcon = "UI/Icons/FireModes/BDP_Overcharge_On";

        public string secondaryWeaponLabel = "BDP_SecondaryWeaponLabelVanilla";

        public string description = "BDP_WeaponDescVanilla";

        public CompProperties_SecondaryVerb()
        {
            compClass = typeof(CompSecondaryVerb);
        }
    }
}
