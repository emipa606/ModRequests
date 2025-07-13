using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace MoonyRepair
{
    public class ModExtension_RepairReferences : DefModExtension
    {
        public JobDef repairJobDef;
    }

    public class JobDriver_RepairItem : JobDriver
    {
        RepairSettings settings = LoadedModManager.GetMod<RepairMod> ().GetSettings<RepairSettings> ();

        public override bool TryMakePreToilReservations (bool errorOnFailed)
        {
            return this.pawn.Reserve (this.job.targetA, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils ()
        {
            this.FailOnDespawnedNullOrForbidden (TargetIndex.A);
            yield return Toils_Goto.GotoThing (TargetIndex.A, PathEndMode.Touch);
            Toil repair = ToilMaker.MakeToil ("MakeNewToils");
            repair.initAction = delegate () {
                this.ticksToNextRepair = WarmupTicks;
            };
            repair.tickAction = delegate () {
                Pawn actor = repair.actor;
                actor.rotationTracker.FaceTarget (actor.CurJob.GetTarget (TargetIndex.A));
                float num;
                if (actor.kindDef == PawnKindDefOf.Colonist)
                {
                    actor.skills.Learn (SkillDefOf.Crafting, 0.05f, false);
                    num = actor.GetStatValue (StatDefOf.GeneralLaborSpeed, true, -1) * 1f;
                } else
                {
                    num = actor.GetStatValue (StatDefOf.WorkSpeedGlobal, true, -1) * 1f;
                }
                this.ticksToNextRepair -= num;
                if (this.ticksToNextRepair <= 0f)
                {
                    this.ticksToNextRepair += TicksBetweenRepairs;
                    this.TargetThingA.HitPoints++;
                    this.TargetThingA.HitPoints = Mathf.Min (this.TargetThingA.HitPoints, this.TargetThingA.MaxHitPoints);
                    //Messages.Message ("", MessageTypeDefOf.NeutralEvent);
                    if (this.TargetThingA.HitPoints == this.TargetThingA.MaxHitPoints) {
                        if (settings.removeTainted && TargetThingA.def.IsWithinCategory (ThingCategoryDefOf.Apparel)) {
                            Apparel apparelA = (Apparel) this.TargetThingA;
                            if (apparelA != null && apparelA.WornByCorpse) {
                                if (apparelA.Wearer != null)
                                    apparelA.Notify_PawnResurrected (apparelA.Wearer);
                                apparelA.WornByCorpse = false;
                            }
                        }
                        actor.records.Increment (RecordDefOf.ThingsRepaired);
                        actor.jobs.EndCurrentJob (JobCondition.Succeeded, true, true);
                    }
                }
            };
            repair.FailOnCannotTouch (TargetIndex.A, PathEndMode.Touch);
            repair.WithEffect (base.TargetThingA.def.repairEffect, TargetIndex.A, null);
            repair.defaultCompleteMode = ToilCompleteMode.Never;
            repair.activeSkill = (() => SkillDefOf.Crafting);
            repair.handlingFacing = true;
            yield return repair;
            yield break;
        }

        protected float ticksToNextRepair;

        private const float WarmupTicks = 80f;

        private const float TicksBetweenRepairs = 20f;
    }

    public class WorkGiver_RepairApparel : WorkGiver_Scanner
    {
        public ModExtension_RepairReferences Props => def.GetModExtension<ModExtension_RepairReferences> ();

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup (ThingRequestGroup.Apparel);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override Danger MaxPathDanger (Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal (Pawn pawn)
        {
            List<Thing> l = pawn.Map.listerThings.ThingsMatching (PotentialWorkThingRequest);
            return l;
        }

        public override bool ShouldSkip (Pawn pawn, bool forced = false)
        {
            List<Thing> l = pawn.Map.listerThings.ThingsMatching (PotentialWorkThingRequest);
            return l.Count == 0;
        }

        public override bool HasJobOnThing (Pawn pawn, Thing t, bool forced = false)
        {
            if (!RepairUtility.PawnCanRepairNow (pawn, t))
            {
                return false;
            }
            if (pawn.Faction == Faction.OfPlayer && !pawn.Map.areaManager.Home[t.Position])
            {
                return false;
            }
            return pawn.CanReserve (t, 1, -1, null, forced) && !t.IsBurning ();
        }

        public override Job JobOnThing (Pawn pawn, Thing t, bool forced = false)
        {
            if (Props.repairJobDef != null && t != null)
                return JobMaker.MakeJob (Props.repairJobDef, t);
            return null;
        }
    }

    public class WorkGiver_RepairWeapon : WorkGiver_RepairApparel
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup (ThingRequestGroup.Weapon);
            }
        }
    }

    public class RepairUtility
    {


        public static bool PawnCanRepairNow (Pawn pawn, Thing t)
        {
            RepairSettings settings = LoadedModManager.GetMod<RepairMod> ().GetSettings<RepairSettings> ();

            bool tIsWeapon = t.def.IsWithinCategory (ThingCategoryDefOf.Weapons);
            bool tIsArmour = t.def.IsWithinCategory (ThingCategoryDefOf.ApparelArmor) || t.def.IsWithinCategory (ThingCategoryDefOf.ArmorHeadgear);
            bool tIsClothing = t.def.IsWithinCategory (ThingCategoryDefOf.Apparel) && !tIsArmour;

            bool tIsTainted = (tIsArmour || tIsClothing) && ((Apparel) t).WornByCorpse;
            bool tIsBiocoded = false;


            bool ans = PawnCanRepairEver (t, tIsWeapon, tIsArmour, tIsClothing, tIsTainted, tIsBiocoded, settings);

            ans &= pawn.Map.listerThings.ThingsInGroup (ThingRequestGroup.Weapon).Contains (t) || pawn.Map.listerThings.ThingsInGroup (ThingRequestGroup.Apparel).Contains (t);

            QualityCategory quality;
            if (t.TryGetQuality (out quality) == false)
            {
                if (!settings.repairWithoutQuality)
                    ans = false;
            } else
                ans &= quality >= settings.qualityThreshold;

            ans &= ((float) t.HitPoints / (float) t.MaxHitPoints) < settings.repairThreshold || settings.repairTainted && settings.removeTainted && tIsTainted;

            return ans;
        }

        public static bool PawnCanRepairEver (Thing t, bool tIsWeapon, bool tIsArmour, bool tIsClothing, bool tIsTainted, bool tIsBiocoded, RepairSettings settings)
        {
            if (((ThingWithComps) t).GetComp<CompBiocodable> () != null)
                tIsBiocoded = tIsWeapon && ((ThingWithComps) t).GetComp<CompBiocodable> ().Biocoded;

            bool ans = false;
            if (tIsWeapon)
                if (tIsBiocoded)
                    ans = settings.repairWeapons && settings.repairBiocoded;
                else
                    ans = settings.repairWeapons;
            else if (tIsArmour)
                if (tIsTainted)
                    ans = settings.repairArmour && settings.repairTainted;
                else
                    ans = settings.repairArmour;
            else if (tIsClothing)
                if (tIsTainted)
                    ans = settings.repairClothing && settings.repairTainted;
                else
                    ans = settings.repairClothing;

            ans &= t.def.useHitPoints;

            //Messages.Message (tIsWeapon.ToString () + " isweapon\n" + tIsArmour.ToString () + " isarmour\n" + tIsClothing.ToString () + " isclothing\n" + tIsTainted.ToString () + " istainted\n" + tIsBiocoded.ToString () + " isbiocoded\n" + ans.ToString() + " ans", MessageTypeDefOf.NeutralEvent);

            return ans;
        }
    }

    public class RepairSettings : ModSettings
    {
        public bool repairWeapons = true;
        public bool repairClothing = true;
        public bool repairArmour = true;
        public bool repairTainted = false;
        public bool repairBiocoded = false;
        public bool removeTainted = false;
        public float repairThreshold = 1f;
        public bool repairWithoutQuality = true;
        public QualityCategory qualityThreshold;

        public override void ExposeData ()
        {
            Scribe_Values.Look (ref repairWeapons, "repairWeapons", true);
            Scribe_Values.Look (ref repairClothing, "repairClothing", true);
            Scribe_Values.Look (ref repairArmour, "repairArmour", true);
            Scribe_Values.Look (ref repairTainted, "repairTainted", false);
            Scribe_Values.Look (ref repairBiocoded, "repairBiocoded", false);
            Scribe_Values.Look (ref removeTainted, "removeTainted", false);
            Scribe_Values.Look (ref repairThreshold, "repairThreshold", 1f);
            Scribe_Values.Look (ref qualityThreshold, "qualityThreshhold", QualityCategory.Awful);
            Scribe_Values.Look (ref repairWithoutQuality, "repairWithoutQuality", true);
            base.ExposeData ();
        }
    }

    public class RepairMod : Mod
    {

        RepairSettings settings;

        public RepairMod (ModContentPack content) : base (content)
        {
            this.settings = GetSettings<RepairSettings> ();
        }

        public override void DoSettingsWindowContents (Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard ();
            inRect.width = inRect.width / 3;
            listingStandard.Begin (inRect);

            listingStandard.CheckboxLabeled ("Allow Weapons:", ref settings.repairWeapons, "Will repair valid weapons.");
            listingStandard.CheckboxLabeled ("Allow Clothing:", ref settings.repairClothing, "Will repair valid clothing.");
            listingStandard.CheckboxLabeled ("Allow Armour:", ref settings.repairArmour, "Will repair valid armour.");
            listingStandard.Gap ();
            listingStandard.CheckboxLabeled ("Allow Tainted:", ref settings.repairTainted, "Tainted items can be valid targets.");
            listingStandard.CheckboxLabeled ("Allow Biocoded:", ref settings.repairBiocoded, "Biocoded items can be valid targets.");
            listingStandard.Gap ();
            listingStandard.CheckboxLabeled ("Allow Items without Quality:", ref settings.repairWithoutQuality, "Items without quality can be valid targets.");
            listingStandard.LabelDouble ("Quality Threshold:", settings.qualityThreshold.ToString (), "Items above this quality threshold will be valid targets. Items without a quality will always be valid repair targets.");
            settings.qualityThreshold = (QualityCategory) Mathf.RoundToInt (listingStandard.Slider ((float) settings.qualityThreshold, 0f, 6f));
            listingStandard.Gap ();
            listingStandard.LabelDouble ("Damage Threshold:", (100 * settings.repairThreshold).ToString () + "%", "Items below this percentage threshold will be valid targets.");
            settings.repairThreshold = (Mathf.Round (listingStandard.Slider (settings.repairThreshold, 0f, 1f) * 100f)) / 100f;
            listingStandard.Gap ();
            listingStandard.CheckboxLabeled ("Repairing Removes Tainted:", ref settings.removeTainted, "Repairing a tainted item removes the tainted status.");

            //ThingFilter filter = new ThingFilter ();
            //ThingFilterUI.DoThingFilterConfigWindow (inRect, new ThingFilterUI.UIState (), filter);

            listingStandard.End ();
            base.DoSettingsWindowContents (inRect);
        }

        public override string SettingsCategory ()
        {
            return "Repair Items";
        }
    }
}