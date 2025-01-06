using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoadFromStockpileZone;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace LoadFromStorage
{
    public class Dialog_LoadFromStorage : Dialog_LoadFromStockpile
    {

		List<Building_Storage> buildings = new List<Building_Storage>();

		public Dialog_LoadFromStorage(Map map, List<CompTransporter> transporters) : base(map, transporters) { }

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
			foreach (Building x in map.listerBuildings.allBuildingsColonist)
			{
				if (x is Building_Storage)
				{
					Building_Storage storage = (Building_Storage)x;
					if (!storage.IsTransfer()) continue;
					buildings.Add(storage);
					tabsList.Add(new TabRecord(storage.Label, delegate ()
					{
						tabindex = buildings.IndexOf(storage);
						//Log.Message(x.label + "index: " + tabindex);
					}, tabindex == buildings.IndexOf(storage)));
				}
			}


			int rows = 1 + tabsList.Count / 8;
			inRect.yMin += 119f + 30f * rows;
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

		protected override void CalculateAndRecacheTransferables()
		{
			string text = null;
			string destinationLabel = null;
			string text2 = Translator.Translate("FormCaravanColonyThingCountTip");
			IgnorePawnsInventoryMode ignorePawnInventoryMass = IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload;
			Func<float> availableMassGetter = () => MassCapacity - MassUsage;
			int tile = map.Tile;

			foreach (Building x in map.listerBuildings.allBuildingsColonist)
			{
				if (x is Building_Storage)
				{
					Building_Storage storage = (Building_Storage)x;
					if (!storage.IsTransfer()) continue;
					List<TransferableOneWay> transferable = new List<TransferableOneWay>();
					AddItemsToTransperableinStorage(transferable, storage);
					grouptransperables.Add(transferable);
					groupsTransfer.Add(new TransferableOneWayWidget(transferable, text, destinationLabel, text2, true, ignorePawnInventoryMass, true, availableMassGetter, 0f, false, tile, true, true, true, false, true, false, false));

				}
			}
			transferables = grouptransperables.SelectMany(x => x).ToList();
			CountToTransferChanged();
		}

		protected void AddItemsToTransperableinStorage(List<TransferableOneWay> trans, Building_Storage storage)
		{
			List<Thing> list = AllReachableStorageItems(map, storage);
			for (int i = 0; i < list.Count; i++)
			{
				AddToTransperableinStorage(list[i], trans);
			}
		}

		protected void AddToTransperableinStorage(Thing t, List<TransferableOneWay> trans)
		{
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching(t, trans, TransferAsOneMode.PodsOrCaravanPacking);
			if (transferableOneWay == null)
			{
				transferableOneWay = new TransferableOneWay();
				trans.Add(transferableOneWay);
			}
			if (transferableOneWay.things.Contains(t))
			{
				Log.Error("Tried to add the same thing twice to TransferableOneWay: " + t, false);
				return;
			}
			transferableOneWay.things.Add(t);
		}

		private List<Thing> AllReachableStorageItems(Map map, Building_Storage storage)
        {
			List<Thing> things = new List<Thing>();
			List<Thing> allthings = map.listerThings.AllThings;
			for (int i=0; i<allthings.Count(); i++)
            {
				Thing thing = allthings[i];
				if (thing.def.category == ThingCategory.Item
					&& !thing.Position.Fogged(thing.Map)
					&& storage.AllSlotCells().Contains(thing.Position)
					&& !map.reservationManager.IsReservedByAnyoneOf(thing, Faction.OfPlayer)
					)
				{
					things.Add(thing);
				}


			}


			return things;
        }


	}
}
