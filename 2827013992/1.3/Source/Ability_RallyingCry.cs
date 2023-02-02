using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace Primaris
{
    public class Ability_RallyingCry : Ability
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            var faction = target.Thing.Faction;
            if (faction == null || faction != this.pawn.Faction && faction.RelationKindWith(this.pawn.Faction) != RimWorld.FactionRelationKind.Ally)
            {
                return false;
            }
            return base.ValidateTarget(target, showMessages);
        }

        public override Hediff ApplyHediff(Pawn targetPawn, HediffDef hediffDef, BodyPartRecord bodyPart, int duration, float severity)
        {
            var hediff = base.ApplyHediff(targetPawn, hediffDef, bodyPart, duration, severity);
            var socialSkill = this.pawn.skills.GetSkill(SkillDefOf.Social);
            severity = 0;
            if (socialSkill.Level <= 5)
            {
                severity = 0.25f;
            }
            else if (socialSkill.Level <= 10)
            {
                severity = 0.5f;
            }
            else if (socialSkill.Level <= 17)
            {
                severity = 0.75f;
            }
            if (this.pawn.apparel.WornApparel.Any(x => x.def == Primaris_DefOf.Primaris_WarcasketHelmet 
            || x.def == Primaris_DefOf.Primaris_LieutenantWarcasketHelmet))
            {
                severity += 0.25f;
            }
            hediff.Severity = severity;
            return hediff;
        }
    }

    [DefOf]
    public static class Primaris_DefOf
    {
        public static ThingDef Primaris_WarcasketHelmet;
        public static ThingDef Primaris_LieutenantWarcasketHelmet;
    }
}
