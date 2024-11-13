using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ImprovedNeurotrainers
{
    class CompUseEffect_LearnSkillImproved : CompUseEffect
	{
		public CompPropertiesUseEffect_LearnSkillImproved Props => (CompPropertiesUseEffect_LearnSkillImproved)props;
		private SkillDef Skill => DefDatabase<SkillDef>.GetNamed(Props.skillDefName);
		public CompUseEffect_LearnSkillImproved() { }

		public override void DoEffect(Pawn user)
		{
			base.DoEffect(user);
			var skill = this.Skill;
			int level = user.skills.GetSkill(skill).Level;
			user.skills.Learn(skill, Props.xpAmount, true);
			int levelAfter = user.skills.GetSkill(skill).Level;
			if (PawnUtility.ShouldSendNotificationAbout(user))
			{
				Messages.Message("SkillNeurotrainerUsed".Translate(user.LabelShort, skill.LabelCap, level, levelAfter, user.Named("USER")), user, MessageTypeDefOf.PositiveEvent, true);
			}
		}

		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
			if (p.skills == null)
			{
				failReason = null;
				return false;
			}
			if (p.skills.GetSkill(this.Skill).TotallyDisabled)
			{
				failReason = "SkillDisabled".Translate();
				return false;
			}
			return base.CanBeUsedBy(p, out failReason);
		}
	}
}
