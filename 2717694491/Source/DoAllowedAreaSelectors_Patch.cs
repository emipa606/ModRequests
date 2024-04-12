using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomSchedules
{
    [HarmonyPatch(typeof(AreaAllowedGUI), "DoAllowedAreaSelectors")]
    internal static class DoAllowedAreaSelectors_Patch
    {
        public static CSModSettings settings;
        public static CustomSchedulesSaveData saveData;

        private static bool isDragging = false;
        private static List<int> toggledPawnIds = new List<int>();

        public static bool Prefix(ref Rect rect, Pawn p)
        {
            if (settings == null)
                settings = CSModStarter.settings;

            if (saveData == null)
                saveData = p.Map.GetComponent<CustomSchedulesSaveData>();

            Rect lockRect = new Rect(new Vector2(rect.xMin - 2f, rect.y), new Vector2(16f, 28f));
            bool autoScheduleEnabled = saveData.PawnScheduleEnabled(p);

            Widgets.DrawBoxSolid(lockRect, autoScheduleEnabled ? Color.green : Color.red);

            if (isDragging)
            {
                if ((Event.current.rawType == EventType.MouseUp && Event.current.button == 0)
                    || (!Input.GetMouseButton(0) && Event.current.type != EventType.MouseDown))
                {
                    isDragging = false;
                    toggledPawnIds.Clear();
                }
            }

            if (Mouse.IsOver(lockRect))
            {
                if (isDragging == false && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    isDragging = true;
                    toggledPawnIds.Clear();
                }

                if (isDragging)
                {
                    if (!toggledPawnIds.Contains(p.thingIDNumber))
                    {
                        toggledPawnIds.Add(p.thingIDNumber);

                        if (autoScheduleEnabled)
                            saveData.RemovePawnFromList(p);
                        else
                            saveData.AddPawnToList(p);

                        SoundDefOf.Designate_DragStandard_Changed_NoCam.PlayOneShotOnCamera();
                    }
                }
            }

            rect.x += 14f;

            TooltipHandler.TipRegion(lockRect, (TipSignal)"When red, area is locked and will not change based on schedule. Mod(CustomSchedules)");
            GUI.color = Color.white;

            return true;
        }

        private static Exception Finalizer(Exception __exception) => __exception is NullReferenceException ? (Exception)null : __exception;
    }
}