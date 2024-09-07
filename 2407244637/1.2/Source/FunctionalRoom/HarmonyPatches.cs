using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace FunctionalRoom
{
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new HarmonyLib.Harmony("miyamiya.functionalroom.latest");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    public class NotifyRoomUpdateCache
    {
        private const int EXECUTE_INTERVAL = 1;

        public static Dictionary<int, List<Room>> cache = new Dictionary<int, List<Room>>();

        public static void ClearCache() => cache.Clear();

        public static void SetCache(Room room)
        {
            int tick = Find.TickManager.TicksGame + EXECUTE_INTERVAL;
            if (!cache.ContainsKey(tick))
            {
                cache[tick] = new List<Room>();
            }
            cache[tick].Add(room);
        }

        public static bool TryGetRoomAndRemove(int tick, out List<Room> rooms)
        {
            if (cache.TryGetValue(tick, out rooms))
            {
                cache.Remove(tick);
                return true;
            }
            return false;
        }

        public static int GetCount() => cache.Count;
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Game))]
    public class Game_Patch
    {
        private static readonly RoomRoleDef ROOM_ROLE_DEF_KITCHEN = DefDatabase<RoomRoleDef>.GetNamed("Kitchen");

        [HarmonyPatch("LoadGame")]
        [HarmonyPostfix]
        public static void LoadGame_Postfix()
        {
            NotifyRoomUpdateCache.ClearCache();
            foreach (Map map in Find.Maps)
            {
                foreach (Room room in map.regionGrid.allRooms)
                {
                    bool valid = true;
                    if (room.Role == ROOM_ROLE_DEF_KITCHEN && room.ContainedAndAdjacentThings != null)
                    {
                        foreach (Thing thing in room.ContainedAndAdjacentThings.Where(x => x.def.designationCategory == DesignationCategoryDefOf.Production && x.def.AllRecipes != null))
                        {
                            if (thing.def.AllRecipes.Any(x => x.products == null || x.products.Any(y => y == null)))
                            {
                                valid = false;
                                break;
                            }
                        }
                    }
                    if (valid)
                    {
                        room.GetStat(RoomStatDefOf.Space);
                    }
                }
            }
        }

        [HarmonyPatch("InitNewGame")]
        [HarmonyPostfix]
        public static void InitNewGame_Postfix()
        {
            NotifyRoomUpdateCache.ClearCache();
        }

    }


    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Map))]
    public class Map_Patch
    {
        [HarmonyPatch("MapPostTick")]
        [HarmonyPostfix]
        public static void MapPostTick_Postfix()
        {
            int tick = Find.TickManager.TicksGame;
            if (NotifyRoomUpdateCache.TryGetRoomAndRemove(tick, out List<Room> rooms))
            {
                foreach (Room room in rooms)
                {
                    room.GetStat(RoomStatDefOf.Space);
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Room))]
    class Room_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("MakeNew")]
        [HarmonyPatch(new Type[] { typeof(Map) })]
        [HarmonyPriority(Priority.VeryLow)]
        static void MakeNew_Postfix(Room __result, Map map)
        {
            NotifyRoomUpdateCache.SetCache(__result);
        }

        [HarmonyPostfix]
        [HarmonyPatch("Notify_ContainedThingSpawnedOrDespawned")]
        [HarmonyPatch(new Type[] { typeof(Thing) })]
        [HarmonyPriority(Priority.VeryLow)]
        static void Notify_ContainedThingSpawnedOrDespawned_Postfix(Thing th)
        {
            if (!(th is Building))
            {
                return;
            }
            Room room = th?.GetRoom();
            if (room == null)
            {
                return;
            }
            room.GetStat(RoomStatDefOf.Space);
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(RoomStatWorker_Space))]
    class RoomStatWorker_Space_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetScore")]
        [HarmonyPatch(new Type[] { typeof(Room) })]
        [HarmonyPriority(Priority.VeryHigh)]
        static bool GetScore_Prefix(ref float __result, Room room)
        {
            if (room.PsychologicallyOutdoors)
            {
                __result = 350f;
                return false;
            }
            float num = 0f;
            foreach (IntVec3 c in room.Cells)
            {
                if (!c.InBounds(room.Map))
                {
                    continue;
                }
                LinkFlags link = room.Map?.linkGrid?.LinkFlagsAt(c) ?? LinkFlags.Wall;
                if (link == LinkFlags.Wall || link == LinkFlags.Rock)
                {
                    continue;
                }
                num += 1.4f;
            }
            __result = Mathf.Min(num, 350f);
            return false;
        }
    }


}
