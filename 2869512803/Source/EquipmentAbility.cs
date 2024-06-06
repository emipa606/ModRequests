using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld.Planet;

namespace VFECore.Abilities
{
    public class EquipmentAbility : Ability
    {
        EquipmentAbilityDef AbilityDef => (EquipmentAbilityDef)this.def;
        public EquipmentAbility(Pawn pawn) : base(pawn)
        {
        }

        public EquipmentAbility(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }

        public EquipmentAbility(Pawn pawn, AbilityDef def, Thing source) : base(pawn, def)
        {
            sourceEquipment = source as ThingWithComps;
        }

        public ThingWithComps sourceEquipment;
        public CompAbilityItem AbilityItem => sourceEquipment?.TryGetComp<CompAbilityItem>();

        /*public override bool CanCast
        {
            get
            {
                if (!base.CanCast)
                {
                    return false;
                }
                if (CooldownTicksLeft > 0) return false;
                if (AbilityDef.requirePsycast)
                {
                    if (def.EntropyGain > float.Epsilon)
                    {
                        if (pawn.GetPsylinkLevel() < AbilityDef.requiredPsycastLevel && AbilityDef.requiredPsycastLevel > 0)
                        {
                            return false;
                        }
                        return !pawn.psychicEntropy.WouldOverflowEntropy(def.EntropyGain);
                    }
                    if (def.PsyfocusCost > pawn.psychicEntropy.CurrentPsyfocus + 0.0005f)
                    {
                        return false;
                    }
                    return true;
                }
                return true;
            }
        }*/

        public int MaxCastingTicks => (int)(AbilityDef.cooldown * GenTicks.TicksPerRealSecond);
        private int TicksUntilCasting = -5;
        public int CooldownTicksLeft
        {
            get => TicksUntilCasting;
            set => TicksUntilCasting = value;
        } //Log.Message(value.ToString()); } }

        public override void ExposeData()
        {
            Scribe_Defs.Look<AbilityDef>(ref this.def, "def");
            if (this.def == null)
            {
                return;
            }
            Scribe_Values.Look<int>(ref this.Id, "Id", -1, false);
            if (Scribe.mode == LoadSaveMode.LoadingVars && this.Id == -1)
            {
                this.Id = Find.UniqueIDsManager.GetNextAbilityID();
            }
            Scribe_References.Look(ref this.sourceEquipment, "sourceEquipment");
            Scribe_References.Look<Precept>(ref this.sourcePrecept, "sourcePrecept", false);
            /*Scribe_Deep.Look<VerbTracker>(ref this.verbTracker, "verbTracker", new object[]
            {
                this
            });
            Scribe_Values.Look<int>(ref this.cooldownTicks, "cooldownTicks", 0, false);
            Scribe_Values.Look<int>(ref this.cooldownTicksDuration, "cooldownTicksDuration", 0, false);*/
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.Initialize();
            }
            Scribe_Values.Look(ref TicksUntilCasting, "EquipmentAbilityTicksUntilcasting", -5);
        }

