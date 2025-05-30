using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Euphoric.YayosAmmunitionPatch
{
    public class CompReloadableDisplayStatsExtension : ThingComp
    {
        public CompProperties_ReloadableDisplayStatsExtension Props => props as CompProperties_ReloadableDisplayStatsExtension;
    }

    // ReSharper disable once InconsistentNaming
    public class CompProperties_ReloadableDisplayStatsExtension : CompProperties
    {
        public CompProperties_ReloadableDisplayStatsExtension() =>
            compClass = typeof(CompReloadableDisplayStatsExtension);

        public CompProperties_Reloadable CompReloadableProperties { get; set; }

        public RecipeDef AmmoRecipe { get; set; }

        public class UsageCost
        {
            public float CostPerShot { get; set; }
            public float CostPerFull { get; set; }
            public List<string> IngredientsPerShot { get; set; }
            public List<string> IngredientsPerFull { get; set; }
        }

        public UsageCost CalculateUsageCost()
        {
            
            CompProperties_Reloadable props = CompReloadableProperties;
            ThingDef ammoDef = props.ammoDef;
            RecipeDef ammoRecipeDef = AmmoRecipe;
            
            var costPerUnit = 1.0 / ammoRecipeDef.products[0].count;
            var ammoPrice = ammoDef.GetStatValueAbstract(StatDefOf.MarketValue);
            
            var ammoPerFull = props.maxCharges * props.ammoCountPerCharge;
            
            
            return new UsageCost
            {
                CostPerShot = ammoPrice * props.ammoCountPerCharge,
                CostPerFull = ammoPrice * ammoPerFull,
                IngredientsPerShot = ammoRecipeDef.ingredients.Select(ingredient =>
                {
                    var ingredientPerShot = ingredient.GetBaseCount() * props.ammoCountPerCharge * costPerUnit;
                    var ingredientSummary = $"{ingredientPerShot:F2}x {ingredient.filter.Summary}";
                    return ingredientSummary;
                }).ToList(),
                IngredientsPerFull = ammoRecipeDef.ingredients.Select(ingredient =>
                {
                    var ingredientPerFull = ingredient.GetBaseCount() * ammoPerFull * costPerUnit;
                    var ingredientSummary = $"{ingredientPerFull:F2}x {ingredient.filter.Summary}";
                    return ingredientSummary;
                }).ToList()
            };
        }
        
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            var props = CompReloadableProperties;

            UsageCost usageCost = CalculateUsageCost();
            
            var details = new StringBuilder();

            details.AppendLine("How much it costs to to fire this weapon.");
            
            details.AppendLine($"Cost per shot {usageCost.CostPerShot:F2}$:");

            foreach (var ingredientSummary in usageCost.IngredientsPerShot)
            {
                details.AppendLine(ingredientSummary);
            }

            details.AppendLine();
            
            details.AppendLine($"Cost per full {usageCost.CostPerFull:F2}$:");

            foreach (var ingredientSummary in usageCost.IngredientsPerFull)
            {
                details.AppendLine(ingredientSummary);
            }
            
            yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Usage cost", $"${usageCost.CostPerShot:F0}/${usageCost.CostPerFull:F0}", details.ToString(), 2750);
        }
    }
}