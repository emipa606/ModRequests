using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Grammar;
using Verse.Noise;
using Verse.Profile;
using Verse.Sound;
using Verse.Steam;
using UnityEngine;
using System.Diagnostics;

namespace NAT
{
	public class CompProperties_UsableByRust : CompProperties
	{
		public string jobReport = null;

		public int useDuration = 80;

		public HediffDef hediff;

		public bool replaceHediff = true;

		public int? duration;

		public float? severity;

		public bool destroyAfterUse;
		public CompProperties_UsableByRust()
		{
			compClass = typeof(CompUsableByRust);
		}
	}
	public class CompUsableByRust : ThingComp
    {
		public CompProperties_UsableByRust Props => (CompProperties_UsableByRust)props;
		public virtual string JobReport => Props.jobReport ?? "NAT_UseItem".Translate();

		public virtual AcceptanceReport CanBeUsedBy(RustedPawn rust)
        {
			if(!Props.replaceHediff && Props.hediff != null && rust.health.hediffSet.GetFirstHediffOfDef(Props.hediff) != null)
            {
				return false;
            }
			return true;
        }

		public virtual void UsedBy(RustedPawn rust)
        {
			if(Props.hediff != null)
            {
				Hediff hediff = rust.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
				if(hediff != null)
                {
					rust.health.RemoveHediff(hediff);
                }
				Hediff hediff2 = rust.health.AddHediff(Props.hediff);
				if(Props.duration != null)
                {
					hediff2.TryGetComp<HediffComp_Disappears>()?.SetDuration(Props.duration.Value);
                }
				if (Props.severity != null)
				{
					hediff2.Severity = Props.severity.Value;
				}
			}
        }
    }