        public override IEnumerable<Command> GetGizmos()
        {
            if (gizmo == null)
            {
                //var command = (Command_EquipmentAbility)Activator.CreateInstance(def.gizmoClass, this);

                Command_Ability command;

                if (ModLister.RoyaltyInstalled && AbilityDef.requirePsycast) 
                {
                    command = new Command_EquipmentPsycast(this)
                    {
                        defaultLabel = AbilityDef.LabelCap,
                        order = def.uiOrder
                    };
                }
                else
                {
                    command = new Command_EquipmentAbility(this)
                    {
                        defaultLabel = AbilityDef.LabelCap,
                        order = def.uiOrder,
                        curTicks = CooldownTicksLeft
                    };
                }

                if (!CanCastPowerCheck("Player", out string reason))
                    command.Disable(reason);
                this.gizmo = command;

                if (AbilityDef.hasCooldown)
                {
                    if (this.CooldownTicksLeft == -5) // Min is -1, but this will do a one-time cooldown upon equipping
                    {
                        this.CooldownTicksLeft = this.MaxCastingTicks;
                        this.StartCooldown(MaxCastingTicks);
                        this.gizmo.Disable("AbilityOnCooldown".Translate(this.TicksUntilCasting.ToStringSecondsFromTicks()));
                    }
                }
            }
            yield return gizmo;


            // Swapping ability with another from floating menu
            /*yield return new Command_Action()
            {
                defaultLabel = ""),
                defaultDesc = "Age of Vatgrown",
                icon = ContentFinder<Texture2D>.Get("Things/Item/Health/HealthItem", true),
                order = -100,
                action = () =>
                {
                    Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
                    {
                        new FloatMenuOption("18",()=>{minAge = pawnKindToGrow.RaceProps.lifeStageAges.Last().minAge;old = true;}),
                        new FloatMenuOption("10",()=>{minAge = 10f; old = false;})
                    }));
                }
            };*/



            // Reset cooldown if devmode is on
            if (Prefs.DevMode && AbilityDef.hasCooldown && CooldownTicksLeft > 0)
            {
                Command_Action command_Action = new Command_Action
                {
                    defaultLabel = "DEV: Reset cooldown",
                    order = def.uiOrder + 1,
                    icon = ContentFinder<Texture2D>.Get("UI/ResetIcon"),
                    action = delegate
                    {
                        CooldownTicksLeft = 0;
                        this.StartCooldown(0);
                    }
                };
                yield return command_Action;
            }

            #region old gizmo code
            /*if (this.gizmo == null)
            {
                var command_CastPower = new Command_EquipmentAbility(this)
                {
                    defaultLabel = AbilityDef.LabelCap,
                };
                command_CastPower.curTicks = CooldownTicksLeft;
                //GetDesc
                var s = new StringBuilder();
                s.AppendLine(AbilityDef.GetDescription());
                command_CastPower.defaultDesc = s.ToString();
                s = null;
                command_CastPower.icon = this.def.uiIcon;
                
                //command_CastPower.action = delegate (Thing target)
                //{
                //    var tInfo = GenUI.TargetsAt(UI.MouseMapPosition(), Verb.verbProps.targetParams, false)?.First() ??
                //                target;
                //    TryCastAbility(AbilityContext.Player, tInfo);
                //};
                
                if (!CanCastPowerCheck("Player", out string reason))
                    command_CastPower.Disable(reason);
                this.gizmo = command_CastPower;
                if (this.CooldownTicksLeft == -5) // Min is -1, but this will do a one-time cooldown upon equipping
                {
                    this.CooldownTicksLeft = this.MaxCastingTicks;
                    this.gizmo.Disable("AbilityOnCooldown".Translate(this.CooldownTicksLeft.ToStringSecondsFromTicks()));
                }
            }
            yield return this.gizmo;*/
            #endregion
        }

        public virtual bool CanCastPowerCheck(string context, out string reason)
        {
            reason = "";

            if (context == "Player" && this.pawn.Faction != Faction.OfPlayer)
            {
                reason = "CannotOrderNonControlled".Translate();
                return false;
            }
            if (this.pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent) && AbilityDef.verbProperties.violent)
            {
                reason = "AbilityDisabled_IncapableOfWorkTag".Translate(this.pawn.Named("PAWN"), WorkTags.Violent.LabelTranslated());
                return false;
            }
            if (AbilityDef.hasCooldown && CooldownTicksLeft > 0)
            {
                reason = "AU_PawnAbilityRecharging".Translate(this.pawn.NameShortColored);
                return false;
            }
            //else if (!Verb.CasterPawn.drafter.Drafted)
            //{
            //    reason = "IsNotDrafted".Translate(new object[]
            //    {
            //        Verb.CasterPawn.Name.ToStringShort
            //    });
            //}

