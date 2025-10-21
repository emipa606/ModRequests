using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace zed_0xff.CPS;

public partial class Building_TSS : Building_MultiEnterable, IStoreSettingsParent, IThingHolderWithDrawnPawn, IBillTab, IBillGiver {

    // thx to Project RimFactory
    protected class BillReport : IExposable {
        public Bill bill;
        public Bill_Medical medBill;
        public float workLeft;
        public float workTotal;
        public List<Thing> ingredients;
      
        // for Scribe
        public BillReport() {}

        public BillReport(Bill b, Bill_Medical bm, List<Thing> ingrs) {
            bill = b;
            medBill = bm;
            workTotal = b.recipe.WorkAmountTotal(null);
            workLeft = workTotal;
            ingredients = ingrs;
        }

        public void ExposeData() {
            Scribe_References.Look(ref bill, "bill");
            Scribe_References.Look(ref medBill, "medBill");
            Scribe_Values.Look(ref workLeft, "workLeft");
            Scribe_Values.Look(ref workTotal, "workTotal");
            // deep because ingredients are not saved anywhere else (at least for hemogen transfusion)
            Scribe_Collections.Look(ref ingredients, "ingredients", LookMode.Deep);
        }
    }

    static Dictionary<RecipeDef, PatientType> recipeMap = new Dictionary<RecipeDef, PatientType>();

    protected BillReport currentBillReport;

    private BillStack billStack = null;
    public BillStack BillStack {
        get {
            if( billStack == null )
                billStack = new BillStack(this);
            return billStack;
        }
    }

    public IEnumerable<RecipeDef> AllRecipes => this.def.AllRecipes;
    public IEnumerable<RecipeDef> GetAllRecipes() {
        return this.AllRecipes;
    }

    public bool CurrentlyUsableForBills() {
        return false;
    }

    public bool UsableForBillsAfterFueling() {
        return false;
    }

    public void Notify_BillDeleted(Bill bill) {
    }

    public IEnumerable<IntVec3> IngredientStackCells => Enumerable.Empty<IntVec3>();

    protected virtual void ProduceItems() {
        if( currentBillReport?.medBill?.GiverPawn == null || !innerContainer.Contains(currentBillReport.medBill.GiverPawn) ) return;
    }

    protected IEnumerable<Bill> AllBillsShouldDoNow => from b in BillStack.Bills
                                                       where b.ShouldDoNow()
                                                       select b;

    public static readonly MethodInfo _TryFindBestBillIngredientsInSet =
        typeof(WorkGiver_DoBill).GetMethod("TryFindBestBillIngredientsInSet", BindingFlags.NonPublic | BindingFlags.Static);

    bool TryFindBestBillIngredientsInSet(List<Thing> accessibleThings, Bill b, List<ThingCount> chosen)
    {
        //TryFindBestBillIngredientsInSet Expects a List of Both Avilibale & Allowed Things as "accessibleThings"
        List<IngredientCount> missing = new List<IngredientCount>(); // Needed for 1.4
        return (bool)_TryFindBestBillIngredientsInSet.Invoke(null, new object[] { accessibleThings, b, chosen, new IntVec3(), false, missing });
    }

    enum PatientType {
        Donor_HemogenFarmPrisoner,
        Donor_Prisoner,
        Donor_Slave,
        Donor_Any,

        Recipient_Colonist_50,
        Recipient_Colonist_100,
        Recipient_All_50
    };

