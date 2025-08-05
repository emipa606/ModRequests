using Verse;
using CombatExtended;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;
using System;
using Verse.AI;
using PipeSystem;
using RimWorld.Utility;

namespace BDsPlasmaWeapon
{
    public class DefModExtension_TankQualityModifier : DefModExtension
    {
        public float Awful = 1;
        public float Poor = 1;
        public float Good = 1;
        public float Excellent = 1;
        public float Masterwork = 1;
        public float Legendary = 1;
    }
    public class CompReloadableFromFiller : CompRangedGizmoGiver, IVerbOwner, ICompWithCharges
    {
        public int remainingCharges;

        private VerbTracker verbTracker;

        public CompProperties_ReloadableFromFiller Props => props as CompProperties_ReloadableFromFiller;

        public DefModExtension_ActiveVent compActiveVentData => parent.def.GetModExtension<DefModExtension_ActiveVent>();

        public DefModExtension_GasJump compGasJumpData => parent.def.GetModExtension<DefModExtension_GasJump>();

        public DefModExtension_TankQualityModifier qualityModifier => parent.def.GetModExtension<DefModExtension_TankQualityModifier>();

        public int MaxCharges
        {
            get
            {
                return (int)Math.Round(Props.maxCharges * QualityMultiplier);
            }
        }

        public float QualityMultiplier
        {
            get
            {
                CompQuality compQuality = parent.TryGetComp<CompQuality>();
                if (compQuality == null || qualityModifier == null)
                {
                    return 1;
                }
                switch (compQuality.Quality)
                {
                    case QualityCategory.Awful:
                        return qualityModifier.Awful;
                    case QualityCategory.Poor:
                        return qualityModifier.Poor;
                    case QualityCategory.Good:
                        return qualityModifier.Good;
                    case QualityCategory.Excellent:
                        return qualityModifier.Excellent;
                    case QualityCategory.Masterwork:
                        return qualityModifier.Masterwork;
                    case QualityCategory.Legendary:
                        return qualityModifier.Legendary;
                    default:
                        return 1;
                }
            }
        }

        public ThingDef AmmoDef => Props.ammoDef;

        public bool CanBeUsed => remainingCharges > 0;

        private int KeepDisplayingTicks = 1000;

        private int lastKeepDisplayTick = -9999;

        public Pawn Wearer
        {
            get
            {
                if (ParentHolder is Pawn_ApparelTracker pawn_ApparelTracker)
                {
                    return pawn_ApparelTracker.pawn;
                }
                return null;
            }
        }

        private void SetupVerbs()
        {
            for (int i = 0; i < AllVerbs.Count; i++)
            {
                AllVerbs[i].caster = Wearer;
            }
        }

        public List<VerbProperties> VerbProperties => parent.def.Verbs;

        public List<Tool> Tools => parent.def.tools;

        public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

        public Thing ConstantCaster => Wearer;

        public VerbTracker VerbTracker
        {
            get
            {
                if (verbTracker == null)
                {
                    verbTracker = new VerbTracker(this);
                }
                return verbTracker;
            }
        }

        public Pawn CasterPawn
        {
            get
            {
                return Verb.CasterPawn;
            }
        }

        private Verb Verb
        {
            get
            {
                if (EquipmentSource != null)
                {
                    return EquipmentSource.PrimaryVerb;
                }
                return VerbTracker.PrimaryVerb;
            }
        }


        private CompEquippable EquipmentSource
        {
            get
            {
                return parent.TryGetComp<CompEquippable>();
            }
        }

        public string LabelRemaining => $"{remainingCharges} / {MaxCharges}";

        public List<Verb> AllVerbs => VerbTracker.AllVerbs;

        public string UniqueVerbOwnerID()
        {
            return "Reloadable_" + parent.ThingID;
        }