            return true;
        }

        public override void QueueCastingJob(LocalTargetInfo target, LocalTargetInfo destination)
        {
            base.QueueCastingJob(target, destination);
            if (AbilityDef.hasCooldown)
            {
                CooldownTicksLeft = MaxCastingTicks;
                this.StartCooldown(MaxCastingTicks);
            }
        }

        public override void AbilityTick()
        {
            if (WorldPawnsUtility.IsWorldPawn(pawn)) return;
            base.AbilityTick();
            if (sourceEquipment != null)
            {
                if (sourceEquipment is Apparel apparel)
                {
                    if (apparel.Wearer != pawn)
                    {
                        pawn.abilities.TryRemoveEquipmentAbility(AbilityDef, sourceEquipment);
                    }
                }
            }
            else
            {
                Log.Warning($"{this} lost source equipment, removing ability");
                pawn.abilities.TryRemoveEquipmentAbility(AbilityDef, sourceEquipment);
            }
            if (CooldownTicksLeft >= 0 && !Find.TickManager.Paused)
            {
                CooldownTicksLeft--;

                if (AbilityItem != null)
                {
                    //    abilityItem;
                }
                if (!this.gizmo.disabled)
                {
                    this.gizmo.Disable("AbilityOnCooldown".Translate(this.CooldownTicksLeft.ToStringSecondsFromTicks()));
                }
            }
            else
            {
                if (!Find.TickManager.Paused)
                {
                    if (this.gizmo != null)
                    {
                        if (this.gizmo.disabled)
                        {
                            this.gizmo.disabled = false;
                        }
                    }
                }
            }
        }
        /*
        // Psycast stuff
        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (AbilityDef.requirePsycast)
            {
                if (!ModLister.CheckRoyalty("Psycast"))
                {
                    return false;
                }
                if (def.EntropyGain > float.Epsilon && !pawn.psychicEntropy.TryAddEntropy(def.EntropyGain))
                {
                    return false;
                }
                float num = FinalPsyfocusCost(target);
                if (num > float.Epsilon)
                {
                    pawn.psychicEntropy.OffsetPsyfocusDirectly(0f - num);
                }
                if (def.showPsycastEffects)
                {
                    if (base.EffectComps.Any((CompAbilityEffect c) => c.Props.psychic))
                    {
                        if (def.HasAreaOfEffect)
                        {
                            FleckMaker.Static(target.Cell, pawn.Map, FleckDefOf.PsycastAreaEffect, def.EffectRadius);
                            SoundDefOf.PsycastPsychicPulse.PlayOneShot(new TargetInfo(target.Cell, pawn.Map));
                        }
                        else
                        {
                            SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(target.Cell, pawn.Map));
                        }
                    }
                    else if (def.HasAreaOfEffect && def.canUseAoeToGetTargets)
                    {
                        SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(target.Cell, pawn.Map));
                    }
                }
                return base.Activate(target, dest);
            }
            return true;
        }
        public override bool Activate(GlobalTargetInfo target)
        {
            if (AbilityDef.requirePsycast)
            {
                if (def.EntropyGain > float.Epsilon && !pawn.psychicEntropy.TryAddEntropy(def.EntropyGain))
                {
                    return false;
                }
                float psyfocusCost = def.PsyfocusCost;
                if (psyfocusCost > float.Epsilon)
                {
                    pawn.psychicEntropy.OffsetPsyfocusDirectly(0f - psyfocusCost);
                }
                return base.Activate(target);
            }
            return true;
        }
        protected override void ApplyEffects(IEnumerable<CompAbilityEffect> effects, LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (AbilityDef.requirePsycast)
            {
                if (CanApplyPsycastTo(target))
                {
                    foreach (CompAbilityEffect effect in effects)
                    {
                        effect.Apply(target, dest);
                    }
                    return;
                }
                MoteMaker.ThrowText(target.CenterVector3, pawn.Map, "TextMote_Immune".Translate());
            }
        }
        public bool CanApplyPsycastTo(LocalTargetInfo target)
        {
            if (AbilityDef.requirePsycast)
            {
                if (!base.EffectComps.Any((CompAbilityEffect e) => e.Props.psychic))
                {
                    return true;
                }
                Pawn pawn = target.Pawn;
                if (pawn != null)
                {
                    if (pawn.GetStatValue(StatDefOf.PsychicSensitivity) < float.Epsilon)
                    {
                        return false;
                    }
                    if (pawn.Faction != null && pawn.Faction == Faction.OfMechanoids && base.EffectComps.Any((CompAbilityEffect e) => !e.Props.applicableToMechs))
                    {
                        return false;
                    }
                }
                return true;
            }
            return true;
        }
        public override bool GizmoDisabled(out string reason)
        {
            if (AbilityDef.requirePsycast)
            {
                if (pawn.psychicEntropy.PsychicSensitivity < float.Epsilon)
                {
                    reason = "CommandPsycastZeroPsychicSensitivity".Translate();
                    return true;
                }
                float num = PsycastUtility.TotalPsyfocusCostOfQueuedPsycasts(pawn);
                if (def.level > 0 && pawn.GetPsylinkLevel() < def.level)
                {
                    reason = "CommandPsycastHigherLevelPsylinkRequired".Translate(def.level);
                    return true;
                }
                if (def.PsyfocusCost + num > pawn.psychicEntropy.CurrentPsyfocus + 0.0005f)
                {
                    reason = "CommandPsycastNotEnoughPsyfocus".Translate(def.PsyfocusCostPercent, (pawn.psychicEntropy.CurrentPsyfocus - num).ToStringPercent("0.#"), def.label.Named("PSYCASTNAME"), pawn.Named("CASTERNAME"));
                    return true;
                }
                if (def.level > pawn.psychicEntropy.MaxAbilityLevel)
                {
                    reason = "CommandPsycastLowPsyfocus".Translate(Pawn_PsychicEntropyTracker.PsyfocusBandPercentages[def.RequiredPsyfocusBand].ToStringPercent());
                    return true;
                }
                if (def.EntropyGain > float.Epsilon && pawn.psychicEntropy.WouldOverflowEntropy(def.EntropyGain + PsycastUtility.TotalEntropyFromQueuedPsycasts(pawn)))
                {
                    reason = "CommandPsycastWouldExceedEntropy".Translate(def.label);
                    return true;
                }
                return base.GizmoDisabled(out reason);
            }
            return base.GizmoDisabled(out reason);
        }*/
    }
}