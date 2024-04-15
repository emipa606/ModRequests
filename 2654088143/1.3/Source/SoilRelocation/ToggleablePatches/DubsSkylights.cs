using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace SR.ToggleablePatches
{
    internal class DubsSkylights
    {
        [ToggleablePatch]
        internal static ToggleablePatchGroup GlassUsesSandPatch = new()
        {
            Name = "Dubs Skylights Glass Uses Sand",
            Enabled = SoilRelocationSettings.DubsSkylightsGlassUsesSandEnabled,
            Patches = new()
                {
                    new ToggleablePatch<RecipeDef>
                    {
                        Name = "Dubs Skylights Glass Uses Sand",
                        TargetDefName = "SmeltGlass",
                        TargetModID = "Dubwise.DubsSkylights",
                        ConflictingModIDs = new()
                        {
                            "Maaxar.DubsSkylights.glasslights.patch",
                        },
                        Patch = (patch, def) =>
                        {
                            patch.State = def.fixedIngredientFilter;
                            def.fixedIngredientFilter = new ThingFilter();
                            def.fixedIngredientFilter.SetAllow(SoilDefs.SR_Sand, true);
                            def.ingredients.Clear();
                            var ingredient = new IngredientCount
                            {
                                filter = def.fixedIngredientFilter,
                            };
                            ingredient.SetBaseCount(10);
                            def.ingredients.Add(ingredient);
                        },
                        Unpatch = (patch, def) =>
                        {
                            def.fixedIngredientFilter = (ThingFilter)patch.State;
                        }
                    },
                    new ToggleablePatch<RecipeDef>
                    {
                        Name = "Dubs Skylights Glass 4x Uses Sand 4x",
                        TargetDefName = "SmeltGlass4x",
                        TargetModID = "Dubwise.DubsSkylights",
                        ConflictingModIDs = new()
                        {
                            "Maaxar.DubsSkylights.glasslights.patch",
                        },
                        Patch = (patch, def) =>
                        {
                            patch.State = def.fixedIngredientFilter;
                            def.fixedIngredientFilter = new ThingFilter();
                            def.fixedIngredientFilter.SetAllow(SoilDefs.SR_Sand, true);
                            def.ingredients.Clear();
                            var ingredient = new IngredientCount
                            {
                                filter = def.fixedIngredientFilter,
                            };
                            ingredient.SetBaseCount(40);
                            def.ingredients.Add(ingredient);
                        },
                        Unpatch = (patch, def) =>
                        {
                            def.fixedIngredientFilter = (ThingFilter)patch.State;
                        },
                    }
                }
        };
    }
}
