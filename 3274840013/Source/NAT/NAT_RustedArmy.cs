using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using DelaunatorSharp;
using Gilzoide.ManagedJobs;
using Ionic.Crc;
using Ionic.Zlib;
using JetBrains.Annotations;
using KTrie;
using LudeonTK;
using NVorbis.NAudioSupport;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using RimWorld.Utility;
using RuntimeAudioClipLoader;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Grammar;
using Verse.Noise;
using Verse.Profile;
using Verse.Sound;
using Verse.Steam;

namespace NAT
{

	public static class RustedArmyUtility
    {
		public static NewAnomalyThreatsSettings Settings
		{
			get
			{
				if (settings == null)
				{
					settings = LoadedModManager.GetMod<NewAnomalyThreatsMod>().GetSettings<NewAnomalyThreatsSettings>();
				}
				return settings;
			}
		}

		private static NewAnomalyThreatsSettings settings;

		public static List<Pawn> ExecuteRaid(Map map, float points, int groupCount = 1, bool stageThenAttack = false, bool sendLetter = true, string extraDescString = null, PawnGroupKindDef groupKindOverride = null)
        {
			var parms = new IncidentParms();
			parms.target = map;
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
			{
				groupKind = groupKindOverride ?? NATDefOf.NAT_RustedArmy,
				tile = map.Tile,
				faction = Faction.OfEntities,
				points = points/groupCount
			};
			List<Pawn> raiders = new List<Pawn>();
			for (int i = 0; i < groupCount; i++)
			{
				parms.spawnRotation = Rot4.Random;
				List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms).ToList();
				if (!RCellFinder.TryFindRandomPawnEntryCell(out var spot, map, CellFinder.EdgeRoadChance_Hostile))
				{
					spot = DropCellFinder.FindRaidDropCenterDistant(map);
				}
				for (int j = 0; j < list.Count; j++)
				{
					IntVec3 loc = CellFinder.RandomClosewalkCellNear(spot, map, 8);
					GenSpawn.Spawn(list[j], loc, map, parms.spawnRotation);
				}
				if (AnomalyIncidentUtility.IncidentShardChance(points / (groupCount * 1.5f)))
				{
					AnomalyIncidentUtility.PawnShardOnDeath(list.RandomElement());
				}
				raiders.AddRange(list);
				if (stageThenAttack)
				{
					LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_StageThenAttack(Faction.OfEntities, RCellFinder.FindSiegePositionFrom(list[0].PositionHeld, map), Rand.Int), map, list);
				}
				else
				{
					LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_AssaultColony(Faction.OfEntities, false, false, false, false, false), map, list);
				}
			}
            if (sendLetter)
            {
				string desc = groupCount > 1 ? "NAT_RustedArmyRaid_Groups".Translate(groupCount) : "NAT_RustedArmyRaid_NoGroups".Translate();
				desc += "\n\n" + (stageThenAttack ? "NAT_RustedArmyRaid_Stage".Translate() : "NAT_RustedArmyRaid_Immediate".Translate());
				if(extraDescString != null)
                {
					desc += "\n\n" + extraDescString;
				}
				Find.LetterStack.ReceiveLetter("NAT_RustedArmyRaid".Translate(), desc, LetterDefOf.ThreatBig, raiders);
			}
			return raiders;
		}
	}

	public class CompRustedSoldier : ThingComp
    {
		public bool updated = false;
        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Values.Look(ref updated, "updated", false);
		}
        public override void CompTick()
		{
			base.CompTick();
            if (updated)
            {
				return;
            }
			Pawn p = null;
			if(!(parent is RustedPawn) && parent is Pawn pawn)
            {
				try
				{
					p = PawnGenerator.GeneratePawn(pawn.kindDef, pawn.Faction);
					p.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeChronologicalTicks;
					p.ageTracker.AgeBiologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
					if(pawn.Name != null && pawn.Name.IsValid)
                    {
						p.Name = pawn.Name;
					}
					p.inventory.innerContainer.TryAddRangeOrTransfer(pawn.inventory.innerContainer);
					if (pawn.Spawned)
					{
						GenSpawn.Spawn(p, pawn.PositionHeld, pawn.MapHeld);
					}
                    else
                    {
						if (pawn.IsOnHoldingPlatform)
						{
							Building_HoldingPlatform building_HoldingPlatform = (Building_HoldingPlatform)pawn.ParentHolder;
							building_HoldingPlatform.innerContainer.TryDrop(pawn, building_HoldingPlatform.Position, building_HoldingPlatform.Map, ThingPlaceMode.Direct, 1, out var _);
							CompHoldingPlatformTarget compHoldingPlatformTarget1 = pawn.TryGetComp<CompHoldingPlatformTarget>();
							if (compHoldingPlatformTarget1 != null)
							{
								compHoldingPlatformTarget1.targetHolder = null;
							}
							CompHoldingPlatformTarget compHoldingPlatformTarget2 = p.TryGetComp<CompHoldingPlatformTarget>();
							if (compHoldingPlatformTarget2 != null)
							{
								compHoldingPlatformTarget2.targetHolder = building_HoldingPlatform;
								building_HoldingPlatform.innerContainer.TryAddOrTransfer(p);
							}
						}
						else
						{
							if (pawn.GetCaravan() != null)
							{
								Caravan caravan = pawn.GetCaravan();
								caravan.AddPawn(p, true);
								caravan.RemovePawn(pawn);
							}
							else if(pawn.ParentHolder != null && pawn.ParentHolder.GetDirectlyHeldThings() != null)
							{
								ThingOwner container = pawn.ParentHolder.GetDirectlyHeldThings();
								container.Remove(pawn);
								container.TryAddOrTransfer(p);
							}
							if (pawn.IsWorldPawn())
							{
								Find.WorldPawns.PassToWorld(p);
							}
						}
					}
					if(pawn.TryGetLord(out var lord))
                    {
						lord.AddPawn(p);
						lord.RemovePawn(pawn);
                    }
					if (!pawn.Destroyed)
					{
						pawn.Destroy();
					}
				}
				catch (Exception ex)
				{
					Log.Error("New Anomaly Threats got an error while updating rusted pawn, please send this log to GoGaTio(gogatio in discord): " + ex);
					parent.Destroy();
					updated = true;
				}
                finally
                {
                    if (!updated)
                    {
						Log.Message(string.Concat(p, "was updated"));
                    }
                }
            }
            else
            {
				updated = true;
            }
        }
    }

	public class CompProperties_RustedShield : CompProperties
    {
		public int maxHealth;

		public int regenInterval;

		public int ticksToRestore;

		public SoundDef destroyedSound;

		public EffecterDef destroyedEffect;

		public PawnRenderNodeProperties renderProps;

		public CompProperties_RustedShield()
        {
			compClass = typeof(CompRustedShield);
        }
	}
	public class CompRustedShield : ThingComp
	{
		public int health = -1;

		public int ticksToRegen = -1;

		public int ticksSinceDestroyed = -1;

		public bool destroyed = false;
		public CompProperties_RustedShield Props => (CompProperties_RustedShield)props;

		public RustedPawn Owner => parent as RustedPawn;

		public override void PostPostMake()
		{
			base.PostPostMake();
			if (health == -1 && !destroyed)
			{
				health = Props.maxHealth;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (Owner == null)
			{
				return;
			}
            if (destroyed)
            {
				ticksSinceDestroyed++;
                if (Props.ticksToRestore <= ticksSinceDestroyed)
                {
					ticksSinceDestroyed = -1;
					health = Props.maxHealth;
					destroyed = false;
					Owner.Drawer.renderer.SetAllGraphicsDirty();
				}
			}
            else if (Props.maxHealth > health)
			{
				ticksToRegen++;
				if (ticksToRegen >= Props.regenInterval)
				{
					ticksToRegen = 0;
					health++;
				}
			}
		}

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach(Gizmo g in base.CompGetGizmosExtra())
            {
				yield return g;
            }
			if(Find.Selector.SingleSelectedThing == parent)
            {
				yield return new RustedShieldGizmo(this);
			}
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			absorbed = false;
			if (destroyed || Owner == null)
			{
				return;
			}
			if (dinfo.Def.harmsHealth && dinfo.Def != DamageDefOf.EMP)
			{
				absorbed = true; 
				//if(dinfo.Def == DamageDefOf.Flame && Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(Owner)))
                //{
				//	Owner.TryAttachFire(Rand.Range(0.15f, 0.25f), dinfo.Instigator);
                //}
				health -= Mathf.RoundToInt(dinfo.Amount);
				if (health <= 0f)
				{
					ticksSinceDestroyed = 0;
					destroyed = true;
					Props.destroyedSound?.PlayOneShot(Owner);
					Owner.Drawer.renderer.SetAllGraphicsDirty();
					Props.destroyedEffect.Spawn(Owner.PositionHeld, Owner.MapHeld);
				}
				else
				{
                    if (Owner.SpawnedOrAnyParentSpawned)
                    {
						EffecterDef effecterDef = (dinfo.Def != DamageDefOf.Bullet) ? EffecterDefOf.Deflect_Metal : EffecterDefOf.Deflect_Metal_Bullet;
						if (Owner.health.deflectionEffecter == null || Owner.health.deflectionEffecter.def != effecterDef)
						{
							if (Owner.health.deflectionEffecter != null)
							{
								Owner.health.deflectionEffecter.Cleanup();
								Owner.health.deflectionEffecter = null;
							}
							Owner.health.deflectionEffecter = effecterDef.Spawn();
						}
						Owner.health.deflectionEffecter.Trigger(Owner, dinfo.Instigator ?? Owner);
					}
					
				}
			}
		}

        public override List<PawnRenderNode> CompRenderNodes()
        {
			List<PawnRenderNode> list = new List<PawnRenderNode>();
            if (!destroyed && Owner != null)
            {
				PawnRenderNode pawnRenderNode = (PawnRenderNode)Activator.CreateInstance(Props.renderProps.nodeClass, Owner, Props.renderProps, Owner.Drawer.renderer.renderTree);
				list.Add(pawnRenderNode);
			}
            return list;
        }

        public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref destroyed, "destroyed", false);
			Scribe_Values.Look(ref health, "health", -1);
			Scribe_Values.Look(ref ticksToRegen, "ticksToRegen", -1);
			Scribe_Values.Look(ref ticksSinceDestroyed, "ticksSinceDestroyed", -1);
		}
	}

	public class CompProperties_RustedOfficer : CompProperties
	{
		public int ticksToRestore = 300000;

		public int cooldown = 7500;

		public float maxRange = 27.9f;

		public float minRange = 4.9f;

		public int maxUnits = 6;

		public ThingDef skyfaller;

		public PawnKindDef pawnKind;

		public TargetingParameters targetingParameters;

		[NoTranslate]
		public string activateTexPath;

		public CompProperties_RustedOfficer()
		{
			compClass = typeof(CompRustedOfficer);
		}
	}
	public class CompRustedOfficer : ThingComp, ITargetingSource
	{
		public int units = -1;

		public int ticksSinceRestore = -1;

		public int ticksSinceUse = -1;

		public int wantedCount = 1;
		public CompProperties_RustedOfficer Props => (CompProperties_RustedOfficer)props;

		public Pawn Officer => parent as Pawn;
		public bool CasterIsPawn => true;
		public bool IsMeleeAttack => false;
		public bool Targetable => true;
		public bool MultiSelect => false;
		public bool HidePawnTooltips => false;
		public Thing Caster => parent;
		public Pawn CasterPawn => null;
		public Verb GetVerb => null;
		public TargetingParameters targetParams => Props.targetingParameters;

		[Unsaved(false)]
		private Texture2D activateTex;

		public Texture2D UIIcon
		{
			get
			{
				if (!(activateTex != null))
				{
					return activateTex = ContentFinder<Texture2D>.Get(Props.activateTexPath);
				}
				return activateTex;
			}
		}

		public virtual ITargetingSource DestinationSelector => null;

		public override void PostPostMake()
		{
			base.PostPostMake();
			if (units == -1)
			{
				units = Props.maxUnits;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if(units < Props.maxUnits)
            {
				ticksSinceRestore++;
				if(ticksSinceRestore >= Props.ticksToRestore)
                {
					ticksSinceRestore = 0;
					units++;
				}
			}
			if(ticksSinceUse <= Props.cooldown)
            {
				ticksSinceUse++;
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (parent.Spawned && (parent.Faction == Faction.OfPlayer || DebugSettings.ShowDevGizmos))
			{
				Command_ActionRustedRequest command_Action = new Command_ActionRustedRequest
				{
					defaultLabel = "NAT_DropTroopers".Translate(),
					defaultDesc = "NAT_DropTroopers_Desc".Translate(),
					comp = this,
					cooldownPercentGetter = () => Mathf.InverseLerp(Props.cooldown, 0f, Props.cooldown - ticksSinceUse),
					action = delegate
					{
						if(units > 1)
                        {
							List<FloatMenuOption> list = new List<FloatMenuOption>(); 
							list.Add(new FloatMenuOption("1", delegate
							{
								wantedCount = 1;
								Find.Targeter.BeginTargeting(this);
							}));
							list.Add(new FloatMenuOption("2", delegate
							{
								wantedCount = 2;
								Find.Targeter.BeginTargeting(this);
							}));
							if(units > 2)
                            {
								list.Add(new FloatMenuOption("3", delegate
								{
									wantedCount = 3;
									Find.Targeter.BeginTargeting(this);
								}));
							}
							Find.WindowStack.Add(new FloatMenu(list));
						}
                        else
                        {
							wantedCount = 1;
							Find.Targeter.BeginTargeting(this);
						}
						
					},
					icon = UIIcon
				};
				command_Action.groupable = false;
				if(Find.Selector.NumSelected > 1 && Officer.Name != null)
                {
					command_Action.defaultLabel += "(" + Officer.Name + ")";
				}
				if(units <= 0)
                {
					command_Action.Disable("NAT_NoUnits".Translate().CapitalizeFirst());
				}
				if (ticksSinceUse < Props.cooldown)
				{
					command_Action.Disable(("NAT_DropOnCooldown".Translate() + " (" + "DurationLeft".Translate((Props.cooldown - ticksSinceUse).ToStringTicksToPeriod()) + ")").CapitalizeFirst());
				}
				yield return command_Action;
			}
			if (DebugSettings.ShowDevGizmos && units < Props.maxUnits)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Set units to max",
					action = delegate
					{
						units = Props.maxUnits;
						ticksSinceRestore = 0;
					}
				};
			}
			if(Find.Selector.SingleSelectedThing == parent && (Officer.Faction == Faction.OfPlayer || DebugSettings.ShowDevGizmos))
            {
				yield return new Gizmo_RustedOfficer(this);
            }
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref units, "units", -1);
			Scribe_Values.Look(ref ticksSinceRestore, "ticksSinceRestore", -1);
			Scribe_Values.Look(ref ticksSinceUse, "ticksSinceUse", -1);
			Scribe_Values.Look(ref wantedCount, "wantedCount", 1);
		}

		public bool CanHitTarget(LocalTargetInfo target)
		{
			return ValidateTarget(target, showMessages: false);
		}

		public virtual AcceptanceReport CanCast(LocalTargetInfo target)
		{
            if (target.Cell.IsValid)
            {
				IntVec3 cell = target.Cell;
				if (Props.maxRange <= target.Cell.DistanceTo(parent.Position))
				{
					return "OutOfRange".Translate();
				}
				if (Props.minRange > target.Cell.DistanceTo(parent.Position))
				{
					return "TooClose".Translate();
				}
				if (!GenSight.LineOfSightToThing(cell, Officer, parent.Map))
                {
					return "CannotHitTarget".Translate();
				}
                if (cell.GetRoof(parent.MapHeld)?.isThickRoof == true)
                {
					return "NAT_CannotDrop_Roof".Translate();
				}
				return true;
			}
			return false;
		}

		public bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
		{
			if (!target.IsValid || target.Cell == null || !target.Cell.IsValid)
			{
				return false;
			}
			AcceptanceReport acceptanceReport = CanCast(target);
			if (!acceptanceReport.Accepted)
			{
				if (showMessages && !acceptanceReport.Reason.NullOrEmpty())
				{
					Messages.Message("CannotGenericWorkCustom".Translate(NATDefOf.NAT_RequestDrop.reportString) + ": " + acceptanceReport.Reason.CapitalizeFirst(), Officer, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}

		public void DrawHighlight(LocalTargetInfo target)
		{
			if (target.IsValid)
			{
				GenDraw.DrawTargetHighlight(target);
				GenDraw.DrawRadiusRing(parent.Position, Props.maxRange);
				GenDraw.DrawRadiusRing(parent.Position, Props.minRange, Color.red);
			}
		}

		public virtual void OrderForceTarget(LocalTargetInfo target)
		{
			if (ValidateTarget(target, showMessages: false))
			{
				Job job = JobMaker.MakeJob(NATDefOf.NAT_RequestDrop, parent, target);
				Officer.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		}

		public void OnGUI(LocalTargetInfo target)
		{
			//string label = ((!string.IsNullOrEmpty(Props.guiLabelString)) ? Props.guiLabelString : ((string)"ChooseWhoShouldActivate".Translate()));
			//Widgets.MouseAttachedLabel(label);
			if (ValidateTarget(target, showMessages: false) && Props.targetingParameters.CanTarget(target.ToTargetInfo(parent.MapHeld ?? Find.CurrentMap), this))
			{
				GenUI.DrawMouseAttachment(UIIcon);
			}
			else
			{
				GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
			}
		}

		public void DropUnits(IntVec3 cell)
        {
			ticksSinceUse = 0;
			List<Pawn> list = new List<Pawn>();
			for(int i = 0;i < wantedCount; i++)
            {
				Pawn p = PawnGenerator.GeneratePawn(Props.pawnKind, Officer.Faction);
				list.Add(p);
				RCellFinder.TryFindRandomCellNearWith(cell, (IntVec3 c) => !c.Impassable(parent.Map) && c.GetRoof(parent.MapHeld)?.isThickRoof != true, parent.MapHeld, out var cell2, 3);
				SkyfallerMaker.SpawnSkyfaller(Props.skyfaller, p, cell2, parent.MapHeld ?? Find.CurrentMap);
				units--;
			}
			RCellFinder.TryFindRandomSpotJustOutsideColony(parent.PositionHeld, parent.MapHeld, out var result);
			LordMaker.MakeNewLord(Officer.Faction, new LordJob_AssistColony_Rust(result), parent.MapHeld, list);
			if(Officer.Faction != Faction.OfPlayer)
            {
				Messages.Message("NAT_DropRequested".Translate(Officer.Name?.ToStringFull ?? Officer.kindDef.LabelCap), list, MessageTypeDefOf.ThreatBig);
			}
		}
	}

	public class LordJob_AssistColony_Rust : LordJob
	{
		private IntVec3 fallbackLocation;

		public LordJob_AssistColony_Rust()
		{
		}

		public LordJob_AssistColony_Rust(IntVec3 fallbackLocation)
		{
			this.fallbackLocation = fallbackLocation;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_HuntEnemies lordToil_HuntEnemies = (LordToil_HuntEnemies)(stateGraph.StartingToil = new LordToil_HuntEnemies(fallbackLocation));
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap();
			stateGraph.AddToil(lordToil_ExitMap);
			Transition transition = new Transition(lordToil_HuntEnemies, lordToil_ExitMap);
			transition.AddPreAction(new TransitionAction_Message("NAT_MessageRustedTroopersLeaving".Translate()));
			transition.AddTrigger(new Trigger_TicksPassed(30000));
			//transition.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			stateGraph.AddTransition(transition);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref fallbackLocation, "fallbackLocation");
		}
	}

	public class Command_ActionRustedRequest : Command_ActionWithCooldown
    {
		public CompRustedOfficer comp;
        public override void GizmoUpdateOnMouseover()
        {
            base.GizmoUpdateOnMouseover();
			GenDraw.DrawRadiusRing(comp.parent.Position, comp.Props.maxRange);
			GenDraw.DrawRadiusRing(comp.parent.Position, comp.Props.minRange, Color.red);
		}
    }

	public class JobGiver_RustedOfficer : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.Faction == Faction.OfPlayer || !pawn.Spawned || pawn.DeadOrDowned)
			{
				return null;
			}
			CompRustedOfficer comp = pawn.TryGetComp<CompRustedOfficer>();
			if(comp == null || comp.units <= 0 || comp.Props.cooldown > comp.ticksSinceUse)
            {
				return null;
            }
			List<Pawn> list = pawn.Map.mapPawns.AllPawnsSpawned.Where((Pawn p) => !p.DeadOrDowned && comp.Props.maxRange >= p.Position.DistanceTo(pawn.Position) && GenSight.LineOfSightToThing(p.Position, pawn, pawn.Map) && !FactionUtility.AllyOrNeutralTo(pawn.Faction, p.Faction)).ToList();
            if (list.NullOrEmpty())
            {
				return null;
            }
			LocalTargetInfo target = list.RandomElement();
			Job job = JobMaker.MakeJob(NATDefOf.NAT_RequestDrop, pawn, target);
			comp.wantedCount = Mathf.Min(comp.units, 3);
			return job;
		}
	}

	public class JobDriver_RustedOfficer : JobDriver
	{
		protected IntVec3 Cell => job.GetTarget(TargetIndex.B).Cell;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			CompRustedOfficer comp = pawn.TryGetComp<CompRustedOfficer>();
			yield return Toils_General.Wait(20).WithProgressBarToilDelay(TargetIndex.A);
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.initAction = delegate
			{
				comp.DropUnits(Cell);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
		}
	}

	public class JobGiver_MakeRustedWeapon : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (Rand.Value < 0.1f)
			{
				if (pawn.equipment.Primary == null)
				{
					if(pawn is RustedPawn rust && rust.weaponDef == null)
                    {
						rust.weaponDef = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef t) => t.IsWeapon && !t.weaponTags.NullOrEmpty() && !t.weaponTags.Contains("NAT_UnableCreatingByPlayer") && t.weaponTags.Any((string s) => rust.kindDef.weaponTags.Contains(s))).RandomElement();
					}
					return JobMaker.MakeJob(NATDefOf.NAT_ChangeRustedWeapon);
				}
			}
			return null;
		}
	}

	public class RustedPawn : Pawn
    {
		public bool Draftable
        {
            get
            {
				if(kindDef.HasModExtension<RustedPawnExtention>() && !kindDef.GetModExtension<RustedPawnExtention>().defaultDraftable)
                {
					return false;
                }
				return true;
            }
        }

		public bool Controllable
        {
            get
            {
				if (kindDef.HasModExtension<RustedPawnExtention>() && kindDef.GetModExtension<RustedPawnExtention>().nonPlayer)
				{
					return false;
				}
				return true;
			}
        }
		public ThingDef weaponDef;
        public override void Tick()
        {
            base.Tick();
			if (DeadOrDowned || !Spawned || Faction == Faction.OfPlayer)
			{
				return;
			}
			if (this.IsHashIntervalTick(300) && IsMissingWeapon())
			{
				PawnWeaponGenerator.TryGenerateWeaponFor(this, default(PawnGenerationRequest));
				if (this.GetLord() == null)
				{
					LordMaker.MakeNewLord(Faction, new LordJob_AssaultColony(), MapHeld, new List<Pawn>() { this });
				}
			}
		}

        public bool IsMissingWeapon()
		{
			List<string> weaponTags = kindDef.weaponTags;
			if (weaponTags.NullOrEmpty())
			{
				return false;
			}
			List<ThingWithComps> allEquipmentListForReading = equipment.AllEquipmentListForReading;
			for (int i = 0; i < allEquipmentListForReading.Count; i++)
			{
				if (allEquipmentListForReading[i].def.weaponTags.Any((string t) => weaponTags.Contains(t)))
				{
					return false;
				}
			}
			return true;
		}

		public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
			if (Name != null && Name.IsValid)
			{
				if(Faction != Faction.OfPlayer || ((Drafted && RustedArmyUtility.Settings.rustedSoldierName_Draft) || (!Drafted && RustedArmyUtility.Settings.rustedSoldierName_NoDraft)))
                {
					Vector2 pos = GenMapUI.LabelDrawPosFor(this, -0.7f);
					GenMapUI.DrawPawnLabel(this, pos);
				}
			}
            else
            {
				Name = PawnBioAndNameGenerator.GeneratePawnName(this, NameStyle.Full);
			}
		}

        public override IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
        {
			foreach(FloatMenuOption op in base.GetExtraFloatMenuOptionsFor(sq))
            {
				yield return op;
			}
            if (!Controllable)
            {
				yield break;
            }
			foreach (Thing thing in sq.GetThingList(Map))
			{
                if (thing.HasComp<CompUsableByRust>())
                {
					CompUsableByRust comp = thing.TryGetComp<CompUsableByRust>();
					Action action = null;
					string s = comp.JobReport + " " + thing.Label;
					if (comp.CanBeUsedBy(this).Accepted)
                    {
						action = delegate
						{
							this.jobs.TryTakeOrderedJob(JobMaker.MakeJob(NATDefOf.NAT_UseItemByRust, thing), JobTag.Misc);
						};
					}
					else if(comp.CanBeUsedBy(this) != false)
                    {
						s += comp.CanBeUsedBy(this).Reason;
					}
					yield return new FloatMenuOption(s, action);
				}
			}
			foreach (LocalTargetInfo item in GenUI.TargetsAt(sq.ToVector3(), TargetingParameters.ForStrip(this), thingsOnly: true))
			{
				LocalTargetInfo stripTarg = item;
				yield return (this.CanReach(stripTarg, PathEndMode.ClosestTouch, Danger.Deadly) ? ((stripTarg.Pawn != null && stripTarg.Pawn.HasExtraHomeFaction()) ? new FloatMenuOption("CannotStrip".Translate(stripTarg.Thing.LabelCap, stripTarg.Thing) + ": " + "QuestRelated".Translate().CapitalizeFirst(), null) : (this.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Strip".Translate(stripTarg.Thing.LabelCap, stripTarg.Thing), delegate
				{
					stripTarg.Thing.SetForbidden(value: false, warnOnFail: false);
					this.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Strip, stripTarg), JobTag.Misc);
					StrippableUtility.CheckSendStrippingImpactsGoodwillMessage(stripTarg.Thing);
				}), this, stripTarg) : new FloatMenuOption("CannotStrip".Translate(stripTarg.Thing.LabelCap, stripTarg.Thing) + ": " + "Incapable".Translate().CapitalizeFirst(), null))) : new FloatMenuOption("CannotStrip".Translate(stripTarg.Thing.LabelCap, stripTarg.Thing) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
			}
		}
        public override IEnumerable<Gizmo> GetGizmos()
        {
			foreach(Gizmo g in base.GetGizmos())
            {
				yield return g;
            }
			if (Faction == Faction.OfPlayer && Draftable)
			{
				if (drafter.ShowDraftGizmo)
				{
					Command_Toggle command_Toggle = new Command_Toggle();
					command_Toggle.hotKey = KeyBindingDefOf.Command_ColonistDraft;
					command_Toggle.isActive = () => drafter.Drafted;
					command_Toggle.toggleAction = delegate
					{
						drafter.Drafted = !drafter.Drafted;
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Drafting, KnowledgeAmount.SpecificInteraction);
						if (drafter.Drafted)
						{
							LessonAutoActivator.TeachOpportunity(ConceptDefOf.QueueOrders, OpportunityType.GoodToKnow);
						}
					};
					command_Toggle.defaultDesc = "CommandToggleDraftDesc".Translate();
					command_Toggle.icon = TexCommand.Draft;
					command_Toggle.turnOnSound = SoundDefOf.DraftOn;
					command_Toggle.turnOffSound = SoundDefOf.DraftOff;
					command_Toggle.groupKeyIgnoreContent = 81729172;
					command_Toggle.defaultLabel = (drafter.Drafted ? "CommandUndraftLabel" : "CommandDraftLabel").Translate();
					if (drafter.pawn.Downed)
					{
						command_Toggle.Disable("IsIncapped".Translate(this.LabelShort, this));
					}
					if (!drafter.Drafted)
					{
						command_Toggle.tutorTag = "Draft";
					}
					else
					{
						command_Toggle.tutorTag = "Undraft";
					}
					yield return command_Toggle;
				}
				if (drafter.Drafted && this.equipment.Primary != null && equipment.Primary.def.IsRangedWeapon)
				{
					Command_Toggle command_Toggle2 = new Command_Toggle();
					command_Toggle2.hotKey = KeyBindingDefOf.Misc6;
					command_Toggle2.isActive = () => drafter.FireAtWill;
					command_Toggle2.toggleAction = delegate
					{
						drafter.FireAtWill = !drafter.FireAtWill;
					};
					command_Toggle2.icon = TexCommand.FireAtWill;
					command_Toggle2.defaultLabel = "CommandFireAtWillLabel".Translate();
					command_Toggle2.defaultDesc = "CommandFireAtWillDesc".Translate();
					command_Toggle2.tutorTag = "FireAtWillToggle";
					yield return command_Toggle2;
				}
				foreach (Gizmo attackGizmo in PawnAttackGizmoUtility.GetAttackGizmos(this))
				{
					AcceptanceReport allowsDrafting = this.GetLord()?.AllowsDrafting(this) ?? ((AcceptanceReport)true);
					if (!allowsDrafting && !attackGizmo.Disabled)
					{
						attackGizmo.Disabled = true;
						attackGizmo.disabledReason = allowsDrafting.Reason;
					}
					yield return attackGizmo;
				}
			}
			foreach (Ability a in abilities.AllAbilitiesForReading)
			{
				if (Faction == Faction.OfPlayer)
				{
					bool visibleSecondary = (Drafted || a.def.displayGizmoWhileUndrafted) && a.GizmosVisible();
					if (visibleSecondary)
					{
						foreach (Command gizmo in a.GetGizmos())
						{
							yield return gizmo;
						}
					}
				}
				foreach (Gizmo item in a.GetGizmosExtra())
				{
					yield return item;
				}
			}
		}
        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
			if (Faction == Faction.OfPlayer && (!kindDef.HasModExtension<RustedPawnExtention>() || kindDef.GetModExtension<RustedPawnExtention>().sendDeathLetter) && RustedArmyUtility.Settings.rustedSoldierDeathNotification)
			{
				Find.LetterStack.ReceiveLetter("Death".Translate() + ": " + Name, dinfo.Value.Def.deathMessage.Formatted(this.LabelShortCap, this.Named("PAWN")), LetterDefOf.Death);
			}
			base.Kill(dinfo, exactCulprit);
        }

        public override void ExposeData()
        {
			Scribe_Defs.Look(ref weaponDef, "weaponDef");
			base.ExposeData();
		}
    }

	public class RustedPawnExtention : DefModExtension
    {
		public bool defaultDraftable = true;

		public bool scenarioAvailable = true;

		public bool sendDeathLetter = true;

		public bool nonPlayer = false;
	}

	public class CompProperties_RustedSculpture : CompProperties_Interactable
	{
		public int shardsRequired = 1;

		public PawnKindDef pawnKind;

		public CompProperties_RustedSculpture()
		{
			compClass = typeof(CompRustedSculpture);
		}
	}
	public class CompRustedSculpture : CompInteractable
	{
		private new CompProperties_RustedSculpture Props => (CompProperties_RustedSculpture)props;

		public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
		{
			if (activateBy != null)
			{
				if (checkOptionalItems && !activateBy.HasReserved(ThingDefOf.Shard) && !ReservationUtility.ExistsUnreservedAmountOfDef_NewTemp(parent.MapHeld, ThingDefOf.Shard, Faction.OfPlayer, Props.shardsRequired, (Thing t) => activateBy.CanReserveAndReach(t, PathEndMode.Touch, Danger.None)))
				{
					return "NAT_RustedSculptureActivateMissingShards".Translate(Props.shardsRequired);
				}
			}
			else if (checkOptionalItems && !ReservationUtility.ExistsUnreservedAmountOfDef(parent.MapHeld, ThingDefOf.Shard, Faction.OfPlayer, Props.shardsRequired))
			{
				return "NAT_RustedSculptureActivateMissingShards".Translate(Props.shardsRequired);
			}
			return base.CanInteract(activateBy, checkOptionalItems);
		}

		public override void OrderForceTarget(LocalTargetInfo target)
		{
			if (ValidateTarget(target, showMessages: false))
			{
				OrderActivation(target.Pawn);
			}
		}

		protected override void OnInteracted(Pawn caster)
		{
			Pawn p = PawnGenerator.GeneratePawn(Props.pawnKind, caster.Faction);
			p.ageTracker.AgeBiologicalTicks = 0;
			p.ageTracker.AgeChronologicalTicks = 0;
			GenSpawn.Spawn(p, parent.PositionHeld, parent.MapHeld);
			parent.Destroy();
		}

		private void OrderActivation(Pawn pawn)
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
	public class LordJob_DefendVoidStructure : LordJob
	{
		private Thing structure;

		private float? wanderRadius;

		private float? defendRadius;

		private bool isCaravanSendable;

		private bool addFleeToil;

		private int ticksBeforeAttack;

		public override bool IsCaravanSendable => isCaravanSendable;

		public override bool AddFleeToil => addFleeToil;

		public LordJob_DefendVoidStructure()
		{
		}

		public LordJob_DefendVoidStructure(Thing structure, int ticksBeforeAttack, float? wanderRadius = null, float? defendRadius = null, bool isCaravanSendable = false, bool addFleeToil = true)
		{
			this.structure = structure;
			this.ticksBeforeAttack = ticksBeforeAttack;
			this.wanderRadius = wanderRadius;
			this.defendRadius = defendRadius;
			this.isCaravanSendable = isCaravanSendable;
			this.addFleeToil = addFleeToil;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_DefendPoint lordToil_DefendStructure = (LordToil_DefendPoint)(stateGraph.StartingToil = new LordToil_DefendPoint(structure.Position, wanderRadius: wanderRadius, defendRadius: defendRadius));
			LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony(attackDownedIfStarving: true)
			{
				useAvoidGrid = true
			};
			stateGraph.AddToil(lordToil_AssaultColony);
			Transition transition = new Transition(lordToil_DefendStructure, lordToil_AssaultColony);
			transition.AddTrigger(new Trigger_FractionPawnsLost(0.1f));
			transition.AddTrigger(new Trigger_PawnHarmed(0.5f));
			transition.AddTrigger(new Trigger_TicksPassed(ticksBeforeAttack));
			transition.AddTrigger(new Trigger_OnClamor(ClamorDefOf.Ability));
			transition.AddTrigger(new Trigger_StructureActivated(structure));
			transition.AddPostAction(new TransitionAction_WakeAll());
			TaggedString taggedString = "MessageDefendersAttacking".Translate("NAT_RustedSoldiers".Translate(), "NAT_RustedArmy".Translate(), Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
			transition.AddPreAction(new TransitionAction_Message(taggedString, MessageTypeDefOf.ThreatBig));
			stateGraph.AddTransition(transition);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_Deep.Look(ref structure, "structure");
			Scribe_Values.Look(ref wanderRadius, "wanderRadius");
			Scribe_Values.Look(ref defendRadius, "defendRadius");
			Scribe_Values.Look(ref isCaravanSendable, "isCaravanSendable", defaultValue: false);
			Scribe_Values.Look(ref addFleeToil, "addFleeToil", defaultValue: false);
		}
	}

	public class TriggerData_StructureActivated : TriggerData
	{
		public Thing structure;

		public override void ExposeData()
		{
			Scribe_References.Look(ref structure, "structure", saveDestroyedThings: true);
		}
	}

	public class Trigger_StructureActivated : Trigger
	{
		protected TriggerData_StructureActivated Data => (TriggerData_StructureActivated)data;

		public Trigger_StructureActivated(Thing structure)
		{
			data = new TriggerData_StructureActivated();
			Data.structure = structure;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick)
			{
				if (data == null || !(data is TriggerData_StructureActivated))
				{
					return true;
				}
				TriggerData_StructureActivated triggerData_StructureActivated = Data;
				Thing structure = triggerData_StructureActivated.structure;
				if(!(structure is ThingWithComps s) || s.GetComp<CompVoidStructure>().Active)
                {
					return true;
                }
			}
			return false;
		}
	}

	public class IncidentWorker_RustedArmyRaid : IncidentWorker
	{
		private static readonly SimpleCurve PointsFromPoints = new SimpleCurve
		{
			new CurvePoint(0f, 600f),
			new CurvePoint(1000f, 1200f),
			new CurvePoint(10000f, 10000f)
		};
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (!ModsConfig.AnomalyActive)
			{
				return false;
			}
			Map map = (Map)parms.target;
			RustedArmyUtility.ExecuteRaid(map, PointsFromPoints.Evaluate(parms.points), Rand.Chance(0.7f) ? 1 : new IntRange(2, 3).RandomInRange, Rand.Chance(0.3f));
			return true;
		}
	}
	[StaticConstructorOnStartup]
	public class RustedShieldGizmo : Gizmo
	{
		private CompRustedShield shield;

		private const float Width = 160f;

		private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(GenUI.FillableBar_Empty);

		private const int BarThresholdTickIntervals = 100;

		public RustedShieldGizmo(CompRustedShield comp)
		{
			shield = comp;
		}

		public override float GetWidth(float maxWidth)
		{
			return 160f;
		}

		private Texture2D BarTex
        {
            get
            {
                if (shield.destroyed)
                {
					return SolidColorMaterials.NewSolidColorTexture(new Color32(95, 88, 88, byte.MaxValue));
				}
				return SolidColorMaterials.NewSolidColorTexture(new Color32(102, 95, 95, byte.MaxValue));
			}
        }

		private string Label
        {
            get
            {
                if (shield.destroyed)
                {
					return "NAT_Restored".Translate() + ": " + Mathf.FloorToInt(FillPercent * 100).ToString() + "%";
				}
				return shield.health + "/" + shield.Props.maxHealth;
			}
        }

		private float FillPercent
        {
            get
            {
                if (shield.destroyed)
                {
					return (float)shield.ticksSinceDestroyed / (float)shield.Props.ticksToRestore;
				}
				return (float)shield.health / (float)shield.Props.maxHealth;
			}
        }

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect rect2 = rect.ContractedBy(10f);
			Widgets.DrawWindowBackground(rect);
			string text = (string)"NAT_RustedShield".Translate();
			Rect rect3 = new Rect(rect2.x, rect2.y, rect2.width, Text.CalcHeight(text, rect2.width) + 8f);
			Text.Font = GameFont.Small;
			Widgets.Label(rect3, text);
			Rect barRect = new Rect(rect2.x, rect3.yMax, rect2.width, rect2.height - rect3.height);
			Widgets.FillableBar(barRect, FillPercent, BarTex, EmptyBarTex, doBorder: true);
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(barRect, Label);
			Text.Anchor = TextAnchor.UpperLeft;
			string tooltip;
			tooltip = "NAT_RustedShieldTip".Translate();
			TooltipHandler.TipRegion(rect2, () => tooltip, Gen.HashCombineInt(shield.GetHashCode(), 34242369));
			return new GizmoResult(GizmoState.Clear);
		}
	}

	[StaticConstructorOnStartup]
	public class Gizmo_RustedOfficer : Gizmo
	{
		private static readonly float Width = 160f;

		private CompRustedOfficer comp;

		public Gizmo_RustedOfficer(CompRustedOfficer comp)
		{
			this.comp = comp;
			Order = -100f;
		}

		public override float GetWidth(float maxWidth)
		{
			return Width;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect rect2 = rect.ContractedBy(6f);
			Widgets.DrawWindowBackground(rect);
			Rect rect3 = rect2;
			rect3.height = rect.height / 2f;
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.Label(rect3, "NAT_Drops".Translate() + ": " + comp.units + "/" + comp.Props.maxUnits);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Tiny;
			Rect rect4 = rect;
			rect4.y += rect3.height - 5f;
			rect4.height = rect.height / 2f;
			Text.Anchor = TextAnchor.MiddleCenter;
			if(comp.units < comp.Props.maxUnits)
            {
				Widgets.Label(rect4, "NAT_Restoring".Translate() + ": " + (comp.Props.ticksToRestore - comp.ticksSinceRestore).ToStringTicksToDays());
			}
			Text.Anchor = TextAnchor.UpperLeft;
			return new GizmoResult(GizmoState.Clear);
		}
	}
}