        public bool VerbsStillUsableBy(Pawn p)
        {
            return Wearer == p;
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            remainingCharges = 0;
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            CompTankFeedWeapon compWeapon = pawn.equipment.Primary.TryGetComp<CompTankFeedWeapon>();
            if (compWeapon != null)
            {
                compWeapon.Notify_Equipped(pawn);
            }
            SetupVerbs();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            CompTankFeedWeapon compWeapon = pawn.equipment.Primary.TryGetComp<CompTankFeedWeapon>();
            if (compWeapon != null)
            {
                compWeapon.compReloadableFromFiller = null;
                compWeapon.searchTank(1, false);
            }
            SetupVerbs();
        }
        public override string CompInspectStringExtra()
        {
            return "ChargesRemaining".Translate(Props.ChargeNounArgument) + ": " + LabelRemaining;
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            CompQuality compQuality = parent.TryGetComp<CompQuality>();
            String s = "BDP_LizionTankCapacityDesc".Translate(MaxCharges);
            if (compQuality != null && qualityModifier != null)
            {
                s = "BDP_LizionTankCapacityDescComplex".Translate(MaxCharges, QualityMultiplier, Props.maxCharges);
            }
            yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "BDP_LizionTankCapacity".Translate(), MaxCharges.ToString(), s, 2749);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            SetupVerbs();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref remainingCharges, "remainingCharges", -999);
            Scribe_Deep.Look(ref verbTracker, "verbTracker", this);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                SetupVerbs();
                if (RemainingCharges == -999)
                {
                    remainingCharges = MaxCharges;
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetWornGizmosExtra())
            {
                yield return item;
            }
            bool drafted = Wearer.Drafted;
            if (Wearer.Faction != Faction.OfPlayer || (drafted && !Props.displayGizmoWhileDrafted) || (!drafted && !Props.displayGizmoWhileUndrafted))
            {
                yield break;
            }
            ThingWithComps gear = parent;


            //hardcoded for lizion backpack
            foreach (Verb allVerb in VerbTracker.AllVerbs)
            {
                if (allVerb is Verb_ActiveVent && compActiveVentData != null)
                {
                    yield return CreateActiveVentTargetCommand(allVerb as Verb_ActiveVent, compActiveVentData);
                }
                if (allVerb is Verb_GasJump && compGasJumpData != null)
                {
                    yield return CreateGasJumpTargetCommand(allVerb as Verb_GasJump, compGasJumpData);
                }
            }


            if (Prefs.DevMode)
            {
                Command_Action command_Action = new Command_Action
                {
                    defaultLabel = "Debug: Reload to full",
                    action = delegate
                    {
                        remainingCharges = MaxCharges;
                    }
                };
                yield return command_Action;
                Command_Action command_Action2 = new Command_Action
                {
                    defaultLabel = "Debug: Empty",
                    action = delegate
                    {
                        remainingCharges = 0;
                    }
                };
                yield return command_Action2;
            }

            if (Find.Selector.SingleSelectedThing == Wearer)
            {
                Gizmo_LizionTankStatus gizmo_EnergyShieldStatus = new Gizmo_LizionTankStatus();
                gizmo_EnergyShieldStatus.filler = this;
                yield return gizmo_EnergyShieldStatus;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (Prefs.DevMode)
            {
                Command_Action command_Action = new Command_Action
                {
                    defaultLabel = "Debug: Reload to full",
                    action = delegate
                    {
                        remainingCharges = MaxCharges;
                    }
                };
                yield return command_Action;
                Command_Action command_Action2 = new Command_Action
                {
                    defaultLabel = "Debug: Empty",
                    action = delegate
                    {
                        remainingCharges = 0;
                    }
                };
                yield return command_Action2;
            }
            if (CasterPawn != null)
            {
                bool drafted = CasterPawn.Drafted;
                if ((drafted && !Props.displayGizmoWhileDrafted) || (!drafted && !Props.displayGizmoWhileUndrafted))
                {
                    yield break;
                }
                if (Find.Selector.SingleSelectedThing == CasterPawn)
                {
                    Gizmo_LizionTankStatus gizmo_EnergyShieldStatus = new Gizmo_LizionTankStatus();
                    gizmo_EnergyShieldStatus.filler = this;
                    yield return gizmo_EnergyShieldStatus;
                }
            }
            if (Find.Selector.SingleSelectedThing == parent)
            {
                Gizmo_LizionTankStatus gizmo_EnergyShieldStatus = new Gizmo_LizionTankStatus();
                gizmo_EnergyShieldStatus.filler = this;
                yield return gizmo_EnergyShieldStatus;
            }
        }





