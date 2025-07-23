using Harmony;
using HugsLib;
using Rimatomics;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace RimworldMod
{
    public class SoSRimatomicPatch : ModBase
    {
        public override string ModIdentifier
        {
            get
            {
                return "SoSRimatomicsPatch";
            }
        }

        public SoSRimatomicPatch()
        {
        }
    }

    [HarmonyPatch(typeof(Building_Railgun))]
    [HarmonyPatch("WorldRange", MethodType.Getter)]
    public class RailgunsInSpace
    {
        [HarmonyPostfix]
        public static void InfiniteRange(Building_Railgun __instance, ref int __result)
        {
            if (__instance.Map != null && __instance.Map.Biome.defName.Equals("OuterSpaceBiome"))
                __result = 99999;
        }
    }

    [HarmonyPatch(typeof(WorldObject_Sabot))]
    [HarmonyPatch("Start", MethodType.Getter)]
    public static class FromSpaceship
    {
        [HarmonyPostfix]
        public static void StartAtShip(WorldObject_Sabot __instance, ref Vector3 __result)
        {
            int initialTile = (int)typeof(WorldObject_Sabot).GetField("initialTile", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            foreach (WorldObject ship in Find.World.worldObjects.AllWorldObjects.Where(o => o.def.defName.Equals("ShipOrbiting")))
                if (ship.Tile == initialTile)
                    __result = ship.DrawPos;
            foreach (WorldObject site in Find.World.worldObjects.AllWorldObjects.Where(o => o.def.defName.Equals("SiteSpace")))
                if (site.Tile == initialTile)
                    __result = site.DrawPos;
        }
    }

    [HarmonyPatch(typeof(WorldObject_Sabot))]
    [HarmonyPatch("End", MethodType.Getter)]
    public static class ToSpaceship
    {
        [HarmonyPostfix]
        public static void EndAtShip(WorldObject_Sabot __instance, ref Vector3 __result)
        {
            int destTile = __instance.destinationTile;
            foreach (WorldObject ship in Find.World.worldObjects.AllWorldObjects.Where(o => o.def.defName.Equals("ShipOrbiting")))
                if (ship.Tile == destTile)
                    __result = ship.DrawPos;
            foreach (WorldObject site in Find.World.worldObjects.AllWorldObjects.Where(o => o.def.defName.Equals("SiteSpace")))
                if (site.Tile == destTile)
                    __result = site.DrawPos;
        }
    }
}
