using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using RimWorld.Utility;
using LudeonTK;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Grammar;
using Verse.Noise;
using Verse.Profile;
using Verse.Sound;
using Verse.Steam;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace NAT
{
	public enum CollectorState
	{
		Wait,
		Collect,
		Escape,
		Rage
	}

	public class CompProperties_ReworkedCollector : CompProperties
	{
		public List<ThingDef> highPriorityThings;

		public IntRange thingsRange;

		public IntRange waitingTicksRange;

		public IntRange rageTicksRange;

		public IntRange damageRange;

		public CompProperties_ReworkedCollector()
		{
			compClass = typeof(CompReworkedCollector);
		}
	}
	public class CompReworkedCollector : ThingComp, IThingHolder
	{
		public CompProperties_ReworkedCollector Props => (CompProperties_ReworkedCollector)props;

		[Unsaved(false)]
		public HediffComp_Invisibility invisibility;

		[Unsaved(false)]
		public Hediff rage;

		private int lastDetectedTick = -99999;

		private static float lastNotified = -99999f;

		public ThingOwner innerContainer;

		public bool hasBag;

		public CollectorState state;

		private List<ThingDef> stealedDefs = new List<ThingDef>();

		public int thingsToStealLeft;

		public int waitingTicksLeft;

		public int rageTicksLeft;

		public int damageBeforeRageLeft = 100;

		public int damageInRage;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Values.Look(ref hasBag, "hasBag", true);
			Scribe_Values.Look(ref state, "state", CollectorState.Wait);
			Scribe_Values.Look(ref thingsToStealLeft, "thingsToStealLeft", 0);
			Scribe_Values.Look(ref rageTicksLeft, "rageTicksLeft", 0);
			Scribe_Values.Look(ref waitingTicksLeft, "waitingTicksLeft", 0);
			Scribe_Values.Look(ref damageBeforeRageLeft, "damageBeforeRageLeft", 100);
			Scribe_Values.Look(ref damageInRage, "damageInRage", 0);
			Scribe_Values.Look(ref lastDetectedTick, "lastDetectedTick", 0);
			Scribe_Collections.Look(ref stealedDefs, "stealedDefs", LookMode.Def);
		}

		private Pawn Collector => (Pawn)parent;

		public HediffComp_Invisibility Invisibility => invisibility ?? (invisibility = Collector.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.HoraxianInvisibility)?.TryGetComp<HediffComp_Invisibility>());

		public Hediff Rage => rage ?? (rage = Collector.health.hediffSet.GetFirstHediffOfDef(NATDefOf.NAT_HediffCollector));

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (DebugSettings.ShowDevGizmos)
			{
				if (DebugSettings.ShowDevGizmos)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.defaultLabel = "DEV: Drop bag";
					command_Action.action = delegate
					{
						DropBag();
					};
					yield return command_Action;
					Command_Action command_Action2 = new Command_Action();
					command_Action2.defaultLabel = "DEV: Current state is...";
					command_Action2.action = delegate
					{
						Messages.Message(state.ToString(), MessageTypeDefOf.CautionInput);
					};
					yield return command_Action2;
					Command_Action command_Action3 = new Command_Action();
					command_Action3.defaultLabel = "DEV: Change state";
					command_Action3.action = delegate
					{
						List<FloatMenuOption> list = new List<FloatMenuOption>();
						list.Add(new FloatMenuOption("Wait", delegate
						{
							state = CollectorState.Wait;
							waitingTicksLeft = 600;
						}));
						list.Add(new FloatMenuOption("Collect", delegate
						{
							state = CollectorState.Collect;
							thingsToStealLeft = Props.thingsRange.RandomInRange;
						}));
						list.Add(new FloatMenuOption("Escape", delegate
						{
							state = CollectorState.Escape;
						}));
						list.Add(new FloatMenuOption("Rage", delegate
						{
							state = CollectorState.Rage;
						}));
						Find.WindowStack.Add(new FloatMenu(list));
					};
					yield return command_Action3;
					Command_Action command_Action4 = new Command_Action();
					command_Action4.defaultLabel = "DEV: Drop all";
					command_Action4.action = delegate
					{
						innerContainer.TryDropAll(Collector.PositionHeld, Collector.MapHeld, ThingPlaceMode.Near);
					};
					yield return command_Action4;
					Command_Action command_Action5 = new Command_Action();
					command_Action5.defaultLabel = "DEV: Return bag";
					command_Action5.action = delegate
					{
						hasBag = true;
					};
					yield return command_Action5;
					Command_Action command_Action6 = new Command_Action();
					command_Action6.defaultLabel = "DEV: Get full info";
					command_Action6.action = delegate
					{
						string s = "=======Collector======";
						s += "\n";
						if (innerContainer.NullOrEmpty())
                        {
							s += "\n" + "innerContainer is NullOrEmpty";
                        }
                        else
                        {
							foreach (Thing item in innerContainer.ToList())
							{
								s += "\n" + item.LabelCap + " (" + item.def.defName + "), Stack count: " + item.stackCount.ToString();
							}
						}
						s += "\n\n"+ "Current state: " + state.ToString();
						s += "\n\n" + "Has bag: " + hasBag.ToString();
						Log.openOnMessage = true;
						Log.Message(s);
						Log.openOnMessage = false;
					};
					yield return command_Action6;
				}
			}
		}
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			hasBag = true;
			state = CollectorState.Wait;
			waitingTicksLeft = 60000;
			damageBeforeRageLeft = Props.damageRange.RandomInRange;
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
			}
		}

		public override void CompTick()
        {
            base.CompTick();
			if (Collector.IsShambler)
			{
				return;
			}
			if (Invisibility == null)
			{
				Collector.health.AddHediff(HediffDefOf.HoraxianInvisibility);
			}
			if (!Collector.Spawned)
			{
				return;
			}
			if (state == CollectorState.Wait && Collector.IsHashIntervalTick(80))
			{
				Hediff hediff = Collector.health.hediffSet.GetHediffsTendable().FirstOrDefault();
				if (hediff != null && !hediff.IsTended())
				{
					hediff.Tended(new FloatRange(0.7f, 0.99f).RandomInRange, 1f);
				}
			}
			if (Collector.IsHashIntervalTick(7))
			{
				if (Find.TickManager.TicksGame > lastDetectedTick + 1200)
				{
					CheckDetected();
				}
				if (Find.TickManager.TicksGame > lastDetectedTick + 1200)
				{
					Invisibility.BecomeInvisible();
				}
			}
            if(damageBeforeRageLeft <= 0)
            {
				state = CollectorState.Rage;
				damageBeforeRageLeft = Props.damageRange.RandomInRange;
				rageTicksLeft = Props.rageTicksRange.RandomInRange;
				Collector.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
			if (state == CollectorState.Rage)
			{
				rageTicksLeft--;
				if (damageInRage <= 20)
				{
					Rage.Severity = 0.7f;
				}
				else if (damageInRage < 50)
				{
					Rage.Severity = 0.8f;
				}
				else
				{
					Rage.Severity = 1f;
				}
				if (rageTicksLeft <= 0)
				{
					damageInRage = 0;
					state = CollectorState.Escape;
					Rage.Severity = 0.5f;
					Collector.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			}
			if (state == CollectorState.Wait)
            {
				Invisibility.BecomeInvisible();
                if (hasBag)
                {
					waitingTicksLeft--;
					if (waitingTicksLeft <= 0)
					{
						state = CollectorState.Collect;
						thingsToStealLeft = Props.thingsRange.RandomInRange;
						Collector.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
            }
			if(state == CollectorState.Collect && thingsToStealLeft <= 0)
            {
				state = CollectorState.Escape;
				IntVec3 spot;
				RCellFinder.TryFindBestExitSpot(Collector, out spot, TraverseMode.PassDoors);
				Job job = JobMaker.MakeJob(JobDefOf.Goto, spot);
				job.locomotionUrgency = LocomotionUrgency.Sprint;
				job.canBashDoors = true;
				job.canBashFences = true;
				Collector.jobs.StartJob(job, JobCondition.InterruptForced);
			}
		}

		private void CheckDetected()
		{
			foreach (Pawn item in Collector.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn))
			{
				if (PawnCanDetect(item))
				{
					if (!Invisibility.PsychologicallyVisible)
					{
						Invisibility.BecomeVisible();
					}
					lastDetectedTick = Find.TickManager.TicksGame;
				}
			}
		}

		private bool PawnCanDetect(Pawn pawn)
		{
			if (pawn.Faction == Faction.OfEntities || pawn.Faction == Faction.OfMechanoids || pawn.Downed || !pawn.Awake())
			{
				return false;
			}
			if (pawn.IsNonMutantAnimal)
			{
				return false;
			}
			if (!Collector.Position.InHorDistOf(pawn.Position, GetPawnSightRadius(pawn, Collector)))
			{
				return false;
			}
			return GenSight.LineOfSightToThing(pawn.Position, Collector, parent.Map);
		}

		private static float GetPawnSightRadius(Pawn pawn, Pawn collector)
		{
			float num = 7f;
			if (pawn.genes == null || pawn.genes.AffectedByDarkness)
			{
				float t = collector.Map.glowGrid.GroundGlowAt(collector.Position);
				num *= Mathf.Lerp(0.33f, 1f, t);
			}
			return num * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight);
		}

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
			if(state != CollectorState.Rage)
            {
				damageBeforeRageLeft -= Mathf.RoundToInt(totalDamageDealt);
				state = CollectorState.Escape;
				Collector.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
            else
            {
				damageInRage += Mathf.RoundToInt(totalDamageDealt);
			}
			base.PostPostApplyDamage(dinfo, totalDamageDealt);
        }

        public override void Notify_BecameVisible()
		{
			SoundDefOf.Pawn_Sightstealer_Howl.PlayOneShotOnCamera();
			foreach (Pawn item in Collector.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.Pawn))
			{
				if (item.kindDef == PawnKindDefOf.Sightstealer && item != Collector && item.Position.InHorDistOf(Collector.Position, 30f) && !item.IsPsychologicallyInvisible() && GenSight.LineOfSight(item.Position, Collector.Position, item.Map))
				{
					return;
				}
			}
			if (RealTime.LastRealTime > lastNotified + 60f)
			{
				Find.LetterStack.ReceiveLetter("NAT_LetterLabelCollectorRevealed".Translate(), "NAT_LetterCollectorRevealed".Translate(), LetterDefOf.ThreatBig, Collector, null, null, null, null, 6);
			}
			else
			{
				Messages.Message("NAT_MessageCollectorRevealed".Translate(), Collector, MessageTypeDefOf.ThreatBig);
			}
			lastNotified = RealTime.LastRealTime;
			lastDetectedTick = Find.TickManager.TicksGame;
			if (state != CollectorState.Rage)
			{
				if(state == CollectorState.Wait)
                {
					state = CollectorState.Escape;
					Collector.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
                else
                {
					state = CollectorState.Escape;
				}
			}
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public Thing ThingToSteal(List<Thing> list)
        {
			foreach (ThingDef d in Props.highPriorityThings)
            {
				IEnumerable<Thing> list2 = list.Where((Thing t)=>t.def == d);
				if (!list2.EnumerableNullOrEmpty())
                {
					return list2.RandomElement();
                }
            }
			IEnumerable<Thing> list3 = list.Where((Thing t2) => !(t2 is Corpse) && !stealedDefs.Contains(t2.def) && t2.MarketValue > 0f);
			if (!list3.EnumerableNullOrEmpty())
            {
				return list3.RandomElementByWeight((Thing t3) => t3.MarketValue);
			}
			return list.Where((Thing t4) => t4.MarketValue > 0f && t4 is Corpse).RandomElement();
        }

		public void AddThing(Thing t)
        {
			innerContainer.TryAddOrTransfer(t);
            if (!Props.highPriorityThings.Contains(t.def) && t.TryGetComp<CompArt>()?.Active != true && !stealedDefs.Contains(t.def))
            {
				stealedDefs.Add(t.def);
            }
			thingsToStealLeft--;
        }

        public override void Notify_Downed()
        {
            if (hasBag)
            {
				DropBag();
            }
            base.Notify_Downed();
        }

        public void DropBag()
        {
			Thing bag = ThingMaker.MakeThing(NATDefOf.NAT_CollectorBag);
			CompCollectorBag comp = bag.TryGetComp<CompCollectorBag>();
			comp.innerContainer = new ThingOwner<Thing>(comp, oneStackOnly: false);
			innerContainer.TryTransferAllToContainer(comp.innerContainer);
			//innerContainer.ClearAndDestroyContents();
			GenSpawn.Spawn(bag, parent.PositionHeld, parent.MapHeld);
			hasBag = false;
        }
	}

	public class ThinkNode_ConditionalCollectorState : ThinkNode_Conditional
	{
		public CollectorState state;

		protected override bool Satisfied(Pawn pawn)
		{
			CompReworkedCollector comp = pawn.TryGetComp<CompReworkedCollector>();
			if (comp != null)
			{
				return comp.state == state;
			}
			return false;
		}
	}

	public class JobGiver_CollectorRage : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			TargetScanFlags flags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
			Thing t = (Thing)AttackTargetFinder.BestAttackTarget(pawn, flags, null, 0f, 9999f, default(IntVec3), float.MaxValue, canBashDoors: true, canTakeTargetsCloserThanEffectiveMinRange: true, canBashFences: true);
			if(t == null)
            {
				CompReworkedCollector comp = pawn.TryGetComp<CompReworkedCollector>();
				comp.state = CollectorState.Escape;
				comp.rageTicksLeft = 0;
				comp.damageInRage = 0;
				comp.Rage.Severity = 0.5f;
				return null;
            }
			Job job2 = JobMaker.MakeJob(JobDefOf.AttackMelee, t);
			job2.locomotionUrgency = LocomotionUrgency.Sprint;
			job2.canBashDoors = true;
			job2.canBashFences = true;
			return job2;
		}

		private bool IsGoodTarget(Thing thing)
		{
			if (thing is Pawn pawn && !pawn.Downed && pawn.RaceProps.Humanlike)
			{
				return !pawn.IsPsychologicallyInvisible();
			}
			return false;
		}
	}

	public class JobGiver_CollectorEscape : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 intVec = RevenantUtility.FindEscapeCell(pawn);
			if (!intVec.IsValid)
			{
				return null;
			}
			using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, intVec, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors)))
			{
				if (!pawnPath.Found)
				{
					Job job1 = JobMaker.MakeJob(NATDefOf.NAT_CollectorEscape_Teleport, intVec);
					return job1;
				}
			}
			Job job2 = JobMaker.MakeJob(NATDefOf.NAT_CollectorEscape, intVec);
			job2.locomotionUrgency = LocomotionUrgency.Sprint;
			job2.canBashDoors = true;
			job2.canBashFences = true;
			return job2;
		}
	}

	public class JobDriver_CollectorEscape : JobDriver
	{
		private static int MinEscapeTime = 300;

		private static int EscapedCheckInterval = 120;

		private CompReworkedCollector Comp => pawn.TryGetComp<CompReworkedCollector>();

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
			{
				if (RevenantUtility.NearbyHumanlikePawnCount(pawn.Position, pawn.Map, 14f) == 0)
				{
					Comp.Invisibility.BecomeInvisible();
				}
				if (Rand.MTBEventOccurs(EscapedCheckInterval, 1f, 1f) && Find.TickManager.TicksGame > pawn.mindState.lastEngageTargetTick + MinEscapeTime && RevenantUtility.NearbyHumanlikePawnCount(pawn.Position, pawn.Map, 20f) == 0)
				{
					ReadyForNextToil();
				}
			});
			yield return toil;
			Toil toil2 = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			toil2.tickAction = (Action)Delegate.Combine(toil2.tickAction, (Action)delegate
			{
				if (RevenantUtility.NearbyHumanlikePawnCount(pawn.Position, pawn.Map, 20f) == 0)
				{
					Comp.Invisibility.BecomeInvisible();
				}
				if (RevenantUtility.NearbyHumanlikePawnCount(pawn.Position, pawn.Map, 35f) == 0 && !Comp.Invisibility.PsychologicallyVisible)
				{
					Comp.state = CollectorState.Wait;
					Comp.waitingTicksLeft = Comp.Props.waitingTicksRange.RandomInRange;
					EndJobWith(JobCondition.Succeeded);
				}
			});
			yield return toil2;
		}
	}

	public class JobDriver_CollectorEscape_Teleport : JobDriver
	{
		private const TargetIndex DestIndex = TargetIndex.A;

		private CompReworkedCollector Comp => pawn.TryGetComp<CompReworkedCollector>();

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_General.Wait(240); 
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.initAction = delegate
			{
				SkipUtility.SkipTo(pawn, base.TargetA.Cell, pawn.MapHeld);
				Comp.waitingTicksLeft = Comp.Props.waitingTicksRange.RandomInRange;
				pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
			};
			yield return toil;
		}
	}

	public class JobGiver_CollectorSteal : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Thing thingToSteal = null;
			CompReworkedCollector comp = pawn.TryGetComp<CompReworkedCollector>();
			List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEverOrMinifiable).Where((Thing t) => t.MarketValue > 0f && pawn.Map.reachability.CanReach(pawn.Position, t, PathEndMode.OnCell, TraverseMode.PassDoors, Danger.Deadly)).ToList();
            if (list.NullOrEmpty())
            {
				comp.state = CollectorState.Wait;
				comp.waitingTicksLeft = comp.Props.waitingTicksRange.RandomInRange;
				return null;
            }
			thingToSteal = comp.ThingToSteal(list);
			if (list.Count == 1)
			{
				comp.thingsToStealLeft = 1;
			}
			Job job = JobMaker.MakeJob(NATDefOf.NAT_CollectorSteal_Reworked, thingToSteal);
			job.count = thingToSteal.stackCount;
			return job;
		}
	}

	public class JobDriver_CollectorSteal : JobDriver
	{
		private const TargetIndex ItemInd = TargetIndex.A;

		protected Thing Item => job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed, true);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			CompReworkedCollector comp = pawn.TryGetComp<CompReworkedCollector>();
			Toil toil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedOrNull(TargetIndex.A);
			yield return toil;
			yield return Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A);
			Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
			toil2.initAction = delegate
			{
				comp.AddThing(Item);
			};
			toil2.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil2;
		}
	}

	public class JobGiver_CollectorWait_Reworked : ThinkNode_JobGiver
	{
		public static float WanderDist = 10f;

		protected override Job TryGiveJob(Pawn pawn)
		{
			CellFinder.TryFindRandomReachableNearbyCell(pawn.Position, pawn.Map, WanderDist, TraverseParms.For(TraverseMode.PassDoors), (IntVec3 x) => x.Standable(pawn.Map), null, out var result);
			Job job = JobMaker.MakeJob(NATDefOf.NAT_CollectorWait_Reworked, result);
			job.locomotionUrgency = LocomotionUrgency.Walk;
			return job;
		}
	}

	public class JobDriver_CollectorWait_Reworked : JobDriver
	{
		private const int SmearMTBDays = 2;

		private const int MinSmearInterval = 60000;

		private CompRevenant Comp => pawn.TryGetComp<CompRevenant>();

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return toil;
			Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
			toil2.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil2;
		}
	}

	public class CompProperties_CollectorBag : CompProperties_Interactable
	{
		public CompProperties_CollectorBag()
		{
			compClass = typeof(CompCollectorBag);
		}
	}
	public class CompCollectorBag : CompInteractable, IThingHolder
	{
		private CompStudyUnlocks studyComp;

		public ThingOwner innerContainer;

		private bool opened;

		private CompStudyUnlocks StudyComp => studyComp ?? (studyComp = parent.GetComp<CompStudyUnlocks>());

		public override string ExposeKey => "Interactor";

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Values.Look(ref opened, "opened", false);
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
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
					string s = "=======Items inside======";
					foreach (Thing item in innerContainer.ToList())
					{
						s += "\n" + item.LabelCap + " (" + item.def.defName + "), Stack count: " + item.stackCount.ToString();
					}
					Log.openOnMessage = true;
					Log.Message(s);
					Log.openOnMessage = false;
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
				parent.overrideGraphicIndex = 1;
				parent.DirtyMapMesh(parent.MapHeld);
				opened = true;
			}
		}

        public override bool HideInteraction
        {
            get
            {
                if (base.HideInteraction || opened)
                {
					return true;
                }
				return false;
            }
        }


        public override void CompTick()
        {
            if (!innerContainer.NullOrEmpty())
            {
                if (opened && parent.IsHashIntervalTick(24))
                {
					innerContainer.TryDrop(innerContainer.RandomElement(), parent.PositionHeld, parent.MapHeld, ThingPlaceMode.Near, out Thing lastResultingThing);
                }
            }
            else
            {
				SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(parent.PositionHeld, parent.MapHeld));
				EffecterDefOf.Skip_Entry.Spawn(parent.PositionHeld, parent.MapHeld);
				parent.Destroy();
            }
            base.CompTick();
        }
    }
}