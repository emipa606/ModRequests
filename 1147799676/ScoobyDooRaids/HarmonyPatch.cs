using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;
using Harmony;
using UnityEngine;
using System.Reflection;

namespace ScoobyDooRaids
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance.DEBUG = true;
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.scoobydooraids.patches");

            //harmony.Patch(AccessTools.Method(typeof(Verb_MeleeAttack), "DamageInfosToApply"), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(MeleeVerbDamageInfoPostfix)));

            harmony.Patch(AccessTools.Method(typeof(SiegeBlueprintPlacer), "MakeSandbagLine"), 
                new HarmonyMethod(typeof(HarmonyPatches), nameof(MakeSandbagLine_PreFix)), null);
            Log.Message("Succesfully injected.");
            //harmony.Patch(AccessTools.Method(typeof(SiegeBlueprintPlacer), "MakeSandbagLine"), null,
            //    new HarmonyMethod(typeof(HarmonyPatches).GetMethod("MakeSandbagLine_PreFix")), null);
        }



        public static bool MakeSandbagLine_PreFix(IntVec3 root, Map map, Rot4 growDir, int maxLength, ref IEnumerable<Blueprint_Build> __result)
        {
            __result = new Func<IEnumerable<Blueprint_Build>>(() =>
            {
                IntVec3 cur = root;
                IEnumerable<Blueprint_Build> blueprint = GenConstruct.PlaceBlueprintForBuild(ThingDefOf.Sandbags, cur, map, Rot4.North, SiegeBlueprintPlacer.faction, null);
                for (int i = 0; i < maxLength; i++)
                {
                    if (!SiegeBlueprintPlacer.CanPlaceBlueprintAt(cur, Rot4.North, ThingDefOf.Sandbags, map))
                    {
                        break;
                    }
                    // yield return GenConstruct.PlaceBlueprintForBuild(ThingDefOf.Sandbags, cur, map, Rot4.North, SiegeBlueprintPlacer.faction, null);
                    SiegeBlueprintPlacer.placedSandbagLocs.Add(cur);
                    cur += growDir.FacingCell;
                }
                return blueprint;
            })();
            return false;
        }

        private static IEnumerable<Blueprint_Build> MakeSandbagLine(IntVec3 root, Map map, Rot4 growDir, int maxLength)
        {
            IntVec3 cur = root;
            for (int i = 0; i < maxLength; i++)
            {
                if (!SiegeBlueprintPlacer.CanPlaceBlueprintAt(cur, Rot4.North, ThingDefOf.Sandbags, map))
                {
                    break;
                }
                yield return GenConstruct.PlaceBlueprintForBuild(ThingDefOf.Sandbags, cur, map, Rot4.North, SiegeBlueprintPlacer.faction, null);
                SiegeBlueprintPlacer.placedSandbagLocs.Add(cur);
                cur += growDir.FacingCell;
            }

            Type type = typeof(PawnRenderer);

            //Get private variable 'pawn' from 'PawnRenderer'.
            int_PawnRenderer_GetPawn = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);



            public static Pawn Pawn_NeedsTracker_GetPawn(Pawn_NeedsTracker instance)
            {
            return (Pawn)int_Pawn_NeedsTracker_GetPawn.GetValue(instance);
            }
        }





        //public override IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
        //{
        //    foreach (Thing t in this.ButcherProducts(butcher, efficiency))
        //    {
        //        yield return t;
        //    }

        //    if ( this.RaceProps.Humanlike)
        //    {
        //        butcher.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ButcheredHumanlikeCorpse, null);
        //        foreach (Pawn p in butcher.Map.mapPawns.SpawnedPawnsInFaction(butcher.Faction))
        //        {
        //            if (p != butcher && p.needs != null && p.needs.mood != null && p.needs.mood.thoughts != null)
        //            {
        //                p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse, null);
        //            }
        //        }
        //    }
        //}

        //public static bool ButcherProductsPrefix(Pawn butcher, float efficiency, ref IEnumerable<Thing> __result, Corpse __instance)
        //{
        //    __result = new Func<IEnumerable<Thing>>(() =>
        //    {
        //        ThingDef_AlienRace alienPropsButcher = butcher.def as ThingDef_AlienRace;

        //        Pawn corpse = __instance.InnerPawn;
        //        IEnumerable<Thing> things = corpse.ButcherProducts(butcher, efficiency);

        //        if (corpse.RaceProps.Humanlike)
        //        {
        //            ThoughtDef thought = alienPropsButcher == null ? ThoughtDefOf.ButcheredHumanlikeCorpse : DefDatabase<ThoughtDef>.GetNamedSilentFail(
        //                alienPropsButcher.alienRace.thoughtSettings.butcherThoughtSpecific?.FirstOrDefault(bt => bt.raceList?.Contains(corpse.def.defName) ?? false)?.thought ??
        //                alienPropsButcher.alienRace.thoughtSettings.butcherThoughtGeneral.thought);

        //            butcher.needs.mood.thoughts.memories.TryGainMemory(thought ?? ThoughtDefOf.ButcheredHumanlikeCorpse, null);

        //            butcher.Map.mapPawns.SpawnedPawnsInFaction(butcher.Faction).ForEach(p =>
        //            {
        //                if (p != butcher && p.needs != null && p.needs.mood != null && p.needs.mood.thoughts != null)
        //                {
        //                    ThingDef_AlienRace alienPropsPawn = p.def as ThingDef_AlienRace;
        //                    thought = alienPropsPawn == null ? ThoughtDefOf.KnowButcheredHumanlikeCorpse : 
        //                    DefDatabase<ThoughtDef>.GetNamedSilentFail(alienPropsPawn.alienRace.thoughtSettings.butcherThoughtSpecific?.FirstOrDefault
        //                        (bt => bt.raceList?.Contains(corpse.def.defName) ?? false)?.knowThought ?? alienPropsPawn.alienRace.thoughtSettings.butcherThoughtGeneral.knowThought);

        //                    p.needs.mood.thoughts.memories.TryGainMemory(thought ?? ThoughtDefOf.KnowButcheredHumanlikeCorpse, null);
        //                }
        //            });
        //        }
        //        return things;
        //    })();
        //    return false;
        //}

    }
}
