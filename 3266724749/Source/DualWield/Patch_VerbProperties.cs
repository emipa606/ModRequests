using HarmonyLib;
using RimWorld;
using System;
using Verse;
using Settings = Tacticowl.ModSettings_Tacticowl;

namespace Tacticowl.DualWield
{
    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedCooldown), new Type[] {typeof(Tool), typeof(Pawn), typeof(Thing)})]
    class Patch_VerbProperties_AdjustedCooldown
    {
        static bool Prepare()
        {
            return Settings.dualWieldEnabled;
        }
        static void Postfix(ref float __result, VerbProperties __instance, Thing equipment, Pawn attacker)
        {
            if (attacker != null && attacker.HasOffHand() && __instance.category != VerbCategory.BeatFire)
            {
                int skillLevel = attacker.skills == null ? 8 : (__instance.IsMeleeAttack ? attacker.skills.GetSkill(SkillDefOf.Melee).Level : attacker.skills.GetSkill(SkillDefOf.Shooting).Level);
                CalcCooldownPenalty(ref __result, skillLevel, (equipment.IsOffHandedWeapon() ? Settings.staticCooldownPOffHand : Settings.staticCooldownPMainHand) / 100f);
            }
        }
        static void CalcCooldownPenalty(ref float __result, int skillLevel, float staticPenalty)
        {
            float perLevelPenalty = Settings.dynamicCooldownP / 100f;
            int levelsShort = 20 - skillLevel;
            float dynamicPenalty = perLevelPenalty * levelsShort;
            __result *= 1.0f + staticPenalty + dynamicPenalty;
        }
    }
}
