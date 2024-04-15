using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace SR.ToggleablePatches
{
    internal class JustGlass
    {
        [ToggleablePatch]
        internal static ToggleablePatch<RecipeDef> GlassUsesSandPatch = new()
        {
            Name = "Just Glass Glass Uses Sand",
            Enabled = SoilRelocationSettings.JustGlassGlassUsesSandEnabled,
            TargetDefName = "MakeGlass",
            TargetModID = "spoden.JustGlass",
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
            },
        };
    }
}
