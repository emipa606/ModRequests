using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

using System.Linq;

namespace OpenedDoorsDontBlockLight
{
    [StaticConstructorOnStartup]
    internal static class OpenedDoorsDontBlockLight
    {
        static OpenedDoorsDontBlockLight()
        {

            //HarmonyInstance.DEBUG = true;
            Harmony harmony = new Harmony("mlph.OpenedDoorsDontBlockLight");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(GlowFlooder), nameof(GlowFlooder.AddFloodGlowFor))]
    internal static class GlowFlooder_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            /*
            if (this.blockers[i].def.blockLight)
            を
            if (GlowFlooder_Patch.CheckDoorBlockLight(this.blockers[i]))
            に変更
            */
            int index = codes.FirstIndexOf(x => (x.opcode == OpCodes.Ldfld) && (x.operand is FieldInfo field) && (field.Name == nameof(ThingDef.blockLight)));
            codes.RemoveAt(index);
            codes[index - 1] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GlowFlooder_Patch), nameof(GlowFlooder_Patch.CheckDoorBlockLight)));

            /*
            int num8 = this.calcGrid[num2].intDist + num7;
            の直後に
            num8 = ChangeDistance(innerArray[num6], num8, num);
            をぶちこむ
            */
            /*
            int num7 = this.calcGrid[curIndex].intDist + num6;
            num7 = ChangeDistance(innerArray[num5], num7, num);
            */
            /*
            バージョンアップの度に変数がずれていく
            int num6 = calcGrid[curIndex].intDist + num5;
            num6 = ChangeDistance(innerArray[num4], num6, num);
            */

            int index2 = codes.FirstIndexOf(x => x.opcode == OpCodes.Stloc_S && x.operand is LocalBuilder num && num.LocalIndex == 12);
            //LocalBuilderって何?
            codes.InsertRange(index2 + 1, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldloc_0),       //innerArray
                new CodeInstruction(OpCodes.Ldloc_S, 10),   //num4
                new CodeInstruction(OpCodes.Ldelem_Ref),    //配列の中身読み込み
                new CodeInstruction(OpCodes.Ldloc_S, 12),   //num6
                new CodeInstruction(OpCodes.Ldloc_2),       //num
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GlowFlooder_Patch), nameof(GlowFlooder_Patch.ChangeDistance))),
                new CodeInstruction(OpCodes.Stloc_S, 12)    //num5
            });

            //Log.Message("IL code\n\n" + string.Join("\n", codes.Select(x => x.ToString()).ToArray()) + "\n\n");
            return codes;
        }

        private static bool CheckDoorBlockLight(Thing thing)
        {
            if (thing.def.blockLight)
            {
                if (thing is Building_Door door)
                {
                    if (door.Open)
                    {
                        //開いてるなら光をブロックしない
                        return false;
                    }
                    else
                    {
                        //リストに含まれてれば閉じてても光をブロックしない
                        return !DoorGlowGridManager.instance.Contains(door);
                    }
                }
            }
            return thing.def.blockLight;
        }

        private static int ChangeDistance(Thing thing, int dist, int growRadius)
        {
            if (thing == null || !thing.def.blockLight)
            {
                return dist;
            }
            if (thing is Building_Door door)
            {
                if (!DoorGlowGridManager.instance.Contains(door))
                {
                    return dist;
                }
                int visualTicksOpen = door.Get_ticksSinceOpen();
                float open = Mathf.Clamp01((float)visualTicksOpen / (float)door.TicksToOpenNow);
                //閉じてるときは0 開ききると1
                int newdist = Mathf.RoundToInt(Mathf.Lerp(growRadius, dist, open));
                //閉じてるときはgrowRadius 開ききるとdist
                //growRadiusなら光がピッタリ届かない距離扱い
                return newdist;
            }
            return dist;
        }
    }


    //開けるときと閉じるときに登録しとく

    [HarmonyPatch(typeof(Building_Door), "DoorOpen")]
    internal static class DoorOpen_Patch
    {
        public static void Postfix(Building_Door __instance)
        {
            DoorGlowGridManager.instance.Add(__instance);
        }
    }

    [HarmonyPatch(typeof(Building_Door), "DoorTryClose")]
    internal static class DoorTryClose_Patch
    {
        public static void Postfix(Building_Door __instance)
        {
            DoorGlowGridManager.instance.Add(__instance);
        }
    }

    //glowgridへの更新を管理

    internal class DoorGlowGridManager : GameComponent
    {
        private static readonly int updateInterval = 5;
        private List<Building_Door> movingDoors = new List<Building_Door>();

        public static DoorGlowGridManager instance;

        public DoorGlowGridManager(Game _)
        {
            instance = this;
        }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % updateInterval == 1)
            {
                //Log.Message(string.Join("\n",movingDoors.Select(x => x?.Map?.info?.Tile ?? 0)));
                for (int i = 0; i < movingDoors.Count; i++)
                {
                    Building_Door door = movingDoors[i];
                    if (door == null)
                    {
                        movingDoors.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (!door.Spawned)
                    {
                        movingDoors.RemoveAt(i);
                        i--;
                        continue;
                    }
                    door.Map.glowGrid.MarkGlowGridDirty(door.Position);
                    if (!door.IsMoving())
                    {
                        movingDoors.RemoveAt(i);
                        i--;
                    }
                }
            }
        }


        public void Add(Building_Door door)
        {
            if (movingDoors.Contains(door))
            {
                //Log.Error(door.ToString() + " is already added to list.");
            }
            else
            {
                movingDoors.Add(door);
            }
        }

        public bool Contains(Building_Door door)
        {
            return movingDoors.Contains(door);
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                foreach (Map map in Find.Maps)
                {
                    foreach (Building_Door door in map.listerBuildings.AllBuildingsColonistOfClass<Building_Door>())
                    {
                        if (door.IsMoving())
                        {
                            Add(door);
                        }
                    }
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class Utility
    {
        //static readonly GetterHandler visualTicksOpen = FastAccess.CreateGetterHandler(AccessTools.Field(typeof(Building_Door), "visualTicksOpen"));
        private static readonly Func<Building_Door, int> ticksSinceOpen;

        static Utility()
        {
            FieldInfo f = AccessTools.Field(typeof(Building_Door), "ticksSinceOpen");
            ticksSinceOpen = (door) => (int)f.GetValue(door);
        }


        public static int Get_ticksSinceOpen(this Building_Door door)
        {
            return ticksSinceOpen(door);
            //return (int)AccessTools.Field(typeof(Building_Door), "visualTicksOpen").GetValue(door);
        }

        public static bool IsMoving(this Building_Door door)
        {
            //うごいてる
            int visualTicksOpen = door.Get_ticksSinceOpen();
            if (!door.Open)
            {
                if (visualTicksOpen > 0)
                {
                    return true;
                }
            }
            else if (visualTicksOpen < door.TicksToOpenNow)
            {
                return true;
            }
            return false;
        }
    }
}
