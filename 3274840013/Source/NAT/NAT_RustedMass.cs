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
	public class IncidentWorker_RustedMassArrival : IncidentWorker
	{
		public override float ChanceFactorNow(IIncidentTarget target)
		{
			if (!(target is Map map))
			{
				return base.ChanceFactorNow(target);
			}
			int num = map.mapPawns.PawnsInFaction(Faction.OfEntities).Where((Pawn p) => p.kindDef == NATDefOf.NAT_RustedSphere).Count();
			return ((num > 0) ? ((float)num * 0.7f) : 1f) * base.ChanceFactorNow(target);
		}

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 cell;
			return TryFindCell(out cell, map);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Skyfaller skyfaller = SpawnObeliskIncoming(map);
			if (skyfaller == null)
			{
				return false;
			}
			skyfaller.impactLetter = LetterMaker.MakeLetter("NAT_RustedMassArrival".Translate(), "NAT_RustedMassArrival_Desc".Translate(), LetterDefOf.NeutralEvent, new TargetInfo(skyfaller.Position, map));
			return true;
		}

		private Skyfaller SpawnObeliskIncoming(Map map)
		{
			if (!TryFindCell(out var cell, map))
			{
				return null;
			}
			Pawn p = PawnGenerator.GeneratePawn(NATDefOf.NAT_RustedSphere);
			return SkyfallerMaker.SpawnSkyfaller(NATDefOf.NAT_RustedMassIncoming, p, cell, map);
		}

		private static bool TryFindCell(out IntVec3 cell, Map map)
		{
			return CellFinderLoose.TryFindSkyfallerCell(NATDefOf.NAT_RustedMassIncoming, map, out cell, 10, default(IntVec3), -1, allowRoofedCells: true, allowCellsWithItems: false, allowCellsWithBuildings: false, colonyReachable: false, avoidColonistsIfExplosive: true, alwaysAvoidColonists: true, delegate (IntVec3 x)
			{
				if ((float)x.DistanceToEdge(map) < 20f + (float)map.Size.x * 0.1f)
				{
					return false;
				}
				foreach (IntVec3 item in CellRect.CenteredOn(x, 1, 1))
				{
					if (!item.InBounds(map) || !item.Standable(map) || item.Fogged(map))
					{
						return false;
					}
				}
				return true;
			});
		}
	}

	public class CompProperties_RustedSphereDeactivation : CompProperties_Interactable
	{
		public int shardsRequired = 1;

		public CompProperties_RustedSphereDeactivation()
		{
			compClass = typeof(CompRustedSphereDeactivation);
		}
	}
	public class CompRustedSphereDeactivation : CompInteractable
	{
		private new CompProperties_RustedSphereDeactivation Props => (CompProperties_RustedSphereDeactivation)props;

		private CompStudyUnlocks studyComp;

		private CompStudyUnlocks StudyComp => studyComp ?? (studyComp = parent.GetComp<CompStudyUnlocks>());

		public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
		{
			if (!StudyComp.Completed)
			{
				return false;
			}
			if (activateBy != null)
			{
				if (checkOptionalItems && !activateBy.HasReserved(ThingDefOf.Shard) && !ReservationUtility.ExistsUnreservedAmountOfDef_NewTemp(parent.MapHeld, ThingDefOf.Shard, Faction.OfPlayer, Props.shardsRequired, (Thing t) => activateBy.CanReserveAndReach(t, PathEndMode.Touch, Danger.None)))
				{
					return "NAT_RustedSphereDeactivateMissingShards".Translate(Props.shardsRequired);
				}
			}
			else if (checkOptionalItems && !ReservationUtility.ExistsUnreservedAmountOfDef(parent.MapHeld, ThingDefOf.Shard, Faction.OfPlayer, Props.shardsRequired))
			{
				return "NAT_RustedSphereDeactivateMissingShards".Translate(Props.shardsRequired);
			}
			return base.CanInteract(activateBy, checkOptionalItems);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
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

		public override void OrderForceTarget(LocalTargetInfo target)
		{
			if (ValidateTarget(target, showMessages: false))
			{
				OrderDeactivation(target.Pawn);
			}
		}

		protected override void OnInteracted(Pawn caster)
		{
			parent.TryGetComp<CompActivity>().Deactivate();
			caster.infectionVectors.AddInfectionVector(DefDatabase<InfectionPathwayDef>.GetNamed("EntityAttacked"), parent as Pawn);
			parent.TryGetComp<CompRustedSphere>().passive = true;
			parent.Kill();
		}

		private void OrderDeactivation(Pawn pawn)
		{
			if (pawn.TryFindReserveAndReachableOfDef(ThingDefOf.Shard, out var thing))
			{
				Job job = JobMaker.MakeJob(JobDefOf.InteractThing, parent, thing);
				job.count = 1;
				job.playerForced = true;
				pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		}
	}

	public class CompProperties_RustedSphere : CompProperties
	{
		public SimpleCurve pointsFromCurrentPoints;

		public SimpleCurve pointsFactorFromRaidIndex;

		public SimpleCurve pointsFactorFromActivity;

		public SimpleCurve cooldownFactorFromRaidIndex;

		public SimpleCurve cooldownFactorFromActivity;

		public FloatRange cooldownDaysRange;
		public CompProperties_RustedSphere()
		{
			compClass = typeof(CompRustedSphere);
		}
	}
	public class CompRustedSphere : ThingComp, IActivity, IRoofCollapseAlert
	{
		public CompActivity activity => parent.TryGetComp<CompActivity>();
		public CompProperties_RustedSphere Props => (CompProperties_RustedSphere)props;

		public int raidIndex;

		public int ticksSinceRaid;

		public int ticksTillRaid;

		public Map mapToAttack;

		public bool passive;

		private static bool IsValidCell(IntVec3 cell, Map map)
		{
			if (cell.InBounds(map))
			{
				return cell.Walkable(map);
			}
			return false;
		}
		public RoofCollapseResponse Notify_OnBeforeRoofCollapse()
		{
			if (RCellFinder.TryFindRandomCellNearWith(parent.Position, (IntVec3 c) => IsValidCell(c, parent.MapHeld), parent.MapHeld, out var result, 10))
			{
				SkipUtility.SkipTo(parent, result, parent.MapHeld);
				activity.AdjustActivity(0.5f);
			}
			return RoofCollapseResponse.RemoveThing;
		}
		public void OnActivityActivated()
		{
			if(parent.MapHeld != null)
            {
				Activate(parent.MapHeld);
				parent.Kill();
			}
		}

		public void Activate(Map map)
        {
			if (map == null)
			{
				return;
			}
			if (parent.IsOnHoldingPlatform)
			{
				Building_HoldingPlatform building_HoldingPlatform = (Building_HoldingPlatform)parent.ParentHolder;
				building_HoldingPlatform.innerContainer.TryDrop(parent, building_HoldingPlatform.Position, building_HoldingPlatform.Map, ThingPlaceMode.Direct, 1, out var _);
				CompHoldingPlatformTarget compHoldingPlatformTarget = parent.TryGetComp<CompHoldingPlatformTarget>();
				if (compHoldingPlatformTarget != null)
				{
					compHoldingPlatformTarget.targetHolder = null;
				}
			}
            if (!parent.Spawned && parent.SpawnedOrAnyParentSpawned)
            {
				Thing t = parent.SpawnedParentOrMe;
				parent.ParentHolder.GetDirectlyHeldThings().TryDrop(parent, t.PositionHeld, t.MapHeld, ThingPlaceMode.Near, 1, out var _);
			}
			ExecuteRaid(2f);
            if (parent.Spawned)
            {
				PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
				{
					groupKind = DefDatabase<PawnGroupKindDef>.GetNamed("NAT_RustedSphereDrop"),
					tile = map.Tile,
					faction = Faction.OfEntities,
					points = new FloatRange(450f, 1000f).RandomInRange
				};
				List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms).ToList();
				foreach (Pawn p in list)
				{
					GenSpawn.Spawn(p, parent.PositionHeld, map);
				}
				LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_AssaultColony(), map, list);
			}
			passive = true;
		}

		public void OnPassive()
		{
		}

		public bool ShouldGoPassive()
		{
			return false;
		}

		public bool CanBeSuppressed()
		{
			return true;
		}

		public bool CanActivate()
		{
			return true;
		}

		public string ActivityTooltipExtra()
		{
			return null;
		}
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref raidIndex, "raidIndex", 0);
			Scribe_Values.Look(ref ticksSinceRaid, "ticksSinceRaid", 60000);
			Scribe_Values.Look(ref ticksTillRaid, "ticksTillRaid", 0);
			Scribe_References.Look(ref mapToAttack, "mapToAttack");
		}

		public override void PostPostMake()
		{
			ticksSinceRaid = 240000;
			mapToAttack = parent.MapHeld;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item2 in base.CompGetGizmosExtra())
			{
				yield return item2;
			}
			if (DebugSettings.ShowDevGizmos)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Force raid",
					action = delegate
					{
						ExecuteRaid();
						raidIndex++;
						ticksSinceRaid = Mathf.RoundToInt(Props.cooldownFactorFromRaidIndex.Evaluate(raidIndex) * Props.cooldownFactorFromActivity.Evaluate(activity.ActivityLevel) * Props.cooldownDaysRange.RandomInRange * 60000f);
					}
				};
			}
		}
		public override void CompTick()
		{
			if (parent.IsHashIntervalTick(250))
			{
				(parent as Pawn).Drawer.renderer.SetAllGraphicsDirty();
			}
			if (parent.MapHeld != null)
			{
				mapToAttack = parent.MapHeld;
			}
			if (mapToAttack != null)
			{
				if (ticksSinceRaid <= 0)
				{
					ExecuteRaid();
					raidIndex++;
					ticksSinceRaid = Mathf.RoundToInt(Props.cooldownFactorFromRaidIndex.Evaluate(raidIndex) * Props.cooldownFactorFromActivity.Evaluate(activity.ActivityLevel) * Props.cooldownDaysRange.RandomInRange * 60000f);
				}
				if (ticksTillRaid > 0)
				{
					if (ticksTillRaid == 1)
					{
						ExecuteRaid(1.2f);
					}
					ticksTillRaid--;
				}
			}
			if (ticksSinceRaid > 0)
			{
				ticksSinceRaid--;
			}
		}
		public void ExecuteRaid(float pointsFactor = 1)
		{
			RustedArmyUtility.ExecuteRaid(mapToAttack, Props.pointsFactorFromRaidIndex.Evaluate(raidIndex) * pointsFactor * Props.pointsFactorFromActivity.Evaluate(activity.ActivityLevel) * Props.pointsFromCurrentPoints.Evaluate(StorytellerUtility.DefaultThreatPointsNow(parent.MapHeld)), Rand.Chance(0.7f) ? 1 : new IntRange(2, 7).RandomInRange, false, true, "NAT_RustedArmyRaid_Mass".Translate());
		}

        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
			if(!passive && prevMap != null)
            {
				mapToAttack = prevMap;
				Activate(prevMap);
            }
            base.Notify_Killed(prevMap, dinfo);
        }
    }
}