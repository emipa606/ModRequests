using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using Harmony;
using Harmony.ILCopying;
using System.Collections;
using System.Reflection;
using UseYourGun;

/// <summary>
/// The following code belongs to me - Knight on Steam
/// </summary>

[StaticConstructorOnStartup]
class Main
{
#pragma warning disable 0649
    public static HarmonyInstance harmony;
#pragma warning restore 0649

    static Main()
    {
        harmony = HarmonyInstance.Create("com.UYG.patch");

        // Patch gun disable range

        MethodInfo range_targetmethod = AccessTools.Method(typeof(VerbUtility), "AllowAdjacentShot");
        HarmonyMethod range_postfix = new HarmonyMethod(typeof(RangePatch).GetMethod("Patch_Postfix"));
        harmony.Patch(range_targetmethod, null, range_postfix);
    }
}

static class RangePatch
{
    public static void Patch_Postfix(ref bool __result, ref Thing caster)
    {
        if (caster is Pawn)
        {
            Pawn pawn = caster as Pawn;
            if (pawn.equipment != null && pawn.equipment.Primary != null)
            {
                bool found = UseYourGun.Base.weaponForbidder.Value.InnerList.TryGetValue(pawn.equipment.Primary.def.defName, out UseYourGun.WeaponRecord value);
                if (found && value.isSelected)
                {
                    // Weapon is excluded from the mod
                    return;
                }
                else
                {
                    // Weapon is enabled for adjacent targetting
                    __result = true;
                }
            }
        }
        

    }
}