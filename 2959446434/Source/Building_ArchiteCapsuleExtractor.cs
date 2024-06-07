using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace ArchiteCapsuleExtractor
{

[StaticConstructorOnStartup]
public class Building_ArchiteCapsuleExtractor : Building_Enterable, IThingHolderWithDrawnPawn, IThingHolder
{
    private static readonly double NextCapsuleChance = 0.75;
    private static System.Random rand = new System.Random();

	private int ticksRemaining;

	private int powerCutTicks;

	[Unsaved(false)]
	private CompPowerTrader cachedPowerComp;

	[Unsaved(false)]
	private Texture2D cachedInsertPawnTex;

	[Unsaved(false)]
	private Sustainer sustainerWorking;

	[Unsaved(false)]
	private Effecter progressBar;

	private const int TicksToExtract = 30000;

	private const int NoPowerEjectCumulativeTicks = 60000;

	private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

    private Pawn ContainedPawn
    {
        get {
            if (innerContainer.Count <= 0) return null;
            return (Pawn)innerContainer[0];
        }
    }

	public bool PowerOn => PowerTraderComp.PowerOn;

	public override bool IsContentsSuspended => false;

	private CompPowerTrader PowerTraderComp
	{
		get
		{
			if (cachedPowerComp == null)
			{
				cachedPowerComp = this.TryGetComp<CompPowerTrader>();
			}
			return cachedPowerComp;
		}
	}

	public Texture2D InsertPawnTex
	{
		get
		{
			if (cachedInsertPawnTex == null)
			{
				cachedInsertPawnTex = ContentFinder<Texture2D>.Get("UI/Gizmos/InsertPawn");
			}
			return cachedInsertPawnTex;
		}
	}

	public float HeldPawnDrawPos_Y => DrawPos.y + 3f / 74f;

	public float HeldPawnBodyAngle => base.Rotation.Opposite.AsAngle;

	public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

	public override Vector3 PawnDrawOffset => IntVec3.West.RotatedBy(base.Rotation).ToVector3() / def.size.x;

	public override void PostPostMake()
	{
		if (!ModLister.CheckBiotech("gene extractor"))
		{
			Destroy();
		}
		else
		{
			base.PostPostMake();
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		sustainerWorking = null;
		if (progressBar != null)
		{
			progressBar.Cleanup();
			progressBar = null;
		}
		base.DeSpawn(mode);
	}

	public override void Tick()
	{
		base.Tick();
		innerContainer.ThingOwnerTick();
		if (this.IsHashIntervalTick(250))
		{
			PowerTraderComp.PowerOutput = (base.Working ? (0f - base.PowerComp.Props.PowerConsumption) : (0f - base.PowerComp.Props.idlePowerDraw));
		}
		if (base.Working)
		{
			if (ContainedPawn == null)
			{
				Cancel();
				return;
			}
			if (PowerTraderComp.PowerOn)
			{
				TickEffects();
				if (PowerOn)
				{
					ticksRemaining--;
				}
				if (ticksRemaining <= 0)
				{
					Finish();
				}
				return;
			}
			powerCutTicks++;
			if (powerCutTicks >= 60000)
			{
				Pawn containedPawn = ContainedPawn;
				if (containedPawn != null)
				{
					Messages.Message("GeneExtractorNoPowerEjectedMessage".Translate(containedPawn.Named("PAWN")), containedPawn, MessageTypeDefOf.NegativeEvent, historical: false);
				}
				Cancel();
			}
		}
		else
		{
			if (selectedPawn != null && selectedPawn.Dead)
			{
				Cancel();
			}
			if (progressBar != null)
			{
				progressBar.Cleanup();
				progressBar = null;
			}
		}
	}

	private void TickEffects()
	{
		if (sustainerWorking == null || sustainerWorking.Ended)
		{
			sustainerWorking = SoundDefOf.GeneExtractor_Working.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
		}
		else
		{
			sustainerWorking.Maintain();
		}
		if (progressBar == null)
		{
			progressBar = EffecterDefOf.ProgressBarAlwaysVisible.Spawn();
		}
		progressBar.EffectTick(new TargetInfo(base.Position + IntVec3.North.RotatedBy(base.Rotation), base.Map), TargetInfo.Invalid);
		MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
		if (mote != null)
		{
			mote.progress = 1f - Mathf.Clamp01((float)ticksRemaining / 30000f);
			mote.offsetZ = ((base.Rotation == Rot4.North) ? 0.5f : (-0.5f));
		}
	}

    // comments show changes from vanilla Building_GeneExtractor
    public override AcceptanceReport CanAcceptPawn(Pawn pawn)
    {
        if (/*!pawn.IsColonist &&*/ !pawn.IsSlaveOfColony && !pawn.IsPrisonerOfColony)
        {
            return false;
        }
        if (selectedPawn != null && selectedPawn != pawn)
        {
            return false;
        }
        if (!pawn.RaceProps.Humanlike || pawn.IsQuestLodger())
        {
            return false;
        }
        if (!PowerOn)
        {
            return "NoPower".Translate().CapitalizeFirst();
        }
        if (innerContainer.Count > 0)
        {
            return "Occupied".Translate();
        }
        if (pawn.genes == null || !pawn.genes.GenesListForReading.Any((Gene x) => x.def.passOnDirectly))
        {
            return "PawnHasNoGenes".Translate(pawn.Named("PAWN"));
        }
        if (!pawn.genes.GenesListForReading.Any((Gene x) => x.def.biostatArc == 1)) // changed 0 -> 1
        {
            // it's PawnHasNorchiteGenes now, but message says "{PAWN_nameDef} has no extractable genes.", so it's OK
            return "PawnHasNoNonArchiteGenes".Translate(pawn.Named("PAWN"));
        }
        /* if (pawn.health.hediffSet.HasHediff(HediffDefOf.XenogerminationComa))
           {
           return "InXenogerminationComa".Translate();
           } */
        return true;
    }

	private void Cancel()
	{
		startTick = -1;
		selectedPawn = null;
		sustainerWorking = null;
		powerCutTicks = 0;
		innerContainer.TryDropAll(def.hasInteractionCell ? InteractionCell : base.Position, base.Map, ThingPlaceMode.Near);
	}

    public int countMaxCapsules(Pawn pawn) {
        int r = 0;
        foreach (Gene gene in pawn.genes.GenesListForReading){
            r += gene.def.biostatArc;
        }
        return r;
    }

    private void Finish()
    {
		startTick = -1;
		selectedPawn = null;
		sustainerWorking = null;
		powerCutTicks = 0;

        Pawn pawn = ContainedPawn;
		if (pawn == null)
		{
			return;
		}

        // count extracted capsules
        int nCapsules = 1; // always extract at least one
        int maxCapsules = countMaxCapsules(pawn);
        for(int i=1; i<maxCapsules; i++){
            if(rand.NextDouble() > NextCapsuleChance)
                break;
            nCapsules += 1;
        }

        // remove pawn's archite genes
        List<Gene> genesToRemove = new List<Gene>();
        foreach (Gene gene in pawn.genes.GenesListForReading){
            if(gene.def.biostatArc != 0)
                genesToRemove.Add(gene);
        }
        foreach (Gene gene in genesToRemove){
            pawn.genes.RemoveGene(gene);
        }

        // kill the pawn
        GeneUtility.ExtractXenogerm(pawn, Mathf.RoundToInt(60000f * GeneTuning.GeneExtractorRegrowingDurationDaysRange.RandomInRange));
        if (!pawn.Dead){
            GeneUtility.ExtractXenogerm(pawn, Mathf.RoundToInt(60000f * GeneTuning.GeneExtractorRegrowingDurationDaysRange.RandomInRange));
        }
        // ..twice }:-]
        if (!pawn.Dead){
            pawn.Kill(null, null);
        }

        // eject pawn
		innerContainer.TryDropAll(def.hasInteractionCell ? InteractionCell : base.Position, base.Map, ThingPlaceMode.Near);

        // drop capsules
        if (nCapsules > 0) {
            Thing capsules = ThingMaker.MakeThing(ThingDefOf.ArchiteCapsule);
            capsules.stackCount = nCapsules;
            GenPlace.TryPlaceThing(capsules, def.hasInteractionCell ? InteractionCell : base.Position, base.Map, ThingPlaceMode.Near);
        }

        if (pawn != null){
            Messages.Message("ArchiteCapsuleExtractionComplete".Translate(pawn.Named("PAWN"), nCapsules), MessageTypeDefOf.PositiveEvent);
        }
    }

	public override void TryAcceptPawn(Pawn pawn)
	{
		if ((bool)CanAcceptPawn(pawn))
		{
			selectedPawn = pawn;
			bool num = pawn.DeSpawnOrDeselect();
			if (innerContainer.TryAddOrTransfer(pawn))
			{
				startTick = Find.TickManager.TicksGame;
				ticksRemaining = 30000;
			}
			if (num)
			{
				Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
			}
		}
	}

    protected override void SelectPawn(Pawn pawn)
    {
        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmExtractArchiteCapsulesWillKill".Translate(pawn.Named("PAWN")), delegate {
                    base.SelectPawn(pawn);
                    }));
    }

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
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
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("EnterBuilding".Translate(this), delegate
			{
				SelectPawn(selPawn);
			}), selPawn, this);
		}
		else if (base.SelectedPawn == selPawn && !selPawn.IsPrisonerOfColony)
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("EnterBuilding".Translate(this), delegate
			{
				selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.EnterBuilding, this), JobTag.Misc);
			}), selPawn, this);
		}
		else if (!acceptanceReport.Reason.NullOrEmpty())
		{
			yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + acceptanceReport.Reason.CapitalizeFirst(), null);
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (base.Working)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandCancelExtraction".Translate();
			command_Action.defaultDesc = "CommandCancelExtractionDesc".Translate();
			command_Action.icon = CancelIcon;
			command_Action.action = delegate
			{
				Cancel();
			};
			command_Action.activateSound = SoundDefOf.Designate_Cancel;
			yield return command_Action;
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "DEV: Finish extraction";
				command_Action2.action = delegate
				{
					Finish();
				};
				yield return command_Action2;
			}
			yield break;
		}
		if (selectedPawn != null)
		{
			Command_Action command_Action3 = new Command_Action();
			command_Action3.defaultLabel = "CommandCancelLoad".Translate();
			command_Action3.defaultDesc = "CommandCancelLoadDesc".Translate();
			command_Action3.icon = CancelIcon;
			command_Action3.activateSound = SoundDefOf.Designate_Cancel;
			command_Action3.action = delegate
			{
				innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
				if (selectedPawn.CurJobDef == JobDefOf.EnterBuilding)
				{
					selectedPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				selectedPawn = null;
				startTick = -1;
				sustainerWorking = null;
			};
			yield return command_Action3;
			yield break;
		}
		Command_Action command_Action4 = new Command_Action();
		command_Action4.defaultLabel = "InsertPerson".Translate() + "...";
		command_Action4.defaultDesc = "InsertPersonGeneExtractorDesc".Translate();
		command_Action4.icon = InsertPawnTex;
		command_Action4.action = delegate
        {
            List<Pawn> pawns = new List<Pawn>();
            foreach (Pawn item in base.Map.mapPawns.AllPawnsSpawned) {
                Pawn pawn = item;
                if (pawn.genes != null && CanAcceptPawn(pawn).Accepted){
                    pawns.Add(pawn);
                }
            }
            pawns.SortBy((Pawn pawn) => countMaxCapsules(pawn));

            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (Pawn pawn in pawns)
            {
                string text = pawn.LabelShortCap + ", " + pawn.genes.XenotypeLabelCap;
                int c = countMaxCapsules(pawn);
                if (c == 1) {
                    text = text + " (1 capsule)";
                } else {
                    if ( c > 10 ) {
                        // actually we might extract more than 10 capsules, but probability is less than 1%,
                        // so don't giving false hopes here
                        c = 10;
                    }
                    text = text + " (1-" + c + " capsules)";
                }
                list.Add(new FloatMenuOption(text, delegate
                            {
                            SelectPawn(pawn);
                            }, pawn, Color.white));
            }
            if (!list.Any())
            {
                list.Add(new FloatMenuOption("NoExtractablePawns".Translate(), null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        };
		if (!PowerOn)
		{
			command_Action4.Disable("NoPower".Translate().CapitalizeFirst());
		}
		yield return command_Action4;
	}

	public override void Draw()
	{
		base.Draw();
		if (base.Working && selectedPawn != null && innerContainer.Contains(selectedPawn))
		{
			selectedPawn.Drawer.renderer.RenderPawnAt(DrawPos + PawnDrawOffset, null, neverAimWeapon: true);
		}
	}

	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (selectedPawn != null && innerContainer.Count == 0)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text += "WaitingForPawn".Translate(selectedPawn.Named("PAWN")).Resolve();
		}
		else if (base.Working && ContainedPawn != null)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text = text + "ExtractingXenogermFrom".Translate(ContainedPawn.Named("PAWN")).Resolve() + "\n";
			text = ((!PowerOn) ? (text + "ExtractionPausedNoPower".Translate((60000 - powerCutTicks).ToStringTicksToPeriod().Named("TIME")).Colorize(ColorLibrary.RedReadable)) : (text + "DurationLeft".Translate(ticksRemaining.ToStringTicksToPeriod()).Resolve()));
		}
		return text;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
		Scribe_Values.Look(ref powerCutTicks, "powerCutTicks", 0);
	}
} // class
} // namespace
