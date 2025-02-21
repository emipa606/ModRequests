using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Meditation
{
    public class Base : Mod
    {
        public static List<ThingDef> AllMeditationSpots;

        public Base(ModContentPack content) : base(content)
        {
            var harm = new Harmony("sf.meditation.comfy");
            harm.Patch(
                AccessTools.Method(AccessTools.FirstInner(typeof(MeditationUtility),
                    type => type.Name.Contains("AllMeditationSpotCandidates") && typeof(IEnumerator).IsAssignableFrom(type)), "MoveNext"),
                transpiler: new HarmonyMethod(typeof(Base), nameof(Transpiler)));
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                AllMeditationSpots = new List<ThingDef> {ThingDefOf.MeditationSpot};
                foreach (var def in DefDatabase<ThingDef>.AllDefs)
                    if (def.HasModExtension<BuildingExtension_MeditationOn>())
                        AllMeditationSpots.Add(def);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<ThingDef, IEnumerable<Thing>> AllOnMapOfPawnWithDef(Pawn pawn) => def => pawn.Map.listerBuildings.AllBuildingsColonistOfDef(def);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Thing> AllMeditationSpotsForPawn(Pawn pawn) => AllMeditationSpots.SelectMany(AllOnMapOfPawnWithDef(pawn));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var info1 = AccessTools.Method(typeof(ListerBuildings), nameof(ListerBuildings.AllBuildingsColonistOfDef));
            var idx1 = list.FindIndex(ins => ins.Calls(info1)) - 3;
            list.RemoveRange(idx1, 4);
            list.Insert(idx1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Base), nameof(AllMeditationSpotsForPawn))));
            return list;
        }
    }

    public class BuildingExtension_MeditationOn : DefModExtension
    {
    }
}