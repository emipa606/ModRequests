using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace zed_0xff.CPS;

public partial class Building_TSS {
    class Dispenser_Internal : IDispenser {
        private Building_TSS parent;

        public bool CanDispenseNow => parent.PowerOn && HasEnoughFeedstockInHoppers();

        public Dispenser_Internal(Building_TSS parent){
            this.parent = parent;
        }

        public float NutritionStored {
            get {
                float num = 0;
                for (int i = 0; i < parent.innerContainer.Count; i++) {
                    Thing thing = parent.innerContainer[i];
                    if( !(thing is Pawn) && !(thing is Corpse) ){
                        num += (float)thing.stackCount * thing.GetStatValue(StatDefOf.Nutrition);
                    }
                }
                return num;
            }
        }

        public virtual bool CanMakePasteFrom(Thing t){
            return t != null
                && !(t is Pawn)
                && !(t is Corpse)
                && Building_NutrientPasteDispenser.IsAcceptableFeedstock(t.def);
        }

        // from Building_NutrientPasteDispenser
        public virtual bool HasEnoughFeedstockInHoppers()
        {
            float num = 0f;
            foreach( Thing feedStock in parent.innerContainer ){
                if( !CanMakePasteFrom(feedStock) ) continue;

                num += (float)feedStock.stackCount * feedStock.GetStatValue(StatDefOf.Nutrition);
                if (num >= parent.def.building.nutritionCostPerDispense) {
                    return true;
                }
            }
            return false;
        }

        // from Building_NutrientPasteDispenser
        public Thing TryDispenseFood() {
            if (!CanDispenseNow) {
                return null;
            }
            float num = parent.def.building.nutritionCostPerDispense - 0.0001f;
            List<ThingDef> list = new List<ThingDef>();

            foreach( Thing thing in parent.innerContainer ){
                if( !CanMakePasteFrom(thing) ) continue;

                int num2 = Mathf.Min(thing.stackCount, Mathf.CeilToInt(num / thing.GetStatValue(StatDefOf.Nutrition)));
                num -= (float)num2 * thing.GetStatValue(StatDefOf.Nutrition);
                list.Add(thing.def);
                thing.SplitOff(num2);
                if( num <= 0f )
                    break;
            }

            Thing meal = ThingMaker.MakeThing(ThingDefOf.MealNutrientPaste);
            CompIngredients compIngredients = meal.TryGetComp<CompIngredients>();
            for (int i = 0; i < list.Count; i++) {
                compIngredients.RegisterIngredient(list[i]);
            }
            return meal;
        }
    }

    List<IDispenser> dispensers = new List<IDispenser>();

    Thing TryDispenseAnyFood(){
        Thing meal = null;
        foreach( IDispenser dispenser in dispensers ){
            meal = dispenser.TryDispenseFood();
            if( meal != null ){
                break;
            }
        }
        return meal;
    }

    bool CanDispenseAnyFood(){
        return dispensers.Any(d => d.CanDispenseNow);
    }

    public float TotalNutritionAvailable {
        get {
            return dispensers.Sum(d => d.NutritionStored);
        }
    }

    public float NutritionNeeded {
        get {
            return IsContentsSuspended ? 0 : (NutritionBuffer - TotalNutritionAvailable);
        }
    }

    void feedOccupants(){
        if( nPawns == 0 ) return;

        foreach( Thing t in innerContainer ){
            Pawn pawn = t as Pawn;
            if( pawn == null || pawn.Dead || pawn.needs?.food == null )
                continue;
            if (pawn.needs.food.CurLevelPercentage > 0.4)
                continue;

            Thing meal = TryDispenseAnyFood();
            if( meal == null )
                break;

            var ingestedNum = meal.Ingested(pawn, pawn.needs.food.NutritionWanted);
            pawn.needs.food.CurLevel += ingestedNum;
            pawn.records.AddTo(RecordDefOf.NutritionEaten, ingestedNum);
        }
    }
}
