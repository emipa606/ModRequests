using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using CPAbilityUser;

namespace CQCTakedown
{
    public class GloryKillAbilityUser : CompAbilityUser
    {
        public void PostInitializeTick()
        {
            if (this.AbilityUser != null)
            {
                if (this.AbilityUser.Spawned)
                {
                    if (this.AbilityUser.story != null)
                    {
                        firstTick = true;
                        this.Initialize();
                        this.AddPawnAbility(DefDatabase<AbilityDef>.GetNamed("CP_GloryKillTakedown"));
                    }
                }
            }
        }

        private bool firstTick = false;
        public override void CompTick()
        {
            if (AbilityUser != null)
            {
                if (AbilityUser.Spawned)
                {
                    if (Find.TickManager.TicksGame > 200)
                    {
                        if (IsGloryKillUser)
                        {
                            if (!firstTick) PostInitializeTick();
                            base.CompTick();
                        }
                    }
                }
            }
        }

        public bool IsGloryKillUser => AbilityUser?.skills?.GetSkill(SkillDefOf.Melee) is SkillRecord meleeSkill && meleeSkill.Level >= 15 ? true : false;

        public override bool TryTransformPawn()
        {
            return IsGloryKillUser;
        }

    }
}
