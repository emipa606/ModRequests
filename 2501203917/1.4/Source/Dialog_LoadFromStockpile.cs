using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace LoadFromStockpileZone
{
	public class Dialog_LoadFromStockpile : Window
	{
		protected Map map;
		protected List<CompTransporter> transporters;
		protected List<TransferableOneWay> transferables;
		protected List<TransferableOneWayWidget> groupsTransfer = new List<TransferableOneWayWidget>();
		protected List<List<TransferableOneWay>> grouptransperables = new List<List<TransferableOneWay>>();
		protected float lastMassFlashTime = -9999f;
		protected bool massUsageDirty = true;
		protected float cachedMassUsage;
		protected bool caravanMassUsageDirty = true;
		protected float cachedCaravanMassUsage;
		protected bool caravanMassCapacityDirty = true;
		protected float cachedCaravanMassCapacity;
		protected string cachedCaravanMassCapacityExplanation;
		protected bool tilesPerDayDirty = true;
		protected float cachedTilesPerDay;
		protected string cachedTilesPerDayExplanation;
		protected bool daysWorthOfFoodDirty = true;
		protected Pair<float, float> cachedDaysWorthOfFood;
		protected bool foragedFoodPerDayDirty = true;
		public bool autoLoot;
		protected Pair<ThingDef, float> cachedForagedFoodPerDay;
		protected string cachedForagedFoodPerDayExplanation;
		protected bool visibilityDirty = true;
		protected float cachedVisibility;
		protected string cachedVisibilityExplanation;
		protected const float TitleRectHeight = 35f;
		protected const float BottomAreaHeight = 55f;
		protected readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);
		protected static List<TabRecord> tabsList = new List<TabRecord>();
		protected int tabindex;
		//protected List<string> labels = new List<string>();
		private List<Zone_Stockpile> zones = new List<Zone_Stockpile>();

		public Dialog_LoadFromStockpile(Map map, List<CompTransporter> transporters)
		{
			this.map = map;
			this.transporters = new List<CompTransporter>();
			this.transporters.AddRange(transporters);
			forcePause = true;
			absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(0f, 0f, inRect.width, 35f);
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, TranslatorFormattedStringExtensions.Translate("LoadTransporters", TransportersLabel));
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(MassUsage, MassCapacity, string.Empty, TilesPerDay, cachedTilesPerDayExplanation, DaysWorthOfFood, ForagedFoodPerDay, cachedForagedFoodPerDayExplanation, Visibility, cachedVisibilityExplanation, CaravanMassUsage, CaravanMassCapacity, cachedCaravanMassCapacityExplanation), null, map.Tile, null, lastMassFlashTime, new Rect(12f, 35f, inRect.width - 24f, 40f), false, null, false);
			tabsList.Clear();
			foreach(Zone x in map.zoneManager.AllZones)
            {
				if (x is Zone_Stockpile)
                {
					Zone_Stockpile stockpile = (Zone_Stockpile)x;
					if (!stockpile.IsTransfer()) continue;
					zones.Add(stockpile);
					tabsList.Add(new TabRecord(stockpile.label, delegate ()
					{
						tabindex = zones.IndexOf(stockpile);
						//Log.Message(x.label + "index: " + tabindex);
					}, tabindex == zones.IndexOf(stockpile)));
				}
				
            }
			

			int rows = 1 + tabsList.Count / 8;
			inRect.yMin += 119f + 30f*rows;
			Widgets.DrawMenuSection(inRect);
			TabDrawer.DrawTabs(inRect, tabsList, rows);
			inRect = inRect.ContractedBy(17f);
			GUI.BeginGroup(inRect);
			Rect rect2 = inRect.AtZero();
			DoBottomButtons(rect2);
			Rect inRect2 = rect2;
			inRect2.yMax -= 59f;
			bool flag = false;
			groupsTransfer[tabindex].OnGUI(inRect2, out flag);
			if (flag)
			{
				CountToTransferChanged();
			}
			GUI.EndGroup();
		}

		protected virtual void CalculateAndRecacheTransferables()
		{
			string text = null;
			string destinationLabel = null;
			string text2 = Translator.Translate("FormCaravanColonyThingCountTip");
			IgnorePawnsInventoryMode ignorePawnInventoryMass = IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload;
			Func<float> availableMassGetter = () => MassCapacity - MassUsage;
			int tile = map.Tile;

			foreach(Zone x in map.zoneManager.AllZones)
            {
				if (x is Zone_Stockpile)
				{
					Zone_Stockpile stockpile = (Zone_Stockpile)x;
					if (!stockpile.IsTransfer()) continue;
					List<TransferableOneWay> transferable = new List<TransferableOneWay>();
					AddItemsToTransperableinZone(transferable, stockpile);
					grouptransperables.Add(transferable);
					groupsTransfer.Add(new TransferableOneWayWidget(transferable, text, destinationLabel, text2, true, ignorePawnInventoryMass, true, availableMassGetter, 0f, false, tile, true, true, true, false, true, false, false));

				}
			}
			transferables = grouptransperables.SelectMany(x => x).ToList();
			CountToTransferChanged();
		}

		protected void AddItemsToTransperableinZone(List<TransferableOneWay> trans, Zone zone)
        {
			List<Thing> list = AllReachableZoneItems(map,zone);
			for (int i = 0; i < list.Count; i++)
			{
				AddToTransperableinZone(list[i], trans);
			}
		}

		protected void AddToTransperableinZone(Thing t, List<TransferableOneWay> trans)
        {
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching(t, trans, TransferAsOneMode.PodsOrCaravanPacking);
			if (transferableOneWay == null)
			{
				transferableOneWay = new TransferableOneWay();
				trans.Add(transferableOneWay);
			}
			if (transferableOneWay.things.Contains(t))
			{
				Log.Error("Tried to add the same thing twice to TransferableOneWay: " + t);
				return;
			}
			transferableOneWay.things.Add(t);
		}

		public static List<Thing> AllReachableZoneItems(Map map, Zone zone)
        {
			List<Thing> things = new List<Thing>();
			List<Thing> allthings = map.listerThings.AllThings;
			for(int i=0; i<allthings.Count; i++)
            {
				Thing thing = allthings[i];
				if (thing.def.category == ThingCategory.Item
                    && !thing.Position.Fogged(thing.Map)
					&& map.zoneManager.ZoneAt(thing.Position) == zone
					&& !map.reservationManager.IsReservedByAnyoneOf(thing,Faction.OfPlayer)
					)
                {
					things.Add(thing);
                }
            }
			return things;
        }



		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(1024f, (float)UI.screenHeight);
			}
		}

		protected override float Margin
		{
			get
			{
				return 0f;
			}
		}

		protected float MassCapacity
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < transporters.Count; i++)
				{
					num += transporters[i].Props.massCapacity;
				}
				return num;
			}
		}

		protected float CaravanMassCapacity
		{
			get
			{
				if (caravanMassCapacityDirty)
				{
					caravanMassCapacityDirty = false;
					StringBuilder stringBuilder = new StringBuilder();
					cachedCaravanMassCapacity = CollectionsMassCalculator.CapacityTransferables(transferables, stringBuilder);
					cachedCaravanMassCapacityExplanation = stringBuilder.ToString();
				}
				return cachedCaravanMassCapacity;
			}
		}

		protected string TransportersLabel
		{
			get
			{
				return Find.ActiveLanguageWorker.Pluralize(transporters[0].parent.Label, -1);
			}
		}

		protected string TransportersLabelCap
		{
			get
			{
				return TransportersLabel.CapitalizeFirst();
			}
		}

		protected BiomeDef Biome
		{
			get
			{
				return map.Biome;
			}
		}

		protected float MassUsage
		{
			get
			{
				if (massUsageDirty)
				{
					massUsageDirty = false;
					cachedMassUsage = CollectionsMassCalculator.MassUsageTransferables(transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, true, false);
					cachedMassUsage += InventoryMassUsage();
				}
				return cachedMassUsage;
			}
		}

		public float CaravanMassUsage
		{
			get
			{
				if (caravanMassUsageDirty)
				{
					caravanMassUsageDirty = false;
					cachedCaravanMassUsage = CollectionsMassCalculator.MassUsageTransferables(transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, false, false);
					cachedCaravanMassUsage += InventoryMassUsage();
				}
				return cachedCaravanMassUsage;
			}
		}

		protected float TilesPerDay
		{
			get
			{
				if (tilesPerDayDirty)
				{
					tilesPerDayDirty = false;
					StringBuilder stringBuilder = new StringBuilder();
					cachedTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDay(transferables, MassUsage, MassCapacity, map.Tile, -1, stringBuilder);
					cachedTilesPerDayExplanation = stringBuilder.ToString();
				}
				return cachedTilesPerDay;
			}
		}

		protected Pair<float, float> DaysWorthOfFood
		{
			get
			{
				if (daysWorthOfFoodDirty)
				{
					daysWorthOfFoodDirty = false;
					float first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(transferables, map.Tile, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, Faction.OfPlayer, null, 0f, 3300);
					cachedDaysWorthOfFood = new Pair<float, float>(first, DaysUntilRotCalculator.ApproxDaysUntilRot(transferables, map.Tile, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, null, 0f, 3300));
				}
				return cachedDaysWorthOfFood;
			}
		}

		protected Pair<ThingDef, float> ForagedFoodPerDay
		{
			get
			{
				if (foragedFoodPerDayDirty)
				{
					foragedFoodPerDayDirty = false;
					StringBuilder stringBuilder = new StringBuilder();
					cachedForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDay(transferables, Biome, Faction.OfPlayer, stringBuilder);
					cachedForagedFoodPerDayExplanation = stringBuilder.ToString();
				}
				return cachedForagedFoodPerDay;
			}
		}

		protected float Visibility
		{
			get
			{
				if (visibilityDirty)
				{
					visibilityDirty = false;
					StringBuilder stringBuilder = new StringBuilder();
					cachedVisibility = CaravanVisibilityCalculator.Visibility(transferables, stringBuilder);
					cachedVisibilityExplanation = stringBuilder.ToString();
				}
				return cachedVisibility;
			}
		}

		public override void PostOpen()
		{
			base.PostOpen();
			CalculateAndRecacheTransferables();
		}


		public override bool CausesMessageBackground()
		{
			return true;
		}

		protected void AddToTransferables(Thing t)
		{
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(t, transferables, TransferAsOneMode.PodsOrCaravanPacking);
			if (transferableOneWay == null)
			{
				transferableOneWay = new TransferableOneWay();
				transferables.Add(transferableOneWay);
			}
			transferableOneWay.things.Add(t);
		}

		protected void AddToTransferablesSelected(Thing t)
		{
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(t, transferables, TransferAsOneMode.PodsOrCaravanPacking);
			if (transferableOneWay == null)
			{
				transferableOneWay = new TransferableOneWay();
				transferables.Add(transferableOneWay);
			}
			transferableOneWay.things.Add(t);
			transferableOneWay.AdjustTo(t.stackCount);
		}

		protected void DoBottomButtons(Rect rect)
		{
			Rect rect2 = new Rect(rect.width / 2f - BottomButtonSize.x / 2f, rect.height - 55f, BottomButtonSize.x, BottomButtonSize.y);
			bool flag = Widgets.ButtonText(rect2, Translator.Translate("AcceptButton"), true, false, true);
			if (flag)
			{
				bool flag2 = CaravanMassUsage > CaravanMassCapacity && CaravanMassCapacity != 0f;
				if (flag2)
				{
					bool flag3 = CheckForErrors(TransferableUtility.GetPawnsFromTransferables(transferables));
					if (flag3)
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(Translator.Translate("TransportersCaravanWillBeImmobile"), delegate ()
						{
							bool flag9 = TryAccept();
							if (flag9)
							{
								SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
								Close(false);
							}
						}, false, null));
					}
				}
				else
				{
					bool flag4 = TryAccept();
					if (flag4)
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
						Close(false);
					}
				}
			}
			Rect rect3 = new Rect(rect2.x - 10f - BottomButtonSize.x, rect2.y, BottomButtonSize.x, BottomButtonSize.y);
			bool flag5 = Widgets.ButtonText(rect3, Translator.Translate("ResetButton"), true, false, true);
			if (flag5)
			{
				SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
				CalculateAndRecacheTransferables();
			}
			Rect rect4 = new Rect(rect2.xMax + 10f, rect2.y, BottomButtonSize.x, BottomButtonSize.y);
			bool flag6 = Widgets.ButtonText(rect4, Translator.Translate("CancelButton"), true, false, true);
			if (flag6)
			{
				Close(true);
			}
			Rect rect6 = new Rect(rect.width - BottomButtonSize.x, rect.height - 55f, BottomButtonSize.x, BottomButtonSize.y);
			bool flag8 = Widgets.ButtonText(rect6, Translations.Button_LoadAllfromZone, true, false, true);
			if (flag8)
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
				SetToLoadEverythingZone();
			}
			Rect rect7 = new Rect(0f, rect.height - 55f, BottomButtonSize.x, BottomButtonSize.y);
			bool flag10 = Widgets.ButtonText(rect7, Translations.Button_UnLoadAllfromZone, true, false, true);
			if (flag10)
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
				SetToUnLoadEverythingZone();
			}

			bool devMode = Prefs.DevMode;
			if (devMode)
			{
				float width = 200f;
				float num = BottomButtonSize.y / 2f;
				Rect rect5 = new Rect(0f, rect.height - num, width, num);
				bool flag7 = Widgets.ButtonText(rect5, "Dev: Load instantly", true, false, true) && DebugTryLoadInstantly();
				if (flag7)
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					Close(false);
				}
			}
		}

		

		protected bool DebugTryLoadInstantly()
		{
			CreateAndAssignNewTransportersGroup();
			int i;
			int j;
			for (i = 0; i < transferables.Count; i = j + 1)
			{
				TransferableUtility.Transfer(transferables[i].things, transferables[i].CountToTransfer, delegate (Thing splitPiece, IThingHolder originalThing)
				{
					transporters[i % transporters.Count].GetDirectlyHeldThings().TryAdd(splitPiece, true);
				});
				j = i;
			}
			return true;
		}

		protected bool TryAccept()
		{
			List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(transferables);
			bool flag = !CheckForErrors(pawnsFromTransferables);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				int transportersGroup = CreateAndAssignNewTransportersGroup();
				KickOutFreeloadingPawns();
				RemoveUnwantedItems();
				ClearContentsNotLoaded();
				AssignTransferablesToRandomTransporters();
				IEnumerable<Pawn> enumerable = from x in pawnsFromTransferables
											   where x.IsColonist && !x.Downed
											   select x;
				bool flag2 = enumerable.Any<Pawn>();
				if (flag2)
				{
					foreach (Pawn pawn in enumerable)
					{
						Lord lord = pawn.GetLord();
						bool flag3 = lord != null;
						if (flag3)
						{
							lord.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord, null);
						}
					}
					LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_LoadAndEnterTransporters(transportersGroup), map, enumerable);
					foreach (Pawn pawn2 in enumerable)
					{
						bool spawned = pawn2.Spawned;
						if (spawned)
						{
							pawn2.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
						}
					}
				}
				Messages.Message(Translator.Translate("MessageTransportersLoadingProcessStarted"), transporters[0].parent, MessageTypeDefOf.TaskCompletion, false);
				result = true;
			}
			return result;
		}

		public void KickOutFreeloadingPawns()
		{
			int num = 0;
			foreach (CompTransporter compTransporter in transporters)
			{
				for (int i = compTransporter.innerContainer.Count - 1; i >= 0; i--)
				{
					Thing thing = compTransporter.innerContainer[i];
					bool flag = thing is Pawn;
					if (flag)
					{
						Thing thing2;
						bool flag2 = compTransporter.innerContainer.TryDrop(thing, ThingPlaceMode.Near, out thing2, null, null);
						bool flag3 = (thing as Pawn).GetLord() != null;
						if (flag3)
						{
							(thing as Pawn).GetLord().lordManager.RemoveLord((thing as Pawn).GetLord());
						}
					}
					num++;
				}
			}
		}

		public void RemoveUnwantedItems()
		{
			foreach (CompTransporter compTransporter in transporters)
			{
				foreach (Thing thing in ((IEnumerable<Thing>)compTransporter.innerContainer))
				{
					bool flag = thing is Pawn && TransferableUtility.GetPawnsFromTransferables(transferables).Contains(thing as Pawn);
					if (flag)
					{
						Log.Warning(string.Concat(new object[]
						{
							"Pawn ",
							thing.LabelShort,
							" was still inside transporter ",
							compTransporter.parent.ThingID,
							" after pawns were dumped.",
							"Removing ",
							thing.LabelShort,
							" from transporter and spawning manually. - Smash Phil"
						}));
						bool flag2 = !thing.Spawned;
						if (flag2)
						{
							GenSpawn.Spawn(thing, compTransporter.parent.Position, compTransporter.Map, WipeMode.Vanish);
						}
						compTransporter.innerContainer.Remove(thing);
					}
				}
			}
		}

		public void ClearContentsNotLoaded()
		{
			foreach (CompTransporter compTransporter in transporters)
			{
				List<TransferableOneWay> leftToLoad = compTransporter.leftToLoad;
				if (leftToLoad != null)
				{
					leftToLoad.Clear();
				}
			}
			List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				foreach (CompTransporter compTransporter2 in transporters)
				{
					bool flag = allPawnsSpawned[i].CurJobDef == JobDefOf.HaulToTransporter;
					if (flag)
					{
						JobDriver_HaulToTransporter jobDriver_HaulToTransporter = (JobDriver_HaulToTransporter)allPawnsSpawned[i].jobs.curDriver;
						bool flag2 = jobDriver_HaulToTransporter.Transporter == compTransporter2;
						if (flag2)
						{
							bool flag3 = jobDriver_HaulToTransporter.ThingToCarry != null;
							if (flag3)
							{
								allPawnsSpawned[i].jobs.EndCurrentJob(JobCondition.InterruptForced, true);
							}
						}
					}
				}
			}
		}

		protected void AssignTransferablesToRandomTransporters()
		{
			TransferableOneWay transferableOneWay = transferables.MaxBy((TransferableOneWay x) => x.CountToTransfer);
			int num = 0;
			for (int i = 0; i < transferables.Count; i++)
			{
				bool flag = transferables[i] != transferableOneWay;
				if (flag)
				{
					bool flag2 = transferables[i].CountToTransfer > 0;
					if (flag2)
					{
						transporters[num % transporters.Count].AddToTheToLoadList(transferables[i], transferables[i].CountToTransfer);
						num++;
					}
				}
			}
			bool flag3 = num < transporters.Count;
			if (flag3)
			{
				int num2 = transferableOneWay.CountToTransfer;
				int num3 = num2 / (transporters.Count - num);
				for (int j = num; j < transporters.Count; j++)
				{
					int num4 = (j != transporters.Count - 1) ? num3 : num2;
					bool flag4 = num4 > 0;
					if (flag4)
					{
						transporters[j].AddToTheToLoadList(transferableOneWay, num4);
					}
					num2 -= num4;
				}
			}
			else
			{
				transporters[num % transporters.Count].AddToTheToLoadList(transferableOneWay, transferableOneWay.CountToTransfer);
			}
		}

		protected int CreateAndAssignNewTransportersGroup()
		{
			int nextTransporterGroupID = Find.UniqueIDsManager.GetNextTransporterGroupID();
			for (int i = 0; i < transporters.Count; i++)
			{
				transporters[i].groupID = nextTransporterGroupID;
			}
			return nextTransporterGroupID;
		}

		protected bool CheckForErrors(List<Pawn> pawns)
		{
			bool flag = !transferables.Any((TransferableOneWay x) => x.CountToTransfer != 0);
			bool result;
			if (flag)
			{
				Messages.Message(Translator.Translate("CantSendEmptyTransportPods"), MessageTypeDefOf.RejectInput, false);
				result = false;
			}
			else
			{
				bool flag2 = MassUsage > MassCapacity;
				if (flag2)
				{
					FlashMass();
					Messages.Message(Translator.Translate("TooBigTransportersMassUsage"), MessageTypeDefOf.RejectInput, false);
					result = false;
				}
				else
				{
					Pawn pawn = pawns.Find((Pawn x) => !x.MapHeld.reachability.CanReach(x.PositionHeld, transporters[0].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)));
					bool flag3 = pawn != null;
					if (flag3)
					{
						Messages.Message(TranslatorFormattedStringExtensions.Translate("PawnCantReachTransporters", pawn.LabelShort, pawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, false);
						result = false;
					}
					else
					{
						Map map = transporters[0].parent.Map;
						for (int i = 0; i < transferables.Count; i++)
						{
							bool flag4 = transferables[i].ThingDef.category == ThingCategory.Item;
							if (flag4)
							{
								int countToTransfer = transferables[i].CountToTransfer;
								int num = 0;
								bool flag5 = countToTransfer > 0;
								if (flag5)
								{
									for (int j = 0; j < transferables[i].things.Count; j++)
									{
										Thing thing = transferables[i].things[j];
										bool flag6 = map.reachability.CanReach(thing.Position, transporters[0].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false));
										if (flag6)
										{
											num += thing.stackCount;
											bool flag7 = num >= countToTransfer;
											if (flag7)
											{
												break;
											}
										}
									}
									bool flag8 = num < countToTransfer;
									if (flag8)
									{
										bool flag9 = countToTransfer == 1;
										if (flag9)
										{
											Messages.Message(TranslatorFormattedStringExtensions.Translate("TransporterItemIsUnreachableSingle", transferables[i].ThingDef.label), MessageTypeDefOf.RejectInput, false);
										}
										else
										{
											Messages.Message(TranslatorFormattedStringExtensions.Translate("TransporterItemIsUnreachableMulti", countToTransfer, transferables[i].ThingDef.label), MessageTypeDefOf.RejectInput, false);
										}
										return false;
									}
								}
							}
						}
						result = true;
					}
				}
			}
			return result;
		}

		protected void AddPawnsToTransferables()
		{
			List<Pawn> list = CaravanFormingUtility.AllSendablePawns(map, false, false, false, false);
			for (int i = 0; i < list.Count; i++)
			{
				AddToTransferables(list[i]);
			}
		}

		protected void AddItemsToTransferables()
		{
			List<Thing> list = CaravanFormingUtility.AllReachableColonyItems(map, false, false, false);
			for (int i = 0; i < list.Count; i++)
			{
				AddToTransferables(list[i]);
			}
		}

		protected void AddContentsToTransferables()
		{
			foreach (CompTransporter compTransporter in transporters)
			{
				foreach (Thing thing in ((IEnumerable<Thing>)compTransporter.innerContainer))
				{
					bool flag = thing is Pawn;
					if (flag)
					{
						AddToTransferablesSelected(thing);
					}
				}
				bool flag2 = compTransporter.leftToLoad != null;
				if (flag2)
				{
					foreach (TransferableOneWay transferableOneWay in compTransporter.leftToLoad)
					{
						bool flag3 = transferableOneWay.AnyThing is Pawn;
						if (flag3)
						{
							AddToTransferablesSelected(transferableOneWay.AnyThing);
						}
					}
				}
			}
		}

		protected void FlashMass()
		{
			lastMassFlashTime = Time.time;
		}

		protected float InventoryMassUsage()
		{
			float num = 0f;
			foreach (CompTransporter compTransporter in transporters)
			{
				foreach (Thing thing in ((IEnumerable<Thing>)compTransporter.innerContainer))
				{
					bool flag = !(thing is Pawn);
					if (flag)
					{
						num += thing.GetStatValue(StatDefOf.Mass, true) * (float)thing.stackCount;
					}
				}
			}
			return num;
		}

		protected void SetToLoadEverything()
		{
			for (int i = 0; i < transferables.Count; i++)
			{
				transferables[i].AdjustTo(transferables[i].GetMaximumToTransfer());
			}
			CountToTransferChanged();
		}

		protected void SetToLoadEverythingZone()
		{
			for (int i = 0; i < grouptransperables[tabindex].Count; i++)
			{
				
				grouptransperables[tabindex][i].AdjustTo(grouptransperables[tabindex][i].GetMaximumToTransfer());
			}
			CountToTransferChanged();
		}
		protected void SetToUnLoadEverythingZone()
		{
			for (int i = 0; i < grouptransperables[tabindex].Count; i++)
			{
				grouptransperables[tabindex][i].AdjustTo(grouptransperables[tabindex][i].GetMinimumToTransfer());
			}
			CountToTransferChanged();
		}

		protected void CountToTransferChanged()
		{
			massUsageDirty = true;
			caravanMassUsageDirty = true;
			caravanMassCapacityDirty = true;
			tilesPerDayDirty = true;
			daysWorthOfFoodDirty = true;
			foragedFoodPerDayDirty = true;
			visibilityDirty = true;
		}

	}
}