        private Command_ReloadableFromFiller CreateActiveVentTargetCommand(Verb_ActiveVent verb, DefModExtension_ActiveVent compActiveVentData)
        {
            Command_ReloadableFromFiller command_Reloadable = new Command_ReloadableFromFiller(this);
            command_Reloadable.defaultDesc = compActiveVentData.description.Translate();
            command_Reloadable.defaultLabel = compActiveVentData.label.Translate();
            command_Reloadable.verb = verb;
            command_Reloadable.icon = ContentFinder<Texture2D>.Get(compActiveVentData.icon, false);
            if (!Wearer.IsColonistPlayerControlled)
            {
                command_Reloadable.Disable();
            }
            else if (verb.verbProps.violent && Wearer.WorkTagIsDisabled(WorkTags.Violent))
            {
                command_Reloadable.Disable("IsIncapableOfViolenceLower".Translate(Wearer.LabelShort, Wearer).CapitalizeFirst() + ".");
            }
            else if (!CanBeUsed)
            {
                command_Reloadable.Disable("CommandReload_NoCharges".Translate(Props.ChargeNounArgument));
            }
            return command_Reloadable;
        }

        private Command_ReloadableFromFiller CreateGasJumpTargetCommand(Verb_GasJump verb, DefModExtension_GasJump compGasJumoData)
        {
            Command_ReloadableFromFiller command_Reloadable = new Command_ReloadableFromFiller(this);
            command_Reloadable.defaultDesc = compGasJumoData.description.Translate();
            command_Reloadable.defaultLabel = compGasJumoData.label.Translate();
            command_Reloadable.verb = verb;
            command_Reloadable.icon = ContentFinder<Texture2D>.Get(compGasJumoData.icon, false);
            if (!Wearer.IsColonistPlayerControlled)
            {
                command_Reloadable.Disable();
            }
            else if (verb.verbProps.violent && Wearer.WorkTagIsDisabled(WorkTags.Violent))
            {
                command_Reloadable.Disable("IsIncapableOfViolenceLower".Translate(Wearer.LabelShort, Wearer).CapitalizeFirst() + ".");
            }
            else if (!CanBeUsed)
            {
                command_Reloadable.Disable("CommandReload_NoCharges".Translate(Props.ChargeNounArgument));
            }
            return command_Reloadable;
        }

        public string DisabledReason(int minNeeded, int maxNeeded)
        {
            if (AmmoDef == null)
            {
                return "CommandReload_NoCharges".Translate(Props.ChargeNounArgument);
            }
            return TranslatorFormattedStringExtensions.Translate(arg3: ((Props.ammoCountToRefill == 0) ? ((minNeeded == maxNeeded) ? minNeeded.ToString() : $"{minNeeded}-{maxNeeded}") : Props.ammoCountToRefill.ToString()).Named("COUNT"), key: "CommandReload_NoAmmo", arg1: Props.ChargeNounArgument, arg2: NamedArgumentUtility.Named(AmmoDef, "AMMO"));
        }

        public bool NeedsReload(bool allowForcedReload)
        {
            if (AmmoDef == null)
            {
                return false;
            }
            if (Props.ammoCountToRefill != 0)
            {
                if (!allowForcedReload)
                {
                    return remainingCharges == 0;
                }
                return remainingCharges != MaxCharges;
            }
            return remainingCharges != MaxCharges;
        }

        public void ReloadFrom(Thing ammo)
        {
            if (!NeedsReload(allowForcedReload: true))
            {
                return;
            }
            if (Props.ammoCountToRefill != 0)
            {
                if (ammo.stackCount < Props.ammoCountToRefill)
                {
                    return;
                }
                ammo.SplitOff(Props.ammoCountToRefill).Destroy();
                remainingCharges = MaxCharges;
            }
            else
            {
                if (ammo.stackCount < Props.ammoCountPerCharge)
                {
                    return;
                }
                int num = Mathf.Clamp(ammo.stackCount / Props.ammoCountPerCharge, 0, emptySpace);
                ammo.SplitOff(num * Props.ammoCountPerCharge).Destroy();
                remainingCharges += num;
            }
            if (Props.soundReload != null)
            {
                Props.soundReload.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
            }
        }

        public int MinAmmoNeeded(bool allowForcedReload)
        {
            if (!NeedsReload(allowForcedReload))
            {
                return 0;
            }
            if (Props.ammoCountToRefill != 0)
            {
                return Props.ammoCountToRefill;
            }
            return Props.ammoCountPerCharge;
        }

        public int MaxAmmoNeeded(bool allowForcedReload)
        {
            if (!NeedsReload(allowForcedReload))
            {
                return 0;
            }
            if (Props.ammoCountToRefill != 0)
            {
                return Props.ammoCountToRefill;
            }
            return Props.ammoCountPerCharge * (emptySpace);
        }

