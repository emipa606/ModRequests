using RimWorld;
using Verse;
using PipeSystem; // CompResource
using VNPE;       // CompRegisterIngredients

namespace zed_0xff.CPS;

public class Dispenser_VNPE : IDispenser {
    private CompResource pasteComp = null;

    public Dispenser_VNPE(ThingWithComps parent){
        foreach (CompResource comp in parent.GetComps<CompResource>()) {
            if (comp?.Props?.pipeNet?.defName == "VNPE_NutrientPasteNet") {
                pasteComp = comp;
            }
        }
    }

    public bool CanDispenseNow => pasteComp != null && pasteComp.PipeNet.Stored >= 1;

    public float NutritionStored {
        get {
            return pasteComp == null ? 0 : pasteComp.PipeNet.Stored * ThingDefOf.MealNutrientPaste.GetStatValueAbstract(StatDefOf.Nutrition);
        }
    }

    public Thing TryDispenseFood(){
        if( !CanDispenseNow )
            return null;

        var net = pasteComp.PipeNet;
        net.DrawAmongStorage(1, net.storages);
        Thing meal = ThingMaker.MakeThing(ThingDefOf.MealNutrientPaste);
        if (meal.TryGetComp<CompIngredients>() is CompIngredients ingredients)
        {
            for (int s = 0; s < net.storages.Count; s++)
            {
                var parent = net.storages[s].parent;
                if (parent.TryGetComp<CompRegisterIngredients>() is CompRegisterIngredients storageIngredients)
                {
                    for (int ig = 0; ig < storageIngredients.ingredients.Count; ig++)
                        ingredients.RegisterIngredient(storageIngredients.ingredients[ig]);
                }
            }
        }

        return meal;
    }
}