	public class JobDriver_UseItemByRust : JobDriver
	{
		private int useDuration = -1;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref useDuration, "useDuration", 0);
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			useDuration = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUsableByRust>().Props.useDuration;
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			CompUsableByRust comp = base.TargetThingA.TryGetComp<CompUsableByRust>();
			this.FailOn(() => comp == null);
			this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
			if(pawn is RustedPawn rust && rust.Controllable)
            {
				this.FailOn(() => !base.TargetThingA.TryGetComp<CompUsableByRust>().CanBeUsedBy(rust).Accepted);
				yield return Toils_Goto.GotoThing(TargetIndex.A, base.TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch);
				Toil toil1 = Toils_General.Wait(useDuration, TargetIndex.A);
				toil1.WithProgressBarToilDelay(TargetIndex.A);
				toil1.FailOnDespawnedNullOrForbidden(TargetIndex.A);
				toil1.FailOnCannotTouch(TargetIndex.A, base.TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch);
				toil1.handlingFacing = true;
				yield return toil1;
				Toil use = ToilMaker.MakeToil("Use");
				use.initAction = delegate
				{
					comp.UsedBy(rust);
					if (comp.Props.destroyAfterUse)
					{
						comp.parent.Destroy();
					}
				};
				use.defaultCompleteMode = ToilCompleteMode.Instant;
				yield return use;
            }
            else
            {
				this.FailOn(() => true);
			}
		}
	}
	public class ScenPart_StartingRustedSoldier : ScenPart
	{
		private PawnKindDef pawnKind;

		private IEnumerable<PawnKindDef> PossibleMechs => DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef td) => td.GetModExtension<RustedPawnExtention>()?.scenarioAvailable == true);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref pawnKind, "pawnKind");
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, 2f * ScenPart.RowHeight + 4f);
			if (Widgets.ButtonText(new Rect(scenPartRect.xMin, scenPartRect.yMin, scenPartRect.width, ScenPart.RowHeight), (pawnKind != null) ? pawnKind.LabelCap : "Random".Translate()))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				list.Add(new FloatMenuOption("Random".Translate().CapitalizeFirst(), delegate
				{
					pawnKind = null;
				}));
				foreach (PawnKindDef possibleMech in PossibleMechs)
				{
					PawnKindDef localKind = possibleMech;
					list.Add(new FloatMenuOption(localKind.LabelCap, delegate
					{
						pawnKind = localKind;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public override void Randomize()
		{
			pawnKind = PossibleMechs.RandomElement();
		}

		public override string Summary(Scenario scen)
		{
			return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith", ScenPart_StartingThing_Defined.PlayerStartWithIntro);
		}

		public override IEnumerable<string> GetSummaryListEntries(string tag)
		{
			if (tag == "PlayerStartsWith")
			{
				yield return "NAT_RustedSoldier".Translate().CapitalizeFirst() + ": " + pawnKind.LabelCap;
			}
		}

		public override IEnumerable<Thing> PlayerStartingThings()
		{
			if (pawnKind == null)
			{
				pawnKind = PossibleMechs.RandomElement();
			}
			//PawnGenerationRequest request = new PawnGenerationRequest(pawnKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
			Pawn pawn = PawnGenerator.GeneratePawn(pawnKind, Faction.OfPlayer);
			yield return pawn;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ ((pawnKind != null) ? pawnKind.GetHashCode() : 0);
		}
	}
	public class NewAnomalyThreatsSettings : ModSettings
    {

		public bool rustedSoldierName_Draft = true;
		public bool rustedSoldierName_NoDraft = true;
		public bool rustedSoldierWeaponChange = true;
		public bool rustedSoldierDeathNotification = true;
		public bool allowEndGameRaid = true;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref rustedSoldierName_Draft, "rustedSoldierName_Draft", true);
			Scribe_Values.Look(ref rustedSoldierName_NoDraft, "rustedSoldierName_Draft", true);
			Scribe_Values.Look(ref rustedSoldierWeaponChange, "rustedSoldierWeaponChange", true);
			Scribe_Values.Look(ref rustedSoldierDeathNotification, "rustedSoldierDeathNotification", true);
			Scribe_Values.Look(ref allowEndGameRaid, "allowEndGameRaid", true);
			base.ExposeData();
		}
	}

	public class NewAnomalyThreatsMod : Mod
	{

		NewAnomalyThreatsSettings settings;

		public NewAnomalyThreatsMod(ModContentPack content) : base(content)
		{
			this.settings = GetSettings<NewAnomalyThreatsSettings>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.Begin(inRect);
			listingStandard.CheckboxLabeled("NAT_Setting_NameDraft".Translate(), ref settings.rustedSoldierName_Draft, "NAT_Setting_NameDraft_Desc".Translate());
			listingStandard.CheckboxLabeled("NAT_Setting_NameNoDraft".Translate(), ref settings.rustedSoldierName_NoDraft, "NAT_Setting_NameNoDraft_Desc".Translate());
			//listingStandard.CheckboxLabeled("NAT_Setting_WeaponChange".Translate(), ref settings.rustedSoldierWeaponChange, "NAT_Setting_WeaponChange_Desc".Translate());
			listingStandard.CheckboxLabeled("NAT_Setting_DeathNotification".Translate(), ref settings.rustedSoldierDeathNotification, "NAT_Setting_DeathNotification_Desc".Translate());
			listingStandard.CheckboxLabeled("NAT_Setting_AllowRaid".Translate(), ref settings.allowEndGameRaid, "NAT_Setting_AllowRaid_Desc".Translate());
			listingStandard.End();
			base.DoSettingsWindowContents(inRect);
		}
		public override string SettingsCategory()
		{
			return "New Anomaly Threats";
		}
	}

	public class JobDriver_ChangeWeapon : JobDriver
	{
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
			return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !(pawn is RustedPawn));
			RustedPawn rust = pawn as RustedPawn;
			
			this.FailOn(() => rust.weaponDef == null);
			this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
			Toil end = Toils_General.Label();
			yield return Toils_General.Wait(2500).WithProgressBarToilDelay(TargetIndex.A);
			Toil toil1 = ToilMaker.MakeToil("NAT_MakeWeapon");
			toil1.initAction = delegate
			{
				if (pawn.equipment.Primary != null)
				{
					pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
				}
				pawn.equipment.AddEquipment(ThingMaker.MakeThing(rust.weaponDef) as ThingWithComps);
				rust.weaponDef.soundInteract.PlayOneShot(pawn);
				rust.weaponDef = null;
			};
			toil1.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil1;
			yield return end;
		}
	}
	public class IncidentWorker_SkullArrival : IncidentWorker
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (!ModsConfig.AnomalyActive)
			{
				return false;
			}
			Map map = (Map)parms.target;
            if (!map.mapPawns.AllHumanlikeSpawned.Where((Pawn p) => p.Faction == Faction.OfPlayer).TryRandomElement(out Pawn pawn))
            {
				return false;
            }
			Thing t = ThingMaker.MakeThing(ThingDefOf.Skull);
			t.TryGetComp<CompHasSources>().AddSource(pawn.LabelShort);
			DropPodUtility.DropThingsNear(CellFinder.StandableCellNear(pawn.Position, map, 45f), map, new List<Thing> { t }, 90, false, true, true, false, false);
			Find.LetterStack.ReceiveLetter("NAT_SkullArrival".Translate(), "NAT_SkullArrival_Desc".Translate(pawn.Named("PAWN")), LetterDefOf.NeutralEvent, t);
			return true;
		}
	}
	public class JobGiver_ExtinguishSelfImmediately : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Fire fire = (Fire)pawn.GetAttachment(ThingDefOf.Fire);
			if (fire != null)
			{
				return JobMaker.MakeJob(JobDefOf.ExtinguishSelf, fire);
			}
			return null;
		}
	}

	public class CollectorSpawner : Thing
	{
		public Pawn Collector;

		public int ticksLeft;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref Collector, "Collector");
			Scribe_Values.Look(ref ticksLeft, "ticksLeft");
		}

		public void ReturnCollector()
        {
			GenSpawn.Spawn(Collector, this.Position, this.Map);
			Collector = null;
			this.Destroy();
		}

        public override void Tick()
        {
			ticksLeft -= 1;
			if (ticksLeft <= 0)
            {
				ReturnCollector();

			}
        }
    }
	public class CompProperties_NotesTriggerInteractor : CompProperties_Interactable
	{
		public CompProperties_NotesTriggerInteractor()
		{
			compClass = typeof(CompNotesTriggerInteractor);
		}
	}
	public class CompNotesTriggerInteractor : CompInteractable
	{
		private CompStudyUnlocks studyComp;

		public int descPawns;

		public List<Pawn> stealedPawns;

		public List<Thing> stealedThings;

		private CompStudyUnlocks StudyComp => studyComp ?? (studyComp = parent.GetComp<CompStudyUnlocks>());

		public override string ExposeKey => "Interactor";

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref stealedPawns, "stealedPawns", LookMode.Reference);
			Scribe_Collections.Look(ref stealedThings, "stealedThings", LookMode.Deep);
			Scribe_Values.Look(ref descPawns, "descPawns", 0);
		}

		public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
		{
			if (!StudyComp.Completed)
			{
				return false;
			}
			return base.CanInteract(activateBy, checkOptionalItems);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action4 = new Command_Action();
				command_Action4.defaultLabel = "DEV: Check list";
				command_Action4.action = delegate
				{
					Log.Message(stealedThings.Count);
				};
				yield return command_Action4;
			}
			if (!StudyComp.Completed)
			{
				yield break;
			}
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			if (!StudyComp.Completed)
			{
				yield break;
			}
			foreach (FloatMenuOption item in base.CompFloatMenuOptions(selPawn))
			{
				yield return item;
			}
		}

		protected override void OnInteracted(Pawn caster)
		{
			if (StudyComp.Completed)
			{
				Slate slate = new Slate();
				slate.Set("stealedThings", stealedThings);
				slate.Set("kidnappedPawns", stealedPawns);
				slate.Set("descPawns", descPawns);
				Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(NATDefOf.NAT_CollectorLair, slate);
				Find.LetterStack.ReceiveLetter(quest.name, quest.description, LetterDefOf.PositiveEvent, null, null, quest);
				parent.Destroy();
			}
		}
	}

	public class CompDarkLairCasket : CompInteractable
	{
		private const int ActiveGraphicIndex = 1;

		protected bool opened;

		protected override void OnInteracted(Pawn caster)
		{
			parent.overrideGraphicIndex = 1;
			parent.DirtyMapMesh(parent.Map);
			Building_DarkLairCasket casket = parent as Building_DarkLairCasket;
			casket.EjectPawn();
			opened = true;
		}

		public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
		{
			if (opened)
			{
				return false;
			}
			return base.CanInteract(activateBy, checkOptionalItems);
		}

		public void Reset()
		{
			opened = false;
		}

		public override string CompInspectStringExtra()
		{
			if (opened)
			{
				return "NAT_Opened".Translate().CapitalizeFirst();
			}
			if (!opened)
			{
				return "NAT_OrderPawnToOpen".Translate().CapitalizeFirst();
			}
			return base.CompInspectStringExtra();
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			if (opened)
			{
				yield break;
			}
			foreach (FloatMenuOption item in base.CompFloatMenuOptions(selPawn))
			{
				yield return item;
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (opened)
			{
				yield break;
			}
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (DebugSettings.ShowDevGizmos)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Open",
					action = delegate
					{
						Interact(parent.Map.mapPawns.FreeColonistsSpawned.RandomElement(), force: true);
					}
				};
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref opened, "opened", defaultValue: false);
		}
	}
	public class Building_DarkLairCasket : Building
	{
		public Pawn containedPawn;

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			EjectPawn();
			base.Destroy(mode);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref containedPawn, "containedPawn");
		}

		public void EjectPawn()
		{
			if (containedPawn != null)
			{
				PawnComponentsUtility.AddComponentsForSpawn(containedPawn);
				if (containedPawn.RaceProps.IsFlesh)
				{
					containedPawn.health.AddHediff(NATDefOf.NAT_ShockFromLair);
				}
				GenSpawn.Spawn(containedPawn, this.Position, this.Map);
				containedPawn = null;
			}
		}
	}
}
