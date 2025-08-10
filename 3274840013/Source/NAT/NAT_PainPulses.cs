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
	public static class PainUtility
    {
		private static List<ThingDef> transformableIntoWall;

		private static List<ThingDef> transformableIntoFigure;

		public static List<ThingDef> TransformableIntoWall
		{
            get
            {
				if (transformableIntoWall.NullOrEmpty())
				{
					transformableIntoWall = DefDatabase<NAT.TransformationDef>.GetNamed("Walls").transformingThings;
				}
				return transformableIntoWall;
            }
        }

		public static List<ThingDef> TransformableIntoFigure
		{
            get
            {
                if (transformableIntoFigure.NullOrEmpty())
                {
					transformableIntoFigure = DefDatabase<NAT.TransformationDef>.GetNamed("Figures").transformingThings;
				}
				return transformableIntoFigure;
            }
        }

		public static bool IsTransformable(Thing t)
        {
			return TransformableIntoWall.Contains(t.def) || TransformableIntoFigure.Contains(t.def);
		}

		public static bool TryTransformMultipleThing(Map map, out List<Thing> transformedThings, int transformations = 5, bool ignoreBeacon = true)
		{
			transformedThings = null;
			Thing t = null;
			if (ignoreBeacon)
			{
				if (!TryTransformRandomThing(map, out var transformedThing))
				{
					return false;
				}
				t = transformedThing;
			}
            else
            {
				Predicate<Building> p = (Building b3) => map.listerBuildings.allBuildingsColonist.Where((Building b4) => b3.PositionHeld.DistanceTo(b4.PositionHeld) <= 21.9f && b4.HasComp<CompPainBeacon>()).EnumerableNullOrEmpty(); 
				if (!map.listerBuildings.allBuildingsColonist.Any((Building b5) => IsTransformable(b5) && p(b5)))
				{
					return false;
				}
				Building b = map.listerBuildings.allBuildingsColonist.Where((Building b1) => IsTransformable(b1) && p(b1))?.RandomElement();
				t = Transform(b);
				DefDatabase<EffecterDef>.GetNamed("AgonyPulseExplosion").Spawn(t.Position, map);
			}
			List<Thing> list = new List<Thing>();
			transformations--;
			list.Add(t);
			for(int i = 0; i < transformations; i++)
            {
				if (map.listerBuildings.allBuildingsColonist.Any((Building b5) => b5.PositionHeld.DistanceTo(t.Position) < 7f && IsTransformable(b5)))
				{
					Building b = map.listerBuildings.allBuildingsColonist.Where((Building b1) => b1.PositionHeld.DistanceTo(t.Position) < 7f && IsTransformable(b1))?.RandomElement();
					list.Add(Transform(b));
				}
                else
                {
					transformedThings = list;
					return true;
				}
			}
			transformedThings = list;
			return true;
		}

		public static bool TryTransformRandomThing(Map map, out Thing transformedThing)
		{
			transformedThing = null;
			if (!map.listerBuildings.allBuildingsColonist.Any((Building b5) => IsTransformable(b5)))
			{
				return false;
			}
			Building b = map.listerBuildings.allBuildingsColonist.Where((Building b1) => IsTransformable(b1))?.RandomElement();
			transformedThing = Transform(b);
			return true;
		}

		public static Thing Transform(Thing t)
		{
			IntVec3 pos = t.Position;
			Map map = t.Map;
			Thing t2 = null;
            if (TransformableIntoWall.Contains(t.def))
            {
				t.Destroy();
				t2 = ThingMaker.MakeThing(NATDefOf.NAT_PainWall);
				GenSpawn.Spawn(t2, pos, map);
			}
            else
            {
				t.Destroy();
				t2 = ThingMaker.MakeThing(NATDefOf.NAT_PainFigure);
				GenSpawn.Spawn(t2, pos, map);
			}
			t2.TryGetComp<CompPainFigure>().ticksBeforePulse = new IntRange(300, 1580).RandomInRange;
			return t2;
		}
	}
	public class PlaceWorker_ShowPainBeaconRadius : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			CompProperties_PainBeacon comp = def.GetCompProperties<CompProperties_PainBeacon>();
			if (comp != null)
			{
				GenDraw.DrawRadiusRing(center, 21.9f, Color.white);
				Map currentMap = Find.CurrentMap;
				if (currentMap != null)
				{
					Vector3 a = GenThing.TrueCenter(center, rot, def.size, def.Altitude);
					List<Building> temp = new List<Building>();
					temp.AddRange(currentMap.listerBuildings.AllBuildingsNonColonistOfDef(NATDefOf.NAT_PainFigure).Where((Building b1) => b1.Position.DistanceTo(center) <= 21.9f));
					temp.AddRange(currentMap.listerBuildings.AllBuildingsNonColonistOfDef(NATDefOf.NAT_PainWall).Where((Building b2) => b2.Position.DistanceTo(center) <= 21.9f));
					foreach (Building b in temp)
					{
						GenDraw.DrawLineBetween(a, b.TrueCenter());
					}
				}
			}
		}
	}
	public class CompProperties_PainBeacon : CompProperties
	{
		public float radius = 21.9f;
		public CompProperties_PainBeacon()
		{
			compClass = typeof(CompPainBeacon);
		}
	}
	public class CompPainBeacon : ThingComp
	{
		public CompProperties_PainBeacon Props => (CompProperties_PainBeacon)props;

		public List<Building> painFigures = new List<Building>();


		public override void CompTickRare()
		{
			List<Building> temp = new List<Building>();
			temp.AddRange(parent.Map.listerBuildings.AllBuildingsNonColonistOfDef(NATDefOf.NAT_PainFigure).Where((Building b) => b.Position.DistanceTo(parent.Position) <= Props.radius));
			temp.AddRange(parent.Map.listerBuildings.AllBuildingsNonColonistOfDef(NATDefOf.NAT_PainWall).Where((Building b) => b.Position.DistanceTo(parent.Position) <= Props.radius));
			painFigures = new List<Building>(temp);
			foreach (Building f in painFigures)
			{
				f.TryGetComp<CompPainFigure>().active = false;
			}
			temp.Clear();
		}

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
			foreach (Building f in painFigures)
			{
				f.TryGetComp<CompPainFigure>().active = true;
			}
			painFigures.Clear();
		}

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
			foreach (Building f in painFigures)
			{
				f.TryGetComp<CompPainFigure>().active = true;
			}
			painFigures.Clear();
		}

        public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref painFigures, "painFigures", lookMode: LookMode.Reference, saveDestroyedThings: true);
		}
	}

	public class IncidentWorker_ObeliskInducer : IncidentWorker_Obelisk
	{
		public override ThingDef ObeliskDef => NATDefOf.NAT_WarpedObelisk_Inducer;
	}

	public class IncidentWorker_PainTransform : IncidentWorker
	{
		
		private static readonly SimpleCurve TransformationsFromPoints = new SimpleCurve
	{
		new CurvePoint(1000f, 5f),
		new CurvePoint(8000f, 20f),
		new CurvePoint(10000f, 30f)
	};
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (!ModsConfig.AnomalyActive)
			{
				return false;
			}
			Map map = (Map)parms.target;
            if (PainUtility.TryTransformMultipleThing(map, out var list, Mathf.CeilToInt(TransformationsFromPoints.Evaluate(parms.points)), false))
            {
				Find.LetterStack.ReceiveLetter("NAT_PainTransform".Translate(), "NAT_PainTransform_Desc".Translate(), LetterDefOf.ThreatSmall, list);
				return true;
			}
			return false;
		}
	}

	public class CompObelisk_Inducer : CompObelisk_ExplodingSpawner
	{
		private IntRange thingsAmount = new IntRange(2,10);

		public int actionsCount;

		private static readonly SimpleCurve TransformationsFromEnemies = new SimpleCurve
	{
		new CurvePoint(0f, 10f),
		new CurvePoint(1f, 5f),
		new CurvePoint(100f, 1f)
	};
		private static readonly SimpleCurve ActionsFromPoints = new SimpleCurve
	{
		new CurvePoint(0f, 10f),
		new CurvePoint(1000f, 10f),
		new CurvePoint(8000f, 15f),
		new CurvePoint(10000f, 30f)
	};
		public override void TriggerInteractionEffect(Pawn interactor, bool triggeredByPlayer = false)
		{
			List<Pawn> list1 = new List<Pawn>();
			List<Thing> list2 = new List<Thing>();
			foreach (Pawn p in parent.Map.mapPawns.AllPawnsSpawned.Where((Pawn p2)=> p2.Faction != Faction.OfPlayer && (p2.RaceProps.Humanlike || p2.IsNonMutantAnimal)))
			{
				list1.Add(p);
            }
			int num = Mathf.CeilToInt(TransformationsFromEnemies.Evaluate(list1.Count));
			for (int i = 0; i < num; i++)
            {
				switch (new IntRange(1, 3).RandomInRange)
				{
					case 1:
                        if (TryInducePain(out Pawn target, 1f))
                        {
							list2.Add(target);
                        }
						break;
					default:
						if (PainUtility.TryTransformMultipleThing(parent.Map, out List<Thing> transformedThings, new IntRange(2, 7).RandomInRange, false))
						{
							list2.AddRange(transformedThings);
						}
						break;
				}
			}
			foreach (Pawn p in list1)
			{
				Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(NATDefOf.NAT_InducedPain);
				if (hediff == null)
				{
					hediff = p.health.AddHediff(NATDefOf.NAT_InducedPain);
					hediff.Severity = new FloatRange(0.5f, 1f).RandomInRange;
					hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = new IntRange(60000, 120000).RandomInRange;
				}
				hediff.Severity += new FloatRange(0.15f, 0.3f).RandomInRange;
				hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear += new IntRange(20000, 120000).RandomInRange;
				p.health.Notify_HediffChanged(hediff);
				if (triggeredByPlayer && p.Faction != null && !p.Faction.HostileTo(Faction.OfPlayer))
				{
					Faction.OfPlayer.TryAffectGoodwillWith(p.Faction, -200, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedHarmfulItem);
				}
			}
			Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("NAT_InducerLetterLabel".Translate(), "NAT_InducerLetter".Translate(interactor.Named("PAWN")), LetterDefOf.ThreatSmall, list2));
		}

		public override void OnActivityActivated()
		{
			base.OnActivityActivated();
			Find.LetterStack.ReceiveLetter("NAT_InducerObeliskLetterLabel".Translate(), "NAT_InducerObeliskLetter".Translate(), LetterDefOf.ThreatBig, parent);
			actionsCount = Mathf.FloorToInt(ActionsFromPoints.Evaluate(pointsRemaining));
		}

		public override void CompTick()
		{
			base.CompTick();
			if (activated && !base.ActivityComp.Deactivated && explodeTick <= 0 && Find.TickManager.TicksGame >= nextSpawnTick && warmupComplete)
			{
				nextSpawnTick = Find.TickManager.TicksGame + SpawnIntervalTicks.RandomInRange;
				switch (new IntRange(1,3).RandomInRange){
					case 1:
						if (PainUtility.TryTransformRandomThing(parent.Map, out Thing transformedThing))
						{
							actionsCount--;
							EffecterDefOf.ObeliskSpark.Spawn(parent.Position, parent.Map).Cleanup();
							Messages.Message("NAT_TransformedOne".Translate(transformedThing), transformedThing, MessageTypeDefOf.NeutralEvent);
						}
						break;
					case 2:
						int n = new IntRange(2, 7).RandomInRange;
						if (PainUtility.TryTransformMultipleThing(parent.Map, out List<Thing> transformedThings, n))
						{
							actionsCount -= Mathf.RoundToInt(n / 3);
							EffecterDefOf.ObeliskSpark.Spawn(parent.Position, parent.Map).Cleanup();
							Thing t = transformedThings.RandomElement();
							DefDatabase<EffecterDef>.GetNamed("AgonyPulseExplosion").Spawn(t.Position, t.Map);
							Messages.Message("NAT_TransformedMultiple".Translate(), transformedThings, MessageTypeDefOf.NeutralEvent);
						}
						break;
					case 3:
						if (parent.Map.mapPawns.AllHumanlikeSpawned.Where((Pawn p) => p.Faction == Faction.OfPlayer && !p.health.Downed).Count() > 2 && TryInducePain(out Pawn target, 10f))
						{
							actionsCount--;
							EffecterDefOf.ObeliskSpark.Spawn(parent.Position, parent.Map).Cleanup();
							if (target != null && target.Faction != null && target.Faction == Faction.OfPlayer)
                            {
								Find.LetterStack.ReceiveLetter("NAT_PainInducedLetter_Label".Translate(target.Named("PAWN")), "NAT_PainInducedLetter".Translate(target.Named("PAWN")), LetterDefOf.ThreatSmall, target);
                            }
                            else
                            {
								Messages.Message("NAT_PainInduced".Translate(target.Named("PAWN")), target, MessageTypeDefOf.NeutralEvent);
							}
						}
						break;
                }
				if (actionsCount <= 0f)
				{
					PrepareExplosion();
				}
			}
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
					defaultLabel = "DEV: transforn one",
					action = delegate
					{
						PainUtility.TryTransformRandomThing(parent.Map, out var t);
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "DEV: transforn multiple",
					action = delegate
					{
						PainUtility.TryTransformMultipleThing(parent.Map, out var t);
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "DEV: induce pain",
					action = delegate
					{
						TryInducePain(out var t);
					}
				};
			}
		}
		public bool TryInducePain(out Pawn target, float timeFactor = 1f)
		{
			target = null;
			parent.Map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.RaceProps.Humanlike && !p.health.hediffSet.HasHediff(NATDefOf.NAT_InducedPain)).TryRandomElement(out var result);
			if (result == null)
			{
				return false;
			}
			Hediff hediff = HediffMaker.MakeHediff(NATDefOf.NAT_InducedPain, result);
			hediff.Severity = new FloatRange(0.5f, 2f).RandomInRange;
			hediff.TryGetComp<HediffComp_Disappears>().disappearsAfterTicks = Mathf.RoundToInt(new IntRange(5000, 25000).RandomInRange * timeFactor);
			result.health.AddHediff(hediff);
			target = hediff.pawn;
			return true;
		}
	}
	public class TransformationDef : Def
    {
		public string tag;

		public ThingDef outputThing;

		public List<ThingDef> transformingThings;
    }

	public class CompProperties_PainFigure : CompProperties
	{
		public CompProperties_PainFigure()
		{
			compClass = typeof(CompPainFigure);
		}
	}
	public class CompPainFigure : ThingComp
	{
		public int ticksBeforePulse = 72;

		public bool active = true;

        public override void CompTickRare()
        {
			ticksBeforePulse--;
			if(ticksBeforePulse <= 0)
            {
				List<Pawn> list1 = new List<Pawn>();
				foreach (Pawn pawn in parent.MapHeld.mapPawns.AllPawnsSpawned)
				{
					if (IsPawnAffected(pawn, 10f))
					{
						list1.Add(pawn);
					}
					if (pawn.carryTracker.CarriedThing is Pawn target && IsPawnAffected(target, 10f))
					{
						list1.Add(target);
					}
				}
				foreach (Pawn p in list1)
				{
					Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(NATDefOf.NAT_InducedPain);
					if (hediff == null)
					{
						hediff = p.health.AddHediff(NATDefOf.NAT_InducedPain);
						hediff.Severity = new FloatRange(0.3f, 0.8f).RandomInRange;
					}
					hediff.Severity += new FloatRange(0.1f, 0.3f).RandomInRange;
					hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear += new IntRange(2000, 2500).RandomInRange;
					p.health.Notify_HediffChanged(hediff);
				}
				list1.Clear();
				DefDatabase<EffecterDef>.GetNamed("AgonyPulseExplosion").Spawn(parent.Position, parent.Map);
				parent.Destroy(DestroyMode.KillFinalize);
				return;
			}
            if (!active)
            {
				return;
            }
			List<Pawn> list2 = new List<Pawn>();
			foreach (Pawn pawn in parent.MapHeld.mapPawns.AllPawnsSpawned)
			{
				if (IsPawnAffected(pawn))
				{
					list2.Add(pawn);
				}
				if (pawn.carryTracker.CarriedThing is Pawn target && IsPawnAffected(target))
				{
					list2.Add(target);
				}
			}
			foreach(Pawn p in list2)
            {
				InducePain(p);
			}
			list2.Clear();
		}

        public override string CompInspectStringExtra()
        {
			string s = ""; 
			if (!active)
            {
				s = "DormantCompInactive".Translate();
			}
			if (DebugSettings.ShowDevGizmos)
			{
				s += "\n" + "DEV: ticks before pulse: " + ticksBeforePulse;
			}
			return s;

		}

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach(Gizmo g in base.CompGetGizmosExtra())
            {
				yield return g;
            }
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action4 = new Command_Action();
				command_Action4.defaultLabel = "DEV: Activate";
				command_Action4.groupable = false;
				command_Action4.action = delegate
				{
					ticksBeforePulse = 1;
				};
				yield return command_Action4;
			}
		}
        private bool IsPawnAffected(Pawn target, float radius = 5f)
		{
			if (target.Dead || target.health == null)
			{
				return false;
			}
			if (target.RaceProps.Humanlike || target.IsNonMutantAnimal)
			{
				return target.PositionHeld.DistanceTo(parent.PositionHeld) <= radius;
			}
			return false;
		}
		public void InducePain(Pawn p)
        {
			Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(NATDefOf.NAT_InducedPain);
			if (hediff == null)
			{
				hediff = p.health.AddHediff(NATDefOf.NAT_InducedPain);
				hediff.Severity = new FloatRange(0.02f, 0.05f).RandomInRange;
			}
			hediff.Severity += new FloatRange(0.01f, 0.02f).RandomInRange;
			hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear += new IntRange(200, 500).RandomInRange;
			p.health.Notify_HediffChanged(hediff);
		}

        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Values.Look(ref active, "active", defaultValue: true);
			Scribe_Values.Look(ref ticksBeforePulse, "ticksBeforePulse", defaultValue: 72);
		}
    }
	public class Hediff_InducedPain : HediffWithComps
	{
        public override bool Visible
        {
            get
            {
				if (pawn.health.hediffSet.PainTotal == 0f)
                {
					return false;
                }
				return base.Visible;
            }
        }
        public override float PainOffset
        {
			get
			{
				if (pawn.genes != null)
                {
					return Severity / pawn.genes.PainFactor;
				}
				return Severity;
			}
		}
    }

	public class Hediff_PainQuench : HediffWithComps
	{
        public override void Tick()
        {
			Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(NATDefOf.NAT_InducedPain);
			if (hediff != null)
			{
				pawn.health.RemoveHediff(hediff);
			}
			base.Tick();
        }
    }
	public class CompUseEffect_AddPainToTargetPawns : CompUseEffect
	{
		private CompTargetable targetable;

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			targetable = parent.GetComp<CompTargetable>();
		}

		public override void DoEffect(Pawn usedBy)
		{
			if (!ModsConfig.AnomalyActive)
			{
				return;
			}
			if (targetable == null)
			{
				Log.Error("CompUseEffect_AddPainToTargetPawns requires a CompTargetable");
				return;
			}
			Thing[] array = targetable.GetTargets().ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is Pawn pawn)
				{
					if(pawn.RaceProps.Humanlike || pawn.IsNonMutantAnimal)
                    {
						Hediff hediff = HediffMaker.MakeHediff(NATDefOf.NAT_InducedPain, pawn);
						if (pawn.health.hediffSet.HasHediff(NATDefOf.NAT_InducedPain))
						{
							Hediff hediff2 = pawn.health.hediffSet.GetFirstHediffOfDef(NATDefOf.NAT_InducedPain);
							hediff.Severity = hediff2.Severity + new FloatRange(0.1f, 0.3f).RandomInRange;
							hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear += hediff2.TryGetComp<HediffComp_Disappears>().ticksToDisappear;
							pawn.health.RemoveHediff(hediff2);
						}
						else
						{
							hediff.Severity = new FloatRange(0.2f, 0.7f).RandomInRange;
						}
						pawn.health.AddHediff(hediff);
					}
				}
			}
		}
	}

	public class Verb_CastTargetEffectPainLance : Verb_CastBase
	{
		public override void DrawHighlight(LocalTargetInfo target)
		{
			base.DrawHighlight(target);
			GenDraw.DrawRadiusRing(target.Cell, 5f, Color.white);
		}
		public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
		{
			Pawn pawn = target.Pawn;
			if (pawn != null)
			{
				if (!pawn.RaceProps.Humanlike && !pawn.IsNonMutantAnimal)
				{
					if (showMessages)
					{
						Messages.Message("MessageBiomutationLanceInvalidTargetRace".Translate(pawn), caster, MessageTypeDefOf.RejectInput, null, historical: false);
					}
					return false;
				}
			}
			return base.ValidateTarget(target, showMessages);
		}

		protected override bool TryCastShot()
		{
			Pawn casterPawn = CasterPawn;
			IntVec3 cell = currentTarget.Cell;
			if (casterPawn == null || cell == null)
			{
				return false;
			}
			foreach (CompTargetEffect comp in base.EquipmentSource.GetComps<CompTargetEffect>())
			{
				foreach(Pawn pawn in CasterPawn.Map.mapPawns.AllPawnsSpawned.Where((Pawn p)=> p.Position.DistanceTo(cell) <= 5f && (p.RaceProps.Humanlike || p.IsNonMutantAnimal)))
                {
					comp.DoEffectOn(CasterPawn, pawn);
				}
			}
			DefDatabase<EffecterDef>.GetNamed("AgonyPulseExplosion").Spawn(cell, CasterPawn.Map);
			base.ReloadableCompSource?.UsedOnce();
			return true;
		}
	}

	public class CompTargetEffect_InducePain : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			if(target is Pawn pawn)
            {
				Hediff hediff = HediffMaker.MakeHediff(NATDefOf.NAT_InducedPain, pawn);
				if (pawn.health.hediffSet.HasHediff(NATDefOf.NAT_InducedPain))
				{
					Hediff hediff2 = pawn.health.hediffSet.GetFirstHediffOfDef(NATDefOf.NAT_InducedPain);
					hediff.Severity = hediff2.Severity + new FloatRange(0.2f, 1f).RandomInRange;
					hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear += hediff2.TryGetComp<HediffComp_Disappears>().ticksToDisappear;
					pawn.health.RemoveHediff(hediff2);
				}
				else
				{
					hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = new IntRange(15000, 20000).RandomInRange;
					hediff.Severity = new FloatRange(0.4f, 3f).RandomInRange;
				}
				pawn.health.AddHediff(hediff);
			}
		}
	}
}