using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace zed_0xff.CPS;

// Cabin: stop colonists delivering food to prisoners in cabin, if they can take food from an adjacent food source
[HarmonyPatch(typeof(WorkGiver_Warden_DeliverFood), "FoodAvailableInRoomTo")]
static class Patch_FoodAvailableInRoomTo {

    private static readonly MethodInfo m_NutritionAvailableForFrom = AccessTools.Method(typeof(WorkGiver_Warden_DeliverFood), "NutritionAvailableForFrom");
    private static readonly FastInvokeHandler _NutritionAvailableForFrom = MethodInvoker.GetHandler(m_NutritionAvailableForFrom);

    static float getRegionFood(Region region, Pawn prisoner){
        float nAvail = 0;
        foreach( Thing thing in region.ListerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree)){
            if (!thing.def.IsIngestible || (int)thing.def.ingestible.preferability > 3) {
                nAvail += (float)_NutritionAvailableForFrom(null, new Object[]{ prisoner, thing } );
            }
        }
        foreach( Pawn pawn in region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn) ){
            if (pawn.IsPrisonerOfColony && pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry + 0.02f && (pawn.carryTracker.CarriedThing == null || !pawn.WillEat_NewTemp(pawn.carryTracker.CarriedThing))) {
                nAvail -= pawn.needs.food.NutritionWanted;
            }
        }
        return nAvail;
    }

    static void Postfix(ref bool __result, Pawn prisoner){
        if( __result == true ) return;

        Room room = prisoner.GetRoom();
        if( !Cache.IsCabin(room) ) return;

        float nAvail = 0f;
        foreach( Region region in room.Regions ){
            nAvail += getRegionFood(region, prisoner);
        }
        foreach( District d in room.Cells.First().GetDistrict(room.Map).Neighbors ){
            foreach( Region region in d.Regions ){
                nAvail += getRegionFood(region, prisoner);
            }
        }
        if ( nAvail >= -0.5f ) {
            __result = true;
        }
    }
}
