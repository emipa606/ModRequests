using HarmonyLib;
using RimWorld;
using Verse;
using static System.Net.Mime.MediaTypeNames;

namespace TeenagerSkincareExpansion
{

    [HarmonyPatch(typeof(RimWorld.ThoughtWorker_Precept_ChildLabor), "ShouldHaveThought")]
    public class Patch
    {
        public static bool Prefix(Pawn p, ref ThoughtState __result, ref ThoughtWorker_Precept_ChildLabor __instance)
        {
            Traverse traverse = Traverse.Create(__instance);
            var Assignment = traverse.Property("AssignmentDef").GetValue();

            if (!ModsConfig.IdeologyActive || !ModsConfig.BiotechActive)
            {
                __result = ThoughtState.Inactive;
                return false;
            }
            else
            {//copy of vanilla method with "developmentStage==Child" replaced by "Growth < 16 years"
                foreach (Pawn pawn in p.MapHeld.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
                {
                    if (pawn.RaceProps.Humanlike && pawn.ageTracker.Growth < 0.88f && pawn.timetable != null)
                    {
                        for (int i = 0; i < 24; i++)
                        {
                            if (pawn.timetable.GetAssignment(i) == Assignment)
                            {
                                __result = true;
                                return false;
                            }
                        }
                    }
                }
                __result = false;
                return false;
            }
        }



        //public static void Postfix(Pawn p, ref ThoughtState __result, ref ThoughtWorker_Precept_ChildLabor __instance)
        //{
        //    Traverse traverse = Traverse.Create(__instance);
        //    var Assignment = traverse.Property("AssignmentDef").GetValue();

        //    if (!ModsConfig.IdeologyActive || !ModsConfig.BiotechActive)
        //    {
        //        __result = ThoughtState.Inactive;
        //    }
        //    else
        //    {//copy of vanilla method with that gets everyone who is not adult
        //        foreach (Pawn pawn in p.MapHeld.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
        //        {
        //            if (pawn.RaceProps.Humanlike && pawn.ageTracker.Growth < 1 && pawn.timetable != null)
        //            {
        //                for (int i = 0; i < 24; i++)
        //                {
        //                    if (pawn.timetable.GetAssignment(i) == Assignment)
        //                    {
        //                        Log.Message("success");
        //                        __result = true;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }

    public class TeenagerSkincareExpansion : Mod
    {
        public TeenagerSkincareExpansion(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("TeenagerSkincareExpansion.patch");
            harmony.PatchAll();

        }
    }
}
