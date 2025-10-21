using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace zed_0xff.CPS;

[HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
public static class Patch_AddHumanlikeOrders {

    static Building_TSS GetClosestTSS(Pawn targetPawn, Pawn pawn){
        return (Building_TSS)GenClosest.ClosestThingReachable(
                targetPawn.PositionHeld,
                targetPawn.MapHeld,
                ThingRequest.ForDef(VDefOf.CPS_TSS),
                PathEndMode.InteractionCell,
                TraverseParms.For(pawn),
                9999f,
                (Thing b) => ((Building_TSS)b).CanAcceptPawn(targetPawn, forcePrisoner: true)
                );
    }

    static void CaptureToTSS(Pawn targetPawn, Pawn pawn){
        Building_TSS tss = GetClosestTSS(targetPawn, pawn);

        if (tss == null) {
            Messages.Message("CannotCapture".Translate() + ": " + "NoPrisonerBed".Translate(), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
        } else {
            tss.SelectPawn2(targetPawn);
            Job job = JobMaker.MakeJob(VDefOf.CaptureToTSS, tss, targetPawn);
            job.count = 1;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            if (targetPawn.Faction != null && targetPawn.Faction != Faction.OfPlayer && !targetPawn.Faction.Hidden && !targetPawn.Faction.HostileTo(Faction.OfPlayer) && !targetPawn.IsPrisonerOfColony) {
                Messages.Message("MessageCapturingWillAngerFaction".Translate(targetPawn.Named("PAWN")).AdjustedFor(targetPawn), targetPawn, MessageTypeDefOf.CautionInput, historical: false);
            }
        }
    }

    static void ArrestToTSS(Pawn targetPawn, Pawn pawn){
        Building_TSS tss = GetClosestTSS(targetPawn, pawn);

        if (tss == null) {
            Messages.Message("CannotArrest".Translate() + ": " + "NoPrisonerBed".Translate(), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
        } else {
            tss.SelectPawn2(targetPawn);
            Job job = JobMaker.MakeJob(VDefOf.ArrestToTSS, targetPawn, tss);
            job.count = 1;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }

    static void Postfix(Pawn pawn, List<FloatMenuOption> opts){
        if( !CPSMod.Settings.tss.menus ) return;
        if( pawn == null ) return;

        int n = opts.Count;
        for( int i=0; i<n; i++ ){
            var opt = opts[i];
            if( !(opt.revalidateClickTarget is Pawn targetPawn) ) continue;

            string sCapture = "Capture".Translate(targetPawn.LabelCap, targetPawn);
            if( opt.Label == sCapture ){
                if( GetClosestTSS(targetPawn, pawn) != null ){
                    TaggedString sCaptureToTSS = "CaptureToTSS".Translate(targetPawn.LabelCap, targetPawn);
                    opts.Insert(i+1, FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(sCaptureToTSS, delegate{ CaptureToTSS(targetPawn, pawn); }, MenuOptionPriority.RescueOrCapture, null, targetPawn), pawn, targetPawn));
                    n++;
                    i++;
                }
            } else {
                string sArrest = "TryToArrest".Translate(targetPawn.LabelCap, targetPawn, targetPawn.GetAcceptArrestChance(pawn).ToStringPercent());
                if( opt.Label == sArrest ){
                    if( GetClosestTSS(targetPawn, pawn) != null ){
                        TaggedString sArrestToTSS = "ArrestToTSS".Translate(targetPawn.LabelCap, targetPawn);
                        opts.Insert(i+1, FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(sArrestToTSS, delegate{ ArrestToTSS(targetPawn, pawn); }, MenuOptionPriority.High, null, targetPawn), pawn, targetPawn));
                        n++;
                        i++;
                    }
                }
            }
        }
    }
}
