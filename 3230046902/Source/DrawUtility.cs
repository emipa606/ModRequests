using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using System.Collections.Generic;
using System;
using System.Linq;
using static CherryPicker.ModSettings_CherryPicker;
using static CherryPicker.CherryPickerUtility;
 
namespace CherryPicker
{
    internal static class DrawUtility
	{
		public static int lineNumber; //Handles row highlighting and also dynamic window size for the scroll bar
		static int cellPosition; //Tracks the vertical placement in pixels
		public const int lineHeight = 22; //Text.LineHeight + options.verticalSpacing;
		public static List<FloatMenuOption> cachedDefMenu, cachedModMenu; //Stores the filter drop-down menu, clears when you leave the mod options
		public static Type filteredType;
		public static string filteredMod;

		public const string REMOVED_DEFS = "RemovedDefs";
		public static void DrawList(Rect container, Listing_Standard options)
		{
			lineNumber = cellPosition = 0; //Reset
			if (searchStringCache == null) MakeLabelCache();
			
			//List out all the unremoved defs from the compiled database
			for (int i = 0; i < allDefs.Length; i++)
			{
				Def def = allDefs[i];
				if (def != null && 
					(!filtered || 
					(searchStringCache.TryGetValue(def, out string label) && label.Contains(filter) )) &&
				(filteredMod.NullOrEmpty() || filteredMod == def.modContentPack?.nameInt 
				|| (filteredMod == REMOVED_DEFS && actualRemovedDefs.Contains(def.ToKey())) 
				) &&
				(filteredType == null || filteredType == def.GetType()) )
				{
					cellPosition += lineHeight;
					++lineNumber;
					
					if (cellPosition > scrollPos.y - container.height && cellPosition < scrollPos.y + container.height) DrawListItem(options, def);
				}
				
			}
		}
		public static void MakeLabelCache()
		{
			searchStringCache = new Dictionary<Def, string>();
			for (int i = allDefs.Length; i-- > 0;)
			{
				var def = allDefs[i];
				searchStringCache.Add(def, (def?.defName + def.label + def.modContentPack?.Name + def.GetType().Name).ToLower());	
			}
		}
		public static void DrawListItem(Listing_Standard options, Def def)
		{
			//Prepare key
			string key = def.ToKey();
			string type = key.ToTypeString();

			//Determine checkbox status...
			bool checkOn = !actualRemovedDefs?.Contains(key) ?? false;
			
			//Fetch bounding rect
			Rect rect = options.GetRect(lineHeight);
			rect.y = cellPosition;

			//Label
			string dataString = type + " :: " + def.modContentPack?.Name + " :: " + def.defName;

			//Actually draw the line item
			if (options.BoundingRectCached == null || rect.Overlaps(options.BoundingRectCached.Value))
			{
				CheckboxLabeled(rect, dataString, def.label, ref checkOn, def);
			}

			//Handle row coloring and spacing
			options.Gap(options.verticalSpacing);
			if (lineNumber % 2 != 0) Widgets.DrawLightHighlight(rect);
			Widgets.DrawHighlightIfMouseover(rect);

			//Tooltip
			TooltipHandler.TipRegion(rect, "Type: " + type + "\nMod: " + def.modContentPack?.Name + "\nDef: " + def.defName + "\nFilename: " + def.fileName);
			
			//Add to working list if missing
			bool flag = false;
			if (!checkOn && (flag = !actualRemovedDefs.Contains(key))) actualRemovedDefs.Add(key);
			//Remove from working list
			else if (checkOn && (flag = actualRemovedDefs.Contains(key))) actualRemovedDefs.Remove(key);
			//Immediately process the list if this is a deflist
			if (flag && type == nameof(DefList)) LoadedModManager.GetMod<Mod_CherryPicker>().WriteSettings();
		}
		static void CheckboxLabeled(Rect rect, string data, string label, ref bool checkOn, Def def)
		{
			Rect leftHalf = rect.LeftHalf();
			
			//Is there an icon?
			Rect iconRect = new Rect(leftHalf.x, leftHalf.y, 32f, leftHalf.height);
			Texture2D icon = null;
			if (def is BuildableDef) icon = ((BuildableDef)def).uiIcon;
			else if (def is RecipeDef) icon = ((RecipeDef)def).ProducedThingDef?.uiIcon;

			if (Widgets.ButtonInvisible(iconRect)&& Current.ProgramState==ProgramState.Playing) 
			{
                Find.WindowStack.Add(new Dialog_InfoCard(def));
			}
			if (icon != null)
			{
				GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true, 1f, Color.white, 0f, 0f);
			}
				
			
			//If there is a label, split the cell in half, otherwise use the full cell for data
			if (!label.NullOrEmpty())
			{
				Rect dataRect = new Rect(iconRect.xMax, iconRect.y, leftHalf.width - 32f, leftHalf.height);

				Widgets.Label(dataRect, data?.Truncate(dataRect.width - 12f, InspectPaneUtility.truncatedLabelsCached));
				Rect rightHalf = rect.RightHalf();
				Widgets.Label(rightHalf, label.Truncate(rightHalf.width - 12f, InspectPaneUtility.truncatedLabelsCached));
			}
			else
			{
				Rect dataRect = new Rect(iconRect.xMax, iconRect.y, rect.width - 32f, leftHalf.height);
				Widgets.Label(dataRect, data?.Truncate(dataRect.width - 12f, InspectPaneUtility.truncatedLabelsCached));
			}

