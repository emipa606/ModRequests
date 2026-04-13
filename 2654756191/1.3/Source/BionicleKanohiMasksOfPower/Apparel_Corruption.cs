using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Networking;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Noise;

namespace BionicleKanohiMasksOfPower
{

    public class CompProperties_Corrupt : CompProperties//adds comp for items
    {
        public CompProperties_Corrupt()
        {
            this.compClass = typeof(CompCorrupt);
        }
    }

    public class CompCorrupt : ThingComp
    {
        [HarmonyPatch(typeof(JobDriver_Wear), "MakeNewToils")]
        public static class JobDriver_WearPatch
        {
            public static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_Wear __instance)
            {
                foreach (var toil in __result)
                {
                    yield return toil;
                }
                yield return new Toil
                {
                    initAction = delegate
                    {
                        var apparel = (Apparel)__instance.job.GetTarget(TargetIndex.A).Thing;
                        if (apparel.Wearer == __instance.pawn)
                        {
                            var corruptComp = apparel.GetComp<CompCorrupt>();
                            if (corruptComp != null)
                            {
                                if (apparel.WornByCorpse)
                                {
                                    HealthUtility.AdjustSeverity(__instance.pawn, HediffDefOf.Scaria, 1f);
                                    __instance.pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);//casues manhunter behavior to start
                                }
                            }
                        }
                    }
                };
            }
        }
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Scaria);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
                pawn.MentalState?.RecoverFromState();//should remove manhunter behavior
            }
        }
    }
}