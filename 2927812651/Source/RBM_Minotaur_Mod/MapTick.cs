using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

namespace RBM_Minotaur
{
    [HarmonyPatch]
    public static class MaptTick
    {
        [HarmonyPatch(typeof(Map), nameof(Map.MapPostTick))]
        [HarmonyPostfix]
        // Regenerates Chunks with no weapon class - allows existing maps to be used when the mod is newly installed.
        public static void MapPostTick_Postfix(Map __instance)
        {

            try
            {
                if (__instance.IsHashIntervalTick(60) && MinotaurSettings.regenChunks == true) //Hash interval should be 3000 
                {
                    if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: public static void MapPostTick_Postfix(ref Map __instance)"); }
                    int numRegen = 0;

                    List<Thing> allThings = __instance.listerThings.AllThings;
                    for (int i = 0; i < allThings.Count; i++)
                    {
                        Thing thing = allThings[i];
                        for (int j = 0; j < RBM_DefLists.ChunkThingDefs.Count; j++)
                        {
                            ThingDef CompareDef = RBM_DefLists.ChunkThingDefs[j];
                            if (thing.def.defName == CompareDef.defName)
                            {
                                if (!RBM_DefLists.IDList.Contains(thing.ThingID))
                                {
                                    IntVec3 position = thing.Position;
                                    thing.Destroy();
                                    Thing thing2 = ThingMaker.MakeThing(CompareDef);
                                    GenSpawn.Spawn(thing2, position, __instance, WipeMode.VanishOrMoveAside);
                                    numRegen++;
                                    RBM_DefLists.IDList.Add(thing2.ThingID);
                                }
                            }
                        }
                    }
                    Log.Message("Found " + numRegen + " chunks to be regenerated");
                    if (numRegen == 0)
                    {
                        Log.Warning("Done regenerating chunks and disabled the 'Regenerate Chunks' setting. Please re-enable if errors persist.");
                        MinotaurSettings.regenChunks = false;
                        RBM_DefLists.IDList = new List<string>();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
