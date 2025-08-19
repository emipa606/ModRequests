using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using CPAbilityUser;

namespace CQCTakedown
{
    public class CQCAbilityUser : CompAbilityUser
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
                        this.AddPawnAbility(DefDatabase<CPAbilityUser.AbilityDef>.GetNamed("CP_CQCTakedown"));
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
                        if (IsCQCUser)
                        {
                            if (!firstTick) PostInitializeTick();
                            base.CompTick();
                        }
                    }
                }
            }
        }

        public bool IsCQCUser => AbilityUser?.skills?.GetSkill(SkillDefOf.Melee) is SkillRecord meleeSkill && meleeSkill.Level >= 8 ? true : false;

        public override bool TryTransformPawn()
        {
            return IsCQCUser;
        }
        
    }
}
