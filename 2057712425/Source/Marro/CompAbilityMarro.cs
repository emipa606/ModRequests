using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse;
using AbilityUser;

namespace Marro
{
    public class CompAbilityMarroWitch : CompAbilityUser
    {
        public override void CompTick()
        {
            base.CompTick();
            if (IsInitialized)
            {
                // any custom code
            }
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

            // add Abilities
            this.AddPawnAbility(Marro.MyAbility);
        }

        public override bool TryTransformPawn()
        {
            // if true, transforms this pawn into this AbilityUser (i.e. initialize)
            // So, you can have it check for a trait before initializing 
            // by default, it returns true
        }
    }
}
