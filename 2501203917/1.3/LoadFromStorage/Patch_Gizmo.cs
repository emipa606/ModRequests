using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using LoadFromStockpileZone;

namespace LoadFromStorage
{
    [HarmonyPatch(typeof(CompTransporter), "CompGetGizmosExtra")]
    public class Patch_Gizmo
    {
        public static void Postfix(ref IEnumerable<Gizmo> __result, CompTransporter __instance)
        {
            List<Gizmo> gizmos = __result.ToList();

            AddGizmos(__instance, ref gizmos);
            __result = gizmos;

        }

        protected static void AddGizmos(CompTransporter __instance, ref List<Gizmo> gizmos)
        {
            gizmos.Add(CreateGizmo_LoadStorage(__instance));
        }

        protected static Gizmo CreateGizmo_LoadStorage(CompTransporter transporter)
        {
            List<CompTransporter> group = transporter.TransportersInGroup(transporter.parent.Map);
            string label;
            if (group != null)
            {
                label = ((group?.Count > 1) ? TranslatorFormattedStringExtensions.Translate("Gizmo_LoadStorageGroup", group.Count) : Translator.Translate("Gizmo_LoadStorage"));
            }
            else
            {
                label = Translations.Gizmo_LoadStorage;
            }

            return new Command_LoadStorage
            {
                defaultLabel = label,
                defaultDesc = Translations.Gizmo_LoadStorageDesc,
                icon = ContentFinder<Texture2D>.Get("UI/LoadStorage", false),
                transporter = transporter,
                group = group
            };
        }


    }

    [HarmonyPatch(typeof(Building_Storage), "GetGizmos")]
    public class Patch_Storage
    {
        public static void Postfix(ref IEnumerable<Gizmo> __result, Building_Storage __instance)
        {
            List<Gizmo> gizmos = __result.ToList();
            gizmos.Add(CreateGizmo_ToggleTransfer(__instance));
            __result = gizmos;
        }

        protected static Gizmo CreateGizmo_ToggleTransfer(Building_Storage storage)
        {
            return new Command_Toggle
            {
                defaultLabel = Translations.Gizmo_ToggleTransfer,
                defaultDesc = Translations.Gizmo_ToggleTransferDesc,
                icon = ContentFinder<Texture2D>.Get("UI/LoadStorage", false),
                isActive = storage.IsTransfer,
                toggleAction = delegate
                {
                    if (!storage.IsTransfer()) storage.RemovefromBlacklist();
                    else storage.AddtoBlacklist();
                }
            };
        }
    }
}
