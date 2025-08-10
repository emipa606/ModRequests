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
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
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
using HarmonyLib;

namespace NAT
{
	[StaticConstructorOnStartup]
	public static class PatchMain
	{
		static PatchMain()
		{
			var val = new Harmony("NAT_Patch");
			val.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
	[HarmonyPatch(typeof(Site), "ShouldRemoveMapNow")]
	public class Patch_DontRemoveMap
	{

		[HarmonyPostfix]
		public static bool postfix(bool alsoRemoveWorldObject, ref bool __result, Site __instance)
		{
			if (__instance.Map.listerThings.AnyThingWithDef(NATDefOf.NAT_DarkLairCasket))
            {
				foreach(Building_DarkLairCasket b in __instance.Map.listerThings.AllThings.Where((Thing t) => t is NAT.Building_DarkLairCasket))
                {
					if (b.containedPawn != null && b.containedPawn.Faction == Faction.OfPlayer)
                    {
						return false;
					}
				}
			}
			return __result;
		}
	}
	[HarmonyPatch(typeof(QuestPart_StructureSpawned), "Notify_QuestSignalReceived")]
	public class Patch_SpawnRustedDefenders
	{
		private static readonly SimpleCurve DefenderPointsByCombatPoints = new SimpleCurve
	{
		new CurvePoint(400f, 1400f),
		new CurvePoint(1000f, 2000f),
		new CurvePoint(2000f, 2250f),
		new CurvePoint(4000f, 2500f),
		new CurvePoint(10000f, 5000f)
	};

		[HarmonyPostfix]
		public static void Postfix(Thing ___structure, Signal signal, string ___spawnedSignal)
		{
			if (signal.tag != ___spawnedSignal)
			{
				return;
			}
			if (Rand.Chance(0.6f) || !LoadedModManager.GetMod<NewAnomalyThreatsMod>().GetSettings<NewAnomalyThreatsSettings>().allowEndGameRaid)
            {
				return;
            }
			float num = DefenderPointsByCombatPoints.Evaluate(StorytellerUtility.DefaultThreatPointsNow(___structure.Map));
			Map map = ___structure.Map;
			var parms = new IncidentParms();
			parms.target = map;
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
			{
				groupKind = parms.pawnGroupKind ?? DefDatabase<PawnGroupKindDef>.GetNamed("NAT_RustedArmy_Defence"),
				tile = map.Tile,
				faction = Faction.OfEntities,
				points = num
			};
			List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms).ToList();
			PawnsArrivalModeDefOf.EdgeWalkIn.Worker.TryResolveRaidSpawnCenter(parms);
			parms.attackTargets = new List<Thing> { ___structure };
			PawnsArrivalModeDefOf.EdgeWalkIn.Worker.Arrive(list, parms);
			if (AnomalyIncidentUtility.IncidentShardChance(num))
			{
				AnomalyIncidentUtility.PawnShardOnDeath(list.RandomElement());
			}
			LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_DefendVoidStructure(___structure, new IntRange(80000, 140000).RandomInRange), map, list);
			Find.LetterStack.ReceiveLetter("NAT_RustedArmyRaid".Translate(), "NAT_RustedArmyRaid_VoidDefence_Desc".Translate(), LetterDefOf.ThreatBig, list, null, null, null, null, new IntRange(0, 120).RandomInRange);
		}
	}

