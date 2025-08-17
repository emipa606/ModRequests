using System;
using System.Reflection;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using Verse.AI;

namespace BDsWeaponJamming
{
    public class HarmonyPatch
    {
        [StaticConstructorOnStartup]
        public static class HarmonyPatches
        {
            private static readonly Type patchType = typeof(HarmonyPatches);
            static HarmonyPatches()
            {
                Harmony harmony = new Harmony("BDsWeaponJam");

                harmony.Patch(AccessTools.Method(typeof(Verb_LaunchProjectile), "TryCastShot"), prefix: new HarmonyMethod(patchType, nameof(TryCastShot_Prefix)));

                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            public static bool TryCastShot_Prefix(ref bool __result, Verb_LaunchProjectile __instance)
            {
                ThingWithComps weapon = __instance.EquipmentSource;
                if (weapon == null)
                {
                    return true;
                }
                CompQuality compQuality = weapon.TryGetComp<CompQuality>();
                DefModExtension_Jamming modExtension = weapon.def.GetModExtension<DefModExtension_Jamming>();
                Thing caster = __instance.caster;
                Pawn casterPawn = __instance.CasterPawn;

                if (compQuality != null && (modExtension == null || !modExtension.disableJamming) && (BDJamMod.settings.JamForNPC || caster.Faction == Faction.OfPlayer))
                {
                    float jamChance = BDJamMod.GetJamChance(weapon, compQuality, modExtension);
                    if (jamChance > 0 && Rand.Chance(jamChance))
                    {
                        __result = false;
                        if (caster != null && caster.Spawned)
                        {
                            MoteMaker.ThrowText(new Vector3(caster.Position.x + 1f, caster.Position.y, caster.Position.z + 1f), caster.Map, "BDJ_Jam".Translate() + jamChance.ToString("P1"), Color.white);
                        }

                        if (casterPawn != null && BDJamMod.DoWeaponDamage(weapon, casterPawn))
                        {
                            return false;
                        }

                        if (__instance.Bursting && __instance.verbProps.burstShotCount < BDJamMod.settings.ExternalPowerThreshold)
                        {
                            foreach (PropertyInfo x in typeof(Verb_LaunchProjectile).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
                            {
                                if (x.Name == "burstShotsLeft")
                                {
                                    x.SetValue(x, 0);
                                    break;
                                }
                            }
                            return false;
                        }
                    }
                }
                return true;
            }
        }
    }
}
