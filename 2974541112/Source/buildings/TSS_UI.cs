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

    [Unsaved]
    private Effecter recipeEffecter = null;

    [Unsaved]
    private Sustainer recipeSound = null;

    static readonly Color TopColorNormal = new Color(0.6313726f, 71f / 85f, 0.7058824f);
    static readonly Color TopColorPrisoner = new Color(1f, 61f / 85f, 11f / 85f);

    private List<Pawn> topPawns = null;

    [Unsaved(false)]
    private Graphic cachedTopGraphic;

    private Graphic TopGraphic {
        get
        {
            if (cachedTopGraphic == null || Prefs.DevMode )
            {
                cachedTopGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Building/CPS/GrowthVatTop",
                        ShaderDatabase.Transparent,
                        new Vector2(1,2),
                        ForPrisoners ? TopColorPrisoner : TopColorNormal );
            }
            return cachedTopGraphic;
        }
    }

    static readonly List<Vector3> PawnDrawOffsets = new List<Vector3> {
        new Vector3(-1.5f, 0, 0),
        new Vector3(-0.5f, 0, 1),
        new Vector3( 0.5f, 0, 1),
        new Vector3( 1.5f, 0, 0),
    };

    private static readonly Vector2 BarSize = new Vector2(0.72f, 0.13f);
    private static readonly Material UnfilledMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f, 0.65f), ShaderDatabase.MetaOverlay);
    private static readonly Material FilledMat = SolidColorMaterials.NewSolidColorMaterial(new ColorInt(138, 3, 3).ToColor, ShaderDatabase.MetaOverlay);

    public override Color DrawColorTwo {
        get {
            return ForPrisoners ? TopColorPrisoner : TopColorNormal;
        }
    }

    public override Color DrawColor {
        get {
            return DrawColorTwo;
        }
    }

    public override void Draw()
    {
        base.Draw();

        if( topPawns != null && topPawns.Any() ){
            int i = 0;
            foreach( Pawn pawn in topPawns ){
                pawn.Drawer.renderer.RenderPawnAt(DrawPos + PawnDrawOffsets[i], null, neverAimWeapon: true);
                i++;
                if( i == 4 ){
                    // should not be here, just in case
                    break;
                }
            }
        }

        for( int i=0; i<4; i++ ){
            TopGraphic.Draw(DrawPos + PawnDrawOffsets[i] + Altitudes.AltIncVect * 2f, base.Rotation, this);
        }

        // draw red recipe progressbar
        if( currentBillReport != null && (int)Find.CameraDriver.CurrentZoom == 0){
            GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
            r.center = DrawPos;
            r.center.z += 0.25f;
            r.size = BarSize;
            r.fillPercent = 1f - Mathf.Clamp01(currentBillReport.workLeft / currentBillReport.workTotal);
            r.filledMat = FilledMat;
            r.unfilledMat = UnfilledMat;
            r.margin = 0.08f;
            GenDraw.DrawFillableBar(r);
        }
    }

    // draw assigned/total slots count
    public override void DrawGUIOverlay()
    {
        if ((int)Find.CameraDriver.CurrentZoom == 0){
            // don't mix up refilling progress bar and capacity label
            GenMapUI.DrawThingLabel(GenMapUI.LabelDrawPosFor(this, 0), nPawns + "/" + MaxSlots, GenMapUI.DefaultThingLabelColor);
        }
    }

    private static readonly Texture2D InsertIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/CPS_InsertPawn");
    private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

    public override IEnumerable<Gizmo> GetGizmos()
    {
        tmpQueuedPawns.Clear();
        foreach (var gizmo in base.GetGizmos()) {
            yield return gizmo;
        }
        if (base.Faction != Faction.OfPlayer) {
            yield break;
        }

        {
            Command_Toggle ct = new Command_Toggle();
            ct.defaultLabel = "CommandBedSetForPrisonersLabel".Translate();
            ct.defaultDesc = "CommandBedSetForPrisonersDesc".Translate();
            ct.icon = ContentFinder<Texture2D>.Get("UI/Commands/ForPrisoners");
            ct.isActive = () => ForPrisoners;
            ct.toggleAction = delegate
            {
                SetBedOwnerTypeByInterface((!ForPrisoners) ? BedOwnerType.Prisoner : BedOwnerType.Colonist);
            };
            //            if (!RoomCanBePrisonCell(this.GetRoom()) && !ForPrisoners)
            //            {
            //                ct.Disable("CommandBedSetForPrisonersFailOutdoors".Translate());
            //            }
            ct.hotKey = KeyBindingDefOf.Misc3;
            ct.turnOffSound = null;
            ct.turnOnSound = null;
            yield return ct;
        }

        {
            Command_Action ca = new Command_Action();
            ca.defaultLabel = "InsertPerson".Translate() + "...";
            ca.icon = InsertIcon;
            ca.action = delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (Pawn item in base.Map.mapPawns.AllPawnsSpawned)
                {
                    Pawn pawn = item;
                    AcceptanceReport acceptanceReport = CanAcceptPawn(pawn);
                    string text = pawn.LabelShortCap;
                    if (acceptanceReport.Accepted) {
                        list.Add(new FloatMenuOption(text, delegate { SelectPawn(pawn); }, pawn, Color.white));
                    }
                }
                if (!list.Any()) {
                    list.Add(new FloatMenuOption("NoViablePawns".Translate(), null)); // Biotech
                }
                Find.WindowStack.Add(new FloatMenu(list));
            };
            if (!PowerOn)
            {
                ca.Disable("NoPower".Translate().CapitalizeFirst());
            }
            yield return ca;
        }

        if( SelectedPawns.Any() ) {
            Command_Action c = new Command_Action();
            c.defaultLabel = "CommandCancelLoad".Translate();
            c.icon = CancelIcon;
            c.activateSound = SoundDefOf.Designate_Cancel;
            c.action = delegate
            {
                List<Pawn> pawns = new List<Pawn>();
                pawns.AddRange(selectedPawns);
                foreach( Pawn p in pawns ){
                    if( p.CurJobDef == VDefOf.EnterMultiBuilding ){
                        p.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    } else if ( p.CarriedBy != null && p.CarriedBy.CurJobDef == VDefOf.CarryToMultiBuilding ) {
                        p.CarriedBy.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                }
                SelectedPawns.Clear();
            };
            yield return c;
        }

        if (ModsConfig.BiotechActive) {
            yield return new Gizmo_ResourceHemogen(this);
        }
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    {
        tmpQueuedPawns.Clear();
        foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
        {
            yield return floatMenuOption;
        }
        if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
        {
            yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            yield break;
        }
        AcceptanceReport acceptanceReport = CanAcceptPawn(selPawn);
        if (acceptanceReport.Accepted)
        {
            yield return new FloatMenuOption("EnterBuilding".Translate(this), delegate { SelectPawn(selPawn); });
        }
        else if (SelectedPawns.Contains(selPawn) && !selPawn.IsPrisonerOfColony)
        {
            yield return new FloatMenuOption("EnterBuilding".Translate(this), delegate {
                    selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(VDefOf.EnterMultiBuilding, this), JobTag.Misc);
                    });
        }
        else if (!acceptanceReport.Reason.NullOrEmpty())
        {
            yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + acceptanceReport.Reason.CapitalizeFirst(), null);
        }
    }

    // only needed between a call to GetMultiSelectFloatMenuOptions() and a click on popped-up item
    private static List<Pawn> tmpQueuedPawns = new List<Pawn>();

    // called only if multiple _colonists_ are selected
    // selected prisoners are just ignored
    public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(List<Pawn> selPawns)
    {
        tmpQueuedPawns.Clear();
        foreach( Pawn p in selPawns ){
            AcceptanceReport acceptanceReport = CanAcceptPawn(p);
            if( !acceptanceReport.Accepted ){
                if( !acceptanceReport.Reason.NullOrEmpty() ){
                    yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + acceptanceReport.Reason.CapitalizeFirst(), null);
                }
                yield break;
            }
        }
        if( nPawns + selPawns.Count > MaxSlots ){
            var reason = "Occupied".Translate();
            yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + reason.CapitalizeFirst(), null);
            yield break;
        }

        tmpQueuedPawns.AddRange(selPawns);

        yield return new FloatMenuOption("EnterBuilding".Translate(this), delegate {
                // selPawns list is empty here
                foreach( Pawn p in tmpQueuedPawns ){
                    p.jobs.TryTakeOrderedJob(JobMaker.MakeJob(VDefOf.EnterMultiBuilding, this), JobTag.Misc);
                }
                tmpQueuedPawns.Clear();
                });
    }

    public float NutritionConsumedPerDay {
        get {
            float num = 0;
            foreach( Thing t in innerContainer ){
                if( t is Pawn pawn && pawn.needs?.food != null ){
                    num += pawn.needs.food.FoodFallPerTickAssumingCategory(HungerCategory.Fed) * 60000f;
                }
            }
            return num * timeSpeed;
        }
    }

    public override string GetInspectString(){
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        if( SelectedPawns.Any() ){
            List <string> names = new List<string>();
            foreach( Pawn p in SelectedPawns ){
                names.Add(p.NameShortColored.Resolve());
            }
            stringBuilder.AppendLineIfNotEmpty().Append( "WaitingFor".Translate().ToString() + ": " + names.ToCommaList() );
        }
        if( nPawns > 0 ){
            List <string> names = new List<string>();
            foreach( Thing t in innerContainer ){
                if( t is Pawn p ){
                    names.Add(p.NameShortColored.Resolve());
                }
            }
            stringBuilder.AppendLineIfNotEmpty().Append( "CasketContains".Translate().ToString() + ": " + names.ToCommaList() );

            stringBuilder.AppendLineIfNotEmpty().Append("Nutrition".Translate()).Append(": ")
                .Append(TotalNutritionAvailable.ToStringByStyle(ToStringStyle.FloatMaxOne));
            stringBuilder.Append(" (-").Append("PerDay".Translate(NutritionConsumedPerDay.ToString("F1"))).Append(")");
        } else {
            stringBuilder.AppendLineIfNotEmpty().Append("Nutrition".Translate()).Append(": ")
                .Append(TotalNutritionAvailable.ToStringByStyle(ToStringStyle.FloatMaxOne));
        }
        if( PowerOn && currentBillReport != null ){
            stringBuilder.AppendLineIfNotEmpty().Append("TSS_BillReport".Translate(JobUtility.GetResolvedJobReport(currentBillReport.medBill.recipe.jobString, currentBillReport.medBill.GiverPawn), currentBillReport.workLeft.ToStringWorkAmount()));
        }
        if( Prefs.DevMode ){
            stringBuilder.AppendLineIfNotEmpty().Append("Ticks without power: " + (Find.TickManager.TicksGame - lastTickWithPower) );
        }
        return stringBuilder.ToString();
    }

}
