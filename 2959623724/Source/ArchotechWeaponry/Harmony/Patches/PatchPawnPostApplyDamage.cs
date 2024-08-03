using ArchotechWeaponry.Comps;
using ArchotechWeaponry.DefOfs;
using ArchotechWeaponry.Defs.Traits;
using HarmonyLib;
using RimWorld;
using Verse;
using MentalStateDefOf = ArchotechWeaponry.DefOfs.MentalStateDefOf;

namespace ArchotechWeaponry.Harmony.Patches
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("PostApplyDamage")]
    public class PatchPawnPostApplyDamage
    {
        [HarmonyPostfix]
        public static void Postfix(DamageInfo dinfo, float totalDamageDealt, Pawn __instance)
        {
            if (totalDamageDealt > 0 &&
                __instance.equipment?.Primary.TryGetComp<CompBladelinkWeapon>() is CompBladelinkWeapon compBladelink &&
                compBladelink.TraitsListForReading.Any(trait => trait.HasModExtension<WrathExtension>()))
            {
                WrathExtension wrath = compBladelink.TraitsListForReading
                    .Find(trait => trait.HasModExtension<WrathExtension>())
                    .GetModExtension<WrathExtension>();
                if (Rand.Chance(wrath.enrageChance))
                {
                    MentalBreakDefOf.ArchotechWrath.Worker.TryStart(__instance,
                        "This happened because of the equipped archotech weapon.", false);
                }
            }

            if (dinfo.Instigator is Pawn pawn && pawn.equipment?.Primary is ThingWithComps thing &&
                thing.GetComp<CompLimitedUsage>() is CompLimitedUsage compLimitedUsage)
            {
                if (compLimitedUsage.currentCharges <= 0)
                {
                    thing.Destroy();
                }
            }
        }
    }
}