using Reload.Defs;
using Reload.UI;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Reload.Components
{
    public class CompReload : ThingComp, IVerbOwner
    {
        VerbTracker _verbTracker;

        public int remainingAmmo;

        public CompProperties_Reload Props => (CompProperties_Reload)this.props;

        public CompEquippable CompEquippable => parent.GetComp<CompEquippable>();

        public Pawn Holder => CompEquippable?.PrimaryVerb.CasterPawn;

        public int MagazineCapacity => Props.magazineCapacity;

        public bool CanBeUsed => remainingAmmo > 0;

        public string LabelRemaining => $"{remainingAmmo} / {MagazineCapacity}";

        public string LabelBaseReloadSeconds => Props.LabelReloadSeconds;

        public string LabelAdjustedReloadSeconds => $"{GetAdjustedReloadTime() / 60f:0.00}s";

        public List<VerbProperties> VerbProperties => parent.def.Verbs;

        public List<Tool> Tools => parent.def.tools;

        public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

        public Thing ConstantCaster => Holder;

        public VerbTracker VerbTracker
        {
            get
            {
                if (_verbTracker == null)
                {
                    _verbTracker = new VerbTracker(this);
                }

                return _verbTracker;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            remainingAmmo = MagazineCapacity;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref remainingAmmo, "remainingAmmo", 0);
        }
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            IEnumerable<StatDrawEntry> enumerable = base.SpecialDisplayStats();
            if (enumerable != null)
            {
                foreach (StatDrawEntry item in enumerable)
                {
                    yield return item;
                }
            }
            if(Setting.EnableAmmo)
                yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, "AmmoType".Translate(), Props.ammoDef.label, "", 10002);
            yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, "MagazineCapacity".Translate(), LabelRemaining, "", 10001);
            if(Props.loadsAmountPerReload > 0)
                yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, "LoadsAmountPerReload".Translate(), Props.loadsAmountPerReload.ToString(), "", 9999);
            yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, "ReloadTime".Translate(), LabelBaseReloadSeconds, "", 9998);
            yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, "MoveSpeedPenaltyWhileReloading".Translate(), $"{Props.moveSpeedPenalty}%", "", 9997);
        }
        public override void PostPostMake()
        {
            base.PostPostMake();
            remainingAmmo = 0;
            if (GenTicks.TicksGame <= 5)
            {
                remainingAmmo = MagazineCapacity;
            }
        }
        public override string CompInspectStringExtra()
        {
            string str = string.Empty;
            if (Setting.EnableAmmo)
                str += $"{Props.ammoDef.label} ";
            str += LabelRemaining;
            return str;
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!Setting.EnableAmmo)
                yield break;
            Action action = null;
            if (CanBeUsed)
                action = () => Unload(selPawn);
            var unload = new FloatMenuOption($"{"Unload".Translate()} {parent.LabelShort} {LabelRemaining}", action);
            yield return unload;
        }
        public string UniqueVerbOwnerID()
        {
            return "Reload_" + parent.ThingID;
        }
        public bool VerbsStillUsableBy(Pawn p)
        {
            return Holder == p;
        }
        public bool NeedsReload()
        {
            return MagazineCapacity != remainingAmmo;
        }
        public bool CanLoad()
        {
            if (!NeedsReload())
                return false;

            if (!Setting.EnableAmmo)
                return true;

            List<Thing> inventoryAmmo = Holder.inventory.innerContainer.Where(x => x.def == Props.ammoDef).ToList();
            if (inventoryAmmo.NullOrEmpty())
                return false;

            return true;
        }
        public Job MakeReloadJob()
        {
            return JobMaker.MakeJob(ReloadJobDefOf.R_Reload, Holder, parent);
        }
        public void Reload()
        {
            if (Holder?.CurJob?.def == ReloadJobDefOf.R_Reload)
                return;
            if (!CanLoad())
                return;

            if (Setting.EnableAmmo)
            {
                var inventoryAmmo = Holder.inventory.innerContainer.Where(x => x.def == Props.ammoDef).ToList();
                if (inventoryAmmo.NullOrEmpty())
                    return;
            }
            Job job = MakeReloadJob();
            Holder.jobs.StartJob(job, JobCondition.InterruptForced, null, Holder?.CurJob?.def != job.def, false, null, JobTag.Misc);
        }
        public void Unload(Pawn pawn)
        {
            if (!CanBeUsed)
                return;
            Job job = JobMaker.MakeJob(ReloadJobDefOf.R_Unload, parent);
            pawn.jobs.TryTakeOrderedJob(job);
        }
        public void LoadAmmo(int amount, bool force = false)
        {
            if (!Setting.EnableAmmo || force)
            {
                remainingAmmo = Math.Max(0, Math.Min(remainingAmmo + amount, MagazineCapacity));
                return;
            }

            List<Thing> inventoryAmmo = Holder.inventory.innerContainer.Where(x => x.def == Props.ammoDef).ToList();

            int ammoInInventory = inventoryAmmo.Sum(x => x.stackCount);
            int remain = Mathf.Min(ammoInInventory, Mathf.Min(amount, MagazineCapacity - remainingAmmo));
            if (remain > 0)
            {
                Holder.inventory.RemoveCount(Props.ammoDef, remain);
                remainingAmmo += remain;
            }
        }
        public void UsedOnce()
        {
            if(remainingAmmo > 0)
                remainingAmmo--;
        }
        public int GetAdjustedReloadTime()
        {
            int result = Props.baseReloadTicks;

            if(Setting.UseDynamicReloadTime)
            {
                float factorShooting = 1f;
                float factorManipulation = 1f;
                float factorConsciousnes = 1f;
                float factorQuality = 1f;
                QualityCategory qc = parent.TryGetQuality(out qc) ? qc : QualityCategory.Normal;

                if (Setting.UseDynamicReloadTime_Shooting && Holder?.skills != null)
                    factorShooting = Setting.ReloadSpeedModifier_Shooting[Holder.skills.GetSkill(SkillDefOf.Shooting).Level];

                if (Setting.UseDynamicReloadTime_Manipulation && Holder?.health != null)
                    factorManipulation = Mathf.Clamp(2f - Holder.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation), 0.5f, 2f);

                if(Setting.UseDynamicReloadTime_Consciousness && Holder?.health != null)
                    factorConsciousnes = Mathf.Clamp(2f - Holder.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness), 0.5f, 1.5f);

                if(Setting.UseDynamicReloadTime_Quality)
                    factorQuality = Mathf.Clamp(2f - Setting.ReloadSpeedModifier_Quality[qc], 0.8f, 1.2f);

                result = (int)(result * factorShooting * factorManipulation * factorConsciousnes * factorQuality);
            }
            return result;
        }
    }
}