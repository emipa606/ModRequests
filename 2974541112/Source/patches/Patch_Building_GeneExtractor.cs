using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace zed_0xff.CPS;

// TSS: add an "Insert a test subject" gizmo to GeneExtractor
[HarmonyPatch(typeof(Building_GeneExtractor), nameof(Building_GeneExtractor.GetGizmos))]
public static class Patch__Building_GeneExtractor__GetGizmos
{
    static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building_GeneExtractor __instance) {
        bool was = false;
        string lText = "InsertPerson".Translate() + "...";

        foreach( Gizmo gizmo in __result ){
            yield return gizmo;
            if( !was && gizmo is Command_Action ca && ca.defaultLabel == lText ){
                was = true;
                yield return newGizmo(ca, __instance);
            }
        }
        if( !was ){
            yield return newGizmo(null, __instance);
        }
    }

    private static readonly MethodInfo m_selectPawn = AccessTools.Method(typeof(Building_GeneExtractor), "SelectPawn");
    private static readonly FastInvokeHandler selectPawn = MethodInvoker.GetHandler(m_selectPawn);

    private static int sortOrder = 0;

    static void showMenu(Building_GeneExtractor b){
        List<Pawn> pawns = new List<Pawn>();
        foreach (var tss in b.Map.listerBuildings.AllBuildingsColonistOfClass<Building_TSS>()) {
            foreach( Thing t in tss.innerContainer ){
                if( !(t is Pawn pawn) ) continue;
                if( pawn.Dead ) continue;
                if (pawn.genes == null) continue;

                AcceptanceReport acceptanceReport = b.CanAcceptPawn(pawn);
                if (!acceptanceReport.Accepted) continue;

                pawns.Add(pawn);
            }
        }

        int sortFunc(Pawn pawn){
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.XenogermReplicating);
            if (hediff == null) {
                return 0;
            } else {
                return hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear;
            }
        }

        List<FloatMenuOption> list = new List<FloatMenuOption>();
        if( pawns.Any() ){
            switch( sortOrder ){
                case 0:
                    pawns.SortBy((Pawn pawn) => pawn.LabelShortCap);
                    break;
                case 1:
                    pawns.SortBy((Pawn pawn) => pawn.genes.XenotypeLabelCap);
                    break;
                case 2:
                    pawns = pawns.OrderBy(sortFunc).ThenBy((Pawn pawn) => pawn.genes.XenotypeLabelCap).ToList();
                    break;
                default:
                    sortOrder = 0;
                    break;
            }

            foreach( Pawn pawn in pawns ){
                string text = pawn.LabelShortCap + ", " + pawn.genes.XenotypeLabelCap;
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.XenogermReplicating);
                if (hediff != null) {
                    int ticksToDisappear = hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear;
                    string addText = " (" + hediff.LabelBase + ", " + ticksToDisappear.ToStringTicksToPeriod(allowSeconds: true, shortForm: true) + ")";
                    if( ticksToDisappear < (60000 * GeneTuning.GeneExtractorRegrowingDurationDaysRange.max) ){
                        text = text + addText.Colorize(ColorLibrary.RedReadable);
                    } else {
                        text = text + addText.Colorize(ColorLibrary.BrickRed);
                    }
                }
                list.Add(new FloatMenuOption(text, delegate
                            {
                            if( pawn.ParentHolder is Building_TSS tss ){
                                tss.Eject(pawn);
                                selectPawn(b, pawn);
                            }
                            }, pawn, Color.white));
            }
        }

        if (list.Any()) {
            list.Add(new FloatMenuOption("CPS.ChangeSort".Translate().Colorize(ColorLibrary.Grey), delegate{ sortOrder++; showMenu(b); }));
        } else {
            list.Add(new FloatMenuOption("NoExtractablePawns".Translate(), null));
        }
        Find.WindowStack.Add(new FloatMenu(list));
    }

    static Gizmo newGizmo(Command_Action ca, Building_GeneExtractor b){
        var icon = ca != null ? ca.icon : ContentFinder<Texture2D>.Get("UI/Gizmos/InsertPawn");
        return new Command_Action{
            defaultLabel = "CPS.InsertTestSubject".Translate() + "...",
            icon = icon,
            action = delegate { showMenu(b); }
        };
    }
}

