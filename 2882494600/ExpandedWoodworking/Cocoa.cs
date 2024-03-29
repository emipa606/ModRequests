using RimWorld;
using UnityEngine;
using Verse;

namespace ExpandedWoodworking
{
    public class Cocoa : Plant
    {
        public override void PlantCollected(Pawn by, PlantDestructionMode plantDestructionMode)
        {
            float harvestYield = def.plant.harvestYield;
            float num = Mathf.InverseLerp(def.plant.harvestMinGrowth, 1f, growthInt);
            num = 0.5f + (num * 0.5f);
            int num2 = GenMath.RoundRandom(harvestYield * num * Mathf.Lerp(0.5f, 1f, HitPoints / (float)base.MaxHitPoints) * Find.Storyteller.difficulty.cropYieldFactor);
            Thing thing = ThingMaker.MakeThing(ThingDef.Named("WoodLog_Cocoa"));
            thing.stackCount = (int)(num2 * 0.5f);
            GenPlace.TryPlaceThing(thing, base.Position, base.Map, ThingPlaceMode.Near);
            base.PlantCollected(by, plantDestructionMode);
        }
    }
}
