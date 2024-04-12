﻿// Copyright (c) 2022 bradson
// This Source Code Form is subject to the terms of the MIT license.
// If a copy of the license was not distributed with this file,
// You can obtain one at https://opensource.org/licenses/MIT/.

using System.Linq;
using Verse;
using XenotypeSpawnControl.GUIExtensions;

namespace XenotypeSpawnControl;

public static class ModSettingsWindow
{
	public static Tab[] Tabs = new Tab[]
	{
		new Tab<FactionDef>
		{
			Title = Strings.Translated.Factions,
			Defs = DefDatabase<FactionDef>.AllDefs.Where(faction => faction.humanlikeFaction)
		},
		new Tab<PawnKindDef>
		{
			Title = Strings.Translated.PawnKinds,
			Defs = DefDatabase<PawnKindDef>.AllDefs.Where(p => p.RaceProps is { } props && props.Humanlike)
		},
		new Tab<MemeDef>
		{
			RequirementsMet = ModsConfig.IdeologyActive,
			Title = Strings.Translated.Memes,
			Defs = DefDatabase<MemeDef>.AllDefs
		},
		new EditorTab
		{
			Title = Strings.Translated.Editor
		}
	};

	public static void DoWindowContents(Rect inRect)
	{
		var windowRect = Find.WindowStack.currentlyDrawnWindow.windowRect;
		if (Widgets.ButtonText(
			new(windowRect.width - Window.CloseButSize.x - (Window.StandardMargin * 2f), windowRect.height - Window.FooterRowHeight - Window.StandardMargin, Window.CloseButSize.x, Window.CloseButSize.y),
			Strings.Translated.ResetAll))
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(Strings.Translated.ConfirmReset, XenotypeChanceDatabases.ResetAll, destructive: true));
		}

		inRect.yMin += 30f;

		Widgets.DrawMenuSection(inRect);
		TabDrawer.DrawTabs(inRect, _tabList);

		inRect = inRect.ContractedBy(20f);

		inRect.width -= 200f;

		/*var titleListing = new Listing_Standard();
		titleListing.Begin(inRect);

		//searchText = titleListing.TextEntry(searchText);
		titleListing.End();*/

		inRect.width += 200f;
		//inRect.yMin += 60f;
		try
		{
			_currentTab.DoWindowContents(inRect);
		}
		catch (Exception ex)
		{
			Log.Error($"{ex}");
		}
	}

	static ModSettingsWindow()
	{
		for (var i = 0; i < Tabs.Length; i++)
		{
			if (Tabs[i].RequirementsMet)
				_tabList.Add(new(Tabs[i], () => ref _currentTab));
		}
	}

	public abstract class Tab
	{
		public virtual string Title { get; set; }
		public virtual bool RequirementsMet { get; set; } = true;
		public abstract void DoWindowContents(Rect inRect);

		public Tab() => Title ??= GetType().Name;
	}

	public class Tab<T> : Tab where T : Def
	{
		public IEnumerable<T> Defs { get; set; } = DefDatabase<T>.AllDefs;

		public IEnumerable<ModifiableXenotype> Xenotypes { get; set; }
			= ModifiableXenotypeDatabase.AllValues.Values;

		public Listing_Standard Listing { get; } = new() { verticalSpacing = 4f };

		private string? _currentlySelectedDefName;
		private ScrollViewStatus _defScrollViewStatus;
		private ScrollViewStatus _xenotypeScrollViewStatus;

		public override void DoWindowContents(Rect inRect)
		{
			var leftHalfRect = inRect.LeftHalf();
			leftHalfRect.width -= 20f;
			ListDefs(leftHalfRect);
			ListXenotypes(inRect.RightHalf());
		}

		public void ListDefs(Rect outRect)
		{
			using var listingScope = new ScrollableListingScope(outRect, ref _defScrollViewStatus, Listing);

			foreach (var def in Defs.OrderBy(def => def.defName))
			{
				var defName = def.defName;

				//if (searchOn && !factionDefName.ToLower().Contains(searchText.ToLower()))
				//	continue;
				//if (factionDefName.Length > LINE_MAX)
				//	factionDefName = factionDefName.Substring(0, LINE_MAX) + "...";

				if (Listing.SelectionButton(defName, _currentlySelectedDefName == defName))
					_currentlySelectedDefName = defName;
			}

			if (typeof(T) == typeof(FactionDef))
			{
				if (Listing.SelectionButton(Strings.Translated.NoFaction, _currentlySelectedDefName == Strings.NoFactionKey))
					_currentlySelectedDefName = Strings.NoFactionKey;
			}
		}

		public void ListXenotypes(Rect outRect)
		{
			if (_currentlySelectedDefName is null)
				return;

			using var listingScope = new ScrollableListingScope(outRect, ref _xenotypeScrollViewStatus, Listing);

			var xenotypeChances = XenotypeChanceDatabase<T>.For(_currentlySelectedDefName);
			foreach (var xenotypeChance in xenotypeChances.AllSavedValues.OrderBy(pair => pair.Key))
			{
				if (!xenotypeChances.AllActiveXenotypes.TryGetValue(xenotypeChance.Key, out var modifiableXenotype))
				{
					InactiveLabel(Listing, xenotypeChance.Key.CapitalizeFirst(), xenotypeChance.Value);
					continue;
				}

				var sliderResult = Listing.IntSlider(modifiableXenotype.Xenotype.Label,
					 modifiableXenotype.Xenotype.Def == XenotypeDefOf.Baseliner
					 ? Mathf.RoundToInt(xenotypeChances.GetBaselinerChance() * 1000f)
					 : xenotypeChance.Value.RawValue,
					 0, 1000, 3, modifiableXenotype.Xenotype.Tooltip);

				if (sliderResult - Mathf.RoundToInt(xenotypeChance.Value.RawValue) != 0)
					xenotypeChances.SetChanceForXenotype(xenotypeChance.Key, sliderResult);
			}

			if (Listing.ButtonText(Strings.Translated.Reset))
				xenotypeChances.Reset();
		}

		private static void InactiveLabel(Listing_Standard listing, string xenotypeName, XenotypeChances<T>.Chance? savedValue)
		{
			var inactiveRect = listing.GetRect(Text.CalcHeight($"{Strings.Translated.Inactive}: ", listing.ColumnWidth) + Listing_Standard.PinnableActionHeight + (listing.verticalSpacing * 2f));
			var currentColor = GUI.color;
			GUI.color = Color.red with { a = 0.3f };
			GUI.DrawTexture(inactiveRect, BaseContent.WhiteTex);
			GUI.color = currentColor;
			if (Widgets.CloseButtonFor(inactiveRect))
				ModifiableXenotypeDatabase.RemoveCustomXenotype(xenotypeName);
			listing.curY -= inactiveRect.height;

			listing.Label($"{Strings.Translated.Inactive}: {xenotypeName}, {(savedValue?.RawValue ?? 0) / 10m}%");
			listing.Gap(Listing_Standard.PinnableActionHeight + listing.verticalSpacing);
		}
	}

	public class EditorTab : Tab
	{
		private ScrollViewStatus _scrollViewStatus;
		public Listing_Standard Listing { get; } = new() { verticalSpacing = 4f };

		public override void DoWindowContents(Rect inRect)
		{
			using var listingScope = new ScrollableListingScope(inRect, ref _scrollViewStatus, Listing);

			foreach (var customXenotype in ModifiableXenotypeDatabase.CustomXenotypes)
			{
				if (customXenotype.Value is ModifiableXenotype.Generated)
					continue;

				var rect = Listing.GetRect(Math.Max(Text.CalcHeight(customXenotype.Key, Listing.ColumnWidth * 0.5f), 30f) + Listing.verticalSpacing);
				Listing.curY -= rect.height;
				if (Widgets.CloseButtonFor(rect))
					OpenDeletionDialog(customXenotype.Value);

				Listing.columnWidthInt -= Widgets.CloseButtonSize + (Widgets.CloseButtonMargin * 2f);
				if (Listing.ButtonTextLabeled(customXenotype.Key.CapitalizeFirst(), Strings.Translated.Edit))
					Find.WindowStack.Add((Dialog_CreateXenotype?)CreateXenotypeDialogFor(customXenotype));
				Listing.columnWidthInt += Widgets.CloseButtonSize + (Widgets.CloseButtonMargin * 2f);
			}

			if (Listing.ButtonTextLabeled(Strings.Translated.MakeYourOwn, "+"))
				Find.WindowStack.Add(new Dialog_CreateXenotype(HarmonyPatches.StartingPawnUtility.MAGIC_NUMBER, null));
		}

		public static void OpenDeletionDialog(ModifiableXenotype xenotype)
			=> Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(Strings.ConfirmDelete.Translate(xenotype.Name), ()
				=> ModifiableXenotypeDatabase.DeleteCustomXenotype(xenotype.Name), destructive: true));

		public static Dialog_CreateXenotype CreateXenotypeDialogFor(KeyValuePair<string, ModifiableXenotype> customXenotype)
		{
			var createXenotypeDialog = new Dialog_CreateXenotype(HarmonyPatches.StartingPawnUtility.MAGIC_NUMBER, null)
			{
				xenotypeName = customXenotype.Key
			};
			createXenotypeDialog.selectedGenes.Clear();
			createXenotypeDialog.selectedGenes.AddRange(customXenotype.Value.CustomXenotype!.genes);
			createXenotypeDialog.inheritable = customXenotype.Value.CustomXenotype.inheritable;
			createXenotypeDialog.OnGenesChanged();
			createXenotypeDialog.ignoreRestrictions = createXenotypeDialog.selectedGenes.Any(def => def.biostatArc > 0) || !createXenotypeDialog.WithinAcceptableBiostatLimits(false);
			return createXenotypeDialog;
		}
	}

	private static Tab _currentTab = Tabs.First();
	private static List<TabRecord> _tabList = new();

	public class TabRecord : Verse.TabRecord
	{
		public Tab Tab { get; }
		public AccessTools.FieldRef<Tab> CurrentTab { get; }
		public TabRecord(Tab tab, AccessTools.FieldRef<Tab> currentTab) : base(tab.Title, null, null)
		{
			Tab = tab;
			CurrentTab = currentTab;
			clickedAction = () => CurrentTab() = Tab;
			selectedGetter = () => CurrentTab() == Tab;
		}
	}
}