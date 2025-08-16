using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using PsychicAnimals;

namespace AnimaFruitPatch
{
    public class StaticConstructor
    {
        [StaticConstructorOnStartup]
        public static class StartUp
        {
            static StartUp()
            {
                DefOf.FeedAnimaFruitOnAnimal.recipeUsers = StaticConstructorOnStartupClass.StartUp.ChangeRecipeDef();
            }
        }

        [RimWorld.DefOf]
        public static class DefOf
        {
            public static RecipeDef FeedAnimaFruitOnAnimal;

            static DefOf()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(DefOf));
            }
        }
    }
}
