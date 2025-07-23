﻿using ProjectRimFactory.SAL3.Exposables;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ProjectRimFactory.SAL3.Things.Assemblers
{
    public class Building_SimpleAssembler : Building_ProgrammableAssembler
    {
        public override IEnumerable<RecipeDef> GetAllRecipes()
        {
            HashSet<RecipeDef> recipes = new HashSet<RecipeDef>();
            // Imports recipes from modextension and recipes tag
            AssemblerDefModExtension extension = def.GetModExtension<AssemblerDefModExtension>();
            if ((extension?.importRecipesFrom?.Count ?? 0) > 0)
            {
                foreach (RecipeDef r in extension.importRecipesFrom.SelectMany(t => t.AllRecipes))
                {
                    if (!recipes.Contains(r) && base.SatisfiesSkillRequirements(r))
                    {
                        recipes.Add(r);
                        yield return r;
                    }
                }
            }
            if (def.recipes != null)
            {
                foreach (RecipeDef r in def.recipes)
                {
                    yield return r;
                }
            }
        }
    }
}
