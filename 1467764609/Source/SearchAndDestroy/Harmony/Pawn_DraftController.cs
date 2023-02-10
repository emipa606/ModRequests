using Harmony;
using RimWorld;
using SearchAndDestroy.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SearchAndDestroy.Harmony
{
    [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
    public static class Pawn_DraftController_set_Drafted
    {
        public static void Postfix(Pawn_DraftController __instance)
        {
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if(store != null && !__instance.Drafted)
            {
                ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance.pawn);
                pawnData.SD_enabled = false;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    public static class Pawn_DraftController_GetGizmos
    {
        public static void Postfix(ref IEnumerable<Gizmo> __result, Pawn_DraftController __instance)
        {
            List<Gizmo> gizmoList = __result.ToList();
            bool isPlayerPawn = __instance.pawn.Faction != null && __instance.pawn.Faction.IsPlayer;

            if (isPlayerPawn && __instance.pawn.equipment != null && __instance.pawn.Drafted && (__instance.pawn.story == null || !__instance.pawn.story.WorkTagIsDisabled(WorkTags.Violent)))
            {
                if (__instance.pawn.equipment.Primary == null || __instance.pawn.equipment.Primary.def.IsMeleeWeapon)
                {
                    gizmoList.Add(CreateGizmo_SearchAndDestroy_Melee(__instance));
                }
                else
                {
                    gizmoList.Add(CreateGizmo_SearchAndDestroy_Ranged(__instance));
                }
            }
            __result = gizmoList;

        }

        private static Gizmo CreateGizmo_SearchAndDestroy_Melee(Pawn_DraftController __instance)
        {
            string disabledReason = "";
            bool disabled = false;
            PawnDuty duty = __instance.pawn.mindState.duty;

            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(__instance.pawn);

            if (__instance.pawn.Downed)
            {
                disabled = true;
                disabledReason = "SD_Reason_Downed".Translate();
            }
            Gizmo gizmo = new Command_Toggle
            {
                defaultLabel = "SD_Gizmo_Melee_Label".Translate(),
                defaultDesc = "SD_Gizmo_Melee_Description".Translate(),
                hotKey = KeyBindingDefOf.Command_ItemForbid,
                disabled = disabled,
                order = 10.5f,
                disabledReason = disabledReason,
                icon = ContentFinder<Texture2D>.Get(("UI/" + "SearchAndDestroy_Gizmo_Melee"), true),
                isActive = () => pawnData.SD_enabled,
                toggleAction = () =>
                {
                    pawnData.SD_enabled = !pawnData.SD_enabled;
                    __instance.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                }
            };
            return gizmo;
        }
        private static Gizmo CreateGizmo_SearchAndDestroy_Ranged(Pawn_DraftController __instance)
        {
            string disabledReason = "";
            bool disabled = false;
            PawnDuty duty = __instance.pawn.mindState.duty;

            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(__instance.pawn);

            if (__instance.pawn.Downed)
            {
                disabled = true;
                disabledReason = "SD_Reason_Downed".Translate();
            }
            Gizmo gizmo = new Command_Toggle
            {
                defaultLabel = "SD_Gizmo_Ranged_Label".Translate(),
                defaultDesc = "SD_Gizmo_Ranged_Description".Translate(),
                hotKey = KeyBindingDefOf.Command_ItemForbid,
                disabled = disabled,
                order = 10.6f,
                disabledReason = disabledReason,
                icon = ContentFinder<Texture2D>.Get(("UI/" + "SearchAndDestroy_Gizmo_Ranged"), true),
                isActive = () => pawnData.SD_enabled,
                toggleAction = () =>
                {
                    pawnData.SD_enabled = !pawnData.SD_enabled;
                    __instance.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                }
            };
            return gizmo;
        }
    }
}
