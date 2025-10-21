using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using UnityEngine;

namespace zed_0xff.CPS;

[HarmonyPatch(typeof(SocialProperness), "IsSociallyProper", new Type[]{ typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool) })]
static class Patch_IsSociallyProper {

    public static bool IsInPrisonDistrict(IntVec3 cell, Map map){
        if( cell.IsInPrisonCell(map) ) return true;

        District D = cell.GetDistrict(map);
        if( D == null ) return false;

        return D.Neighbors.Any((District d) => Cache.Get(d.Cells.First(), map) is Building_Cabin cabin && cabin.ForPrisoners );
    }

    public static bool NewIsSociallyProper(Thing t, Pawn p, bool forPrisoner, bool animalsCare = false) {
        if (!animalsCare && p != null && !p.RaceProps.Humanlike) {
            return true;
        }
        if (!t.def.socialPropernessMatters) {
            return true;
        }
        if (!t.Spawned) {
            return true;
        }
        IntVec3 cell = (t.def.hasInteractionCell ? t.InteractionCell : t.Position);
        if (forPrisoner) {
            if (p != null) {
                // 1. prisoner from Cabin wants to access smth in adjacent prison district
                Room room = p.GetRoom();
                if( room != null && Cache.IsCabin(room) && p.GetDistrict().Neighbors.Contains(cell.GetDistrict(t.Map)) ){
                    return true;
                }
                // 2. prisoner from prison district wants to go back to adjacent Cabin
                if( t is Building_Cabin && p.GetDistrict().Neighbors.Contains(t.GetDistrict()) ){
                    return true;
                }
                return cell.GetRoom(t.Map) == p.GetRoom(); // vanilla
            }
            return true;
        }
        if (ModsConfig.BiotechActive && t.def == ThingDefOf.HemogenPack) {
            return !SocialProperness.BloodfeedingPrisonerInRoom(t.GetRoom());
        }
        // disallow pawns to eat on surfaces in _districts_ adjacent to a prison cabin
        return !IsInPrisonDistrict(cell, t.Map);
    }

    public static void Postfix(ref bool __result, Thing t, Pawn p, bool forPrisoner, bool animalsCare) {
        // disallow pawns to eat on surfaces in _districts_ adjacent to a prison cabin
        if( !forPrisoner && __result ){
            __result = NewIsSociallyProper( t, p, forPrisoner, animalsCare );
            return;
        }

        // Cabin: allow prisoners to get out of cabin freely
        if( forPrisoner && !__result ){
            __result = NewIsSociallyProper( t, p, forPrisoner, animalsCare );
            return;
        }
    }
}