    // TryGetNextBill returns a new BillReport to start if one is available
    protected BillReport TryGetNextBill()
    {
        IEnumerable<Bill> allBills = AllBillsShouldDoNow;
        if( !allBills.Any() ) return null;

        var dict = new Dictionary<PatientType, Pawn>();
        foreach( Thing t in innerContainer ){
            if( t is Pawn pawn ){
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
                if( hediff == null ){
                    // donor
                    if( pawn.genes != null && pawn.genes.HasGene(GeneDefOf.Hemogenic)) continue;

                    if( pawn.IsPrisonerOfColony ){
                        dict[PatientType.Donor_Prisoner] = pawn;
                        if( pawn?.guest?.interactionMode == PrisonerInteractionModeDefOf.HemogenFarm ){
                            dict[PatientType.Donor_HemogenFarmPrisoner] = pawn;
                        }
                    } else if( pawn.IsSlaveOfColony ){
                        dict[PatientType.Donor_Slave] = pawn;
                    }
                    dict[PatientType.Donor_Any] = pawn;
                } else {
                    // recipient
                    if( hediff.Severity < 0.05f ) continue;

                    if( hediff.Severity > 0.49f ){
                        dict[PatientType.Recipient_All_50] = pawn;
                    }
                    if( !pawn.IsPrisonerOfColony && !pawn.IsSlaveOfColony ){
                        if( hediff.Severity > 0.49f ){
                            dict[PatientType.Recipient_Colonist_50] = pawn;
                        }
                        dict[PatientType.Recipient_Colonist_100] = pawn;
                    }
                }
            }
        }

        foreach (Bill b in allBills) {
            PatientType dtype;
            if( recipeMap.TryGetValue(b.recipe, out dtype) ){
                Pawn pawn;
                if(dict.TryGetValue(dtype, out pawn)){
                    if( b.recipe.Worker.AvailableReport(pawn).Accepted ){
                        List<Thing> ingredients = new List<Thing>();
                        foreach( var ingr in b.recipe.ingredients ){
                            if( ingr.FixedIngredient == ThingDefOf.HemogenPack ){
                                ingredients.Add(ThingMaker.MakeThing(ThingDefOf.HemogenPack));
                                ConsumeAnyHemogen(); // Recipe_BloodTransfusion_TSS checks that TSS has enough
                            }
                        }
                        Bill_Medical bm = HealthCardUtility.CreateSurgeryBill(
                                pawn,
                                b.recipe,
                                null, // bodyPart
                                null, // uniqueIngredients
                                false // sendMessages
                                );
                        return new BillReport(b, bm, ingredients);
                    }
                }
            }
        }
        return null;
    }

    private void Tick_Bills(){
        if (PowerOn && this.IsHashIntervalTick(10)){
            if (currentBillReport != null) {
                currentBillReport.workLeft -= 10f*0.5f; /* ProductionSpeedFactor */
                if (currentBillReport.workLeft <= 0) {
                    ProduceItems();
                    try {
                        // HACK: bc we don't create a virtual pawn as RimFactory does, the record will be as if pawn operated on themselves %)
                        Pawn billDoer = currentBillReport.medBill.GiverPawn;
                        int op0 = billDoer.records.GetAsInt(RecordDefOf.OperationsPerformed);
                        // tick workbench bill counters, like 'make 10 things'
                        currentBillReport.bill.Notify_IterationCompleted(billDoer, currentBillReport.ingredients);
                        // apply medical bill effects and results
                        currentBillReport.medBill.Notify_IterationCompleted(billDoer, currentBillReport.ingredients);
                        if( billDoer.records.GetAsInt(RecordDefOf.OperationsPerformed) != op0 ){
                            // fix them back
                            billDoer.records.AddTo(RecordDefOf.OperationsPerformed, -1);
                        }
                    } catch (Exception ex) {
                        Log.Error("[!] TSS: error finishing " + currentBillReport.bill + ": " + ex);
                    }
                    currentBillReport = null;
                }
            } else if ( this.IsHashIntervalTick(60) ) {
                //Start Bill if Possible
                currentBillReport = TryGetNextBill();
            }
        }

        // effects
        if (currentBillReport != null && PowerOn){
            if ( recipeEffecter == null && CPSMod.Settings.tss.effects ) {
                recipeEffecter = currentBillReport.bill.recipe?.effectWorking?.Spawn();
            }
            if ( recipeSound == null && CPSMod.Settings.tss.sounds ) {
                recipeSound = currentBillReport.bill.recipe?.soundWorking?.TrySpawnSustainer(this);
            }
            if( (int)Find.CameraDriver.CurrentZoom == 0 ){
                recipeEffecter?.EffectTick(this, this);
            }
            recipeSound?.SustainerUpdate();
        } else {
            if (recipeEffecter != null) {
                recipeEffecter.Cleanup();
                recipeEffecter = null;
            }
            if (recipeSound != null) {
                recipeSound.End();
                recipeSound = null;
            }
        }
    }
}
