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
	public class IncidentWorker_Collector : IncidentWorker
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 result = parms.spawnCenter;
			if (!result.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out result, map, CellFinder.EdgeRoadChance_Hostile))
			{
				return false;
			}
			Rot4 rot = Rot4.FromAngleFlat((map.Center - result).AngleFlat);
			GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(NATDefOf.NAT_Collector_Reworked, Faction.OfEntities, PawnGenerationContext.NonPlayer, map.Tile)), result, map, rot);
			return true;
		}
	}

	public static class DebugActions_NAT
	{
		[DebugAction("Anomaly", "Spawn random collector back", false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresAnomaly = true)]
		public static void NAT_ReturnRandomCollectors()
		{
			foreach (Thing thing in Find.CurrentMap.listerThings.ThingsOfDef(NATDefOf.NAT_CollectorSpawner))
            {
				if(thing is CollectorSpawner)
                {
					((CollectorSpawner)thing).ReturnCollector();
					break;
				}
            }
		}

		[DebugAction("Anomaly", "Generate collector's lair", false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresAnomaly = true)]
		public static void NAT_GenerateCollectorLair(Pawn p)
        {
			foreach (Hediff hediff in p.health.hediffSet.GetHediffsTendable())
			{
				if (!hediff.IsTended())
				{
					hediff.Tended(new FloatRange(0.5f, 0.89f).RandomInRange, 0.9f);
				}
			}
			if (p.Faction == Faction.OfPlayer)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelPawnsKidnapped".Translate((p).Named("PAWN")), "NAT_LetterPawnStealed".Translate((p).Named("PAWN")), LetterDefOf.ThreatSmall, null, null, null, null, null, 1);
			}
			PodContentsType value = Gen.RandomEnumValue<PodContentsType>(disallowFirstValue: true);
			Building_DarkLairCasket building = (Building_DarkLairCasket)ThingMaker.MakeThing(NATDefOf.NAT_DarkLairCasket);
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.podContentsType = value;
			building.SetFaction(Faction.OfEntities);
			building.containedPawn = p;
			QuestUtility.SendQuestTargetSignals(p.questTags, "LeftMap");
			Find.IdeoManager.Notify_PawnLeftMap(p);
			Slate slate = new Slate();
			slate.Set("stealedThings", new List<Thing>() { building });
			slate.Set("kidnappedPawns", new List<Pawn>() { p });
			Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(NATDefOf.NAT_CollectorLair, slate);
			p.DeSpawn();
			Find.LetterStack.ReceiveLetter(quest.name, quest.description, LetterDefOf.PositiveEvent, null, null, quest);
		}
	}
	
	public class CompCollector : ThingComp
	{
		private const float BaseVisibleRadius = 14f;

		private const int UndetectedTimeout = 1200;

		private const int CheckDetectedIntervalTicks = 7;

		private const float FirstDetectedRadius = 30f;

		private const int RevealedLetterDelayTicks = 6;

		private const int AmbushCallMTBTicks = 600;

		public Map mapForNotes;

		public int descPawns;

		[Unsaved(false)]
		public HediffComp_Invisibility invisibility;

		public bool attackMode;

		public int cooldownTicks;

		private int lastDetectedTick = -99999;

		private static float lastNotified = -99999f;

		private const float NotifyCooldownSeconds = 60f;

		public List<Pawn> stealedPawns = new List<Pawn>();

		public List<Thing> stealedThings = new List<Thing>();

		private Pawn Collector => (Pawn)parent;

		private HediffComp_Invisibility Invisibility => invisibility ?? (invisibility = Collector.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.HoraxianInvisibility)?.TryGetComp<HediffComp_Invisibility>());

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
					command_Action.defaultLabel = "DEV: Turn to AttackMode";
					command_Action.action = delegate
					{
						attackMode = true;
						cooldownTicks = 0;
					};
					yield return command_Action;
					Command_Action command_Action2 = new Command_Action();
					command_Action2.defaultLabel = "DEV: CalmMode +1 hour";
					command_Action2.action = delegate
					{
						attackMode = false;
						cooldownTicks += 2500;
					};
					yield return command_Action2;
					Command_Action command_Action3 = new Command_Action();
					command_Action3.defaultLabel = "DEV: CalmMode +10 hour";
					command_Action3.action = delegate
					{
						attackMode = false;
						cooldownTicks += 25000;
					};
					yield return command_Action3;
					Command_Action command_Action4 = new Command_Action();
					command_Action4.defaultLabel = "DEV: Check list";
					command_Action4.action = delegate
					{
						Log.Message(stealedThings.Count);
					};
					yield return command_Action4;
				}
			}
		}
		public override void PostExposeData()
		{
			Scribe_Values.Look(ref attackMode, "attackMode", false);
			Scribe_Values.Look(ref cooldownTicks, "cooldownTicks", 0);
			Scribe_Values.Look(ref lastDetectedTick, "lastDetectedTick", 0);
			Scribe_Values.Look(ref descPawns, "descPawns", 0);
			Scribe_Collections.Look(ref stealedPawns, "stealedPawns", LookMode.Reference);
			Scribe_Collections.Look(ref stealedThings, "stealedThings", LookMode.Deep);
		}

		public override void CompTick()
		{
            if (!attackMode)
            {
				Hediff h = Collector.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DisruptorFlash);
				if (h != null)
                {
					Collector.health.RemoveHediff(h);
                }
				Invisibility.BecomeInvisible();
            }
			if (parent.Map != null)
            {
				mapForNotes = parent.Map;
            }
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
			foreach (Lord lord in Collector.Map.lordManager.lords)
			{
				if (lord.faction != Faction.OfEntities && lord.faction != null && lord.faction != Faction.OfPlayer && lord.faction != Faction.OfMechanoids && lord.faction != Faction.OfInsects)
				{
					attackMode = false;
					cooldownTicks++;
					break;
				}
			}
			if(cooldownTicks <= 0)
            {
				cooldownTicks = 0;
				attackMode = true;
			}
            else
            {
				cooldownTicks--;
				attackMode = false;
			}
			if (invisibility.PsychologicallyVisible && !attackMode && !Collector.health.Downed && Collector.IsHashIntervalTick(430))
            {
				TeleportAway();
			}
			if (Collector.Map != null && !Collector.CanReachMapEdge() && !Collector.health.Downed && !Collector.IsCarrying() && Collector.IsHashIntervalTick(430))
			{
				TeleportAway();
			}
		}

		public void TeleportAway(bool removeFlare = true)
        {
			Collector.GetAttachment(ThingDefOf.Fire)?.Destroy();
			EffecterDefOf.Skip_Entry.Spawn(Collector.Position, parent.Map).Cleanup();
            if (Collector.health.hediffSet.HasHediff(HediffDefOf.DisruptorFlash) && removeFlare)
            {
				Collector.health.RemoveHediff(Collector.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DisruptorFlash));
			}
			if (Collector.health.hediffSet.HasHediff(HediffDefOf.CoveredInFirefoam) && removeFlare)
			{
				Collector.health.RemoveHediff(Collector.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.CoveredInFirefoam));
			}
			invisibility.BecomeInvisible();
			Collector.Position = CellFinder.RandomEdgeCell(Collector.Map);
		}

		public bool HasTargets()
        {
			bool b = false;
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater);
			RegionTraverser.BreadthFirstTraverse(Collector.Position, Collector.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate (Region x)
			{
				List<Thing> list2 = x.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
				foreach (Thing item in list2)
				{
					if (item.MarketValue > 700f || item.def == ThingDefOf.GoldenCube || item.def == ThingDefOf.GrayFleshSample || item.def == ThingDefOf.RevenantFleshChunk || item.def == ThingDefOf.RevenantSpine || item.def == ThingDefOf.Shard)
					{
						b = true;
					}
				}
				return false;
			});
			if(b == true)
            {
				return true;
            }
			RegionTraverser.BreadthFirstTraverse(Collector.Position, Collector.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate (Region x)
			{
				List<Thing> list2 = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
				for (int i = 0; i < list2.Count; i++)
				{
					Pawn pawn2 = (Pawn)list2[i];
					if (pawn2.Faction != Faction.OfEntities && !pawn2.NonHumanlikeOrWildMan())
					{
						b = true;
					}
				}
				return false;
			});
			return b;
		}

		private void CheckDetected()
		{
			foreach (Pawn item in Collector.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn))
			{
				if (attackMode && PawnCanDetect(item))
				{
					if (!Invisibility.PsychologicallyVisible)
					{
						Invisibility.BecomeVisible();
					}
					lastDetectedTick = Find.TickManager.TicksGame;
				}
			}
		}

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
			Map map = parent.MapHeld ?? Find.CurrentMap;
			if (map == null)
            {
				map = mapForNotes;
			}
			if (map != null)
            {
				if (!stealedThings.NullOrEmpty())
				{
					Thing thing = ThingMaker.MakeThing(NATDefOf.NAT_FleshChunkWithNotes);
					thing.TryGetComp<NAT.CompNotesTriggerInteractor>().stealedThings = stealedThings;
					thing.TryGetComp<NAT.CompNotesTriggerInteractor>().stealedPawns = stealedPawns;
					thing.TryGetComp<NAT.CompNotesTriggerInteractor>().descPawns = descPawns;
					GenSpawn.Spawn(thing, parent.Position, map);
				}
				else
				{
					Thing thing = ThingMaker.MakeThing(NATDefOf.NAT_FleshChunkWithNotes);
					thing.TryGetComp<NAT.CompNotesTriggerInteractor>().stealedThings = stealedThings;
					thing.TryGetComp<NAT.CompNotesTriggerInteractor>().stealedPawns = stealedPawns;
					thing.TryGetComp<NAT.CompNotesTriggerInteractor>().descPawns = 0;
					GenSpawn.Spawn(thing, parent.Position, map);
				}
			}
			base.PostDestroy(mode, previousMap);
		}

        private bool PawnCanDetect(Pawn pawn)
		{
			if (pawn.Faction == Faction.OfEntities || pawn.Faction == Faction.OfMechanoids || pawn.Downed || !pawn.Awake())
			{
				return false;
			}
			if (pawn.IsNonMutantAnimal && !(pawn is RustedPawn))
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
			float num = 15f;
			if (pawn.genes == null || pawn.genes.AffectedByDarkness)
			{
				float t = collector.Map.glowGrid.GroundGlowAt(collector.Position);
				num *= Mathf.Lerp(0.33f, 1f, t);
			}
			return num * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight);
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
		}
	}

	public class JobGiver_CollectorWander : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Job job = new Job();
			if (CellFinder.TryFindRandomReachableNearbyCell(pawn.Position, pawn.Map, 5f, TraverseParms.For(TraverseMode.PassDoors), (IntVec3 x) => x.Standable(pawn.Map), null, out var result))
            {
				job = JobMaker.MakeJob(NATDefOf.NAT_CollectorWander, result);
				job.locomotionUrgency = LocomotionUrgency.Walk;
				
			}
            else
            {
				pawn.TryGetComp<NAT.CompCollector>().TeleportAway(false);
				job = JobMaker.MakeJob(NATDefOf.NAT_CollectorWait);
			}
			return job;
		}
	}

	public class ThinkNode_CanSteal : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (!pawn.CanReachMapEdge())
			{
				return false;
			}
			foreach (Lord lord in pawn.Map.lordManager.lords)
			{
				if (lord.faction != Faction.OfEntities && lord.faction != null && lord.faction != Faction.OfPlayer && lord.faction != Faction.OfMechanoids && lord.faction != Faction.OfInsects)
				{
					return false;
				}
			}
			return true;
		}
	}

	public class ThinkNode_HasThingToSteal : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			bool b = false;
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater);
			RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate (Region x)
			{
				List<Thing> list2 = x.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
				foreach (Thing item in list2)
				{
					if (item.MarketValue > 700f || item.def == ThingDefOf.GoldenCube || item.def == ThingDefOf.GrayFleshSample || item.def == ThingDefOf.RevenantFleshChunk || item.def == ThingDefOf.RevenantSpine || item.def == ThingDefOf.Shard)
					{
						b = true;
					}
				}
				return false;
			});
			return b;
		}

		public bool OutsideCheck(Pawn p)
		{
			return Satisfied(p);
		}
	}

	public class ThinkNode_HasPawnToSteal : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			bool b = false;
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater);
			RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate (Region x)
			{
				List<Thing> list2 = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
				for (int i = 0; i < list2.Count; i++)
				{
					Pawn pawn2 = (Pawn)list2[i];
					if (pawn2.Faction != Faction.OfEntities && !pawn2.NonHumanlikeOrWildMan())
					{
						b = true;
					}
				}
				return false;
			});
			return b;
		}

		public bool OutsideCheck(Pawn p)
        {
			return Satisfied(p);
		}
	}

	public class ThinkNode_InStealMode : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
            if (pawn.HasComp<CompCollector>())
            {
				return pawn.TryGetComp<CompCollector>().attackMode;
			}
			return false;
		}
	}

	public class JobGiver_CollectorSteal_Pawns : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			List<Pawn> list = new List<Pawn>();
			
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater);
			RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate (Region x)
			{
				List<Thing> list2 = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
				for (int i = 0; i < list2.Count; i++)
				{
					Pawn pawn2 = (Pawn)list2[i];
                    if (pawn2.Faction != Faction.OfEntities && !pawn2.NonHumanlikeOrWildMan())
                    {
						list.Add(pawn2);
                    }
				}
				return false;
			});
			Pawn result = null;
			float num = float.MaxValue;
			foreach (Pawn item in list)
			{
				if (pawn.Position.InHorDistOf(item.Position, 500f) && (float)item.Position.DistanceToSquared(pawn.Position) < num)
				{
					num = item.Position.DistanceToSquared(pawn.Position);
					result = item;
				}
			}
			Job job = JobMaker.MakeJob(NATDefOf.NAT_CollectorSteal, result);
			IntVec3 spot;
			RCellFinder.TryFindBestExitSpot(pawn, out spot);
			job.targetB = spot;
			job.count = 1;
			return job;
		}
	}

	public class JobGiver_CollectorSteal_Things : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			List<Thing> list = new List<Thing>();

			TraverseParms traverseParms = TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater);
			RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate (Region x)
			{
				List<Thing> list2 = x.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
				foreach (Thing item in list2)
				{
					if (item.MarketValue > 500f || item.MarketValue * item.stackCount > 700f || item.def == ThingDefOf.GoldenCube || item.def == ThingDefOf.GrayFleshSample || item.def == ThingDefOf.RevenantFleshChunk || item.def == ThingDefOf.RevenantSpine || item.def == ThingDefOf.Shard)
					{
						list.Add(item);
					}
				}
				return false;
			});
			Thing result = null;
			float num = 0;
			foreach (Thing item in list)
			{
				if (item.MarketValue * item.stackCount > num)
				{
					num = item.MarketValue * item.stackCount;
					result = item;
				}
				if (item.def == ThingDefOf.GoldenCube)
                {
					result = item;
					break;
                }
				if (item.def == ThingDefOf.RevenantSpine)
				{
					result = item;
					break;
				}
				if (item.def == ThingDefOf.RevenantFleshChunk)
				{
					result = item;
					break;
				}
				if (item.def == ThingDefOf.Shard)
				{
					result = item;
					break;
				}
				if (item.def == ThingDefOf.GrayFleshSample)
				{
					result = item;
					break;
				}
			}
			Job job = JobMaker.MakeJob(NATDefOf.NAT_CollectorSteal, result);
			IntVec3 spot;
			RCellFinder.TryFindBestExitSpot(pawn, out spot);
			job.targetB = spot;
			job.count = 1;
			return job;
		}
	}

	public class JobDriver_CollectorWander : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			CompCollector comp = pawn.TryGetComp<NAT.CompCollector>();
			toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
			{
				if (comp.attackMode && comp.HasTargets())
				{
					EndJobWith(JobCondition.InterruptForced);
				}
			});
			yield return toil;
		}
	}

	public class JobDriver_CollectorWait : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = Toils_General.Wait(300);
			CompCollector comp = pawn.TryGetComp<NAT.CompCollector>();
			toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
			{
				if (comp.attackMode && comp.HasTargets())
				{
					EndJobWith(JobCondition.InterruptForced);
				}
			});
			yield return toil;
		}
	}

	public class JobDriver_StealPawn : JobDriver
	{
		private const TargetIndex ItemInd = TargetIndex.A;

		private const TargetIndex ExitCellInd = TargetIndex.B;

		protected Thing Item => job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed, true);
		}

		public void LeaveWithStealed()
        {
			if (pawn.HasComp<NAT.CompCollector>())
			{
				pawn.GetAttachment(ThingDefOf.Fire)?.Destroy();
				if (pawn.carryTracker.CarriedThing is Pawn)
                {
					Pawn p = (Pawn)pawn.carryTracker.CarriedThing;
					foreach(Hediff hediff in p.health.hediffSet.GetHediffsTendable())
                    {
                        if (!hediff.IsTended())
                        {
							hediff.Tended(new FloatRange(0.5f, 0.89f).RandomInRange, 0.9f);
						}
                    }
					if (p.Faction == Faction.OfPlayer)
                    {
						Find.LetterStack.ReceiveLetter("LetterLabelPawnsKidnapped".Translate((p).Named("PAWN")), "NAT_LetterPawnStealed".Translate((p).Named("PAWN")), LetterDefOf.ThreatSmall, null, null, null, null, null, 1);
					}
					pawn.carryTracker.innerContainer.Remove(p);
					PodContentsType value = Gen.RandomEnumValue<PodContentsType>(disallowFirstValue: true);
					Building_DarkLairCasket building = (Building_DarkLairCasket)ThingMaker.MakeThing(NATDefOf.NAT_DarkLairCasket);
					ThingSetMakerParams parms = default(ThingSetMakerParams);
					parms.podContentsType = value;
					building.SetFaction(Faction.OfEntities);
					building.containedPawn = p;
					pawn.TryGetComp<CompCollector>().stealedPawns.Add(p);
					pawn.TryGetComp<CompCollector>().descPawns++;
					pawn.TryGetComp<CompCollector>().stealedThings.Add(building);
					QuestUtility.SendQuestTargetSignals(p.questTags, "LeftMap", this.Named("SUBJECT"));
					Find.IdeoManager.Notify_PawnLeftMap(p);
					pawn.TryGetComp<CompCollector>().cooldownTicks = 100000;
				}
                else
                {
					Thing t = pawn.carryTracker.CarriedThing;
                    if(t.def != ThingDefOf.RevenantSpine)
                    {
						pawn.TryGetComp<CompCollector>().stealedThings.Add(t);
						pawn.carryTracker.innerContainer.Remove(t);
						if (t.def == ThingDefOf.GoldenCube)
						{
							Find.LetterStack.ReceiveLetter("NAT_CubeStealed".Translate(), "NAT_LetterGoldenCubeStealed".Translate(), LetterDefOf.ThreatSmall, null, null, null, null, null, 1);
						}
					}
					pawn.TryGetComp<CompCollector>().cooldownTicks += Mathf.RoundToInt(t.MarketValue * t.stackCount);
				}
				pawn.TryGetComp<CompCollector>().invisibility.BecomeInvisible();
				pawn.TryGetComp<CompCollector>().attackMode = false;
				Hediff h = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DisruptorFlash);
				if (h != null)
				{
					pawn.health.RemoveHediff(h);
				}
			}
			CollectorSpawner spawner = (CollectorSpawner)ThingMaker.MakeThing(NATDefOf.NAT_CollectorSpawner);
			spawner.Collector = pawn;
			spawner.ticksLeft = new IntRange(15000, 30000).RandomInRange;
			GenSpawn.Spawn(spawner, CellFinder.RandomEdgeCell(pawn.Map), pawn.Map);
			pawn.DeSpawn();
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A);
			Toil toil = Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			toil.AddPreTickAction(delegate
			{
				if (base.Map.exitMapGrid.IsExitCell(pawn.Position))
				{
					LeaveWithStealed();
				}
			});
			yield return toil;
			Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
			toil2.initAction = delegate
			{
				if (pawn.Position.OnEdge(pawn.Map) || pawn.Map.exitMapGrid.IsExitCell(pawn.Position))
				{
					LeaveWithStealed();
				}
			};
			toil2.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil2;
		}
	}
}