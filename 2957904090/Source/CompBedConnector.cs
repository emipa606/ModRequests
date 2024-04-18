using PipeSystem;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using VNPE;
using Verse;
using UnityEngine;

namespace zed_0xff.VNPE;

// code partially copied from Vanilla Nutrient Paste Expanded (c) Oskar Potocki
// https://steamcommunity.com/sharedfiles/filedetails/?id=2920385763

public class CompBedConnector : ThingComp {
    CompProperties_BedConnector Props => (CompProperties_BedConnector)props;

    IPlugin dbh = null;
    CompPowerTrader powerComp = null;
    CompResource pasteComp = null;
    CompResource hemogenComp = null;

    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
        if( parent is MinifiedThing || parent == null || !parent.Spawned )
            return;

        powerComp = parent.GetComp<CompPowerTrader>();

        foreach (CompResource comp in parent.GetComps<CompResource>()) {
            if (comp?.Props?.pipeNet?.defName == "VRE_HemogenNet") {
                hemogenComp = comp;
            }
            if (comp?.Props?.pipeNet?.defName == "VNPE_NutrientPasteNet") {
                pasteComp = comp;
            }
        }

        if (ModLister.HasActiveModWithName("Dubs Bad Hygiene")) {
            Type t = GenTypes.GetTypeInAnyAssembly("zed_0xff.VNPE.Plugin_DBH");
            if( t != null ){
                dbh = (IPlugin)Activator.CreateInstance(t, new object[] { parent });
            }
        }
    }

    public override void PostDeSpawn(Map map){
        dbh = null;
        powerComp = null;
        pasteComp = null;
        hemogenComp = null;
    }

    void FeedOccupant(){
        var net = pasteComp.PipeNet;
        if( net.Stored < 1 )
            return;

        var occupants = CurOccupants.ToList();
        for (int o = 0; o < occupants.Count; o++)
        {
            var pawn = occupants[o];
            if (pawn.needs.food.CurLevelPercentage <= 0.4)
            {
                net.DrawAmongStorage(1, net.storages);
                var meal = ThingMaker.MakeThing(ThingDefOf.MealNutrientPaste);
                if (meal.TryGetComp<CompIngredients>() is CompIngredients ingredients)
                {
                    for (int s = 0; s < net.storages.Count; s++)
                    {
                        var parent = net.storages[s].parent;
                        if (parent.TryGetComp<CompRegisterIngredients>() is CompRegisterIngredients storageIngredients)
                        {
                            for (int ig = 0; ig < storageIngredients.ingredients.Count; ig++)
                                ingredients.RegisterIngredient(storageIngredients.ingredients[ig]);
                        }
                    }
                }

                var ingestedNum = meal.Ingested(pawn, pawn.needs.food.NutritionWanted);
                pawn.needs.food.CurLevel += ingestedNum;
                pawn.records.AddTo(RecordDefOf.NutritionEaten, ingestedNum);
            }
        }
    }

    ref ConnectedBedSettings.TypeSettings GetSettingForPawn(Pawn pawn){
        if( pawn.IsPrisonerOfColony ){
            return ref ModConfig.Settings.prisoners;
        } else if ( pawn.IsSlaveOfColony ){
            return ref ModConfig.Settings.slaves;
        } else if( pawn.IsColonist ){
            return ref ModConfig.Settings.colonists;
        }
        return ref ModConfig.Settings.others;
    }

    IEnumerable<Pawn> CurOccupants {
        get {
            if( parent is Building_Bed bed ){
                // I'm a part of connected bed
                foreach( Pawn pawn in bed.CurOccupants )
                    yield return pawn;
            } else {
                // I'm a separate building, built over a bed or smth
                foreach( Thing t in parent.Map.thingGrid.ThingsListAt(parent.Position) ){
                    if( t is Pawn p && !p.Dead && p.GetPosture().Laying() ){
                        yield return p;
                    } else if( t is Building_Enterable be && be.SelectedPawn is Pawn p2 && p2.ParentHolder == be ){
                        yield return p2;
                    }
                }
            }
        }
    }

    void TransfuseBlood(){
        var net = hemogenComp.PipeNet;
        if( net.Stored < 1 )
            return;

        float stored = net.Stored;

        var occupants = CurOccupants.ToList();
        for (int o = 0; o < occupants.Count; o++)
        {
            var pawn = occupants[o];
            var bloodLossHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if( bloodLossHediff == null) continue;

            var ts = GetSettingForPawn(pawn);

            if( bloodLossHediff.Severity < (1.0f - ts.transfuseIfLess) ) continue;

            while( bloodLossHediff.Severity > (1.0f - ts.fillUpTo) && stored >= 1 ){
                if( ModConfig.Settings.general.debugLog ) Log.Message("[d] ConnectedBed: transfuse " + pawn + " " + ts.fillUpTo);
                net.DrawAmongStorage(1, net.storages);
                bloodLossHediff.Severity -= 0.35f; // see RimWorld/Recipe_BloodTransfusion.cs
                if (pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>() != null)
                {
                    GeneUtility.OffsetHemogen(pawn, JobGiver_GetHemogen.HemogenPackHemogenGain);
                }
                stored -= 1;
            }

            if( stored < 1 ) break;
        }
    }

    void DrawBlood(){
        if( !ModConfig.Settings.prisoners.draw ) return;

        var net = hemogenComp.PipeNet;

        var occupants = CurOccupants.ToList();
        for (int o = 0; o < occupants.Count; o++)
        {
            var pawn = occupants[o];
            if( !pawn.IsPrisonerOfColony ) continue;
            if( pawn.guest.interactionMode != PrisonerInteractionModeDefOf.HemogenFarm ) continue;
            if( !RecipeDefOf.ExtractHemogenPack.Worker.AvailableOnNow(pawn) ) continue;
            if( pawn.health.hediffSet.HasHediff(HediffDefOf.BloodLoss) ) continue;

            // should be created by Pawn_GuestTracker::GuestTrackerTick
            if( !pawn.BillStack.Bills.Any((Bill x) => x.recipe == RecipeDefOf.ExtractHemogenPack) ) continue;

            float cap = net.AvailableCapacity;
            if( cap > 1 ){
                float fillRate = net.Stored / (net.Stored + cap);
                if( fillRate >= ModConfig.Settings.general.maxFillRate )
                    return;

                if( ModConfig.Settings.general.debugLog ) Log.Message("[d] ConnectedBed: draw " + pawn);

                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn);
                hediff.Severity = 0.59f; // 0.6 pops up unwanted health alert
                pawn.health.AddHediff(hediff);
                net.DistributeAmongStorage(1);
                if (IsViolationOnPawn(pawn, Faction.OfPlayer))
                {
                    ReportViolation(pawn, pawn.HomeFaction, -1, HistoryEventDefOf.ExtractedHemogenPack);
                }
            }

            if( net.storages.Any() ){
                // remove pawn's all ExtractHemogenPack bills, if any
                List<Bill> bills = pawn.BillStack.Bills.Where((Bill b) => b.recipe == RecipeDefOf.ExtractHemogenPack).ToList();
                foreach (Bill b in bills) {
                    pawn.BillStack.Delete(b);
                }
            } else {
                // bed is not connected to the network, leave bill as is for manual extraction
            }
        }
    }

    bool IsViolationOnPawn(Pawn pawn, Faction billDoerFaction) {
        if (pawn.Faction == billDoerFaction && !pawn.IsQuestLodger())
        {
            return false;
        }
        return true;
    }

    void ReportViolation(Pawn pawn, Faction factionToInform, int goodwillImpact, HistoryEventDef overrideEventDef = null)
    {
        if (factionToInform != null )
        {
            Faction.OfPlayer.TryAffectGoodwillWith(factionToInform, goodwillImpact, canSendMessage: true, !factionToInform.temporary, overrideEventDef ?? HistoryEventDefOf.PerformedHarmfulSurgery);
            QuestUtility.SendQuestTargetSignals(pawn.questTags, "SurgeryViolation", pawn.Named("SUBJECT"));
        }
    }

    public override void CompTickRare() {
        if (!powerComp.PowerOn)
            return;

        if( pasteComp != null)
            FeedOccupant();

        if( hemogenComp != null){
            TransfuseBlood();
            DrawBlood();
        }

        if( dbh != null ){
            foreach( Thing t in CurOccupants.ToList() ){
                if( t is Pawn pawn ){
                    dbh.ProcessPawn(pawn);
                }
            }
        }

        base.CompTickRare();
    }

    public override void PostDraw(){
        base.PostDraw();
        if( Props.graphicData == null ) return;

        Mesh mesh = Props.graphicData.Graphic.MeshAt(parent.Rotation);
        Quaternion quat = Quaternion.Euler(Vector3.up * parent.Rotation.AsAngle);
        Vector3 loc = parent.DrawPos;
        loc += Props.graphicData.Graphic.DrawOffset(parent.Rotation);
        Material mat = Props.graphicData.Graphic.MatAt(parent.Rotation);
        var matrix = Matrix4x4.TRS(loc, quat, new Vector3(1, 1, 1));
        Graphics.DrawMesh(mesh, matrix, mat, 0);
    }

    public override string CompInspectStringExtra() {
        if( parent is MinifiedThing || parent is null || !parent.Spawned )
            return null;

        return "BedConnector: " + CurOccupants.Count() + " pawn(s)";
    }
}

