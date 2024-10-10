using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using UnityEngine;

namespace ThronesForAll
{
    [StaticConstructorOnStartup]
    static class HarmonyPatcher
    {

        static HarmonyPatcher()
        {
            Harmony harmony = new Harmony("thesePeople.ThronesForAll.HarmonyPatches");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(CompAssignableToPawn_Throne), "get_AssigningCandidates")]
        internal class Patch_AssigningCandidates
        {
            private static void Postfix(CompAssignableToPawn_Throne __instance, ref IEnumerable<Pawn> __result)
            {
                if (!__instance.parent.Spawned)
                {
                    __result = Enumerable.Empty<Pawn>();
                }
                else
                {
                    __result = __instance.parent.Map.mapPawns.FreeColonists;
                }

            }
        }

        [HarmonyPatch(typeof(MeditationUtility), "AllMeditationSpotCandidates")]
        internal class Patch_AllMeditationSpotCandidates
        {
            private static void Postfix(Pawn pawn, ref IEnumerable<LocalTargetInfo> __result, bool allowFallbackSpots = true)
            {
                if (ModsConfig.RoyaltyActive && MeditationUtility.CanMeditateNow(pawn) && pawn.ownership.AssignedThrone != null)
                {
                    Log.Message("Adding throne");
                    __result = __result.AddItem(pawn.ownership.AssignedThrone);
                } 
            }
        }

        //RitualBehaviorWorker_Speech
        [HarmonyPatch(typeof(RitualBehaviorWorker_Speech), "CreateLordJob")]
        internal class Patch_CreateLordJob
        {
            private static void Postfix(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments, ref LordJob __result)
            {
                //Log.Message("Creating LordJob");

                if(!ModsConfig.IdeologyActive)
                {
                    return;
                }

                Pawn organizer2 = assignments.AssignedPawns("speaker").First<Pawn>();

              /*  if (target != null)
                {
                    Log.Message("target for CreateLordJob " + target.Label);
                }*/

                // so the target should only be null if they aren't started it at the throne
                if ((target.Thing as Building_Throne) != null && organizer2.ownership.AssignedThrone != null && target != null && RoomRoleWorker_ThroneRoom.Validate(organizer2.ownership.AssignedThrone.GetRoom()) == null)
                {
                    //Log.Message("assigning throne");
                    target = new TargetInfo(organizer2.ownership.AssignedThrone);
                    //ritual.behavior = new RitualBehaviorWorker_ThroneSpeech();
                    //ritual.behavior.def = DefDatabase<RitualBehaviorDef>.GetNamed("ThroneSpeech");
                    __result = new LordJob_Joinable_Speech(target, organizer2, ritual, ritual.behavior.def.stages, assignments, true);
                }
            }
        }

        //RitualBehaviorWorker_Speech
        [HarmonyPatch(typeof(LordJob_Joinable_Speech), MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(TargetInfo), typeof(Pawn), typeof(Precept_Ritual), typeof(List<RitualStage>), typeof(RitualRoleAssignments), typeof(bool) })]
        internal class Patch_LordJob_Joinable_Speech
        {
            private static void Prefix(TargetInfo spot, Pawn organizer, Precept_Ritual ritual, List<RitualStage> stages, RitualRoleAssignments assignments, bool titleSpeech)
            {
                //Log.Message("Creating LordJob_Joinable_Speech");

                if(!ModsConfig.IdeologyActive || (ritual != null && ritual.behavior != null && ritual.behavior is RitualBehaviorWorker_ThroneSpeech))
                {
                    return;
                }
                
                // shouldn't we only do this if they're not targeting another valid speech spot?
                // but there's something weird about this whole thing
                /*Log.Message("spot selected " + spot.Label);
                if(organizer != null)
                {
                    Log.Message("organizer is " + organizer.Label);
                }
                if (organizer.ownership.AssignedThrone != null)
                {
                    Log.Message("organizer has throne. Throne usability " + RoomRoleWorker_ThroneRoom.Validate(organizer.ownership.AssignedThrone.GetRoom()));
                }

                if(spot != null)
                {
                    if(spot.Thing is Building_Throne)
                    {
                        Log.Message("spot is throne!");
                    }
                    Log.Message("spot.Thing is " + spot.Thing.Label + " with defName " + spot.Thing.def.defName);
                    if((spot.Thing as Building_Throne) != null)
                    {
                        Log.Message("Thing can be cast as Throne");
                    }

                }*/

                

                if (spot != null && (spot.Thing as Building_Throne) != null && organizer.ownership != null && organizer.ownership.AssignedThrone != null && RoomRoleWorker_ThroneRoom.Validate(organizer.ownership.AssignedThrone.GetRoom()) == null) // also check that throne is usable
                {
                    //Log.Message("overriding the behaviorworker");
                    ritual.behavior = new RitualBehaviorWorker_ThroneSpeech();
                    ritual.behavior.def = DefDatabase<RitualBehaviorDef>.GetNamed("ThroneSpeech");
                }
            }
        }
    }
}
