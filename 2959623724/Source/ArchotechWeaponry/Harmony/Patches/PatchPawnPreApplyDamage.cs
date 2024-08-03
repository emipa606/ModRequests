using System.Linq;
using ArchotechWeaponry.Comps;
using ArchotechWeaponry.Defs;
using ArchotechWeaponry.Defs.Traits;
using ArchotechWeaponry.Utils;
using HarmonyLib;
using RimWorld;
using Verse;
using MentalStateDefOf = ArchotechWeaponry.DefOfs.MentalStateDefOf;

namespace ArchotechWeaponry.Harmony.Patches
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("PreApplyDamage")]
    public class PatchPawnPreApplyDamage
    {
        [HarmonyPrefix]
        public static void Prefix(ref DamageInfo dinfo, Pawn __instance)
        {
            HandleWrath(ref dinfo, __instance);
            HandleArchotechWeapon(ref dinfo, __instance);
            HandlePrecognitiion(ref dinfo, __instance);
        }

        private static void HandleArchotechWeapon(ref DamageInfo dinfo, Pawn __instance)
        {
            if (dinfo.Weapon != null && dinfo.Weapon.HasModExtension<ArchotechDamageExtension>() && dinfo.Instigator is Pawn instigator && instigator.equipment.Primary.def == dinfo.Weapon)
            {
                ThingWithComps weaponComp = instigator?.equipment?.Primary;
                if (weaponComp?.TryGetComp<CompArchotechWeapon>() is CompArchotechWeapon compArchotech)
                {
                    if (!__instance.def.race.IsMechanoid)
                    {
                        ArchotechDamageExtension extension = dinfo.Weapon.GetModExtension<ArchotechDamageExtension>();
                        if (!compArchotech.Lethal)
                        {
                            dinfo.SetAmount(0);
                        }

                        HediffDef toApply = compArchotech.Lethal
                            ? extension.lethalHediffToApplyOnOrganics
                            : extension.nonLethalHediffToApplyOnOrganics;

                        if (toApply != null)
                        {
                            float severity = compArchotech.Lethal ? extension.lethalSeverityPerHit : extension.nonLethalSeverityPerHit;
                            if (weaponComp.TryGetComp<CompBladelinkWeapon>() is CompBladelinkWeapon compBladelink &&
                                compBladelink.TraitsListForReading.Any(trait =>
                                    trait.HasModExtension<PlaguebearerExtension>()))
                            {
                                PlaguebearerExtension plaguebearerTrait = compBladelink.TraitsListForReading
                                    .Find(trait => trait.HasModExtension<PlaguebearerExtension>())
                                    .GetModExtension<PlaguebearerExtension>();
                                severity += plaguebearerTrait.extraSeverityOnHit;
                                if (severity > 1f)
                                {
                                    severity = 1f;
                                }
                            }

                            HediffUtils.AddOrUpdateHediffWithSeverity(__instance,
                                toApply, null,
                                severity); //TO-DO : Fixed severity if mode is changed
                        }
                    }
                    else
                    {
                        if (!compArchotech.Lethal){
                            dinfo.Def = DamageDefOf.EMP;
                        }
                    }
                }
            }
        }

        public static void HandlePrecognitiion(ref DamageInfo dinfo, Pawn __instance)
        {
            if (__instance.equipment?.Primary?.TryGetComp<CompBladelinkWeapon>() is CompBladelinkWeapon compBladelink && compBladelink.TraitsListForReading.Any(trait => trait.HasModExtension<PrecognitionExtension>()))
            {
                PrecognitionExtension precog = compBladelink.TraitsListForReading
                    .Find(trait => trait.HasModExtension<PrecognitionExtension>())
                    .GetModExtension<PrecognitionExtension>();
                if (Rand.Chance(precog.negationChance))
                {
                    dinfo.SetAmount(0);
                }
            }
        }

        public static void HandleWrath(ref DamageInfo dinfo, Pawn __instance)
        {
            if (__instance.MentalStateDef ==MentalStateDefOf.ArchotechWrath)
            {
                dinfo.SetAmount(0);
            }
        }
    }
}