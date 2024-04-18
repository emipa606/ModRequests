using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using WildPlantPicker.DataStore;
using WildPlantPicker.DataStore.Entities;
using WildPlantPicker.Helper;
using WildPlantPicker.StaticResources;

namespace WildPlantPicker.HarmonyPatches
{
    [HarmonyPatch(typeof(PlaySettings))]
    internal class PlaySettings_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlaySettings.DoPlaySettingsGlobalControls), new Type[] { typeof(WidgetRow), typeof(bool) })]
        static void DoPlaySettingsGlobalControls_Postfix(WidgetRow row, bool worldView)
        {
            if (row == null)
            {
                return;
            }
            DataContext dc = DataContext.Current();
            bool preToggle = dc.m_AutoHarvestEnabled;
            row.ToggleableIcon(ref preToggle, Images.TEX_ICON_TINY, "WPP_IconTooltip".Translate());
            if (preToggle != dc.m_AutoHarvestEnabled)
            {
                preToggle = !preToggle;
                if (!dc.m_SettingDialog.m_Opened)
                {
                    Find.WindowStack.Add(dc.m_SettingDialog);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Plant))]
    internal class Plant_Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Plant.SpawnSetup), new Type[] { typeof(Map), typeof(bool) })]
        internal static IEnumerable<CodeInstruction> SpawnSetup_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return CodeInstruction.Call(typeof(Plant_Patch), nameof(Plant_Patch.InjectMethodAdd), new Type[] { typeof(Map), typeof(Plant) });
            foreach (CodeInstruction ci in instructions)
            {
                yield return ci;
            }
        }
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Plant.DeSpawn), new Type[] { typeof(DestroyMode) })]
        internal static IEnumerable<CodeInstruction> DeSpawn_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return CodeInstruction.Call(typeof(Thing), "get_Map");
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return CodeInstruction.Call(typeof(Plant_Patch), nameof(Plant_Patch.InjectMethodRemove), new Type[] { typeof(Map), typeof(Plant) });
            foreach (CodeInstruction ci in instructions)
            {
                yield return ci;
            }
        }

        static void InjectMethodAdd(Map map, Plant plant)
        {
            DataContext dc = DataContext.Current();
            if (CommonHelper.IsHarvestableMap(map, dc.m_OnlyPlayerColony, true) && dc.GetTargetPlantConditions().Any(x => x.m_PlantDef == plant.def))
            {
                dc.Add(map, plant);
#if DEBUG
                //Log.Message($"@@@InjectMethodAdd: map={map}, plant={plant.LabelCap}");
#endif
            }
        }
        static void InjectMethodRemove(Map map, Plant plant)
        {
            DataContext dc = DataContext.Current();
            dc.Remove(map, plant);
#if DEBUG
            if (dc.GetTargetPlantConditions().Any(x => x.m_PlantDef == plant.def))
            {
                Log.Message($"@@@InjectMethodRemove: map={map}, plant={plant.LabelCap}");
            }
#endif
        }
    }

    [HarmonyPatch(typeof(Map))]
    internal class Map_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Map.FinalizeInit))]
        static void FinalizeInit_Postfix(Map __instance)
        {
#if DEBUG
            Log.Message($"map[{__instance}]が追加されます！Map情報からコレクションを構築します！");
#endif
            DataContext.Current().Notify_MapFinalizeInit(__instance);
        }
    }

    [HarmonyPatch(typeof(MapDeiniter))]
    internal class MapDeiniter_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MapDeiniter.Deinit), new Type[] { typeof(Map) })]
        static void Deinit_Postfix(Map map)
        {
#if DEBUG
            Log.Message($"map[{map}]が削除されます！Map情報をコレクションから除外します！");
#endif
            DataContext.Current().RemoveMap(map);
        }
    }

    [HarmonyPatch(typeof(AreaManager))]
    internal class AreaManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("NotifyEveryoneAreaRemoved", new Type[] { typeof(Area) })]
        static bool NotifyEveryoneAreaRemoved_Prefix(Area area)
        {
#if DEBUG
            Log.Message($"エリア[{area?.Label}]が削除されます！該当するエリア制限を持つMapConditionのエリア制限をデフォルトに変更します！");
#endif
            DataContext.Current().RemoveArea(area);
            return true;
        }
    }
}