        public int emptySpace => MaxCharges - remainingCharges;

        public int RemainingCharges => remainingCharges;

        public int MaxAmmoAmount()
        {
            if (AmmoDef == null)
            {
                return 0;
            }
            if (Props.ammoCountToRefill == 0)
            {
                return Props.ammoCountPerCharge * MaxCharges;
            }
            return Props.ammoCountToRefill;
        }

        public void UsedOnce()
        {
            if (remainingCharges > 0)
            {
                remainingCharges--;
            }
            if (Props.destroyOnEmpty && remainingCharges == 0 && !parent.Destroyed)
            {
                parent.Destroy();
            }
        }

        public void DrawGas(int amount)
        {
            IntVec3 pos = parent.Position;

            if (amount <= 0)
            {
                Log.Error("tried to draw zero or negative amount of gas from CompReloadableFromFiller");
            }
            Map map = parent.Map;
            if (VerbTracker != null)
            {
                map = Verb.caster.Map;
                pos = Verb.caster.Position;
            }
            if (Wearer != null)
            {
                map = Wearer.Map;
                pos = Wearer.Position;
            }
            TargetInfo target = new TargetInfo(pos, map, true);
            if (amount < remainingCharges)
            {
                remainingCharges -= amount;
                for (int i = 0; i < amount; i++)
                {
                    Effecter effecter = BDStatDefOf.LizionCoolerHigh.Spawn(pos, map);
                    effecter.Trigger(target, target);
                }
            }
            else
            {
                amount = remainingCharges;
                remainingCharges = 0;
                ThingDefOf.BDP_HissOneShot.PlayOneShot(parent);
                if (parent.ParentHolder is Pawn pawn)
                {
                    Messages.Message(string.Format("BDP_LizionTankDepletedWithPawn".Translate(), parent.LabelCap, pawn.Name, pawn), parent, MessageTypeDefOf.RejectInput, historical: false);
                }
                else
                {
                    Messages.Message(string.Format("BDP_LizionTankDepleted".Translate(), parent.LabelCap), parent, MessageTypeDefOf.RejectInput, historical: false);
                }
                for (int i = 0; i < amount; i++)
                {
                    Effecter effecter = BDStatDefOf.LizionCoolerHigh.Spawn(pos, map);
                    effecter.Trigger(target, target);
                }
            }
        }

        public void DrawGas(float amount)
        {
            DrawGas((int)amount);
        }

        public void Empty()
        {
            DrawGas(remainingCharges);
        }

        public void RefillGas(int amount)
        {
            if (amount <= 0)
            {
                Log.Error("tried to refill zero or negative amount of gas from CompReloadableFromFiller");
            }
            if (remainingCharges < MaxCharges)
            {
                if (remainingCharges + amount > MaxCharges)
                {
                    amount = emptySpace;
                }
                ThingDefOf.BDP_HissOneShot.PlayOneShot(parent);
                remainingCharges += amount;
                Effecter effecter = BDStatDefOf.LizionCoolerLow.Spawn(parent.Position, parent.Map);
                effecter.Trigger(parent, TargetInfo.Invalid);
            }
        }

        public void RefillGas(float amount)
        {
            RefillGas((int)amount);
        }

