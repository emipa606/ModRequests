using Verse;
using HarmonyLib;
using System.Reflection;
using Verse.AI;
using RimWorld;

/// <summary>
/// The following code belongs to me - Knight on Steam
/// </summary>

[StaticConstructorOnStartup]
class Main
{
#pragma warning disable 0649
    public static Harmony harmony;
#pragma warning restore 0649

    static Main()
    {
        harmony = new Harmony("knight.UYG.patch");

        // Patch gun disable range

        MethodInfo range_targetmethod = AccessTools.Method(typeof(VerbUtility), "AllowAdjacentShot", null, null);
        HarmonyMethod range_postfix = new HarmonyMethod(typeof(RangePatch).GetMethod("Patch_Postfix"));
        harmony.Patch(range_targetmethod, null, range_postfix, null, null);

        //// Patch gun disable range

        //MethodInfo range_targetmethod = AccessTools.Method(typeof(VerbProperties), "EffectiveMinRange", new System.Type[] { typeof(LocalTargetInfo), typeof(Thing) });
        //HarmonyMethod range_postfix = new HarmonyMethod(typeof(RangePatch).GetMethod("Patch_Postfix"));
        //harmony.Patch(range_targetmethod, null, range_postfix);

        //// Patch melee cancel job if shooting

        MethodInfo meleecancel_targetmethod = AccessTools.Method(typeof(JobGiver_ReactToCloseMeleeThreat), "TryGiveJob", null, null);
        HarmonyMethod meleecancel_postfix = new HarmonyMethod(typeof(MeleeCancelPatch).GetMethod("Patch_Postfix"));
        harmony.Patch(meleecancel_targetmethod, null, meleecancel_postfix, null, null);
    }
}

static class RangePatch
{
    public static void Patch_Postfix(ref bool __result, ref Thing caster) {
        Pawn pawn = caster as Pawn;
        if (pawn != null && pawn.equipment != null && pawn.equipment.Primary != null) {
            bool found = UseYourGun.Base.weaponForbidder.Value.InnerList.TryGetValue(pawn.equipment.Primary.def.defName, out UseYourGun.WeaponRecord value);
            if (found && value.isSelected) {
                return;
            }
            // Weapon is enabled for adjacent targetting
            __result = true;
        }
    }
}

//static class RangePatch
//{
//    public static void Patch_Postfix(ref float __result, ref Thing caster)
//    {
//        Pawn pawn = caster as Pawn;
//        if (pawn != null && pawn.equipment != null && pawn.equipment.Primary != null)
//        {
//            bool found = UseYourGun.Base.weaponForbidder.Value.InnerList.TryGetValue(pawn.equipment.Primary.def.defName, out UseYourGun.WeaponRecord value);
//            if (found && value.isSelected)
//            {
//                return;
//            }
//            // Weapon is enabled for adjacent targetting
//            __result = 0f;
//        }
//    }
//}

static class MeleeCancelPatch
{
    public static void Patch_Postfix(ref Job __result, Pawn pawn) 
    {
        if (__result != null && UseYourGun.Base.experimentalAutoShoot == true) {
            var meleeAttacker   = pawn;
            var rangedShooter   = meleeAttacker.mindState.meleeThreat;
            var weapon          = rangedShooter?.equipment?.Primary;
            var shooterCurJob   = rangedShooter?.jobs.curJob; 

            if (rangedShooter != null && weapon != null) {
                if (shooterCurJob?.endIfCantShootInMelee == false && shooterCurJob.targetA == meleeAttacker) {
                    // Log.Message("Already executing shoot-at-melee-target job; not starting a new one!");
                    return;
                }
                if (rangedShooter.Drafted || shooterCurJob?.def == JobDefOf.AttackStatic) {

                    // Check if weapon is enabled for adjacent targeting
                    var found = UseYourGun.Base.weaponForbidder.Value.InnerList.TryGetValue(weapon.def.defName, out UseYourGun.WeaponRecord value);
                    if (found && value.isSelected) {
                        return;
                    }

                    var job = JobMaker.MakeJob(JobDefOf.AttackStatic);
                    job.verbToUse = rangedShooter.equipment.PrimaryEq.PrimaryVerb;
                    job.targetA = meleeAttacker;
                    job.endIfCantShootInMelee = false;
                    job.reactingToMeleeThreat = true;

                    rangedShooter.jobs.StartJob(job, JobCondition.InterruptForced);
                    // Log.Message("Attacking " + meleeAttacker.Name + " with " + rangedShooter.equipment.Primary.def.defName + " despite being in melee!");
                }
            }
        }
    }
}