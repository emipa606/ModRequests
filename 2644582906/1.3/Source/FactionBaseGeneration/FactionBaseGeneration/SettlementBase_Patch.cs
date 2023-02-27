using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FactionBaseGeneration
{
	[StaticConstructorOnStartup]
	static class HarmonyContainer
	{
		static HarmonyContainer()
		{
			Harmony harmony = new Harmony("ChickenPlucker.FactionBaseGeneration");
			harmony.PatchAll();
			var postfix = AccessTools.Method(typeof(HarmonyContainer), "Postfix");

			foreach (var type in typeof(SymbolResolver).AllSubclassesNonAbstract())
            {
				try
                {
					var origMethod = AccessTools.Method(type, "CanResolve");
					harmony.Patch(origMethod, null, new HarmonyMethod(postfix));
				}
				catch { };
            }
		}

		public static void Postfix(bool __result, SymbolResolver __instance, ResolveParams rp)
		{
			Log.Message(__instance + " can resolve: " + __result);
			Log.ResetMessageCount();
		}
	}

	[HarmonyPatch(typeof(SymbolResolver_SinglePawn))]
	[HarmonyPatch("TryFindSpawnCell")]
	public static class Patch_TryFindSpawnCell
	{
		public static void Postfix(ResolveParams rp, out IntVec3 cell)
		{
			Map map = BaseGen.globalSettings.map;
			var result = CellFinder.TryFindRandomCellInsideWith(rp.rect, (IntVec3 x) => x.Standable(map) && (rp.singlePawnSpawnCellExtraPredicate == null 
			|| rp.singlePawnSpawnCellExtraPredicate(x)), out cell);
			Log.Message("Result: " + result + " - " + rp.rect.CenterCell + " = " + rp.rect.Any(x => x.Standable(map)));

		}
	}

	[HarmonyPatch(typeof(SymbolResolver_Settlement), "Resolve")]
	public class VisitSettlementFloat
	{
		public static readonly FloatRange DefaultPawnsPoints = new FloatRange(1150f, 1600f);
		private static bool Prefix(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			if (GetOrGenerateMapPatch.customSettlementGeneration)
			{
				Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
				SettlementGeneration.DoSettlementGeneration(map, GetOrGenerateMapPatch.locationData.file.FullName, GetOrGenerateMapPatch.locationData.locationDef, faction, false);

				Log.Message($"map.Center: {map.Center} - rp.rect.CenterCell: {rp.rect.CenterCell}");
				rp.rect = rp.rect.MovedBy(map.Center - rp.rect.CenterCell);
				Log.Message($"2 map.Center: {map.Center} - rp.rect.CenterCell: {rp.rect.CenterCell}");
				//foreach (var cell in rp.rect.Cells)
                //{
				//	Log.Message(cell + " - " + cell.Standable(BaseGen.globalSettings.map));
				//}

				Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map);
				TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
				ResolveParams resolveParams = rp;
				resolveParams.rect = rp.rect;
				resolveParams.faction = faction;
				resolveParams.singlePawnLord = singlePawnLord;
				resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement);
				//resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((Predicate<IntVec3>)((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms))));
				if (resolveParams.pawnGroupMakerParams == null)
				{
					resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
					resolveParams.pawnGroupMakerParams.tile = map.Tile;
					resolveParams.pawnGroupMakerParams.faction = faction;
					resolveParams.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? DefaultPawnsPoints.RandomInRange);
					resolveParams.pawnGroupMakerParams.inhabitants = true;
					resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
				}
				BaseGen.symbolStack.Push("pawnGroup", resolveParams);
				return false;
			}
			return true;
		}
	}
	[HarmonyPatch(typeof(SettlementDefeatUtility))]
	[HarmonyPatch("CheckDefeated")]
	public static class Patch_SettlementDefeatUtility_IsDefeated
	{
		private static bool IsDefeated(Map map, Faction faction)
		{
			List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].RaceProps.Humanlike)
				{
					return false;
				}
			}
			return true;
		}

		private static bool Prefix(Settlement factionBase)
		{
			bool result;
			if (factionBase.HasMap)
			{
				if (!IsDefeated(factionBase.Map, factionBase.Faction))
				{
					result = false;
				}
				else
				{
					result = true;
				}
			}
			else
			{
				result = true;
			}
			return result;
		}
	}

	//[HarmonyPatch(typeof(CaravanArrivalAction_VisitSettlement))]
	//[HarmonyPatch("Arrived")]
	//public static class CaravanVisitPatch
	//{
	//	[HarmonyPostfix]
	//	public static void Postfix(CaravanArrivalAction_VisitSettlement __instance, Caravan caravan)
	//	{
	//		Settlement settlement = Traverse.Create(__instance).Field("settlement").GetValue<Settlement>();
	//		if (!settlement.HasMap)
	//		{
	//			LongEventHandler.QueueLongEvent(delegate ()
	//			{
	//				Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
	//				CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);
	//				SettlementGeneration.InitialiseSettlementGeneration(orGenerateMap, settlement);
	//			}, "GeneratingMapForNewEncounter", false, null, true);
	//			return;
	//		}
	//		Map orGenerateMap2 = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
	//		CaravanEnterMapUtility.Enter(caravan, orGenerateMap2, CaravanEnterMode.Edge, 0, true, null);
	//	}
	//}

	[HarmonyPatch(typeof(SettlementUtility), "AttackNow")]
	public class GetOrGenerateMapPatch
	{
		public class LocationData
		{
			public LocationDef locationDef;
			public FileInfo file;
		}

		public static bool customSettlementGeneration;

		public static LocationData locationData;

		public static void Prefix(ref Caravan caravan, ref Settlement settlement)
		{
			var filePreset = SettlementGeneration.GetPresetFor(settlement, out LocationDef locationDef);
			if (filePreset != null)
			{
				locationData = new LocationData { file = filePreset, locationDef = locationDef };
				customSettlementGeneration = true;
			}
		}
		public static void Postfix(ref Caravan caravan, ref Settlement settlement)
		{
			if (customSettlementGeneration)
			{
				customSettlementGeneration = false;
			}
		}
	}

	//[HarmonyPatch(typeof(Settlement), "GetCaravanGizmos")]
	//public class VisitSettlement
	//{
	//	[HarmonyPostfix]
	//	public static void Postfix(Settlement __instance, ref IEnumerable<Gizmo> __result, Caravan caravan)
	//	{
	//		Command_Action command_Action = new Command_Action
	//		{
	//			icon = SettleUtility.SettleCommandTex,
	//			defaultLabel = Translator.Translate("VisitSettlement"),
	//			defaultDesc = Translator.Translate("VisitSettlementDesc"),
	//			action = delegate ()
	//			{
	//				Action action = delegate ()
	//				{
	//					Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(__instance.Tile, null);
	//					CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);
	//					SettlementGeneration.InitialiseSettlementGeneration(orGenerateMap, __instance);
	//				};
	//				LongEventHandler.QueueLongEvent(action, "GeneratingMapForNewEncounter", false, null, true);
	//			}
	//		};
	//		__result = HarmonyLib.CollectionExtensions.AddItem<Gizmo>(__result, command_Action);
	//	}
	//}



	//[HarmonyPatch(typeof(Site), "GetCaravanGizmos")]
	//public class VisitSite
	//{
	//	[HarmonyPostfix]
	//	public static void Postfix(Site __instance, ref IEnumerable<Gizmo> __result, Caravan caravan)
	//	{
	//		Command_Action command_Action = new Command_Action
	//		{
	//			icon = SettleUtility.SettleCommandTex,
	//			defaultLabel = Translator.Translate("VisitSite"),
	//			defaultDesc = Translator.Translate("VisitSiteDesc"),
	//			action = delegate ()
	//			{
	//				Action action = delegate ()
	//				{
	//					Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(__instance.Tile, null);
	//					CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);
	//				};
	//				LongEventHandler.QueueLongEvent(action, "GeneratingMapForNewEncounter", false, null, true);
	//			}
	//		};
	//		__result = CollectionExtensions.AddItem<Gizmo>(__result, command_Action);
	//	}
	//}

	//internal class SettlementBase_FloatPatch
	//{
	//	[HarmonyPatch(typeof(Settlement), "get_Visitable")]
	//	public class VisitSettlementFloat
	//	{
	//		[HarmonyPostfix]
	//		public static void Postfix(Settlement __instance, ref bool __result)
	//		{
	//			//List<FloatMenuOption> list = __result.ToList<FloatMenuOption>();
	//			//CaravanArrivalAction_VisitSettlement test = 
	//			//foreach (FloatMenuOption floatMenuOption in CaravanArrivalAction_VisitSettlement
	//			//	.GetFloatMenuOptions(caravan, __instance))
	//			//{
	//			//	list.Add(floatMenuOption);
	//			//}
	//			__result = true;
	//		}
	//	}
	//
	//	[HarmonyPatch(typeof(Site), "get_Visitable")]
	//	public class VisitSiteFloat
	//	{
	//		[HarmonyPostfix]
	//		public static void Postfix(Site __instance, ref bool __result)
	//		{
	//			//List<FloatMenuOption> list = __result.ToList<FloatMenuOption>();
	//			//CaravanArrivalAction_VisitSettlement test = 
	//			//foreach (FloatMenuOption floatMenuOption in CaravanArrivalAction_VisitSettlement
	//			//	.GetFloatMenuOptions(caravan, __instance))
	//			//{
	//			//	list.Add(floatMenuOption);
	//			//}
	//			__result = true;
	//		}
	//	}
	//
}



