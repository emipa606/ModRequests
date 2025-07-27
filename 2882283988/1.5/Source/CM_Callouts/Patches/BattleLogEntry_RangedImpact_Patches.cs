using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

using CM_Callouts.PendingCallouts.Combat;

namespace CM_Callouts
{
    [StaticConstructorOnStartup]
    public static class BattleLogEntry_RangedImpact_Patches
    {
        [HarmonyPatch(typeof(BattleLogEntry_RangedImpact))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(Thing), typeof(Thing), typeof(Thing), typeof(ThingDef), typeof(ThingDef), typeof(ThingDef) })]
        public static class BattleLogEntry_RangedImpact_Constructor
        {
            [HarmonyPostfix]
            public static void Postfix(BattleLogEntry_RangedImpact __instance, Thing initiator, Thing recipient, Thing originalTarget, ThingDef weaponDef, ThingDef projectileDef, ThingDef coverDef)
            {
                CalloutUtility.pendingCallout = null;

                if (!(initiator is Pawn))
                    return;


                if (recipient != originalTarget)
                    return;

                if (recipient is Pawn && CalloutUtility.CanCalloutNow(initiator) && CalloutUtility.CanCalloutAtTarget(recipient))
                {
                    CalloutUtility.pendingCallout = new PendingCalloutEventRangedImpact(initiator as Pawn, recipient as Pawn, originalTarget as Pawn, weaponDef, projectileDef, coverDef);
                }
            }
        }
    }
}
