using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace RottenMeat
{
    [StaticConstructorOnStartup]
    public class HarmonyPatch_CompRottable
    {
        //the static constructor
        static HarmonyPatch_CompRottable()
        {
            //creates a new Harmony instance and assigns it an ID
            var harmonyInstance = new Harmony("RimWorld.Carnagion.RottenMeat.HarmonyPatch_CompRottable");

            //gets the original method and the method that's supposed to be a prefix to it
            var originalMethod = AccessTools.Method(typeof(CompRottable), "Tick");
            var prefixMethod = AccessTools.Method(typeof(HarmonyPatch_CompRottable), "Prefix_Tick");

            //calls the patch method
            harmonyInstance.Patch(originalMethod, prefix: new HarmonyMethod(prefixMethod));
        }

        //the prefix method
        static bool Prefix_Tick(CompRottable __instance)
        {
            //proceeds if thing is active, rotting, and does not get destroyed when rotting
            if (__instance.Active && __instance.Stage == RotStage.Rotting && !__instance.PropsRot.rotDestroys)
            {
                //proceeds if thing is not null, is an item, and not a corpse
                Thing instanceThing = __instance.parent as Thing;
                if (instanceThing != null && instanceThing.def.category == ThingCategory.Item && instanceThing.def.thingClass != typeof(Corpse))
                {
                    //alerts player if rotting item is in a storage zone
                    if (__instance.parent.IsInAnyStorage() && __instance.parent.SpawnedOrAnyParentSpawned)
                    {
                        Messages.Message("MessageRottedAwayInStorage".Translate(__instance.parent.Label, __instance.parent).CapitalizeFirst(), new TargetInfo(__instance.parent.PositionHeld, __instance.parent.MapHeld, false), MessageTypeDefOf.NegativeEvent, true);
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.SpoilageAndFreezers, OpportunityType.GoodToKnow);
                    }
                    //spawns rotten meat at item's location and destroys item
                    IntVec3 replacementThingPosition = __instance.parent.Position;
                    int replacementThingStackCount = __instance.parent.stackCount;
                    Map replacementThingMap = __instance.parent.Map;
                    __instance.parent.Destroy(DestroyMode.Vanish);
                    Thing replacementThing = ThingMaker.MakeThing(ThingDefOf.MeatRotten);
                    replacementThing.stackCount = replacementThingStackCount;
                    GenPlace.TryPlaceThing(replacementThing, replacementThingPosition, replacementThingMap, ThingPlaceMode.Near, null, null, default);
                    return false;
                }

            }
            //by default, lets original method run
            return true;
        }
    }
}
