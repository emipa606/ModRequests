using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace LoadFromStockpileZone
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
            gizmos.Add(CreateGizmo_LoadStockpile(__instance));
        }

        protected static Gizmo CreateGizmo_LoadStockpile(CompTransporter transporter)
        {
            List<CompTransporter> group = transporter.TransportersInGroup(transporter.parent.Map);
            string label;
            if (group != null)
            {
                label = ((group?.Count > 1) ? TranslatorFormattedStringExtensions.Translate("Gizmo_LoadStockpileGroup", group.Count) : Translator.Translate("Gizmo_LoadStockpile"));
            }
            else
            {
                label = Translations.Gizmo_LoadStockpile;
            }

            return new Command_LoadStockpile
            {
                defaultLabel = label,
                defaultDesc = Translations.Gizmo_LoadStockpileDesc,
                icon = ContentFinder<Texture2D>.Get("UI/LoadStockpile", false),
                transporter = transporter,
                group = group
            };
        }
    }

    [HarmonyPatch(typeof(Zone_Stockpile), "GetGizmos")]
    public class Patch_Stockpile
    {
        public static void Postfix(ref IEnumerable<Gizmo> __result, Zone_Stockpile __instance)
        {
            List<Gizmo> gizmos = __result.ToList();
            gizmos.Add(CreateGizmo_ToggleTransfer(__instance));
            __result = gizmos;
        }

        protected static Gizmo CreateGizmo_ToggleTransfer(Zone_Stockpile zone)
        {
            return new Command_Toggle
            {
                defaultLabel = Translations.Gizmo_ToggleTransfer,
                defaultDesc = Translations.Gizmo_ToggleTransferDesc,
                icon = ContentFinder<Texture2D>.Get("UI/LoadStockpile", false),
                isActive = zone.IsTransfer,
                toggleAction = delegate
                {
                    if (!zone.IsTransfer()) zone.RemovefromBlacklist();
                    else zone.AddtoBlacklist();
                }
            };
        }
    }





}
