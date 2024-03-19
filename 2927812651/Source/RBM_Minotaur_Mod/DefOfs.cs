using RimWorld;
using System.Collections.Generic;
using Verse;

[DefOf]
public static class RBM_DefOf
{
    //Abilites
    public static AbilityDef RBM_Lactation;
    public static AbilityDef RBM_SeeRed;

    //Genes
    public static GeneDef RBM_EstrousCycle;
    public static GeneDef RBM_RuminantStomach;
    public static GeneDef RBM_Herculean;
    public static GeneDef RBM_PeriodicLactation;

    //HeDiffs
    public static HediffDef HeDiffTerrified;
    public static HediffDef HeDiffSeeRed;
    public static HediffDef EstrousHeat;
    public static HediffDef MidasTouch;

    //Traits
    public static TraitDef RBM_Herculean_Trait;

    //MentalStates
    public static MentalStateDef RBM_TerrifiedFlee;

    //WeaponClassDefs
    public static WeaponClassDef RBM_HerculeanClass;

    //ThoughtDefs
    public static ThoughtDef RBM_Crushed;
    public static ThoughtDef RBM_CrushedMasochist;

    //ThingDefs
    public static ThingDef Milk;

    //Jobdrivers
    public static JobDef JobDriver_GotoAndUseAbility;

    //Worktypes
    public static WorkTypeDef BasicWorker;

    //Chunks
    public static ThingDef ChunkGranite;
    public static ThingDef ChunkSandstone;
    public static ThingDef ChunkLimestone;
    public static ThingDef ChunkSlate;
    public static ThingDef ChunkMarble;
    public static ThingDef ChunkSlagSteel;

}

public static class RBM_DefLists
{
    public static List<ThingDef> ChunkThingDefs = new List<ThingDef>
        {
            RBM_DefOf.ChunkGranite,
            RBM_DefOf.ChunkSandstone,
            RBM_DefOf.ChunkLimestone,
            RBM_DefOf.ChunkSlate,
            RBM_DefOf.ChunkMarble,
            RBM_DefOf.ChunkSlagSteel
        };

    public static List<string> IDList = new List<string>();
}
