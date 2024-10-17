using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Metro
{
    [HarmonyPatch(typeof(Mineable), "PreApplyDamage")]
    public class Patch_PreApplyDamage
    {
        private static void Prefix(Mineable __instance, ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (dinfo.Instigator is Mutant)
            {
                dinfo.SetAmount(dinfo.Amount * 10);
            }
        }
    }
}