			Widgets.Checkbox(new Vector2(rect.xMax - 24f, rect.y), ref checkOn, 24f, false, true, null, null);
		}
		public static List<FloatMenuOption> MenuOfDefs()
		{
			if (cachedDefMenu != null) return cachedDefMenu;
			cachedDefMenu = new List<FloatMenuOption>();

			HashSet<string> isDistinct = new HashSet<string>();
			for (int i = 0; i < allDefs.Length; i++)
			{
				var type = allDefs[i].GetType();
				var nameSpace = type.Namespace;
				var label = type.Name;
				var visualLabel = label;
                var key = label;
				if (nameSpace != "RimWorld" && nameSpace != "Verse")
				{
                    if (DefUtility.typeCache.ContainsKey(label))
                    {
                        visualLabel = nameSpace + "." + label;
						key = label + "/_/" + nameSpace;
                    } else continue;
				}
				if (isDistinct.Add(key))
				{
					cachedDefMenu.Add(new FloatMenuOption(visualLabel, delegate()
					{
						ApplyCategoryFilter(key);
					}, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
				}
			}
			cachedDefMenu.SortBy(x => x.labelInt);
			cachedDefMenu.Insert(0, new FloatMenuOption("CherryPicker.AllDefTypes".Translate(), delegate()
				{
					ApplyCategoryFilter("AllDefTypes");
				}, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));

			return cachedDefMenu;
		}
		static void ApplyCategoryFilter(string defType)
		{
			if (defType == "AllDefTypes") 
			{
				filteredType = null;
				return;
			}
			filteredType = DefUtility.ToType(defType, false);
		}

		public static List<FloatMenuOption> MenuOfPacks(bool RemovedDefs=true)
		{
			if (cachedModMenu != null) return cachedModMenu;
			cachedModMenu = new List<FloatMenuOption>();
			
			foreach (var mod in ActiveMods)
			{
				if (mod==null)
				{
					continue;
				}
				string label = mod.Name;
				
				cachedModMenu.Add(new FloatMenuOption(mod.IsOfficialMod ?$"*{label}":label, delegate()
					{
						ApplyModFilter(label);
					}, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
			}
			cachedModMenu.SortBy(x => x.labelInt);
			
            if (RemovedDefs)
            {
                cachedModMenu.Insert(0, new FloatMenuOption("CherryPicker.RemovedDefs".Translate(), delegate ()
                {
                    ApplyModFilter(REMOVED_DEFS);
                }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
            }
            cachedModMenu.Insert(0, new FloatMenuOption("CherryPicker.AllPacks".Translate(), delegate()
				{
					ApplyModFilter("");
				}, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
            
            return cachedModMenu;
		}

		static void ApplyModFilter(string modName)
		{
			filteredMod = modName;
		}
	}
}