        bool ICompWithCharges.CanBeUsed(out string reason)
        {
            reason = string.Format("BDP_LizionTankDepleted".Translate(), parent.LabelCap);
            return CanBeUsed;
        }
    }

    public class Command_ReloadableFromFiller : Command_VerbTarget
    {
        private readonly CompReloadableFromFiller comp;

        public Color? overrideColor;

        public override string TopRightLabel => comp.LabelRemaining;

        public override Color IconDrawColor => overrideColor ?? base.IconDrawColor;

        public Command_ReloadableFromFiller(CompReloadableFromFiller comp)
        {
            this.comp = comp;
        }

        public override void GizmoUpdateOnMouseover()
        {
            verb.DrawHighlight(LocalTargetInfo.Invalid);
        }
        public override bool GroupsWith(Gizmo other)
        {
            if (!base.GroupsWith(other))
            {
                return false;
            }
            if (!(other is Command_ReloadableFromFiller command_Reloadable))
            {
                return false;
            }
            return comp.parent.def == command_Reloadable.comp.parent.def;
        }
    }
    public class JobGiver_ReloadFromFiller : ThinkNode_JobGiver
    {
        private const bool forceReloadWhenLookingForWork = true;
        public override float GetPriority(Pawn pawn)
        {
            return 5.9f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            ThingWithComps Thing = FindSomeReloadableComponent(pawn, allowForcedReload: false);

            if (Thing == null || Thing.TryGetComp<CompReloadableFromFiller>() == null)
            {
                return null;
            }
            Thing filler = FindFiller(pawn);
            if (filler == null)
            {
                return null;
            }
            return MakeReloadJob(Thing, filler);
        }

        public static Job MakeReloadJob(ThingWithComps thing, Thing chosenFiller)
        {
            Job job = JobMaker.MakeJob(JobDefOf.BDP_JobDefRefillFromFiller, thing);
            job.targetB = chosenFiller;
            return job;
        }

        public static ThingWithComps FindSomeReloadableComponent(Pawn pawn, bool allowForcedReload)
        {
            if (pawn.apparel == null)
            {
                return null;
            }
            List<ThingWithComps> weapons = pawn?.equipment.AllEquipmentListForReading;
            for (int i = 0; i < weapons.Count; i++)
            {
                CompReloadableFromFiller compReloadableFromFiller = weapons[i].TryGetComp<CompReloadableFromFiller>();
                if (compReloadableFromFiller != null && compReloadableFromFiller.NeedsReload(allowForcedReload))
                {
                    return weapons[i];
                }
            }
            List<Apparel> wornApparel = pawn?.apparel.WornApparel;
            for (int i = 0; i < wornApparel.Count; i++)
            {
                CompReloadableFromFiller compReloadableFromFiller = wornApparel[i].TryGetComp<CompReloadableFromFiller>();
                if (compReloadableFromFiller != null && compReloadableFromFiller.NeedsReload(allowForcedReload))
                {
                    return wornApparel[i];
                }
            }
            return null;
        }

        public static Thing FindFiller(Pawn pawn)
        {
            Predicate<Thing> validator = delegate (Thing x)
            {
                CompLizionCellFiller compfiller = x.TryGetComp<CompLizionCellFiller>();
                if (compfiller != null)
                {
                    if (compfiller.PipeNet.Stored > 1)
                    {
                        if (compfiller.IsAvaliable())
                        {
                            return true;
                        }
                    }
                }

                return false;
            };
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
        }
    }

    public class JobDriver_ReloadFromFiller : JobDriver
    {
        private Thing Gear => job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.Reserve(job.GetTarget(TargetIndex.B).Thing, job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompReloadableFromFiller compReloadableFromFiller = Gear?.TryGetComp<CompReloadableFromFiller>();
            PipeNet filler = job.GetTarget(TargetIndex.B).Thing.TryGetComp<CompLizionCellFiller>().PipeNet; int LizionAvaliable = (int)filler.Stored;
            if (LizionAvaliable > compReloadableFromFiller.emptySpace)
            {
                LizionAvaliable = compReloadableFromFiller.emptySpace;
            }
            this.FailOn(() => compReloadableFromFiller == null);
            this.FailOn(() => !compReloadableFromFiller.NeedsReload(allowForcedReload: true));
            this.FailOn(() => filler.Stored < 1);
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_General.Wait(compReloadableFromFiller.Props.baseReloadTicks * LizionAvaliable).WithProgressBarToilDelay(TargetIndex.A);
            Toil toil2 = new Toil();
            toil2.initAction = delegate
            {
                compReloadableFromFiller.RefillGas(LizionAvaliable);
                filler.DrawAmongStorage(LizionAvaliable, filler.storages);
            };
            toil2.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil2;
        }
    }

    public class CompProperties_ReloadableFromFiller : CompProperties_ApparelReloadable
    {
        public CompProperties_ReloadableFromFiller()
        {
            compClass = typeof(CompReloadableFromFiller);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            //I don't give a damn this is apparel or not, skip base configerror
            if (ammoDef != null && ammoCountToRefill == 0 && ammoCountPerCharge == 0)
            {
                yield return "Reloadable component has ammoDef but one of ammoCountToRefill or ammoCountPerCharge must be set";
            }
            if (ammoCountToRefill != 0 && ammoCountPerCharge != 0)
            {
                yield return "Reloadable component: specify only one of ammoCountToRefill and ammoCountPerCharge";
            }
        }
    }
}
