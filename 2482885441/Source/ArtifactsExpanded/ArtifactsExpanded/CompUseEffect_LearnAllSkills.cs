using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ArtifactsExpanded
{
    public class CompUseEffect_LearnAllSkills : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            //calls base method first
            base.DoEffect(usedBy);
            //loops over all skills and gives exp to pawn for each one
            foreach (SkillDef currentSkillDef in DefDatabase<SkillDef>.AllDefs)
            {
                //proceeds if skill is not disabled for pawn
                if (!usedBy.skills.GetSkill(currentSkillDef).TotallyDisabled)
                {
                    //calculates skill gain and increases skill gain
                    int existingLevel = usedBy.skills.GetSkill(currentSkillDef).Level;
                    float experienceGain = 42000f * ArtifactsExpandedMod.modSettings.archotechNeurotrainerExpFactor;
                    usedBy.skills.Learn(currentSkillDef, experienceGain, true);
                    int newLevel = usedBy.skills.GetSkill(currentSkillDef).Level;
                    //notifies player of skill gain
                    if (PawnUtility.ShouldSendNotificationAbout(usedBy))
                    {
                        Messages.Message("SkillArchotechNeurotrainerUsed".Translate(usedBy.LabelShort, existingLevel, newLevel, currentSkillDef.LabelCap, usedBy.Named("USER")), usedBy, MessageTypeDefOf.PositiveEvent, true);
                    }
                }
            }
        }
    }
}
