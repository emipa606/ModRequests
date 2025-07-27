using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

using CM_Callouts.PendingCallouts;

namespace CM_Callouts
{
    [StaticConstructorOnStartup]
    public static class DamageWorker_DamageResult_Patches
    {
        [HarmonyPatch(typeof(DamageWorker.DamageResult))]
        [HarmonyPatch("AssociateWithLog", MethodType.Normal)]
        public static class DamageWorker_DamageResult_AssociateWithLog
        {
            [HarmonyPostfix]
            public static void Postfix(DamageWorker.DamageResult __instance)
            {
                if (__instance.deflected)
                    return;

                Pawn hitPawn = __instance.hitThing as Pawn;
                if (hitPawn != null && !__instance.parts.NullOrEmpty())
                {
                    List<BodyPartRecord> recipientPartsDamaged = __instance.parts.Distinct().ToList();
                    List<bool> recipientPartsDestroyed = recipientPartsDamaged.Select((BodyPartRecord part) => hitPawn.health.hediffSet.GetPartHealth(part) <= 0f).ToList();

                    if (CalloutUtility.pendingCallout != null)
                    {
                        CalloutUtility.pendingCallout.FillBodyPartInfo(hitPawn.RaceProps.body, recipientPartsDamaged, recipientPartsDestroyed);
                        CalloutUtility.pendingCallout.AttemptCallout();

                        CalloutUtility.pendingCallout = null;
                    }
                    else if (CalloutUtility.CanCalloutNow(hitPawn))
                    {
                        new PendingCalloutEventWounded(hitPawn).AttemptCallout();
                    }

                    ThrowDestroyedPartMotes(hitPawn, recipientPartsDamaged, recipientPartsDestroyed);
                }
            }

            private static void ThrowDestroyedPartMotes(Pawn pawn, List<BodyPartRecord> recipientPartsDamaged, List<bool> recipientPartsDestroyed)
            {
                ShowWoundLevel showWoundLevel = CalloutMod.settings.showWoundLevel;
                if (showWoundLevel == ShowWoundLevel.None || !pawn.SpawnedOrAnyParentSpawned)
                    return;

                Vector3 thingVector3 = pawn.SpawnedParentOrMe.DrawPos;
                Map thingMap = pawn.SpawnedParentOrMe.Map;

                for (int i = 0; i < recipientPartsDestroyed.Count; ++i)
                {
                    if (recipientPartsDestroyed[i])
                    {
                        CalloutTracker.CreateWoundTextMote(thingVector3, thingMap, recipientPartsDamaged[i].def.label, Color.magenta);
                    }
                    else
                    {
                        float partHealth = pawn.health.hediffSet.GetPartHealth(recipientPartsDamaged[i]);
                        float partMaxHealth = recipientPartsDamaged[i].def.GetMaxHealth(pawn);
                        float partPercentHealth = partHealth / partMaxHealth;
                        bool showWound = false;

                        if (partPercentHealth < 0.4f)
                            showWound = (showWoundLevel >= ShowWoundLevel.Major);
                        else if (partPercentHealth < 0.7f)
                            showWound = (showWoundLevel >= ShowWoundLevel.Serious);
                        else
                            showWound = (showWoundLevel >= ShowWoundLevel.All);

                        if (showWound)
                        {
                            Color moteColor = HealthUtility.GetPartConditionLabel(pawn, recipientPartsDamaged[i]).Second;
                            CalloutTracker.CreateWoundTextMote(thingVector3, thingMap, recipientPartsDamaged[i].def.label, moteColor);
                        }
                    }
                }
            }
        }
    }
}