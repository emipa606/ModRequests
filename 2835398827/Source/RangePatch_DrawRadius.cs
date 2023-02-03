using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ShardMods
{
    [HarmonyPatch(typeof(Verb_CastAbility), nameof(Verb_CastAbility.DrawRadius))]
    [HarmonyPriority(Priority.First)]
    class RangePatch_DrawRadius
    {
        static bool Prefix(Verb_CastAbility __instance)
        {
            if (!(__instance is Verb_CastPsycast || __instance.GetType().FullName == "CombatPsycasts.Verbs.Verb_PsychicShoot"))
                return true;
            if (__instance.ability.pawn.Spawned)
                GenDraw_Custom.DrawRadiusRing(__instance.ability.pawn.Position, __instance.verbProps.range * __instance.ability.pawn.GetStatValue(StatDefOf.PsychicSensitivity));
            return false;
        }
    }
}
