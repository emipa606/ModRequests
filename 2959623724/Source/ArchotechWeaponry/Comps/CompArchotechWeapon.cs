using System.Collections.Generic;
using ArchotechWeaponry.Defs.Traits;
using ArchotechWeaponry.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using HediffDefOf = ArchotechWeaponry.DefOfs.HediffDefOf;

namespace ArchotechWeaponry.Comps
{
    public class CompProperties_ArchotechWeapon : CompProperties
    {
        public string LethalIconPath = "UI/DamageIcon";
        public string NonLethalIconPath = "UI/PsychicIcon";
        public float SelfNecrosisSeverity = 0.25f;

        public CompProperties_ArchotechWeapon()
        {
            this.compClass = typeof(CompArchotechWeapon);
        }
    }
    
    public class CompArchotechWeapon : ThingComp
    {
        public CompProperties_ArchotechWeapon Props => (CompProperties_ArchotechWeapon)this.props;
        
        private Texture2D LethalTex
        {
            get { return ContentFinder<Texture2D>.Get(Props.LethalIconPath); }
        }
        
        private Texture2D NonLethalTex
        {
            get { return ContentFinder<Texture2D>.Get(Props.NonLethalIconPath); }
        }

        private bool _lethal = false;
        
        protected virtual Pawn GetWearer
        {
            get
            {
                if (ParentHolder != null && ParentHolder is Pawn_EquipmentTracker)
                {
                    return (Pawn)ParentHolder.ParentHolder;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool Lethal
        {
            get { return _lethal;}
            set { _lethal = value; }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (GetWearer != null && GetWearer.IsColonist && Find.Selector.SingleSelectedThing == GetWearer)
            {
                if (Lethal)
                {
                    yield return new Command_Action()
                    {
                        icon = this.LethalTex,
                        defaultLabel = "Switch Mode",
                        defaultDesc = "Switch to void toxin mode",
                        action = () =>
                        {
                            this.Lethal = false;
                        },
                    };
                }
                else
                {
                    yield return new Command_Action()
                    {
                        icon = this.NonLethalTex,
                        defaultLabel = "Switch Mode",
                        defaultDesc = "Switch to void necrosis mode",
                        action = () =>
                        {
                            this.Lethal = true;
                        },
                    };
                }
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (this.parent.TryGetComp<CompBladelinkWeapon>() is CompBladelinkWeapon compBladelinkWeapon)
            {
                if (compBladelinkWeapon.TraitsListForReading.Any(
                    trait => trait.HasModExtension<PrecognitionExtension>()))
                {
                    HediffUtils.AddOrUpdateHediffWithSeverity(pawn, HediffDefOf.VoidNecrosisPerm, null, Props.SelfNecrosisSeverity);
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            if (pawn.health.hediffSet.HasHediff(HediffDefOf.VoidNecrosisPerm))
            {
                pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.VoidNecrosisPerm));
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this._lethal, "lethal",false);
        }
    }
}