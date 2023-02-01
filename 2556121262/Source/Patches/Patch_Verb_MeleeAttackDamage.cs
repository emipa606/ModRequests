using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace HalfDragons.Patch1
{
    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget")]
    class Patch_ApplyMeleeDamageToTarget
    {
        [HarmonyPostfix]
        private static void AddDragonRageHediff(Verb_MeleeAttackDamage __instance)
        {
            try
            {
                if (!(__instance.Caster is Pawn caster))
                {
                    return;
                }
                if (!caster.IsHalfDragon())
                {
                    //Log.Message("Not a half dragon");
                    return;
                }
                if (caster?.equipment?.Primary?.def?.IsMeleeWeapon != true)
                {
                    //Log.Message("Not a melee weapon");
                    return;
                }
                caster.IncreaseDragonRage();
            }
            catch (Exception e)
            {
                Log.Warning("Half-Dragons: Something went wrong " + e);
                return;
            }
        }
    }

    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "DamageInfosToApply")]
    class Patch_DamageInfosToApply
    {
        [HarmonyPostfix]
        private static IEnumerable<DamageInfo> AddDragonRageDamageBuff(IEnumerable<DamageInfo> __result, Verb_MeleeAttackDamage __instance)
        {
            DamageInfo primaryAttack = default(DamageInfo);
            bool hasPrimaryAttack = false;
            foreach(DamageInfo damage in __result)
            {
                if(damage.Def == __instance.verbProps.meleeDamageDef)
                {
                    primaryAttack = damage;
                    hasPrimaryAttack = true;
                    continue;
                }
                yield return damage;
            }
            if(!hasPrimaryAttack)
            {
                yield break;
            }
            //Log.Message("previous amount: " + primaryAttack.Amount + " penetration " + primaryAttack.ArmorPenetrationInt);
            try
            {
                primaryAttack = TryMakeNewPrimaryAttack(__instance, primaryAttack);
            }
            catch (Exception e)
            {
                Log.Warning("Half-Dragons: Something went wrong " + e);
            }
            //Log.Message("after amount " + primaryAttack.Amount + " penetration " + primaryAttack.ArmorPenetrationInt);
            yield return primaryAttack;
        }

        private static DamageInfo TryMakeNewPrimaryAttack(Verb_MeleeAttackDamage __instance, DamageInfo primaryAttack)
        {
            if (!(__instance.Caster is Pawn caster))
            {
                return primaryAttack;
            }
            if (!caster.IsHalfDragon())
            {
                //Log.Message("Not a half dragon");
                return primaryAttack;
            }
            if (caster?.equipment?.Primary?.def?.IsMeleeWeapon == false)
            {
                //Log.Message("Not a melee weapon");
                return primaryAttack;
            }
            if (!caster.HasDragonRage())
            {
                //Log.Message("Pawn does not have dragon rage");
                return primaryAttack;
            }
            Hediff dragonRage = caster.GetDragonRageHediff();
            float dragonRageMultiplier = 1 + dragonRage.Severity;
            float newAmount = primaryAttack.Amount * dragonRageMultiplier;
            float newPenetration = primaryAttack.ArmorPenetrationInt * dragonRageMultiplier;
            primaryAttack = new DamageInfo(
                primaryAttack.Def,
                newAmount,
                newPenetration,
                primaryAttack.Angle,
                primaryAttack.Instigator,
                primaryAttack.HitPart,
                primaryAttack.Weapon,
                primaryAttack.Category,
                primaryAttack.IntendedTarget,
                primaryAttack.InstigatorGuilty,
                primaryAttack.SpawnFilth);
            return primaryAttack;
        }
    }
}