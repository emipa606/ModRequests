using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace DamageMultipliers
{
    [HarmonyPatch(typeof(Verse.Thing))]
    class ThingTakeDamagePatch
    {
        [HarmonyPatch("TakeDamage")]
        //[HarmonyPatch(new Type[] { typeof(Verse.DamageInfo) })]
        static void Prefix(ref DamageInfo dinfo)
        {
            //Verse.Log.Message("PATCH: " + dinfo.Amount + " to " + dinfo.Amount * DamageMultiplierSettings.damageMultiplier);
            dinfo.SetAmount(dinfo.Amount * DamageMultiplierSettings.damageMultiplier);
        }
    }
}
