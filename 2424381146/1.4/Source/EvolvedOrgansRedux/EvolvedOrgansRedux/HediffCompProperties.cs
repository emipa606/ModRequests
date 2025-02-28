using Verse;

namespace EvolvedOrgansRedux {
    public class EVOR_HediffCompProperties_ButchersNailsBloodlust : HediffCompProperties {
		public float daysUntilBerserk = 7;
        public EVOR_HediffCompProperties_ButchersNailsBloodlust() {
            compClass = typeof(EVOR_HediffComp_ButchersNailsBloodlust);
        }
    }
    public class EVOR_HediffComp_ButchersNailsBloodlust : HediffComp {
		private float ticksUntilBreakBase => Props.daysUntilBerserk * 60000f;
		private float ticksUntilBreakCurrent;
        private int amountOfKilledHumanlikes;
        public EVOR_HediffCompProperties_ButchersNailsBloodlust Props => (EVOR_HediffCompProperties_ButchersNailsBloodlust)props;
        public override void CompPostPostAdd(DamageInfo? dinfo) {
			ticksUntilBreakCurrent = ticksUntilBreakBase;
            amountOfKilledHumanlikes = (int)Pawn.records.GetValue(RimWorld.RecordDefOf.KillsHumanlikes);
        }
        public override void CompExposeData() {
            Scribe_Values.Look(ref ticksUntilBreakCurrent, "ticksUntilBreakCurrent");
            Scribe_Values.Look(ref amountOfKilledHumanlikes, "amountOfKilledHumanlikes");
        }
        public override void CompPostTick(ref float severityAdjustment) {
            if (ticksUntilBreakCurrent-- % 2500 != 0) return;
            int killedHumanlikes = (int)Pawn.records.GetAsInt(RimWorld.RecordDefOf.KillsHumanlikes);
            if (amountOfKilledHumanlikes < killedHumanlikes) {
                ticksUntilBreakCurrent = ticksUntilBreakBase;
                amountOfKilledHumanlikes = killedHumanlikes;
            } else if (ticksUntilBreakCurrent <= 0) {
                Pawn.mindState.mentalStateHandler.TryStartMentalState(RimWorld.MentalStateDefOf.Berserk);
                ticksUntilBreakCurrent = ticksUntilBreakBase;
            }
        }
        public override string CompLabelInBracketsExtra => (ticksUntilBreakCurrent / ticksUntilBreakBase).ToStringPercent();
    }
    public class EVOR_HediffCompProperties_PurglesRot : HediffCompProperties {
        public Verse.HediffDef hediffDef;
        public EVOR_HediffCompProperties_PurglesRot() {
            compClass = typeof(EVOR_HediffComp_PurglesRot);
        }
    }
    public class EVOR_HediffComp_PurglesRot : HediffComp {
        public EVOR_HediffCompProperties_PurglesRot Props => (EVOR_HediffCompProperties_PurglesRot)props;
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt) {
            //If the damage dealer is a human, attacked in melee range and has not this Hediff, apply Hediff.
            if (dinfo.Instigator.def.race.Humanlike && !dinfo.Def.isRanged && !((Pawn)dinfo.Instigator).health.hediffSet.HasHediff(parent.def)) {
                if (((Pawn)dinfo.Instigator).health.hediffSet.HasHediff(Props.hediffDef)) {
                    ((Pawn)dinfo.Instigator).health.hediffSet.GetFirstHediffOfDef(Props.hediffDef).Severity += (float)0.05;
                } else {
                    ((Pawn)dinfo.Instigator).health.AddHediff(Props.hediffDef);
                }
            }
        }
        public override void CompPostPostAdd(DamageInfo? dinfo) {
            //Heal from the Hediff after installing Hediff.
            if (Pawn.health.hediffSet.HasHediff(Props.hediffDef)) {
                Pawn.health.RemoveHediff(Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef));
            }
        }
    }
    public class EVOR_HediffCompProperties_AddTrait : HediffCompProperties {
        public RimWorld.TraitDef traitDef;
        public EVOR_HediffCompProperties_AddTrait() {
            compClass = typeof(EVOR_HediffComp_AddTrait);
        }
    }
    public class EVOR_HediffComp_AddTrait : HediffComp {
        public EVOR_HediffCompProperties_AddTrait Props => (EVOR_HediffCompProperties_AddTrait)props;
        public override void CompPostPostAdd(DamageInfo? dinfo) {
            Pawn.story.traits.allTraits.Add(new RimWorld.Trait() { def = Props.traitDef });
            Pawn.Notify_DisabledWorkTypesChanged();
        }
        public override void CompPostPostRemoved() {
            Pawn.story.traits.allTraits.Remove(Pawn.story.traits.allTraits.Find(e => e.def == Props.traitDef));
        }
    }
    public class EVOR_HediffCompProperty_ManipulateSkills : Verse.HediffCompProperties {
        public System.Collections.Generic.Dictionary<RimWorld.SkillDef, int> increasedSkills;
        public EVOR_HediffCompProperty_ManipulateSkills() {
            compClass = typeof(EVOR_HediffComp_ManipulateSkills);
        }
    }
    public class EVOR_HediffComp_ManipulateSkills : HediffComp {
        int timer = 15000;
        public EVOR_HediffCompProperty_ManipulateSkills Props => (EVOR_HediffCompProperty_ManipulateSkills)props;
        public override void CompPostPostAdd(Verse.DamageInfo? dinfo) {
            foreach (RimWorld.SkillDef skill in Props.increasedSkills.Keys) {
                if (Pawn.skills.GetSkill(skill).Level < 21) {
                    Pawn.skills.GetSkill(skill).EnsureMinLevelWithMargin(Props.increasedSkills[skill]);
                }
                Pawn.skills.GetSkill(skill).passion = RimWorld.Passion.Major;
            }
        }
        public override void CompPostTick(ref float severityAdjustment) {
            if (timer < 1) {
                foreach (RimWorld.SkillDef skill in Props.increasedSkills.Keys) {
                    if (Pawn.skills.GetSkill(skill).Level < 21) {
                        Pawn.skills.GetSkill(skill).EnsureMinLevelWithMargin(Props.increasedSkills[skill]);
                    }
                }
                timer = 15000;
            }
            timer--;
        }
    }
    public class EVOR_HediffCompProperties_AddHediffOnDakingDamage : HediffCompProperties {
        public EVOR_HediffCompProperties_AddHediffOnDakingDamage() {
            compClass = typeof(EVOR_HediffComp_AddHediffOnDakingDamage);
        }
        public HediffDef hediff;
    }
    public class EVOR_HediffComp_AddHediffOnDakingDamage : HediffComp {
        public EVOR_HediffCompProperties_AddHediffOnDakingDamage Props => (EVOR_HediffCompProperties_AddHediffOnDakingDamage)props;
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt) {
            Pawn.health.AddHediff(Props.hediff);
        }
    }
}