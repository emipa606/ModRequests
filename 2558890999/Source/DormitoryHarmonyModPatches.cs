using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Dormitories
{
    static class DormitoryHarmonyModPatches
    {
        /// <summary>
        /// Syntax for method names = Patch_ModMethodName_ModName_PatchType
        /// </summary>
        public static void Patch_Replacement_Hospitality_PostFix(Pawn actor)
        {
            if (actor.needs.mood != null)
            {
                Building_Bed building_Bed = actor.CurrentBed();
                if (building_Bed != null && building_Bed == actor.ownership.OwnedBed && !building_Bed.ForPrisoners && !actor.story.traits.HasTrait(TraitDefOf.Ascetic))
                {
                    if (building_Bed.GetRoom().Role == InternalDefOf.GuestRoom)
                    {
                        int numberOfBeds = building_Bed.GetRoom().ContainedBeds.Count();

                        if (numberOfBeds > 1 && numberOfBeds <= 3)
                        {
                            int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom().GetStat(RoomStatDefOf.Impressiveness));
                            if (DormitoriesDefOf.SleptInDormitory.stages[scoreStageIndex] != null)
                            {
                                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
                                actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(DormitoriesDefOf.SleptInDormitory, scoreStageIndex));
                            }
                        }
                    }
                } 
            }
        }

        /// <summary>
        /// Unused as of now, only a small conflict which doesnt really matter
        /// </summary>
        public static void Patch_WrongRoomType_Hospitality_PostFix(Room room, ref bool __result)
        {
            __result = room.Role != DefDatabase<RoomRoleDef>.GetNamed("GuestRoom") && room.Role != RoomRoleDefOf.Bedroom && room.Role != RoomRoleDefOf.PrisonCell && room.Role != RoomRoleDefOf.Barracks && room.Role != RoomRoleDefOf.PrisonBarracks && room.Role != DormitoriesDefOf.Dormitory;
        }
    }
}
