using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using RimWorld;
using System.Reflection.Emit;
using Verse;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace MoreTimeInfo
{
    [HarmonyPatch]
    public static class ClocksPatcher
    {
        static int accuracyInt = -1;
        static readonly MethodInfo widgets_label = typeof(Widgets).GetMethod(nameof(Widgets.Label), new Type[] { typeof(Rect), typeof(string) });
        static readonly MethodInfo listIndexer = typeof(List<string>).GetMethod("get_Item");
        static readonly FieldInfo fastHourStrings = typeof(DateReadout).GetField("fastHourStrings", BindingFlags.Static | BindingFlags.NonPublic);
        static readonly MethodInfo labelTimePart = typeof(ClocksPatcher).GetMethod("LabelTimePart", BindingFlags.Static | BindingFlags.NonPublic);
        static readonly MethodInfo target = typeof(DateReadout).GetMethod(nameof(DateReadout.DateOnGUI));
        static readonly MethodInfo tooltipHandler_tipRegion = typeof(TooltipHandler).GetMethod(nameof(TooltipHandler.TipRegion), new Type[] { typeof(Rect), typeof(TipSignal) });
        static readonly MethodInfo tipRegion_Patch = typeof(ClocksPatcher).GetMethod(nameof(TipRegionAltMethod));
        static readonly Type anonStorey = GetAnonStoreyType();
        static readonly FieldInfo locationField;
        static ClocksPatcher()
        {
            locationField = anonStorey.GetField("location", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        public static MethodInfo TargetMethod()
        {
            return target;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            short localIndexAnonStorey = (short)target.GetMethodBody().LocalVariables.First(v => v.LocalType == anonStorey).LocalIndex;
            List<CodeInstruction> instructionsList = instructions.MethodReplacer(tooltipHandler_tipRegion, tipRegion_Patch).ToList();
            for (int i = 0; i < instructionsList.Count; i++)
            {
                if (instructionsList[i].operand == widgets_label && instructionsList[i - 1].operand == listIndexer && instructionsList[i - 3].operand == fastHourStrings)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc, localIndexAnonStorey);
                    yield return new CodeInstruction(OpCodes.Ldfld, locationField);
                    instructionsList[i].operand = labelTimePart;
                }
                yield return instructionsList[i];
            }
        }
        static Type GetAnonStoreyType()
        {
            Type[] types = typeof(DateReadout).GetNestedTypes(BindingFlags.NonPublic);
            for (int i = 0; i < types.Length; i++)
            {
                Log.Message(types[i].Name);
                if (types[i].HasAttribute<CompilerGeneratedAttribute>())
                {
                    return types[i];
                }
            }
            throw new Exception("SS Clocks :: Could not find nested types in DateReadout.");
        }
        static void LabelTimePart(Rect rect, string message, Vector2 location)
        {
            int accuracy = accuracyInt = GetAccuracyForMap(Find.VisibleMap);
            if (accuracy >= 0)
            {
                int minute = (int)Math.Floor((decimal)(Find.TickManager.TicksAbs) / 2500 * 60) % 60;
                int second = (int)Math.Floor((decimal)(Find.TickManager.TicksAbs) / 2500 * 3600) % 60;
                int hour = GenDate.HourInteger(Find.TickManager.TicksAbs, location.x);
                int hourFinal = hour;
                bool pm = hour > 11;
                if (hourFinal == 0)
                {
                    hourFinal = 12;
                }
                else if (hourFinal > 12)
                {
                    hourFinal %= 12;
                }
                message = (pm ? "pmannotation" : "amannotation").Translate($"{hour.ToString("00")}:{minute.ToString("00")}{(accuracy > 0 ? $":{second.ToString("00")}" : "")}");
            }
            Widgets.Label(rect, message);
        }
        public static void TipRegionAltMethod(Rect rect, TipSignal tip)
        {
            string text = tip.textGetter();
            if (accuracyInt >= 1)
            {
                text += "\nCurrent game tick: " + Find.TickManager.TicksGame;
            }
            tip.textGetter = () => text;
            TooltipHandler.TipRegion(rect, tip);
        }
        static int GetAccuracyForMap(Map map)
        {
            if (map == null)
                return -1;
            return map.GetComponent<ClockAccuracyManager>().ClockAccuracy;
        }
    }
}
