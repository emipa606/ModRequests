using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace zed_0xff.CPS;

// a bunch of spaghetti to link hemogen indicator with CompRefuelable %)
public partial class Building_TSS : IResourceStore {
    public float MaxLevelOffset => 0f;

    public int ValueForDisplay => PostProcessValue(RefuelableComp.Fuel);
    public int MaxForDisplay => PostProcessValue(Max);

    public List<float> resourceGizmoThresholds => null;

    private IPipeNetAdapter hemogenNetAdapter = null;

    [Unsaved(false)]
    private CompRefuelable cachedRefuelableComp;
    private CompRefuelable RefuelableComp {
        get {
            if (cachedRefuelableComp == null) {
                cachedRefuelableComp = this.TryGetComp<CompRefuelable>();
            }
            return cachedRefuelableComp;
        }
    }

    public string ResourceLabel => RefuelableComp.Props.FuelLabel;
    public float  TargetValue   => RefuelableComp.TargetFuelLevel;
    public float  Max           => RefuelableComp.Props.fuelCapacity;
    public float  ValuePercent  => RefuelableComp.FuelPercentOfMax;
    public float  Value         => RefuelableComp.Fuel;

    public int PostProcessValue(float value) {
        return (int)value;
    }

    public bool ResourceAllowed {
        get { return RefuelableComp.allowAutoRefuel; }
        set { RefuelableComp.allowAutoRefuel = value; }
    }

    // called by Recipe_ExtractHemogen_TSS
    public void AddHemogenPack(){
        if( Value < TargetValue ){
            RefuelableComp.Refuel(1);
        } else if( hemogenNetAdapter != null ) {
            hemogenNetAdapter.Push(1);
        } else {
            GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.HemogenPack), Position, Map, ThingPlaceMode.Near);
        }
    }

    public bool HasAnyHemogen(){
        return Value >= 1 || (hemogenNetAdapter != null && hemogenNetAdapter.Stored >= 1);
    }

    public void ConsumeAnyHemogen(){
        if( hemogenNetAdapter != null && hemogenNetAdapter.Stored >= 1 ){
            hemogenNetAdapter.Draw(1);
        } else {
            RefuelableComp.ConsumeFuel(1);
        }
    }

    public int CountAllHemogen(){
        return (int)Value + (hemogenNetAdapter != null ? (int)hemogenNetAdapter.Stored : 0);
    }

    public void SetTargetValuePct(float val){
        RefuelableComp.TargetFuelLevel = val * Max;
        // push extra blood to the hemogen network
        float delta = RefuelableComp.Fuel - RefuelableComp.TargetFuelLevel;
        if( delta > 0 && hemogenNetAdapter != null ){
            RefuelableComp.ConsumeFuel(delta);
            hemogenNetAdapter.Push(delta);
        }
    }
}

