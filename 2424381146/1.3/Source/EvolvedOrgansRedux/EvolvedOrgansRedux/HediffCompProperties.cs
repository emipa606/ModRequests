using Verse;

namespace EvolvedOrgansRedux {
    public class EVOR_HediffCompProperties_ReplaceHediff : Verse.HediffCompProperties {
        public Verse.HediffDef hediffDef;
        public EVOR_HediffCompProperties_ReplaceHediff() {
			compClass = typeof(EVOR_HediffComp_ReplaceHediff);
		}
    }
    public class EVOR_HediffComp_ReplaceHediff : Verse.HediffComp {
        public EVOR_HediffCompProperties_ReplaceHediff Props => (EVOR_HediffCompProperties_ReplaceHediff)props;
        public override void CompPostTick(ref float severityAdjustment) {
            try {
                Pawn.health.AddHediff(Props.hediffDef, parent.Part);
                Pawn.health.RemoveHediff(parent);
                Verse.Log.Message("Replaced " + parent.def.defName + " with " + Props.hediffDef.defName);
            } catch (System.Exception e) {
                try {
                    Verse.Log.Error("EVOR: Error when replacing " + parent.def.defName + " with " + Props.hediffDef.defName + "\n" + e);
                } catch {
                    Verse.Log.Error("EVOR: Critical error when replacing something.\n" + e);
                }
            }

        }
    }
    public class EVOR_HediffCompProperties_ButchersNailsBloodlust : Verse.HediffCompProperties {
		public float daysUntilBerserk = 7;
        public EVOR_HediffCompProperties_ButchersNailsBloodlust() {
            compClass = typeof(EVOR_HediffComp_ButchersNailsBloodlust);
        }
    }
    public class EVOR_HediffComp_ButchersNailsBloodlust : Verse.HediffComp {
		private string percentUntilBreak = "";
		private float ticksUntilBreakBase => Props.daysUntilBerserk * 60000f;
		private float ticksUntilBreakCurrent;
        private int amountOfKilledHumanlikes;
        public EVOR_HediffCompProperties_ButchersNailsBloodlust Props => (EVOR_HediffCompProperties_ButchersNailsBloodlust)props;
        public override void CompPostPostAdd(DamageInfo? dinfo) {
			ticksUntilBreakCurrent = ticksUntilBreakBase;
            amountOfKilledHumanlikes = (int)Pawn.records.GetValue(RimWorld.RecordDefOf.KillsHumanlikes);
        }
        public override void CompExposeData() {
            Verse.Scribe_Values.Look(ref ticksUntilBreakCurrent, "ticksUntilBreakCurrent");
            Verse.Scribe_Values.Look(ref amountOfKilledHumanlikes, "amountOfKilledHumanlikes");
        }
        public override void CompPostTick(ref float severityAdjustment) {
			if (ticksUntilBreakCurrent > 0) {
				ticksUntilBreakCurrent--;
				percentUntilBreak = (ticksUntilBreakCurrent / ticksUntilBreakBase).ToStringPercent();
                if (amountOfKilledHumanlikes < (int)Pawn.records.GetValue(RimWorld.RecordDefOf.KillsHumanlikes)) {
                    ticksUntilBreakCurrent = ticksUntilBreakBase;
                    amountOfKilledHumanlikes = (int)Pawn.records.GetValue(RimWorld.RecordDefOf.KillsHumanlikes);
                }
            } else {
				Pawn.mindState.mentalStateHandler.TryStartMentalState(RimWorld.MentalStateDefOf.Berserk);
				ticksUntilBreakCurrent = ticksUntilBreakBase;
			}
        }
        public override string CompLabelInBracketsExtra => percentUntilBreak;
    }
    public class EVOR_HediffCompProperties_PurglesRot : Verse.HediffCompProperties {
        public Verse.HediffDef hediffDef;
        public EVOR_HediffCompProperties_PurglesRot() {
            compClass = typeof(EVOR_HediffComp_PurglesRot);
        }
    }
    public class EVOR_HediffComp_PurglesRot : Verse.HediffComp {
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
    public class EVOR_HediffCompProperties_AddTrait : Verse.HediffCompProperties {
        public RimWorld.TraitDef traitDef;
        public EVOR_HediffCompProperties_AddTrait() {
            compClass = typeof(EVOR_HediffComp_AddTrait);
        }
    }
    public class EVOR_HediffComp_AddTrait : Verse.HediffComp {
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
    public class EVOR_HediffComp_ManipulateSkills : Verse.HediffComp {
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