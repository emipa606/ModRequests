using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MarketValueFilter
{
    static class MarketValueCalc {

        public const int ID_MIN = -1;
        public const int ID_MAX = 10;

        public static int id2value(int marketValue)
        {
            switch (marketValue)
            {
                case 0: return 0;
                case 1: return 10;
                case 2: return 50;
                case 3: return 100;
                case 4: return 200;
                case 5: return 500;
                case 6: return 1000;
                case 7: return 2000;
                case 8: return 5000;
                case 9: return 10000;
                // infinite
                default: // case 10
                    return -1;
            }
        }

        public static IntRange id2value(IntRange marketValues)
        {
            return new IntRange(id2value(marketValues.min), id2value(marketValues.max));
        }

        public static string marketValueIdToString(IntRange marketValue)
        {
            if (marketValue.min == ID_MIN && marketValue.max == ID_MAX)
                return "MarketValue_Any".Translate();

            // invalid combinations
            if (marketValue.min == ID_MAX || marketValue.max == ID_MIN)
                return "MarketValue_Any".Translate();

            if (marketValue.min == ID_MIN)
                return "MarketValue_LessThan".Translate(id2value(marketValue.max));
            if (marketValue.max == ID_MAX)
                return "MarketValue_GreaterThan".Translate(id2value(marketValue.min));
            if (marketValue.min == marketValue.max)
                return "MarketValue_Exact".Translate(id2value(marketValue.min));
            return "MarketValue_Range".Translate(id2value(marketValue.min), id2value(marketValue.max));
        }
    }

    class WrappedIntRange
    {
        public IntRange value;

        public WrappedIntRange(int start, int stop)
        {
            value = new IntRange(start, stop);
        }
    }

    class MyWorldComponent : WorldComponent
    {
        private Dictionary<ThingFilter, WrappedIntRange> filterMarketValues = new Dictionary<ThingFilter, WrappedIntRange>();

        public MyWorldComponent(World world)
            : base(world)
        {
        }

        public ref IntRange getMarketValue(ThingFilter filter)
        {
            if (!filterMarketValues.ContainsKey(filter))
                filterMarketValues[filter] = new WrappedIntRange(MarketValueCalc.ID_MIN, MarketValueCalc.ID_MAX);
            WrappedIntRange w = filterMarketValues[filter];
            return ref w.value;
        }
    }

    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static string MagicTranslatePrefix = "{zed_0xff.TranslatedAlready}";

        // this static constructor runs to create a HarmonyInstance and install a patch.
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("zed_0xff.MarketValueFilter");

            harmony.Patch(
                AccessTools.Method(typeof(ThingFilter), nameof(ThingFilter.Allows), new Type[] { typeof(Thing) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(HarmonyPatches.IsAllowed_Postfix)))
            );

            harmony.Patch(
                AccessTools.Method(typeof(ThingFilter), nameof(ThingFilter.CopyAllowancesFrom)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(HarmonyPatches.CopyAllowancesFrom_Postfix)))
            );

            harmony.Patch(
                AccessTools.Method(typeof(ThingFilter), nameof(ThingFilter.ExposeData)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(HarmonyPatches.ExposeData_Postfix)))
            );

            harmony.Patch(
                AccessTools.Method(typeof(ThingFilterUI), "DrawHitPointsFilterConfig"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(HarmonyPatches.DrawMarketValueFilterConfig)))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Verse.Translator), nameof(Verse.Translator.Translate), new Type[] { typeof(string) }),
                prefix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(HarmonyPatches.Translate_Prefix)))
            );
        }

        public static bool Translate_Prefix(string key, ref TaggedString __result)
        {
            if (key != null && key.StartsWith(MagicTranslatePrefix))
            {
                __result = key.Substring(MagicTranslatePrefix.Length);
                return false;
            }
            return true;
        }

        public static void IsAllowed_Postfix(ThingFilter __instance, ref bool __result, Thing t)
        {
            // already disallowed by previous filters
            if (!__result)
                return;

            MyWorldComponent world = Find.World.GetComponent<MyWorldComponent>();
            IntRange raw = world.getMarketValue(__instance);
            IntRange translated = MarketValueCalc.id2value(raw);

            // fix invalid values that may arise in the mod upgrade/development process
            if(raw.min < MarketValueCalc.ID_MIN) raw.min = MarketValueCalc.ID_MIN;
            if(raw.max > MarketValueCalc.ID_MAX) raw.max = MarketValueCalc.ID_MAX;

            // invalid combinations
            if (raw.min == MarketValueCalc.ID_MAX || raw.max == MarketValueCalc.ID_MIN)
                return;

            if (raw.min != MarketValueCalc.ID_MIN && t.MarketValue < translated.min)
                    __result = false;
            else if(raw.max != MarketValueCalc.ID_MAX && t.MarketValue > translated.max)
                    __result = false;
        }

        public static void DrawMarketValueFilterConfig(ref float y, float width, ThingFilter filter)
        {
            MyWorldComponent world = Find.World.GetComponent<MyWorldComponent>();
            Rect rect = new Rect(20f, y, width - 20f, 28f);
            ref IntRange raw = ref world.getMarketValue(filter);

            // fix invalid values that may arise in the mod upgrade/development process
            if(raw.min < MarketValueCalc.ID_MIN) raw.min = MarketValueCalc.ID_MIN;
            if(raw.max > MarketValueCalc.ID_MAX) raw.max = MarketValueCalc.ID_MAX;

            Widgets.IntRange(rect, 1884285699, ref raw, MarketValueCalc.ID_MIN, MarketValueCalc.ID_MAX,
                labelKey: MagicTranslatePrefix + MarketValueCalc.marketValueIdToString(raw)
            );
            y += 28f;
            y += 5f;
            Text.Font = GameFont.Small;
        }

        public static void ExposeData_Postfix(ThingFilter __instance)
        {
            // Not sure how, but early in loading we get called when Find.World does not exists?
            if (Find.World == null)
                return;

            MyWorldComponent world = Find.World.GetComponent<MyWorldComponent>();

            ref IntRange raw = ref world.getMarketValue(__instance);
            Scribe_Values.Look<IntRange>(ref raw, "zed_0xff.allowedMarketValues", new IntRange(MarketValueCalc.ID_MIN, MarketValueCalc.ID_MAX), false);
        }

        public static void CopyAllowancesFrom_Postfix(ThingFilter __instance, ThingFilter other)
        {
            if (Find.World == null) return; // "Terraform Rimworld" mod

            MyWorldComponent world = Find.World.GetComponent<MyWorldComponent>();
            ref IntRange raw = ref world.getMarketValue(__instance);
            raw = world.getMarketValue(other);
        }
    }
}
