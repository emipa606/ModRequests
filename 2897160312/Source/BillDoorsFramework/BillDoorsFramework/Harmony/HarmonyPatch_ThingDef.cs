using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace BillDoorsFramework
{
    static class PostLoad_PreFix
    {
        internal static MethodBase PostLoadMethod => typeof(ThingDef).GetMethod("PostLoad");
        public static void Prefix(ThingDef __instance)
        {
            DefModExtension_RemoveComps ext = __instance.GetModExtension<DefModExtension_RemoveComps>();
            if (__instance.comps != null && ext != null)
            {
                __instance.comps = __instance.comps.Where(c => !ext.compProps.Contains(c.GetType())).ToList();
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(ThingDef), "SpecialDisplayStats")]
    public static class FireRatePatch
    {
        public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result, ThingDef __instance)
        {
            foreach (StatDrawEntry command in __result)
            {
                if (command.LabelCap == "BurstShotFireRate".Translate().CapitalizeFirst() && !__instance.Verbs.NullOrEmpty() && __instance.GetModExtension<ModExtension_MultiShot>() is ModExtension_MultiShot ext)
                {
                    VerbProperties verb = __instance.Verbs.First((VerbProperties x) => x.isPrimary);
                    StatCategoryDef verbStatCategory = ((__instance.category == ThingCategory.Pawn) ? StatCategoryDefOf.PawnCombat : null);
                    if (verb.Ranged)
                    {
                        int burstShotCount = verb.burstShotCount;
                        float dmgBuildingsPassable = 60f * ext.shotCount / verb.ticksBetweenBurstShots.TicksToSeconds();
                        StatCategoryDef statCat = verbStatCategory ?? StatCategoryDefOf.Weapon_Ranged;
                        if (burstShotCount > 1)
                        {
                            yield return new StatDrawEntry(statCat, "BurstShotFireRate".Translate(), dmgBuildingsPassable.ToString("0.##") + " rpm", "Stat_Thing_Weapon_BurstShotFireRate_Desc".Translate(), 5392);
                            continue;
                        }
                    }
                }
                yield return command;
            }
        }
    }

    public class DefModExtension_RemoveComps : DefModExtension
    {
        public List<Type> compProps;
    }
}