	[HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
	public class Patch_DraftedRustedSoldiers
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn pawn, bool actAsIfSpawned)
		{
			if (pawn.Spawned && pawn.Faction == Faction.OfPlayer && pawn is RustedPawn rust && rust.Draftable)
            {
				pawn.drafter = new Pawn_DraftController(pawn);
			}
		}
	}


	[HarmonyPatch(typeof(PawnAttackGizmoUtility), "CanOrderPlayerPawn")]
	public class Patch_AddAttackGizmo
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn pawn, ref bool __result)
		{
			if (pawn is RustedPawn rust && rust.Draftable && pawn.Faction == Faction.OfPlayer)
			{
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(StunHandler), "CanAdaptToDamage")]
	public class Patch_AddEMPResistance
	{
		[HarmonyPostfix]
		public static void Postfix(DamageDef def, ref bool __result, StunHandler __instance)
		{
			if (__instance.parent is RustedPawn rust && def == DamageDefOf.EMP)
			{
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(CaravanUIUtility), "AddPawnsSections")]
	public class Patch_AddCaravanSection
	{
		[HarmonyPostfix]
		public static void Postfix(TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
		{
			IEnumerable<TransferableOneWay> source = transferables.Where((TransferableOneWay x) => x.ThingDef.category == ThingCategory.Pawn);
			if (source.Any((TransferableOneWay x) => x.AnyThing is RustedPawn))
			{
				widget.AddSection("NAT_RustedSoldiers".Translate(), source.Where((TransferableOneWay x) => x.AnyThing is RustedPawn rust && rust.Faction == Faction.OfPlayer && rust.Controllable));
			}
		}
	}

	[HarmonyPatch(typeof(PawnUtility), "ShouldSendNotificationAbout")]
	public class Patch_ShouldSendNotification
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn p, ref bool __result)
		{
			if (p is RustedPawn rust && p.Faction == Faction.OfPlayer)
			{
				__result = false;
			}
		}
	}

	[HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
	public class Patch_GenereteRustedPawn
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn __result)
		{
			if (__result == null)
			{
				return;
			}
			if(__result is RustedPawn rust && rust.kindDef?.nameMaker != null)
            {
				__result.Name = new NameSingle(NameGenerator.GenerateName(rust.kindDef.nameMaker, (string x) => !NameTriple.FromString(x).UsedThisGame, appendNumberIfNameUsed: false, null, null));
			}
		}
	}

	[HarmonyPatch(typeof(Building_Door), "PawnCanOpen")]
	public class Patch_CollectorOpenDoor
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn p, ref bool __result)
		{
			if (p.HasComp<NAT.CompReworkedCollector>())
			{
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(Pawn), "DrawExtraSelectionOverlays")]
	public static class Patch_RustedSoldierPathDraw
	{
		public static void Postfix(Pawn __instance)
		{
			if (__instance is RustedPawn rust && __instance.Faction == Faction.OfPlayer)
			{
				__instance.pather.curPath?.DrawPath(__instance);
				__instance.jobs.DrawLinesBetweenTargets();
			}
		}
	}


	[HarmonyPatch(typeof(Selector), "SelectInsideDragBox")]
	public static class Patch_SelectRustedSoldiers
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			List<CodeInstruction> codes = codeInstructions.ToList();
			foreach (CodeInstruction code in codes)
			{
				yield return code;
				if (code.opcode == OpCodes.Stloc_3)
				{
					yield return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Patch_SelectRustedSoldiers), "WrappedPredicator", (Type[])null, (Type[])null));
					yield return new CodeInstruction(OpCodes.Stloc_3, (object)null);
				}
			}
		}

		public static Predicate<Thing> WrappedPredicator(Predicate<Thing> predicate)
		{
			return wrappedPredicate;
			bool wrappedPredicate(Thing t)
			{
				bool flag = predicate(t);
				if (!flag)
				{
					if (t.Faction == Faction.OfPlayer && t is RustedPawn rust)
					{
						return true;
					}
				}
				return flag;
			}
		}
	}

	[HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
	public class Patch_MovingOrders
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn pawn, ref bool __result)
		{
			if (pawn.Spawned && pawn is RustedPawn rust && rust.Draftable && pawn.Faction == Faction.OfPlayer)
			{
				__result = true;
			}
		}
	}
	[HarmonyPatch(typeof(FloatMenuMakerMap))]
	[HarmonyPatch("AddJobGiverWorkOrders")]
	public static class Patch_RemoveWorkFromSoldiers
	{
		[HarmonyPrefix]
		public static bool SkipIfAnimal(Pawn pawn)
		{
			if (pawn.Faction == Faction.OfPlayer && pawn is RustedPawn rust)
			{
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(FloatMenuUtility), "GetRangedAttackAction")]
	public class Patch_DraftedRustedSoldiers_Ranged
	{
		[HarmonyPostfix]
		public static void MakePawnControllable(Pawn pawn, LocalTargetInfo target, ref string failStr, ref Action __result)
		{
			if (pawn.Faction != Faction.OfPlayer || !(pawn is RustedPawn rust) || !rust.Draftable)
			{
				return;
			}
			Verb primaryVerb = pawn.equipment.PrimaryEq.PrimaryVerb;
			if (primaryVerb.verbProps.IsMeleeAttack)
			{
				__result = null;
				return;
			}
			if (!pawn.Drafted)
			{
				failStr = "IsNotDraftedLower".Translate(pawn.LabelShort, pawn);
			}
			else if (target.IsValid && !pawn.equipment.PrimaryEq.PrimaryVerb.CanHitTarget(target))
			{
				if (!pawn.Position.InHorDistOf(target.Cell, primaryVerb.verbProps.range))
				{
					failStr = "OutOfRange".Translate();
				}
				else
				{
					float num = primaryVerb.verbProps.EffectiveMinRange(target, pawn);
					if ((float)pawn.Position.DistanceToSquared(target.Cell) < num * num)
					{
						failStr = "TooClose".Translate();
					}
					else
					{
						failStr = "CannotHitTarget".Translate();
					}
				}
			}
			else if (pawn == target.Thing)
			{
				failStr = "CannotAttackSelf".Translate();
			}
            else
            {
				__result = delegate
				{
					Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, target);
					pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				};
			}
			failStr = failStr.CapitalizeFirst();
		}
	}

	[HarmonyPatch(typeof(FloatMenuUtility), "GetMeleeAttackAction")]
	public class Patch_DraftedRustedSoldiers_Melee
	{
		[HarmonyPostfix]
		public static void MakePawnControllable(Pawn pawn, LocalTargetInfo target, ref string failStr, ref Action __result)
		{
			if (pawn.Faction != Faction.OfPlayer || !(pawn is RustedPawn rust) || !rust.Draftable )
			{
				return;
			}
			if (!pawn.Drafted)
			{
				failStr = "IsNotDraftedLower".Translate(pawn.LabelShort, pawn);
			}
			else if (target.IsValid && !pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
			{
				failStr = "NoPath".Translate();
			}
			else if (pawn.meleeVerbs.TryGetMeleeVerb(target.Thing) == null)
			{
				failStr = "Incapable".Translate();
			}
			else if (pawn == target.Thing)
			{
				failStr = "CannotAttackSelf".Translate();
			}
			else
			{
				if (!(target.Thing is Pawn pawn2) || !pawn2.RaceProps.Animal || !HistoryEventUtility.IsKillingInnocentAnimal(pawn, pawn2) || new HistoryEvent(HistoryEventDefOf.KilledInnocentAnimal, pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
				{
					__result = delegate
					{
						Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
						if (target.Thing is Pawn pawn3)
						{
							job.killIncappedTarget = pawn3.Downed;
						}
						pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
					};
				}
				failStr = "IdeoligionForbids".Translate();
			}
			failStr = failStr.CapitalizeFirst();
		}
	}

}