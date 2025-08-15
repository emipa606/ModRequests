using UnityEngine;
using Verse;
using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using AlienRace;
using HarmonyLib;

namespace StylishRim
{

	// このクラス散らかりすぎじゃね
	// 最初に書き始めたクラスはもう手遅れだ
	// 手遅れだ！！！
	public class StylishRimSettings : Mod
	{

		public static readonly bool DEBUG = false;
		public static readonly string MOD_NAME = "Stylish Rim";

		public StylishRimSettings(ModContentPack content)
			: base(content)
		{
			PreUpdate();
			GetSettings<StylishModSettings>();
			if (StylishModSettings.ModVersion < StylishUpdater.MOD_VERSION) UpdateVersion();
			Init();
		}
		private void PreUpdate()
		{
			string path = System.IO.Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename($"Mod_{Content.FolderName}_SettingsController.xml"));
			if (System.IO.File.Exists(path))
			{
				System.IO.FileInfo fi = new System.IO.FileInfo(path);
				if (!System.IO.File.Exists(System.IO.Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename($"Mod_{Content.FolderName}_{GetType().Name}.xml"))))
				{
					fi.CopyTo(System.IO.Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename($"Mod_{Content.FolderName}_{GetType().Name}.xml")));
				}
				fi.Delete();
			}
		}
		private void UpdateVersion()
		{
			StylishUpdater.Run();
			if (StylishModSettings.ModVersion == StylishUpdater.MOD_VERSION)
			{
				base.WriteSettings();
			}
			else
			{
				Log.Error("[Stylish Rim] Update failed, settings were not updated correctly.");
			}
		}
		public override string SettingsCategory()
		{
			return MOD_NAME;
		}
		public override void WriteSettings()
		{
			RevertSetting();
			base.WriteSettings();
			runOnce = false;
			gameLoaded = false;
			ClosePreview();
			switch (menuMode)
			{
				case MenuMode.SAVE:
				case MenuMode.LOAD:
					MenuModeToGlobal();
					break;
			}
		}
		private void Init()
		{
			buttonTarget = LABEL_GLOBAL_SETTINGS;
			InitExposer(DIR_NAME, ELEMENT_NAME, SUB_FOLDER_NAME);
			StylishBodyChanger.SetCustomFolders();
			FixValue();

			RefleshAll();
		}

		public static readonly string RACE_HUMAN = "Human";

		public static readonly string COMMON = "Common";
		public static readonly string EXTRA1 = "ExtraPart1";
		public static readonly string EXTRA2 = "ExtraPart2";

		public static readonly string KEY_FRONT = "Front";
		public static readonly string KEY_LEFT = "Left";
		public static readonly string KEY_RIGHT = "Right";
		public static readonly string KEY_BACK = "Back";
		public static readonly string KEY_ASTERISK = "* ";
		public static readonly string KEY_SPACE = "  ";

		private static readonly string LABEL_HEAD = "Head";
		private static readonly string LABEL_HAIR = "Hair";
		private static readonly string LABEL_BODY = "Body";
		private static readonly string LABEL_INDEX = "index: ";
		private static readonly string LABEL_ROTATE = "Rotate: ";
		private static readonly string LABEL_STYLISH = "Stylish";
		private static readonly string LABEL_DEFAULT = "(default)";
		private static readonly string LABEL_NONE = "None";


		private static readonly string CAT_PORTRAIT = "Portrait";
		private static readonly string CAT_HAIR = "Hair";
		private static readonly string CAT_GENES = "Genes";
		private static readonly string CAT_ADDONS = "Addons";
		private static readonly string CAT_APPARELS = "Apparel";
		private static readonly string CAT_EQUIPMENT = "Equip";
		private static readonly string CAT_BODY = "Body";
		private static readonly string CAT_OPTIONAL = "Optional";

		public static readonly string LABEL_OVERVIEW = "StylishRim.Overview";
		public static readonly string LABEL_OVERVIEW_OPTIONAL = "StylishRim.Overview.Optional";

		public static readonly string LABEL_BODY_SIZE_X = "StylishRim.BodySizeX";
		public static readonly string LABEL_BODY_SIZE_Y = "StylishRim.BodySizeY";

		public static readonly string LABEL_HEAD_SIZE_X = "StylishRim.HeadSizeX";
		public static readonly string LABEL_HEAD_SIZE_Y = "StylishRim.HeadSizeY";
		public static readonly string LABEL_HEAD_SIZE_Z = "StylishRim.HeadSizeZ";
		public static readonly string LABEL_HEAD_OFFSET_X = "StylishRim.HeadOffsetX";
		public static readonly string LABEL_HEAD_OFFSET_Y = "StylishRim.HeadOffsetY";
		public static readonly string LABEL_HEAD_OFFSET_VERTICAL_SIDE = "StylishRim.HeadOffsetVerticalSide";
		public static readonly string LABEL_HEAD_OFFSET_VERTICAL_BACK = "StylishRim.HeadOffsetVerticalBack";

		public static readonly string LABEL_HAIR_SIZE_X = "StylishRim.HairSizeX";
		public static readonly string LABEL_HAIR_SIZE_Y = "StylishRim.HairSizeY";
		public static readonly string LABEL_HAIR_SIZE_Z = "StylishRim.HairSizeZ";
		public static readonly string LABEL_HAIR_OFFSET_X = "StylishRim.HairOffsetX";
		public static readonly string LABEL_HAIR_OFFSET_Y = "StylishRim.HairOffsetY";
		public static readonly string LABEL_HAIR_OFFSET_Z = "StylishRim.HairOffsetZ";
		public static readonly string LABEL_HAIR_OFFSET_W = "StylishRim.HairOffsetW";
		public static readonly string LABEL_HAIR_ANGLE_X = "StylishRim.HairAngleX";
		public static readonly string LABEL_HAIR_ANGLE_Y = "StylishRim.HairAngleY";
		public static readonly string LABEL_HAIR_SHOW = "StylishRim.ShowHair";

		public static readonly string LABEL_APPAREL_SIZE_X = "StylishRim.ApparelSizeX";
		public static readonly string LABEL_APPAREL_SIZE_Y = "StylishRim.ApparelSizeY";
		public static readonly string LABEL_APPAREL_SIZE_Z = "StylishRim.ApparelSizeZ";
		public static readonly string LABEL_APPAREL_OFFSET_X = "StylishRim.ApparelOffsetX";
		public static readonly string LABEL_APPAREL_OFFSET_Y = "StylishRim.ApparelOffsetY";
		public static readonly string LABEL_APPAREL_OFFSET_Z = "StylishRim.ApparelOffsetZ";
		public static readonly string LABEL_APPAREL_OFFSET_W = "StylishRim.ApparelOffsetW";
		public static readonly string LABEL_APPAREL_PRIORITY = "StylishRim.ApparelPriority";
		public static readonly string LABEL_APPAREL_ANGLE_X = "StylishRim.ApparelAngleX";
		public static readonly string LABEL_APPAREL_ANGLE_Y = "StylishRim.ApparelAngleY";
		public static readonly string LABEL_APPAREL_HIDE = "StylishRim.ApparelHide";

		public static readonly string LABEL_ADDON_SIZE_X = "StylishRim.AddonSizeX";
		public static readonly string LABEL_ADDON_SIZE_Y = "StylishRim.AddonSizeY";
		public static readonly string LABEL_ADDON_SIZE_Z = "StylishRim.AddonSizeZ";
		public static readonly string LABEL_ADDON_OFFSET_X = "StylishRim.AddonOffsetX";
		public static readonly string LABEL_ADDON_OFFSET_Y = "StylishRim.AddonOffsetY";
		public static readonly string LABEL_ADDON_OFFSET_Z = "StylishRim.AddonOffsetZ";
		public static readonly string LABEL_ADDON_OFFSET_W = "StylishRim.AddonOffsetW";
		public static readonly string LABEL_ADDON_ANGLE_X = "StylishRim.AddonAngleX";
		public static readonly string LABEL_ADDON_ANGLE_Y = "StylishRim.AddonAngleY";
		public static readonly string LABEL_ADDON_HIDE = "StylishRim.AddonHide";
		public static readonly string LABEL_ADDON_IS_HEAD = "StylishRim.AddonIsHead";

		public static readonly string LABEL_GENE_SIZE_X = "StylishRim.GeneSizeX";
		public static readonly string LABEL_GENE_SIZE_Y = "StylishRim.GeneSizeY";
		public static readonly string LABEL_GENE_SIZE_Z = "StylishRim.GeneSizeZ";
		public static readonly string LABEL_GENE_OFFSET_X = "StylishRim.GeneOffsetX";
		public static readonly string LABEL_GENE_OFFSET_Y = "StylishRim.GeneOffsetY";
		public static readonly string LABEL_GENE_OFFSET_Z = "StylishRim.GeneOffsetZ";
		public static readonly string LABEL_GENE_OFFSET_W = "StylishRim.GeneOffsetW";
		public static readonly string LABEL_GENE_ANGLE_X = "StylishRim.GeneAngleX";
		public static readonly string LABEL_GENE_ANGLE_Y = "StylishRim.GeneAngleY";
		public static readonly string LABEL_GENE_HIDE = "StylishRim.GeneHide";

		public static readonly string LABEL_EQUIPMENT_SIZE_X = "StylishRim.EquipmentSizeX";
		public static readonly string LABEL_EQUIPMENT_SIZE_Y = "StylishRim.EquipmentSizeY";
		public static readonly string LABEL_EQUIPMENT_SIZE_Z = "StylishRim.EquipmentSizeZ";
		public static readonly string LABEL_EQUIPMENT_SIZE_XZ = "StylishRim.EquipmentSizeXZ";
		public static readonly string LABEL_EQUIPMENT_OFFSET_X = "StylishRim.EquipmentOffsetX";
		public static readonly string LABEL_EQUIPMENT_OFFSET_Y = "StylishRim.EquipmentOffsetY";
		public static readonly string LABEL_EQUIPMENT_OFFSET_Z = "StylishRim.EquipmentOffsetZ";
		public static readonly string LABEL_EQUIPMENT_ANGLE_X = "StylishRim.EquipmentAngleX";
		public static readonly string LABEL_EQUIPMENT_ANGLE_Y = "StylishRim.EquipmentAngleY";
		public static readonly string LABEL_EQUIPMENT_ANGLE_Z = "StylishRim.EquipmentAngleZ";
		public static readonly string LABEL_EQUIPMENT_ANGLE_W = "StylishRim.EquipmentAngleW";
		public static readonly string LABEL_EQUIPMENT_PRIORITY = "StylishRim.EquipmentPriority";
		public static readonly string LABEL_EQUIPMENT_FLIPS = "StylishRim.EquipmentFlips";
		public static readonly string LABEL_EQUIPMENT_ROT_N = "StylishRim.EquipmentRotN";
		public static readonly string LABEL_EQUIPMENT_ROT_E = "StylishRim.EquipmentRotE";
		public static readonly string LABEL_EQUIPMENT_ROT_S = "StylishRim.EquipmentRotS";
		public static readonly string LABEL_EQUIPMENT_ROT_W = "StylishRim.EquipmentRotW";
		public static readonly string LABEL_EQUIPMENT_USE_COMMON = "StylishRim.EquipmentUseCommon";
		public static readonly string LABEL_NORTH_INVERT = "StylishRim.NorthInvert";

		public static readonly string LABEL_BODY_ADDON_SIZE_X = "StylishRim.BodyAddonSizeX";
		public static readonly string LABEL_BODY_ADDON_SIZE_Y = "StylishRim.BodyAddonSizeY";
		public static readonly string LABEL_BODY_ADDON_SIZE_Z = "StylishRim.BodyAddonSizeZ";
		public static readonly string LABEL_BODY_ADDON_OFFSET_X = "StylishRim.BodyAddonOffsetX";
		public static readonly string LABEL_BODY_ADDON_OFFSET_Y = "StylishRim.BodyAddonOffsetY";
		public static readonly string LABEL_BODY_ADDON_OFFSET_Z = "StylishRim.BodyAddonOffsetZ";

		public static readonly string LABEL_HEAD_ADDON_SIZE_X = "StylishRim.HeadAddonSizeX";
		public static readonly string LABEL_HEAD_ADDON_SIZE_Y = "StylishRim.HeadAddonSizeY";
		public static readonly string LABEL_HEAD_ADDON_SIZE_Z = "StylishRim.HeadAddonSizeZ";
		public static readonly string LABEL_HEAD_ADDON_OFFSET_X = "StylishRim.HeadAddonOffsetX";
		public static readonly string LABEL_HEAD_ADDON_OFFSET_Y = "StylishRim.HeadAddonOffsetY";
		public static readonly string LABEL_HEAD_ADDON_OFFSET_Z = "StylishRim.HeadAddonOffsetZ";

		public static readonly string LABEL_HEAD_CORRECT_Y = "StylishRim.HeadCorrectY";
		public static readonly string LABEL_HEAD_CORRECT_Z = "StylishRim.HeadCorrectZ";
		public static readonly string LABEL_BODY_CORRECT_Y = "StylishRim.DrawLocCorrectY";
		public static readonly string LABEL_BODY_CORRECT_Z = "StylishRim.DrawLocCorrectZ";
		public static readonly string LABEL_BODY_CORRECT_PAWN_Y = "StylishRim.DrawLocCorrectPawnY";
		public static readonly string LABEL_BODY_CORRECT_PAWN_Z = "StylishRim.DrawLocCorrectPawnZ";

		public static readonly string LABEL_BORDERSCALE_MULT = "StylishRim.BorderScaleMult";
		public static readonly string LABEL_CACHE_SIZE = "StylishRim.CacheSize";
		public static readonly string LABEL_CACHE_SIZE_AFTER = "StylishRim.CacheSizeAfter";
		public static readonly string LABEL_RESOLUTION_BORDER_MAX = "StylishRim.ResolutionBorderMax";
		public static readonly string LABEL_RESOLUTION = "StylishRim.Resolution";
		public static readonly string LABEL_CACHE_WIDTH = "StylishRim.CacheWidth";

		public static readonly string LABEL_PORTRAIT_ROTATE = "StylishRim.PortraitRotate";
		public static readonly string LABEL_PORTRAIT_SIZE = "StylishRim.PortraitSize";
		public static readonly string LABEL_PORTRAIT_OFFSET_X = "StylishRim.PortraitOffsetX";
		public static readonly string LABEL_PORTRAIT_OFFSET_Y = "StylishRim.PortraitOffsetY";
		public static readonly string LABEL_PORTRAIT_ANGLE = "StylishRim.PortraitAngle";
		public static readonly string LABEL_PORTRAIT_HEAD_OFFSET_MULT = "StylishRim.PortraitHeadOffsetMult";
		public static readonly string LABEL_PORTRAIT_DISABLE_HEADGEAR = "StylishRim.PortraitDisableHeadGear";
		public static readonly string LABEL_PORTRAIT_DISABLE_CLOTHES = "StylishRim.PortraitDisableClothes";

		public static readonly string LABEL_APPARELS_BUTTON = "StylishRim.Apparels.Button";

		public static readonly string LABEL_CHANGER_DRAW_LOC_X = "StylishRim.ChangerDrawLocX";
		public static readonly string LABEL_CHANGER_DRAW_LOC_Y = "StylishRim.ChangerDrawLocY";
		public static readonly string LABEL_CHANGER_BODY_SIZE_X = "StylishRim.ChangerBodySizeX";
		public static readonly string LABEL_CHANGER_BODY_SIZE_Y = "StylishRim.ChangerBodySizeY";
		public static readonly string LABEL_CHANGER_BODY_SIZE_Z = "StylishRim.ChangerBodySizeZ";
		public static readonly string LABEL_CHANGER_BODY_OFFSET_X = "StylishRim.ChangerBodyOffsetX";
		public static readonly string LABEL_CHANGER_BODY_OFFSET_Y = "StylishRim.ChangerBodyOffsetY";
		public static readonly string LABEL_CHANGER_BODY_OFFSET_Z = "StylishRim.ChangerBodyOffsetZ";
		public static readonly string LABEL_CHANGER_BODY_ANGLE_X = "StylishRim.ChangerBodyAngleX";
		public static readonly string LABEL_CHANGER_BODY_ANGLE_Y = "StylishRim.ChangerBodyAngleY";
		public static readonly string LABEL_CHANGER_BODY_HUE = "StylishRim.ChangerBodyHue";
		public static readonly string LABEL_CHANGER_BODY_SATURATION = "StylishRim.ChangerBodySaturation";
		public static readonly string LABEL_CHANGER_BODY_VALUE = "StylishRim.ChangerBodyValue";

		public static readonly string LABEL_WEIGHT_TINY = "StylishRim.WeightTiny";
		public static readonly string LABEL_WEIGHT_SMALL = "StylishRim.WeightSmall";
		public static readonly string LABEL_WEIGHT_MIDDLE = "StylishRim.WeightMiddle";
		public static readonly string LABEL_WEIGHT_LARGE = "StylishRim.WeightLarge";
		public static readonly string LABEL_WEIGHT_EXTREME = "StylishRim.WeightExtreme";

		public static readonly string LABEL_GLOBAL_CACHE_ADJUST = "StylishRim.OptionalCacheAdjust";
		public static readonly string LABEL_GLOBAL_IGNORE_HEADONLY = "StylishRim.OptionalIgnoreHeadonly";
		public static readonly string LABEL_GLOBAL_DISABLE_CACHE_COLONIST = "StylishRim.OptionalDisableCacheColonist";
		public static readonly string LABEL_GLOBAL_RESOLVE_UNFINDED_BODY = "StylishRim.OptionalResolveUnfindedBody";
		public static readonly string LABEL_GLOBAL_FOR_CORPSE_CACHE = "StylishRim.OptionalForCorpseCache";
		public static readonly string LABEL_GLOBAL_DISABLE_ZOOM_CACHE_OFF = "StylishRim.OptionalDisableZoomCacheOff";
		public static readonly string LABEL_GLOBAL_COMMONIZATION_PORTRAIT = "StylishRim.OptionalCommonizationPortrait";
		public static readonly string LABEL_GLOBAL_DISABLE_ANIMAL_CACHE = "StylishRim.OptionalDisableAnimalCache";
		public static readonly string LABEL_GLOBAL_WEIGHT_CPU = "StylishRim.OptionalWeightCPU";
		public static readonly string LABEL_GLOBAL_WEIGHT_MEMORY = "StylishRim.OptionalWeightMemory";
		public static readonly string LABEL_GLOBAL_AUTO_SETTING = "StylishRim.OptionalAutoSetting";
		public static readonly string LABEL_OPTIONAL_LEAVE_CACHE = "StylishRim.OptionalLeaveCache";
		public static readonly string LABEL_OPTIONAL_DISABLE_CACHE = "StylishRim.OptionalDisableCache";
		public static readonly string LABEL_OPTIONAL_UTILITY_ADJUST = "StylishRim.OptionalUtilityAdjust";
		public static readonly string LABEL_OPTIONAL_IGNORE_RESTRICT = "StylishRim.OptionalIgnoreRestrict";
		public static readonly string LABEL_OPTIONAL_HAIR_ADJUST = "StylishRim.OptionalHairAdjust";


		public static readonly string LABEL_TARGET_RACE = "StylishRim.TargetRace";

		public static readonly string LABEL_SAVE_MENU = "StylishRim.SaveMenu";
		public static readonly string LABEL_LOAD_MENU = "StylishRim.LoadMenu";

		public static readonly string TOOLTIP_NEWSTYLE = "StylishRim.Tooltip.NewStyle";
		public static readonly string TOOLTIP_CACHE_ADJUST = "StylishRim.Tooltip.CacheAdjust";
		public static readonly string TOOLTIP_IGNORE_HEADONLY = "StylishRim.Tooltip.IgnoreHeadonly";
		public static readonly string TOOLTIP_DISABLE_CACHE = "StylishRim.Tooltip.DisableCache";
		public static readonly string TOOLTIP_DISABLE_CACHE_COLONIST = "StylishRim.Tooltip.DisableCacheColonist";
		public static readonly string TOOLTIP_RESOLVE_UNFINDED_BODY = "StylishRim.Tooltip.ResolveUnfindedBody";
		public static readonly string TOOLTIP_FOR_CORPSE_CACHE = "StylishRim.Tooltip.ForCorpseCache";
		public static readonly string TOOLTIP_DISABLE_ZOOM_CACHE_OFF = "StylishRim.Tooltip.DisableZoomCacheOff";
		public static readonly string TOOLTIP_COMMONIZATION_PORTRAIT = "StylishRim.Tooltip.CommonizationPortrait";
		public static readonly string TOOLTIP_DISABLE_ANIMAL_CACHE = "StylishRim.Tooltip.DisableAnimalCache";
		public static readonly string TOOLTIP_WEIGHT_CPU = "StylishRim.Tooltip.WeightCPU";
		public static readonly string TOOLTIP_WEIGHT_MEMORY = "StylishRim.Tooltip.WeightMemory";
		public static readonly string TOOLTIP_AUTO_SETTING = "StylishRim.Tooltip.AutoSetting";
		public static readonly string TOOLTIP_LEAVE_CACHE = "StylishRim.Tooltip.LeaveCache";
		public static readonly string TOOLTIP_UTILITY_ADJUST = "StylishRim.Tooltip.UtilityAdjust";
		public static readonly string TOOLTIP_IGNORE_RESTRICT = "StylishRim.Tooltip.IgnoreRestrict";
		public static readonly string TOOLTIP_HAIR_SHOW = "StylishRim.Tooltip.ShowHair";
		public static readonly string TOOLTIP_PRIORITY = "StylishRim.Tooltip.Priority";
		public static readonly string TOOLTIP_USE_COMMON = "StylishRim.Tooltip.UseCommon";
		public static readonly string TOOLTIP_NORTH_INVERT_A = "StylishRim.Tooltip.NorthInvertA";
		public static readonly string TOOLTIP_NORTH_INVERT_E = "StylishRim.Tooltip.NorthInvertE";
		public static readonly string TOOLTIP_PORTRAIT_HEAD_OFFSET_MULT = "StylishRim.Tooltip.PortraitHeadOffsetMult";
		public static readonly string TOOLTIP_CHANGER_ONLY_BODY = "StylishRim.Tooltip.OnlyBody";
		public static readonly string TOOLTIP_CHANGER_NO_INVERT = "StylishRim.Tooltip.NoInvert";
		public static readonly string TOOLTIP_COMMON = "StylishRim.Tooltip.Common";
		public static readonly string TOOLTIP_SAVELOAD_MENU = "StylishRim.Tooltip.SaveLoadMenu";
		public static readonly string TOOLTIP_COPY_PASTE = "StylishRim.Tooltip.CopyPaste";
		public static readonly string TOOLTIP_CACHE_STATUS = "StylishRim.Tooltip.CacheStatus";

		public static readonly string LABEL_GLOBAL_SETTINGS = "Global Settings";
		public static readonly string LABEL_BUTTON_APPLY = "StylishRim.ButtonApply";
		public static readonly string LABEL_BUTTON_REVERT = "StylishRim.ButtonRevert";
		public static readonly string LABEL_BUTTON_RESET = "StylishRim.ButtonReset";
		public static readonly string LABEL_BUTTON_RESET_ALL = "StylishRim.ButtonResetAll";
		public static readonly string LABEL_BUTTON_REFLESH_ALL = "StylishRim.ButtonRefleshAll";
		public static readonly string LABEL_BUTTON_REMOVE = "StylishRim.ButtonRemove";
		public static readonly string LABEL_BUTTON_REMOVE_ALL = "StylishRim.ButtonRemoveAll";
		public static readonly string LABEL_BUTTON_NOT_LOADED_RACE = "StylishRim.ButtonNotLoadedRace";
		public static readonly string LABEL_BUTTON_NOT_LOADED_PAWN = "StylishRim.ButtonNotLoadedPawn";
		public static readonly string LABEL_BUTTON_SAVE_AS = "StylishRim.ButtonSaveAs";
		public static readonly string LABEL_BUTTON_OVERWRITE = "StylishRim.ButtonOverwrite";
		public static readonly string LABEL_BUTTON_LOAD = "StylishRim.ButtonLoad";
		public static readonly string LABEL_BUTTON_RETURN = "StylishRim.ButtonReturn";
		public static readonly string LABEL_BUTTON_COPY = "Copy";
		public static readonly string LABEL_BUTTON_PASTE = "Paste";

		private static readonly float HEIGHT_SLIDER_TEXT = 42f;

		public static readonly string DIR_NAME = "StylishRim";
		public static readonly string ELEMENT_NAME = "StylishRim";
		public static readonly string SUB_FOLDER_NAME = "Settings";

		public static readonly string ADDON_HEAD = "Head Common";
		public static readonly string ADDON_BODY = "Body Common";
		//public static string LABEL_TARGET_PAWN = "StylishRim.TargetPawn";
		//private const string NONE = "None";

		private readonly Dictionary<int, string> Rot = new Dictionary<int, string>() { { 2, KEY_FRONT }, { 3, KEY_LEFT }, { 1, KEY_RIGHT }, { 0, KEY_BACK } };
		/*uooooooooooooooooo
				private enum Category { Unknown = -1, Portrait = 0, Hair = 1, Apparels = 2, Equipment = 3, Addons = 4, Body = 5, Optional = 6 }
				private enum Category { Unknown = -1, Portrait = 0, Hair = 1, Apparels = 2, Equipment = 3, Addons = 4, Optional = 5 }
		*/
		private enum Category { Unknown = -1, Portrait = 0, Hair = 1, Apparels = 2, Equipment = 3, Addons = 4, Genes = 5, Optional = 6 }
		private enum MenuMode { GLOBAL = 0, SETTINGS = 1, SAVE = 2, LOAD = 3 }
		private enum Shaders { Default = 0, Cutout = 1, CutoutComplex = 2, Transparent = 3, MetaOverlay = 4, EdgeDetect = 5 }

		private Dictionary<string, ApparelDict> Apparels
		{
			get
			{
				if (apparels == null) new Dictionary<string, ApparelDict>();
				return apparels;
			}
		}
		private bool ContainApparelSetting(Apparel app)
		{
			return GetApparelStyleSet(app) != null;
		}
		private bool ContainApparelSetting(string modName, string layer)
		{
			return GetApparelStyleSet(modName, layer) != null;
		}
		private StylishSet GetApparelStyleSet(Apparel app)
		{
			return GetApparelStyleSet(app.def.modContentPack.Name, StylishSet.LayerIs(app).defName);
		}
		private StylishSet GetApparelStyleSet(string modName, string layer)
		{
			if (!Apparels.ContainsKey(modName) || !Apparels[modName].ContainsKey(layer)) return null;
			return Apparels[modName][layer];
		}
		private void RemoveApparelStyleSet(Apparel app)
		{
			RemoveApparelStyleSet(app.def.modContentPack.Name, StylishSet.LayerIs(app).defName);
		}
		private void RemoveApparelStyleSet(string modName, string layer)
		{
			if (Apparels == null || Apparels.Count == 0) return;
			if (!Apparels.ContainsKey(modName) || !Apparels[modName].ContainsKey(layer)) return;
			Apparels[modName].Remove(layer);
			if (Apparels[modName].Count == 0) Apparels.Remove(modName);
		}

		private bool EmptyApparel()
		{
			return apparel == null || apparel.IsEmpty;
		}

		private Dictionary<string, StylishSet> Equipments
		{
			get
			{
				NewEquipments();
				return equipments;
			}
		}
		private Dictionary<string, StylishSet> Hairs
		{
			get
			{
				if (hairs == null) hairs = new Dictionary<string, StylishSet>(); return hairs;
			}
		}
		private Dictionary<string, StylishSet> Genes
		{
			get
			{
				if (genes == null) genes = new Dictionary<string, StylishSet>(); return genes;
			}
		}
		private Dictionary<int, StylishAddon> Addons
		{
			get
			{
				if (addons == null) addons = new Dictionary<int, StylishAddon>();
				return addons;
			}
		}
		private Vector3 GetCorrection(string bodyType)
		{
			if (corrections == null) corrections = new Dictionary<string, Vector3>();
			if (!corrections.ContainsKey(bodyType))
			{
				corrections.Add(bodyType, Vector3.zero);
			}
			return corrections[bodyType];
		}
		private Vector3 GetDrawLocCorrection(string bodyType)
		{
			if (drawLocCorrections == null) drawLocCorrections = new Dictionary<string, Vector3>();
			if (!drawLocCorrections.ContainsKey(bodyType))
			{
				drawLocCorrections.Add(bodyType, Vector3.zero);
			}
			return drawLocCorrections[bodyType];
		}

		bool isPawn = false;
		bool inGame = false;
		string apparelKey = "Core";
		string hairKey = "Core";
		string equipKey = string.Empty;
		string geneKey = string.Empty;
		Vector3 correction = Vector3.zero;
		Vector3 drawLocCorrection = Vector3.zero;
		StylishSet apparel = null;
		StylishSet apparelTemp = null;
		StylishSet hair = new StylishSet();
		StylishSet equipment = new StylishSet();
		StylishSet gene = new StylishSet();
		StylishAddon addon = null;

		PawnStyleSet pss = new PawnStyleSet();
		Dictionary<string, ApparelDict> apparels = new Dictionary<string, ApparelDict>();
		Dictionary<string, StylishSet> equipments = new Dictionary<string, StylishSet>();
		Dictionary<string, StylishSet> hairs = new Dictionary<string, StylishSet>();
		Dictionary<string, StylishSet> genes = new Dictionary<string, StylishSet>();
		Dictionary<int, StylishAddon> addons = new Dictionary<int, StylishAddon>();
		Dictionary<string, Vector3> corrections = new Dictionary<string, Vector3>();
		Dictionary<string, Vector3> drawLocCorrections = new Dictionary<string, Vector3>();

		Vector3 bodySize = Vector3.one * 100;
		Vector3 headSize = Vector3.one * 100;
		Vector3 bodyAddonSize = Vector3.one * 100;
		Vector3 headAddonSize = Vector3.one * 100;
		Vector2 headOffset = Vector2.zero;
		Vector2 headOffsetVerticalDiff = Vector2.zero;
		Vector2 bodyAddonOffset = Vector2.zero;
		Vector2 headAddonOffset = Vector2.zero;
		Vector2 drawLoc = Vector2.zero;
		Vector3 changeBodySize = Vector3.one;
		Vector3 changeBodyOffset = Vector3.zero;
		Vector2 changeBodyAngle = Vector2.zero;
		Vector3 changeBodyColor = Vector3.zero;
		Vector2 portraitOffset = Vector2.zero;
		int portraitRotation = 2;
		float portraitSize = 100f;
		bool portraitDisableHeadgear = false;
		bool portraitDisableClothes = false;
		float portraitAngle = 0f;
		float portraitHeadOffsetMult = 100;

		float borderScaleMult = 1f;
		int cacheSize = 6;
		int resolution = 0;

		int cacheWidth = 16;
		bool disableCache = false;
		bool disableUtilityAdjust = false;
		bool ignoreRaceRestriction = false;
		bool leaveOneCache = false;

		public string changeBodyRaceName = null;
		public string changeBodyTypeName = null;
		//public string changeShaderName = null;

		public bool showHair = false;

		string targetKey = RACE_HUMAN;
		string buttonTarget = string.Empty;
		string buttonRotate = LABEL_ROTATE + KEY_FRONT;
		string buttonApparels = string.Empty;
		string buttonHairs = "  Core";
		string buttonGenes = string.Empty;
		string buttonEquipments = string.Empty;
		string buttonBodyChanger = string.Empty;
		string buttonAddons = string.Empty;
		string buttonBodyType = string.Empty;

		PawnStyleSet cachePss = null;
		StylishSet cacheHair = null;
		StylishSet cacheApparel = null;
		StylishSet cacheEquipment = null;
		StylishSet cacheGene = null;

		int selectedAddon = -2;
		string selectedBodyType = null;

		public static float weightCPU = 1;
		public static float weightMemory = 1;

		MenuMode menuMode = MenuMode.GLOBAL;
		Category selectedCategory = Category.Portrait;

		public StylishExposer exposer = new StylishExposer(DIR_NAME, ELEMENT_NAME, SUB_FOLDER_NAME);
		private void InitExposer(string dirName, string elementName, string subFolder)
		{
			exposer = new StylishExposer(dirName, elementName, subFolder);
			exposer.SetSaveAsLabel(LABEL_BUTTON_SAVE_AS);
			exposer.SetOverwriteLabel(LABEL_BUTTON_OVERWRITE);
			exposer.SetLoadLabel(LABEL_BUTTON_LOAD);
			exposer.SetDeleteLabel(LABEL_BUTTON_REMOVE);
			exposer.SetReturnLabel(LABEL_BUTTON_RETURN);
		}
		readonly int[] bools = new int[22];

		private void ModSettingsExpose()
		{
			GetSettings<StylishModSettings>().Expose();
		}
		public void SavePrefix()
		{
			base.WriteSettings();
		}
		public void SavePostfix(bool result)
		{
			if (!result)
			{
				Log.Error($"[Stylish Rim] Failed save file.");
			}
			else
			{
				SetFileList(true);
				Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
			}
		}
		public void LoadPostfix(bool result)
		{
			if (!result)
			{
				Log.Error($"[Stylish Rim] Failed load file.");
			}
			else
			{
				if (StylishModSettings.ModVersion < StylishUpdater.MOD_VERSION) UpdateVersion();
				FixValue();
				Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
				RefleshAll();
			}

		}
		public void DeletePostfix(bool result)
		{
			if (!result)
			{
				Log.Error($"[Stylish Rim] Failed delete file.");
			}
			else
			{
				SetFileList(false);
			}
		}




		private void FixValue()
		{
			foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
			{
				if (pss.Apparels != null)
				{
					pss.Apparels.RemoveAll(x => x.Value == null);
					foreach (ApparelDict p in pss.Apparels.Values)
					{
						p.RemoveNull();
					}
				}
				pss.Hairs?.RemoveAll(x => x.Value == null);
				pss.Equipments?.RemoveAll(x => x.Value == null);
				pss.addons?.RemoveAll(x => x.Value == null);
				pss.Genes?.RemoveAll(x => x.Value == null);
			}
		}
		private void RemoveNotExistRace()
		{
			if (PawnStyleSet.Styles == null || PawnStyleSet.Styles.Count == 0) return;
			RemoveNotExist(NotExistRace().ToList());
		}
		private void RemoveNotExistPawn()
		{
			if (PawnStyleSet.Styles == null || PawnStyleSet.Styles.Count == 0) return;
			RemoveNotExist(NotExistPawn().ToList());
		}
		private IEnumerable<PawnStyleSet> NotExistRace()
		{
			foreach (PawnStyleSet psss in PawnStyleSet.Styles.Values.Where(x => x.IsRace))
			{
				if (!DefDatabase<ThingDef>.AllDefsListForReading.Any(x => x.defName == psss.key))
				{
					yield return psss;
				}
			}
		}
		private IEnumerable<PawnStyleSet> NotExistPawn()
		{
			foreach (PawnStyleSet psss in PawnStyleSet.Styles.Values.Where(x => x.IsPawn))
			{
				if (!PawnsFinder.AllMaps.Any(x => x.ThingID == psss.key))
				{
					yield return psss;
				}
			}
		}
		private void RemoveNotExist(List<PawnStyleSet> list)
		{
			PawnStyleSet.Styles.RemoveAll(x => list.Contains(x.Value));
		}
		private string GetWeightLabel(float weight)
		{
			switch (weight)
			{
				case 1: return LABEL_WEIGHT_SMALL;
				case 2: return LABEL_WEIGHT_MIDDLE;
				case 3: return LABEL_WEIGHT_LARGE;
				case 4: return LABEL_WEIGHT_EXTREME;
				default: return LABEL_WEIGHT_TINY;
			}
		}
		private void SetLocalApparel()
		{
			SetLocalApparel(new StylishSet());
		}
		private void SetLocalApparel(StylishSet assTemp, bool included = false)
		{
			apparel = assTemp;
			apparelTemp = apparel.Clone;
			apparelKey = apparel.key;
			apparel.size *= 100;
			apparel.offset *= 1000;
			buttonApparels = $"{(included ? KEY_ASTERISK : KEY_SPACE)}{apparelKey} - {apparel.layer}";
		}
		private void SetLocalApparel(string modName, string layer, string label, bool included = false)
		{
			SetLocalApparel(new StylishSet(modName, layer, label), included);
		}
		private void SetLocalApparel(Apparel app, bool included = false)
		{
			SetLocalApparel(new StylishSet(app), included);
		}
		private void SetLocalHair(StylishSet hssTemp, bool included = false)
		{
			hair = hssTemp;
			hairKey = hair.key;
			hair.size *= 100;
			hair.offset *= 1000;
			buttonHairs = (included ? KEY_ASTERISK : KEY_SPACE) + hairKey;
		}
		private void SetLocalHair(string key, bool included = false)
		{
			SetLocalHair(new StylishSet(key), included);
		}
		private void SetLocalGene(StylishSet hssTemp, bool included = false)
		{
			gene = hssTemp;
			geneKey = gene.key;
			gene.size *= 100;
			gene.offset *= 1000;
			buttonGenes = (included ? KEY_ASTERISK : KEY_SPACE) + geneKey;
		}
		private void SetLocalGene(string key, bool included = false)
		{
			SetLocalGene(new StylishSet(key), included);
		}
		private void SetLocalGene()
		{
			gene = null;
			geneKey = string.Empty;
			buttonGenes = string.Empty;
		}
		private void SetLocalEquipment(StylishSet essTemp, bool included = false)
		{
			equipment = essTemp;
			equipKey = equipment.key;
			equipment.size *= 100;
			equipment.offset *= 1000;
			buttonEquipments = (included ? KEY_ASTERISK : KEY_SPACE) + equipKey;
		}
		private void SetLocalEquipment(ThingWithComps eq, bool included = false)
		{
			SetLocalEquipment(eq.def.defName, included);
		}
		private void SetLocalEquipment(string key, bool included = false)
		{
			SetLocalEquipment(new StylishSet(key), included);
		}
		private void SetLocalEquipmentCommon()
		{
			NewEquipments();
			SetLocalEquipment(equipments[COMMON].Clone);
		}
		private void SetLocalEquipmentExtra1()
		{
			NewEquipments();
			SetLocalEquipment(equipments[EXTRA1].Clone);
		}
		private void SetLocalEquipmentExtra2()
		{
			NewEquipments();
			SetLocalEquipment(equipments[EXTRA2].Clone);
		}
		private void NewEquipments()
		{
			if (equipments == null) equipments = new Dictionary<string, StylishSet> { { COMMON, new StylishSet(COMMON) }, { EXTRA1, new StylishSet(EXTRA1) }, { EXTRA2, new StylishSet(EXTRA2) } };
			if (!equipments.ContainsKey(COMMON)) equipments.Add(COMMON, new StylishSet(COMMON));
			if (!equipments.ContainsKey(EXTRA1)) equipments.Add(EXTRA1, new StylishSet(EXTRA1));
			if (!equipments.ContainsKey(EXTRA2)) equipments.Add(EXTRA2, new StylishSet(EXTRA2));
		}
		private void SetLocalAddon(StylishAddon addonTemp)
		{
			addon = addonTemp;
			addon.size *= 100;
			addon.offset *= 1000;
			ChangeAddon(addon.key);
		}
		private void SetLocalAddon(int key)
		{
			ChangeAddon(key);
			if (key >= 0) SetLocalAddon(new StylishAddon(key));
		}

		private bool runOnce = false;
		public bool gameLoaded = false;
		public List<ThingDef> defs = new List<ThingDef>();
		public List<StylishModSettings> settingFiles = new List<StylishModSettings>();
		private void RunOnce()
		{
			if (!runOnce)
			{
				runOnce = true;
				gameLoaded = Find.Maps != null;
				switch (menuMode)
				{
					case MenuMode.SETTINGS:
						NewPawnStyleSet(pss.key, pss.name, pss.raceName);
						break;
					case MenuMode.SAVE:
					case MenuMode.LOAD:
						SetFileList(true);
						break;
				}
				defs = DefDatabase<ThingDef>.AllDefsListForReading;
			}
		}
		private void InitPreview()
		{
			window = new StylishPreviewWindow(previewSize);
			SetPawnList();
		}
		private void MenuModeToGlobal() => MenuModeTo(MenuMode.GLOBAL);
		private void MenuModeToSettings() => MenuModeTo(MenuMode.SETTINGS);
		private void MenuModeToSave()
		{
			SetFileList(true);
			MenuModeTo(MenuMode.SAVE);
		}
		private void MenuModeToLoad()
		{
			SetFileList(false);
			MenuModeTo(MenuMode.LOAD);
		}
		private void SetFileList(bool resetFileName)
		{
			exposer.SetFileList(resetFileName);
		}
		private void MenuModeTo(MenuMode menu) => menuMode = menu;
		private bool draw = false;
		private Vector2 previewSize = new Vector2(420, 598);
		private StylishPreviewWindow window;

		private void SetPawnList()
		{
			if (!gameLoaded || window == null) return;
			if (pss == null)
			{
				window.ListClear();
			}
			else
			{
				if (isPawn)
				{
					window.PawnList = PawnsFinder.AllMaps.Where(x => x.ThingID == pss.key).ToList();
				}
				else
				{
					window.PawnList = Find.CurrentMap.mapPawns.AllPawns.Where(x => x.def.defName == pss.key).ToList();
				}
				DrawPreview();
			}
		}
		private void DrawPreview()
		{
			if (draw && window != null && window.IsOpen)
			{
				window.Update();
				window.Draw();
			}
		}
		private void OpenPreiew()
		{
			if (window == null)
			{
				InitPreview();
			}
			if (!window.IsOpen)
			{
				window.Open();
			}
		}
		private void ClosePreview()
		{
			if (window != null)
			{
				if (window.IsOpen)
				{
					window.Close();
				}
				window = null;
			}
		}
		private void Preview(bool doOpen)
		{
			if (gameLoaded && doOpen)
			{
				OpenPreiew();
			}
			else
			{
				ClosePreview();
			}
		}
		public override void DoSettingsWindowContents(Rect rect)
		{
			RunOnce();
			StylishWidgets.StylishWidgets.Begin();
			Listing_Standard ls1 = new Listing_Standard
			{
				verticalSpacing = 0.5f,
				ColumnWidth = rect.width * 0.7f
			};
			ls1.Begin(rect);
			Text.Font = GameFont.Small;
			Preview(menuMode == MenuMode.SETTINGS && draw);
			switch (menuMode)
			{
				case MenuMode.GLOBAL:
					DrawHeader(ls1);
					ls1.Gap(24);
					DrawGlobalMenu(rect, ls1);
					Widgets.DrawLineVertical(rect.width * 0.71f, 0, rect.height);
					DrawGlobalSideMenu(ls1);
					break;

				case MenuMode.SAVE:
					exposer.RunSaveMenu(new Rect(rect.width * 0.1f, rect.y, rect.width * 0.8f, rect.height * 0.8f), LABEL_SAVE_MENU, ModSettingsExpose, MenuModeToGlobal, SavePrefix, SavePostfix, null, DeletePostfix);
					Rect bottom = new Rect(rect.width * 0.1f, rect.height * 0.8f, rect.width * 0.8f, rect.height * 0.05f);
					TooltipHandler.TipRegion(bottom, TOOLTIP_SAVELOAD_MENU.Translate());
					break;

				case MenuMode.LOAD:
					exposer.RunLoadMenu(new Rect(rect.width * 0.1f, rect.y, rect.width * 0.8f, rect.height * 0.8f), LABEL_LOAD_MENU, ModSettingsExpose, MenuModeToGlobal, null, LoadPostfix, null, DeletePostfix);
					break;

				case MenuMode.SETTINGS:

					DrawHeader(ls1);
					DrawMainSettingMenu(ls1);

					switch (selectedCategory)
					{
						case Category.Portrait:
							OpenPortraitMenu(ls1);
							break;

						case Category.Hair:
							OpenHairMenu(ls1);
							break;

						case Category.Apparels:
							OpenApparelMenu(ls1);
							break;

						case Category.Equipment:
							OpenEquipmentMenu(ls1);
							break;

						case Category.Addons:
							OpenAddonMenu(ls1);
							break;

						case Category.Genes:
							OpenGeneMenu(ls1);
							break;
						/*uooooooooooooooo
												case Category.Body:
													OpenBodyChanger(ls1);
													break;
						*/
						case Category.Optional:
							OpenOptionalMenu(ls1);
							break;
					}
					DrawSideSettingMenu(ls1);
					break;
			}
			ls1.End();
			StylishWidgets.StylishWidgets.End();
		}
		private void DrawHeader(Listing_Standard ls)
		{
			FloatMenuOption AddFloatMenu(string key, bool hasData = false, string name = null, string refKey = null)
			{
				string pre = KEY_SPACE;
				string post = string.Empty;
				if (hasData) pre = KEY_ASTERISK;
				if (refKey != null)
				{
					pre += "(Pawn) ";
					post = " - " + refKey;
				}
				else
				{
					pre += "(Race) ";
				}
				return new FloatMenuOption($"{pre}{name ?? key}{post}", delegate
				{
					MenuModeToSettings();
					isPawn = name != null;
					buttonTarget = $"{pre}{name ?? key}{post}";
					targetKey = key;
					RevertSetting();
					NewPawnStyleSet(key, name, refKey);
				});

			}
			Rect rect = ls.GetRect(30);
			if (Widgets.ButtonText(rect.LeftHalf().ContractedBy(10, 0), buttonTarget))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				List<string> included = new List<string>();
				list.Add(new FloatMenuOption(LABEL_GLOBAL_SETTINGS, delegate
				{
					buttonTarget = LABEL_GLOBAL_SETTINGS;
					MenuModeToGlobal();
				}));
				foreach (PawnStyleSet style in PawnStyleSet.Styles.Values.Where(x => x.IsRace))
				{
					if (!included.Contains(style.key))
					{
						included.Add(style.key);
						list.Add(AddFloatMenu(style.key, true));
					}
				}
				/*
				if (gameLoaded)
				{
					foreach (Map map in Find.Maps.Where(map => map.IsPlayerHome))
					{
						foreach (string raceName in map.mapPawns.AllPawns.Where(p => p.RaceProps.Humanlike).Select(p => p.def.defName))
						{
							if (!included.Contains(raceName))
							{
								included.Add(raceName);
								list.Add(AddFloatMenu(raceName));
							}
						}
					}
				}
				*/
				foreach (ThingDef_AlienRace def in DefDatabase<ThingDef_AlienRace>.AllDefs)
				{
					if (!included.Contains(def.defName))
					{
						included.Add(def.defName);
						list.Add(AddFloatMenu(def.defName));
					}
				}
				IEnumerable<PawnStyleSet> psss = PawnStyleSet.Styles.Values.Where(x => x.IsPawn);
				if (gameLoaded)
				{
					psss = psss.Where(x => PawnsFinder.AllMaps.Any(y => y.ThingID == x.key));
				}
				foreach (PawnStyleSet style in psss)
				{
					if (!included.Contains(style.key))
					{
						included.Add(style.key);
						list.Add(AddFloatMenu(style.key, true, style.name, style.raceName));
					}
				}
				if (gameLoaded)
				{
					foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsAndPrisoners)
					{
						string key = pawn.ThingID;
						if (!included.Contains(key))
						{
							included.Add(key);
							list.Add(AddFloatMenu(key, false, pawn.Name.ToStringShort, pawn.def.defName));
						}
					}
				}

				if (list.Count == 0) list.Add(AddFloatMenu(RACE_HUMAN, PawnStyleSet.ContainsKey(RACE_HUMAN)));
				Find.WindowStack.Add(new FloatMenu(list));
			}
			if (menuMode == MenuMode.SETTINGS)
			{
				rect = rect.RightHalf();
				rect.SplitVertically(rect.width * 0.7f, out Rect tempRect2, out Rect tempRect3/*, 10*/);
				TooltipHandler.TipRegion(tempRect2, TOOLTIP_COPY_PASTE.Translate());
				if (Widgets.ButtonText(tempRect2.LeftHalf().ContractedBy(10, 0), LABEL_BUTTON_COPY))
				{
					cachePss = pss.Copy();
					Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
				}
				if (cachePss != null)
				{
					if (Widgets.ButtonText(tempRect2.RightHalf().ContractedBy(10, 0), LABEL_BUTTON_PASTE))
					{
						SetStyleDataToLocalFromCache();
						SetStylePortraitDataFromCache();
						Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
						LoadHair();
						LoadApparel();
						LoadEquipment();
						LoadGene();
					}
				}
				Widgets.CheckboxLabeled(tempRect3, "Preview", ref draw);
			}
			ls.GapLine(3f);
		}
		private void DrawGlobalMenu(Rect rect, Listing_Standard ls)
		{
			Rect temp = ls.GetRect(HEIGHT_SLIDER_TEXT);
			Rect tempL = temp.LeftPart(0.485f);
			Rect tempR = temp.RightPart(0.485f);
			Widgets.Label(tempL.TopHalf(), $"{LABEL_GLOBAL_WEIGHT_CPU.Translate()}: {GetWeightLabel(weightCPU).Translate()}");
			Widgets.Label(tempR.TopHalf(), $"{LABEL_GLOBAL_WEIGHT_MEMORY.Translate()}: {GetWeightLabel(weightMemory).Translate()}");
			weightCPU = (float)Math.Round(Widgets.HorizontalSlider(tempL.BottomHalf(), weightCPU, 0, 4), 0);
			weightMemory = (float)Math.Round(Widgets.HorizontalSlider(tempR.BottomHalf(), weightMemory, 0, 4), 0);
			TooltipHandler.TipRegion(tempL, TOOLTIP_WEIGHT_CPU.Translate());
			TooltipHandler.TipRegion(tempR, TOOLTIP_WEIGHT_MEMORY.Translate());
			temp = ls.GetRect(HEIGHT_SLIDER_TEXT);
			if (Widgets.ButtonText(temp.RightPart(0.4f), LABEL_GLOBAL_AUTO_SETTING.Translate()))
			{
				Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>
						{
							new FloatMenuOption("Yes", RecommandedSettings)
						}));
			}
			TooltipHandler.TipRegion(temp.RightPart(0.4f), TOOLTIP_AUTO_SETTING.Translate());

			ls.Gap(12);

			bool tempBool = StylishAtlasManager.disableCacheColonist;
			ls.CheckboxLabeled(LABEL_GLOBAL_DISABLE_CACHE_COLONIST.Translate(), ref StylishAtlasManager.disableCacheColonist, TOOLTIP_DISABLE_CACHE_COLONIST.Translate());
			if (tempBool != StylishAtlasManager.disableCacheColonist) RefleshAll();

			ls.Gap(12);

			if (!StylishTextureAtlas.disableAdjustCache)
			{
				tempBool = StylishTextureAtlas.ignoreHeadOnly;
				ls.CheckboxLabeled(LABEL_GLOBAL_IGNORE_HEADONLY.Translate(), ref StylishTextureAtlas.ignoreHeadOnly, TOOLTIP_IGNORE_HEADONLY.Translate());
				if (tempBool != StylishTextureAtlas.ignoreHeadOnly) RefleshAll();

				ls.Gap(12);
			}
			tempBool = StylishModSettings.resolveUnfindedApparelBodytype;
			ls.CheckboxLabeled(LABEL_GLOBAL_RESOLVE_UNFINDED_BODY.Translate(), ref StylishModSettings.resolveUnfindedApparelBodytype, TOOLTIP_RESOLVE_UNFINDED_BODY.Translate());
			if (tempBool != StylishModSettings.resolveUnfindedApparelBodytype)
			{
				GraphicDatabase.Clear();
				RefleshAll();
			}

			ls.Gap(12);

			tempBool = StylishAtlasManager.forCorpseCache;
			ls.CheckboxLabeled(LABEL_GLOBAL_FOR_CORPSE_CACHE.Translate(), ref StylishAtlasManager.forCorpseCache, TOOLTIP_FOR_CORPSE_CACHE.Translate());
			if (tempBool != StylishAtlasManager.forCorpseCache) RefleshAll();

			ls.Gap(12);

			tempBool = StylishModSettings.commonizationPortrait;
			ls.CheckboxLabeled(LABEL_GLOBAL_COMMONIZATION_PORTRAIT.Translate(), ref StylishModSettings.commonizationPortrait, TOOLTIP_COMMONIZATION_PORTRAIT.Translate());
			if (tempBool != StylishModSettings.commonizationPortrait)
			{
				foreach (PawnStyleSet tempSet in PawnStyleSet.Styles.Values)
				{
					tempSet.loadedScale = false;
				}
				/*
				StylishRedefinition.RedefinePortrait();
				*/
				RefleshAll();
			}

			ls.Gap(12);

			tempBool = StylishAtlasManager.disableZoomCacheOff;
			ls.CheckboxLabeled(LABEL_GLOBAL_DISABLE_ZOOM_CACHE_OFF.Translate(), ref StylishAtlasManager.disableZoomCacheOff, TOOLTIP_DISABLE_ZOOM_CACHE_OFF.Translate());
			if (tempBool != StylishAtlasManager.disableZoomCacheOff) RefleshAll();

			ls.Gap(12);

			if (StylishAtlasManager.enableAnimalGear)
			{
				tempBool = StylishAtlasManager.disableAnimalCache;
				ls.CheckboxLabeled(LABEL_GLOBAL_DISABLE_ANIMAL_CACHE.Translate(), ref StylishAtlasManager.disableAnimalCache, TOOLTIP_DISABLE_ANIMAL_CACHE.Translate());
				if (tempBool != StylishAtlasManager.disableAnimalCache) RefleshAll();

				ls.Gap(12);
			}

			if (gameLoaded)
			{
				StylishWidgets.StylishWidgets.SetButtons(
					ls,
					32,
					new List<Action> { RemoveNotExistRace, RemoveNotExistPawn },
					new List<string> { LABEL_BUTTON_NOT_LOADED_RACE, LABEL_BUTTON_NOT_LOADED_PAWN },
					new List<bool> { true, true }
					);

				ls.Gap(12);
			}

			StylishWidgets.StylishWidgets.SetButtons(new Rect(rect.width * 0.3f, rect.height - 96, rect.width * 0.4f, 32),
				new List<Action> { MenuModeToSave, MenuModeToLoad },
				new List<string> { LABEL_BUTTON_SAVE_AS, LABEL_BUTTON_LOAD },
				new List<bool> { false, false }
				);

			/*
			if (DEBUG)
			{
				ls.Gap(24f);
				if (ls.ButtonText("Debug"))
				{
					StylishAtlasManager.SendDebug();
				}

			}
			*/
		}
		private void DrawGlobalSideMenu(Listing_Standard ls)
		{
			ls.NewColumn();
			ls.ColumnWidth *= 0.4f;
			Rect temp = ls.GetRect(HEIGHT_SLIDER_TEXT).ContractedBy(1);
			TooltipHandler.TipRegion(temp, TOOLTIP_CACHE_STATUS.Translate());
			Widgets.Label(temp.TopHalf(), "Cache status.");
			Widgets.Label(temp.BottomHalf(), $"Total cache count: {StylishAtlasManager.PawnTextureAtlases.Count}.");
			temp.yMin += 6;
			temp.yMax += 6;
			foreach (KeyValuePair<string, List<PawnTextureAtlas>> p in StylishAtlasManager.AtlasMap)
			{
				temp.yMin += HEIGHT_SLIDER_TEXT;
				temp.yMax += HEIGHT_SLIDER_TEXT;
				Widgets.Label(temp.TopHalf(), p.Key);
				if (p.Value == null || p.Value.Count == 0)
				{
					Widgets.Label(temp.BottomHalf(), LABEL_NONE);
				}
				else
				{
					List<PawnTextureAtlas> atlases = p.Value;
					int free = 0;
					int used = 0;
					atlases.ForEach(x => free += x.FreeCount);
					atlases.ForEach(x => used += (x as StylishTextureAtlas).UsedCount);
					Widgets.Label(temp.BottomHalf(), $"{atlases.First().RawTexture.width} * {atlases.First().RawTexture.height}: count[{atlases.Count}] ({used}/{used + free})");
				}
			}
		}
		private void RecommandedSettings()
		{
			StylishAutoSetting.gameLoaded = gameLoaded;
			List<StylishAutoSetting> sasList = new List<StylishAutoSetting>();
			StylishAtlasManager.disableCacheColonist = weightCPU > 0;
			StylishAtlasManager.forCorpseCache = weightMemory < 2;
			foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(x => x?.race?.Humanlike ?? false))
			{
				StylishAutoSetting sas = new StylishAutoSetting(def);
				NewPawnStyleSet(def.defName);
				sas.Apply(pss);
				pss.Apply();
				sasList.Add(sas);
			}
			RefleshAll();
			buttonTarget = LABEL_GLOBAL_SETTINGS;
			MenuModeToGlobal();
			Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
			foreach (StylishAutoSetting sas in sasList)
			{
				Log.Message(sas.Send());
			}
		}
		private void DrawMainSettingMenu(Listing_Standard ls)
		{
			ls.Gap(6f);
			StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref bodySize.x, ref bodySize.y, ref bools[0], ref bools[1], LABEL_BODY_SIZE_X, LABEL_BODY_SIZE_Y, 0, 200, 0, 0, 9999, 1, true);
			bodySize.z = bodySize.x;
			StylishWidgets.StylishWidgets.SetTextAndSliderRect(ls.GetRect(HEIGHT_SLIDER_TEXT).LeftPart(0.485f), ref headSize.y, ref bools[2], LABEL_HEAD_SIZE_Y, 0, 200, 0, 0, 9999, 1, true);

			StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref headSize.x, ref headSize.z, ref bools[3], ref bools[4], LABEL_HEAD_SIZE_X, LABEL_HEAD_SIZE_Z, 0, 200, 0, 0, 9999, 1, true);

			StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref headOffset.y, ref headOffset.x, ref bools[5], ref bools[6], LABEL_HEAD_OFFSET_Y, LABEL_HEAD_OFFSET_X, -500, 500);

			ls.GapLine(6f);

			selectedCategory = (Category)StylishWidgets.StylishWidgets.SetRadioButtons(
				ls,
				HEIGHT_SLIDER_TEXT,
				new List<string> { CAT_PORTRAIT, CAT_HAIR, CAT_APPARELS, CAT_EQUIPMENT, CAT_ADDONS, CAT_GENES, /*uoooooooooooooCAT_BODY, */CAT_OPTIONAL },
				(int)selectedCategory
				);
		}
		private void DrawSideSettingMenu(Listing_Standard ls)
		{
			ls.NewColumn();
			ls.ColumnWidth *= 0.4f;
			ls.CheckboxLabeled(LABEL_OPTIONAL_DISABLE_CACHE.Translate(), ref disableCache, TOOLTIP_DISABLE_CACHE.Translate());
			ls.GapLine(3f);
			float tempf;
			int tempi;

			PawnStyleSet pssTemp = PawnStyleSet.Get(pss.raceName);
			ls.Label($"{LABEL_BORDERSCALE_MULT.Translate()}: {borderScaleMult}", -1f, $"BorderScale: {(pssTemp != null && pssTemp.Calc != null ? pssTemp.Calc.borderScale / pssTemp.borderScaleMult * borderScaleMult : (Math.Max(headSize.x, headSize.y) + Math.Max(bodySize.x, bodySize.y) * 2) / 300 * borderScaleMult * pss.raceBorderScale)}\n(Default: {pss.raceBorderScale}){TOOLTIP_COMMON.Translate()}");
			tempf = borderScaleMult;
			borderScaleMult = (float)Math.Round(ls.Slider(borderScaleMult, 0, 4), 2);
			if (disableCache) borderScaleMult = tempf;

			ls.GapLine(0f);
			ls.Gap(1f);

			int res = resolution == 0 ? (int)Math.Round(Math.Max(Math.Max(headSize.x, headSize.y), Math.Max(bodySize.x, bodySize.y)) * pss.raceAtlasScale / 100, MidpointRounding.AwayFromZero) : resolution;
			int cacheX = 8;
			int cacheY = 8;
			for (int i = cacheSize; i > 0; i--)
			{
				if (i % 2 == 1)
				{
					cacheX /= 2;
				}
				else
				{
					cacheY /= 2;
				}
			}
			int count = StylishAtlasManager.AtlasMap.ContainsKey(pss.raceName ?? pss.key) ? StylishAtlasManager.AtlasMap[pss.raceName ?? pss.key].Count : 0;
			ls.Label($"{LABEL_CACHE_SIZE.Translate()}: {(StylishTextureAtlas.ignoreHeadOnly ? 2 : 1) * (int)Math.Max(1, Math.Pow(2, cacheSize) / 2)}{LABEL_CACHE_SIZE_AFTER.Translate()}", -1f, $"cache size: {res * 2048 / 16 * cacheWidth / cacheX} * {res * 2048 / cacheY}\n(Default: {pss.raceAtlasScale * 2048} * {pss.raceAtlasScale * 2048})\nCreated cache count for Race: {count}{TOOLTIP_COMMON.Translate()}");
			tempi = cacheSize;
			cacheSize = (int)Math.Round(ls.Slider(cacheSize, 1, 6));
			if (disableCache) cacheSize = tempi;
			ls.Gap(1f);

			ls.Label($"{LABEL_CACHE_WIDTH.Translate()}: {cacheWidth} / 16", -1f, $"{res * 128 / 16 * cacheWidth} * {res * 128}\n(Default: {pss.raceAtlasScale * 128} * {pss.raceAtlasScale * 128}\n){TOOLTIP_COMMON.Translate()}");
			tempi = cacheWidth;
			cacheWidth = (int)Math.Round(ls.Slider(cacheWidth, 1, 16));
			if (disableCache) cacheWidth = tempi;
			ls.GapLine(0f);
			ls.Gap(1f);

			ls.Label($"{LABEL_RESOLUTION.Translate()}: {(resolution == 0 ? "(Auto)" : string.Empty)}{res * 128 / 16 * cacheWidth} * {res * 128}", -1f, $"{res * 128 / 16 * cacheWidth} * {res * 128}\n(Default: {pss.raceAtlasScale * 128} * {pss.raceAtlasScale * 128}\n){TOOLTIP_COMMON.Translate()}");
			tempi = resolution;
			resolution = (int)Math.Round(ls.Slider(resolution, 0, 8));
			if (disableCache) resolution = tempi;
			ls.GapLine(0f);

			ls.Gap(20f);
			if (ls.ButtonText(LABEL_BUTTON_REVERT.Translate(), null)) RevertSetting();
			ls.Gap(20f);
			if (ls.ButtonText(LABEL_BUTTON_RESET.Translate(), null)) ResetSetting();
			ls.Gap(40f);
			if (ls.ButtonText(LABEL_BUTTON_APPLY.Translate(), null)) ApplySetting();
			ls.Gap(20f);
			if (ls.ButtonText(LABEL_BUTTON_REFLESH_ALL.Translate(), null)) RefleshAll();
			ls.GapLine(40f);
			if (ls.ButtonText(LABEL_BUTTON_REMOVE.Translate(), null))
			{
				Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>() { new FloatMenuOption("Yes", RemoveSetting) }));
			}
			ls.Gap(40f);
			if (ls.ButtonText(LABEL_BUTTON_REMOVE_ALL.Translate(), null))
			{
				Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>() { new FloatMenuOption("Yes", RemoveSettingAll) }));
			}
		}
		private void OpenPortraitMenu(Listing_Standard ls)
		{
			Rect tempRect = ls.GetRect(30f);
			if (Widgets.ButtonText(tempRect.LeftHalf(), buttonRotate))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (KeyValuePair<int, string> rot in Rot)
				{
					list.Add(new FloatMenuOption(rot.Value, delegate
					{
						pss.adjustPortraitRotation = rot.Key;
						SetButtonRotation();
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			ls.Gap(6);
			StylishWidgets.StylishWidgets.SetTextAndSlider(ls, HEIGHT_SLIDER_TEXT, ref pss.adjustPortraitSize, ref bools[10], LABEL_PORTRAIT_SIZE, 0, 500, 0, 0, 9999, 1, true, true);
			StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref pss.adjustPortraitOffset.x, ref pss.adjustPortraitOffset.y, ref bools[11], ref bools[12], LABEL_PORTRAIT_OFFSET_X, LABEL_PORTRAIT_OFFSET_Y, -500, 500, 0);
			StylishWidgets.StylishWidgets.SetTextAndSlider(ls, HEIGHT_SLIDER_TEXT, ref pss.adjustPortraitAngle, ref bools[13], LABEL_PORTRAIT_ANGLE, 0, 359, 0, 0, 359, 1, false, true, true);
			StylishWidgets.StylishWidgets.SetTextAndSlider(ls, HEIGHT_SLIDER_TEXT, ref pss.adjustPortraitHeadOffsetMult, ref bools[14], LABEL_PORTRAIT_HEAD_OFFSET_MULT, 1, 500, 0, 1, 9999, 1, true, true, false, TOOLTIP_PORTRAIT_HEAD_OFFSET_MULT);
			ls.Gap(6);
			Rect r = ls.GetRect(HEIGHT_SLIDER_TEXT);

			Widgets.CheckboxLabeled(r.LeftHalf().ContractedBy(10, 0), LABEL_PORTRAIT_DISABLE_HEADGEAR.Translate(), ref pss.adjustPortraitDisableHeadGear);
			Widgets.CheckboxLabeled(r.RightHalf().ContractedBy(10, 0), LABEL_PORTRAIT_DISABLE_CLOTHES.Translate(), ref pss.adjustPortraitDisableClothes);
		}
		private void OpenHairMenu(Listing_Standard ls)
		{
			if (hair != null)
			{
				Rect tempRect = ls.GetRect(30f);
				if (Widgets.ButtonText(tempRect.LeftHalf(), buttonHairs))
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					List<string> included = new List<string>();
					foreach (KeyValuePair<string, StylishSet> p in Hairs)
					{
						if (p.Value == null) Hairs[p.Key] = new StylishSet(p.Key);
						if (!included.Contains(p.Key))
						{
							included.Add(p.Key);
							list.Add(new FloatMenuOption(KEY_ASTERISK + p.Key, delegate
							{
								SetLocalHair(p.Value.Clone, true);
							}));
						}
					}
					foreach (HairDef hd in DefDatabase<HairDef>.AllDefs)
					{
						string modName = hd.modContentPack.Name;
						if (!included.Contains(modName))
						{
							included.Add(modName);
							list.Add(new FloatMenuOption(KEY_SPACE + modName, delegate
							{
								SetLocalHair(modName);
							}));
						}
					}
					foreach (BeardDef bd in DefDatabase<BeardDef>.AllDefs)
					{
						string modName = bd.modContentPack.Name;
						if (!included.Contains(modName))
						{
							included.Add(modName);
							list.Add(new FloatMenuOption(KEY_SPACE + modName, delegate
							{
								SetLocalHair(modName);
							}));
						}
					}
					if (list.Count == 0)
					{
						hair = null;
					}
					Find.WindowStack.Add(new FloatMenu(list));

				}
				tempRect = tempRect.RightHalf();
				if (Widgets.ButtonText(tempRect.LeftHalf().ContractedBy(10, 0), LABEL_BUTTON_COPY))
				{
					cacheHair = hair.Clone;
					Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
				}
				if (cacheHair != null)
				{
					if (Widgets.ButtonText(tempRect.RightHalf().ContractedBy(10, 0), LABEL_BUTTON_PASTE))
					{
						SetHairFromCache();
						Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
					}
				}
				ls.Gap(6);
				StylishWidgets.StylishWidgets.SetTextAndSlider(ls, HEIGHT_SLIDER_TEXT, ref hair.size.y, ref bools[10], LABEL_HAIR_SIZE_Y, 0, 200, 0, 0, 9999, 1, true, true);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref hair.size.x, ref hair.size.z, ref bools[11], ref bools[12], LABEL_HAIR_SIZE_X, LABEL_HAIR_SIZE_Z, 0, 200, 0, 0, 9999, 1, true);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref hair.offset.y, ref hair.offset.w, ref bools[13], ref bools[14], LABEL_HAIR_OFFSET_Y, LABEL_HAIR_OFFSET_W, -500, 500, 0);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref hair.offset.x, ref hair.offset.z, ref bools[15], ref bools[16], LABEL_HAIR_OFFSET_X, LABEL_HAIR_OFFSET_Z, -500, 500, 0);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref hair.angle.x, ref hair.angle.y, ref bools[17], ref bools[18], LABEL_HAIR_ANGLE_X, LABEL_HAIR_ANGLE_Y, 0, 359, 0, 0, 359, 1, false, true);
				StylishWidgets.StylishWidgets.SetTextAndSliderRect(ls.GetRect(HEIGHT_SLIDER_TEXT).LeftPart(0.485f), ref hair.priority, ref bools[19], LABEL_APPAREL_PRIORITY, -99, 99, 0, -99, 99, 1, false, false, true);
				ls.Gap(6);
				StylishWidgets.StylishWidgets.SetButtons(
					ls,
					30,
					new List<Action> { RemoveHairAll, RemoveHair, ResetHair, RevertHair, ApplyHair },
					new List<string> { LABEL_BUTTON_REMOVE_ALL, LABEL_BUTTON_REMOVE, LABEL_BUTTON_RESET, LABEL_BUTTON_REVERT, LABEL_BUTTON_APPLY },
					new List<bool> { true, true, false, false, false }
					);
			}
		}
		private void OpenApparelMenu(Listing_Standard ls)
		{
			if (apparel != null)
			{
				Rect tempRect = ls.GetRect(30f);
				if (Widgets.ButtonText(tempRect.LeftHalf(), buttonApparels))
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					List<string> included = new List<string>();
					void AddList(string key, string layer, string label)
					{
						string fullKey = $"{key} - {layer}";
						if (!included.Contains(fullKey))
						{
							included.Add(fullKey);
							bool include = ContainApparelSetting(key, layer);
							list.Add(new FloatMenuOption($"{(include ? KEY_ASTERISK : KEY_SPACE)}{fullKey}", delegate
							{
								if (include)
								{
									SetLocalApparel(GetApparelStyleSet(key, layer).Clone, include);
								}
								else
								{
									SetLocalApparel(key, layer, label);
								}
							}));
						}
					}
					foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawns.Where(p => targetKey == (isPawn ? p.ThingID : p.def.defName)))
					{
						if (pawn.apparel.WornApparel != null)
						{
							foreach (Apparel app in pawn.apparel.WornApparel)
							{
								if (app.def.apparel.wornGraphicPath == null) continue;
								ApparelLayerDef ld = StylishSet.LayerIs(app);
								if (ld == null) continue;
								string key = app.ContentSource.Name;
								string layer = ld.defName;
								string label = ld.label.Translate();
								AddList(key, StylishSet.BODY_COMMON, StylishSet.BODY_COMMON);
								AddList(key, layer, label);
							}
						}
					}
					foreach (KeyValuePair<string, ApparelDict> pair in apparels)
					{
						foreach (StylishSet ss in pair.Value.Values)
						{
							if (ss.key.NullOrEmpty()) ss.key = pair.Key;
							string fullKey = $"{ss.key} - {ss.layer}";
							if (!included.Contains(fullKey))
							{
								list.Add(new FloatMenuOption($"* {fullKey}", delegate
								{
									SetLocalApparel(ss.Clone, true);
								}));
							}
						}
					}
					if (list.Count == 0)
					{
						apparel = null;
						return;
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}

				tempRect = tempRect.RightHalf();
				if (Widgets.ButtonText(tempRect.LeftHalf().ContractedBy(10, 0), LABEL_BUTTON_COPY))
				{
					cacheApparel = apparel.Clone;
					Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
				}
				if (cacheApparel != null)
				{
					if (Widgets.ButtonText(tempRect.RightHalf().ContractedBy(10, 0), LABEL_BUTTON_PASTE))
					{
						SetApparelFromCache();
						Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
					}
				}
				ls.Gap(6);
				tempRect = ls.GetRect(HEIGHT_SLIDER_TEXT);
				StylishWidgets.StylishWidgets.SetTextAndSliderRect(tempRect.LeftPart(0.485f), ref apparel.size.y, ref bools[10], LABEL_APPAREL_SIZE_Y, 0, 200, 0, 0, 9999, 1, true);
				tempRect.RightHalf().SplitVertically(tempRect.width * 0.275f, out Rect rl, out Rect rr);
				if (!apparel.IsBodyCommon)
				{
					Widgets.CheckboxLabeled(rl.ContractedBy(10, 0), LABEL_APPAREL_HIDE.Translate(), ref apparel.hide);
					Widgets.CheckboxLabeled(rr.ContractedBy(10, 0), LABEL_NORTH_INVERT.Translate(), ref apparel.northInvert);
					TooltipHandler.TipRegion(rr, TOOLTIP_NORTH_INVERT_A.Translate());
				}
				ls.Gap(-1);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref apparel.size.x, ref apparel.size.z, ref bools[11], ref bools[12], LABEL_APPAREL_SIZE_X, LABEL_APPAREL_SIZE_Z, 0, 200, 0, 0, 9999, 1, true);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref apparel.offset.y, ref apparel.offset.w, ref bools[13], ref bools[14], LABEL_APPAREL_OFFSET_Y, LABEL_APPAREL_OFFSET_W, -500, 500, 0);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref apparel.offset.x, ref apparel.offset.z, ref bools[15], ref bools[16], LABEL_APPAREL_OFFSET_X, LABEL_APPAREL_OFFSET_Z, -500, 500, 0);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref apparel.angle.x, ref apparel.angle.y, ref bools[17], ref bools[18], LABEL_APPAREL_ANGLE_X, LABEL_APPAREL_ANGLE_Y, 0, 359, 0, 0, 359, 1, false, true);
				StylishWidgets.StylishWidgets.SetTextAndSliderRect(ls.GetRect(HEIGHT_SLIDER_TEXT).LeftPart(0.485f), ref apparel.priority, ref bools[19], LABEL_APPAREL_PRIORITY, -99, 99, 0, -99, 99, 1, false, false, true, TOOLTIP_PRIORITY);
				ls.Gap(6);
				StylishWidgets.StylishWidgets.SetButtons(
					ls,
					30,
					new List<Action> { RemoveApparelAll, RemoveApparel, ResetApparel, RevertApparel, ApplyApparel },
					new List<string> { LABEL_BUTTON_REMOVE_ALL, LABEL_BUTTON_REMOVE, LABEL_BUTTON_RESET, LABEL_BUTTON_REVERT, LABEL_BUTTON_APPLY },
					new List<bool> { true, true, false, false, false }
					);
			}
		}
		private void OpenEquipmentMenu(Listing_Standard ls)
		{
			if (equipment != null)
			{
				Rect tempRect = ls.GetRect(30f);
				if (Widgets.ButtonText(tempRect.LeftHalf(), buttonEquipments))
				{
					List<FloatMenuOption> list = new List<FloatMenuOption> {
						new FloatMenuOption($"  {COMMON}", delegate { SetLocalEquipmentCommon(); }),
						new FloatMenuOption($"  {EXTRA1}", delegate { SetLocalEquipmentExtra1(); }),
						new FloatMenuOption($"  {EXTRA2}", delegate { SetLocalEquipmentExtra2(); })
					};
					List<string> included = new List<string> { COMMON, EXTRA1, EXTRA2 };
					foreach (KeyValuePair<string, StylishSet> p in Equipments)
					{
						if (p.Value == null) Equipments[p.Key] = new StylishSet(p.Key);
						if (!included.Contains(p.Key))
						{
							included.Add(p.Key);
							list.Add(new FloatMenuOption(KEY_ASTERISK + p.Key, delegate
							{
								SetLocalEquipment(p.Value.Clone, true);
							}));
						}
					}
					foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawns.Where(p => targetKey == (isPawn ? p.ThingID : p.def.defName)))
					{
						foreach (ThingWithComps eq in pawn.equipment.AllEquipmentListForReading)
						{
							string label = eq.def.defName;
							if (!included.Contains(label))
							{
								included.Add(label);
								bool include = equipments.ContainsKey(label);
								list.Add(new FloatMenuOption($"{(include ? KEY_ASTERISK : KEY_SPACE)}{label}", delegate
								{
									if (include)
									{
										SetLocalEquipment(equipments[label].Clone, include);
									}
									else
									{
										SetLocalEquipment(label);
									}
								}));
							}
						}
					}
					if (list.Count == 0)
					{
						equipment = null;
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}

				tempRect = tempRect.RightHalf();
				if (Widgets.ButtonText(tempRect.LeftHalf().ContractedBy(10, 0), LABEL_BUTTON_COPY))
				{
					cacheEquipment = equipment.Clone;
					Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
				}
				if (cacheEquipment != null)
				{
					if (Widgets.ButtonText(tempRect.RightHalf().ContractedBy(10, 0), LABEL_BUTTON_PASTE))
					{
						SetEquipmentFromCache();
						Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
					}
				}
				ls.Gap(6);

				tempRect = ls.GetRect(HEIGHT_SLIDER_TEXT);
				StylishWidgets.StylishWidgets.SetTextAndSliderRect(tempRect.LeftPart(0.485f), ref equipment.size.y, ref bools[10], LABEL_EQUIPMENT_SIZE_Y, 0, 200, 0, 0, 9999, 1, true);
				Rect rec = tempRect.RightPart(0.485f);
				Widgets.Label(rec.TopHalf(), LABEL_EQUIPMENT_FLIPS.Translate());
				rec.BottomHalf().LeftPart(0.55f).SplitVertically(rec.width * 0.275f, out Rect bottomLL, out Rect bottomLR);
				Rect bottomRight = rec.BottomHalf().RightPart(0.45f);
				Widgets.CheckboxLabeled(bottomLL.LeftHalf().LeftPart(0.9f), LABEL_EQUIPMENT_ROT_W.Translate(), ref equipment.flipSet[3]);
				Widgets.CheckboxLabeled(bottomLL.RightHalf().LeftPart(0.9f), LABEL_EQUIPMENT_ROT_N.Translate(), ref equipment.flipSet[0]);
				Widgets.CheckboxLabeled(bottomLR.LeftHalf().LeftPart(0.9f), LABEL_EQUIPMENT_ROT_S.Translate(), ref equipment.flipSet[2]);
				Widgets.CheckboxLabeled(bottomLR.RightHalf().LeftPart(0.9f), LABEL_EQUIPMENT_ROT_E.Translate(), ref equipment.flipSet[1]);
				Widgets.CheckboxLabeled(bottomRight.ContractedBy(10, 0), LABEL_NORTH_INVERT.Translate(), ref equipment.northInvert);
				TooltipHandler.TipRegion(bottomRight, TOOLTIP_NORTH_INVERT_E.Translate());
				ls.Gap(-1);

				if (equipment.key.Contains("ExtraPart"))
				{
					StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref equipment.size.x, ref equipment.size.z, ref bools[11], ref bools[12], LABEL_EQUIPMENT_SIZE_X, LABEL_EQUIPMENT_SIZE_Z, 0, 200, 0, 0, 9999, 1, true);
					StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref equipment.offset.y, ref equipment.priority, ref bools[13], ref bools[14], LABEL_EQUIPMENT_OFFSET_Y, LABEL_EQUIPMENT_PRIORITY, -500, 500, 0, -9999, 9999, 10, -999, 999, 0, -999, 999, 1, false, false, false, true);
				}
				else
				{
					tempRect = ls.GetRect(HEIGHT_SLIDER_TEXT);
					StylishWidgets.StylishWidgets.SetTextAndSliderRect(tempRect.LeftPart(0.485f), ref equipment.size.x, ref bools[11], LABEL_EQUIPMENT_SIZE_XZ, 0, 200, 0, 0, 9999, 1, true);
					equipment.size.z = equipment.size.x;
					if (equipment.key != COMMON)
					{
						tempRect = tempRect.RightPart(0.485f);
						tempRect.position += new Vector2(0, HEIGHT_SLIDER_TEXT / 2);
						Widgets.CheckboxLabeled(tempRect, LABEL_EQUIPMENT_USE_COMMON.Translate(), ref equipment.useCommonWhenAiming);
						TooltipHandler.TipRegion(tempRect, TOOLTIP_USE_COMMON.Translate());
					}
					ls.Gap(-1);
					StylishWidgets.StylishWidgets.SetTextAndSliderRect(ls.GetRect(HEIGHT_SLIDER_TEXT).LeftPart(0.485f), ref equipment.offset.y, ref bools[13], LABEL_EQUIPMENT_OFFSET_Y, -500, 500, 0, -9999, 9999, 10, false, false, true);
					ls.Gap(-1);
				}
				equipment.offset.w = equipment.offset.y;

				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref equipment.offset.x, ref equipment.offset.z, ref bools[15], ref bools[16], LABEL_EQUIPMENT_OFFSET_X, LABEL_EQUIPMENT_OFFSET_Z, -2500, 2500, 0);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref equipment.angle.x, ref equipment.angle.y, ref bools[17], ref bools[18], LABEL_EQUIPMENT_ANGLE_X, LABEL_EQUIPMENT_ANGLE_Y, 0, 359, 0, 0, 359, 1, false, true);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref equipment.angle.z, ref equipment.angle.w, ref bools[19], ref bools[20], LABEL_EQUIPMENT_ANGLE_Z, LABEL_EQUIPMENT_ANGLE_W, 0, 359, 0, 0, 359, 1, false, true);
				ls.Gap(6);
				StylishWidgets.StylishWidgets.SetButtons(
					ls,
					30,
					new List<Action> { RemoveEquipmentAll, RemoveEquipment, ResetEquipment, RevertEquipment, ApplyEquipment },
					new List<string> { LABEL_BUTTON_REMOVE_ALL, LABEL_BUTTON_REMOVE, LABEL_BUTTON_RESET, LABEL_BUTTON_REVERT, LABEL_BUTTON_APPLY },
					new List<bool> { true, true, false, false, false }
					);
			}
		}
		private void OpenAddonMenu(Listing_Standard ls)
		{
			if (!inGame) return;
			Rect tempRect = ls.GetRect(30f);
			if (Widgets.ButtonText(tempRect.LeftHalf(), buttonAddons))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>
				{
					new FloatMenuOption(ADDON_HEAD, delegate
					{
						ChangeAddon(-2);
					}),
					new FloatMenuOption(ADDON_BODY, delegate
					{
						ChangeAddon(-1);
					})
				};
				List<int> included = new List<int>();
				foreach (KeyValuePair<int, StylishAddon> p in addons)
				{
					if (p.Value == null) addons[p.Key] = new StylishAddon(p.Key);
					if (!included.Contains(p.Key))
					{
						included.Add(p.Key);
						list.Add(new FloatMenuOption(GetAddonLabel(p.Value), delegate
						{
							SetLocalAddon(p.Value.Clone);
						}));
					}
				}
				if (list.Count == 0)
				{
					addon = null;
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			ls.Gap(6);
			switch (selectedAddon)
			{
				case -2:
					StylishWidgets.StylishWidgets.SetTextAndSlider(ls, HEIGHT_SLIDER_TEXT, ref headAddonSize.y, ref bools[10], LABEL_HEAD_ADDON_SIZE_Y, 0, 200, 0, 0, 9999, 1, true, true);

					StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref headAddonSize.x, ref headAddonSize.z, ref bools[11], ref bools[12], LABEL_HEAD_ADDON_SIZE_X, LABEL_HEAD_ADDON_SIZE_Z, 0, 200, 0, 0, 9999, 1, true);

					StylishWidgets.StylishWidgets.SetTextAndSlider(ls, HEIGHT_SLIDER_TEXT, ref headAddonOffset.y, ref bools[13], LABEL_HEAD_ADDON_OFFSET_Y, -500, 500, 0, -9999, 9999, 10, false, true);

					StylishWidgets.StylishWidgets.SetTextAndSlider(ls, HEIGHT_SLIDER_TEXT, ref headAddonOffset.x, ref bools[14], LABEL_HEAD_ADDON_OFFSET_X, -500, 500, 0, -9999, 9999, 10, false, true);
					break;
				case -1:
					StylishWidgets.StylishWidgets.SetTextAndSlider(ls, HEIGHT_SLIDER_TEXT, ref bodyAddonSize.y, ref bools[10], LABEL_BODY_ADDON_SIZE_Y, 0, 200, 0, 0, 9999, 1, true, true);

					StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref bodyAddonSize.x, ref bodyAddonSize.z, ref bools[11], ref bools[12], LABEL_BODY_ADDON_SIZE_X, LABEL_BODY_ADDON_SIZE_Z, 0, 200, 0, 0, 9999, 1, true);

					StylishWidgets.StylishWidgets.SetTextAndSlider(ls, HEIGHT_SLIDER_TEXT, ref bodyAddonOffset.y, ref bools[13], LABEL_BODY_ADDON_OFFSET_Y, -500, 500, 0, -9999, 9999, 10, false, true);

					StylishWidgets.StylishWidgets.SetTextAndSlider(ls, HEIGHT_SLIDER_TEXT, ref bodyAddonOffset.x, ref bools[14], LABEL_BODY_ADDON_OFFSET_X, -500, 500, 0, -9999, 9999, 10, false, true);
					break;
				default:
					if (addon != null)
					{
						Widgets.CheckboxLabeled(tempRect.RightPart(0.4f).LeftPart(0.4f), LABEL_ADDON_HIDE.Translate(), ref addon.hide);
						if (!addon.alignWithHead)
						{
							bool f = addon.isHead;
							Widgets.CheckboxLabeled(tempRect.RightPart(0.4f).RightPart(0.4f), LABEL_ADDON_IS_HEAD.Translate(), ref addon.isHead);
							if (f != addon.isHead)
							{
								SetButtonAddon();
							}
						}
						StylishWidgets.StylishWidgets.SetTextAndSlider(ls, HEIGHT_SLIDER_TEXT, ref addon.size.y, ref bools[10], LABEL_ADDON_SIZE_Y, 0, 200, 0, 0, 9999, 1, true, true);

						StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref addon.size.x, ref addon.size.z, ref bools[11], ref bools[12], LABEL_ADDON_SIZE_X, LABEL_ADDON_SIZE_Z, 0, 200, 0, 0, 9999, 1, true);

						StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref addon.offset.y, ref addon.offset.w, ref bools[13], ref bools[14], LABEL_ADDON_OFFSET_Y, LABEL_ADDON_OFFSET_W, -500, 500, 0);

						StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref addon.offset.x, ref addon.offset.z, ref bools[15], ref bools[16], LABEL_ADDON_OFFSET_X, LABEL_ADDON_OFFSET_Z, -500, 500, 0);

						StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref addon.angle.x, ref addon.angle.y, ref bools[17], ref bools[18], LABEL_ADDON_ANGLE_X, LABEL_ADDON_ANGLE_Y, 0, 359, 0, 0, 359, 1, false, true);
					}
					break;
			}
			if (selectedAddon < 0 || addon != null)
			{
				ls.Gap(6);
				StylishWidgets.StylishWidgets.SetButtons(
					ls,
					30,
					new List<Action> { ResetAddonAll, ResetAddon, RevertAddon, ApplyAddon },
					new List<string> { LABEL_BUTTON_RESET_ALL, LABEL_BUTTON_RESET, LABEL_BUTTON_REVERT, LABEL_BUTTON_APPLY },
					new List<bool> { true, false, false, false }
					);
			}

		}
		private void OpenGeneMenu(Listing_Standard ls)
		{
			if (gene != null)
			{
				Rect tempRect = ls.GetRect(30f);
				if (Widgets.ButtonText(tempRect.LeftHalf(), buttonGenes))
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					List<string> included = new List<string>();
					foreach (KeyValuePair<string, StylishSet> p in Genes)
					{
						if (p.Value == null) Genes[p.Key] = new StylishSet(p.Key);
						if (!included.Contains(p.Key))
						{
							included.Add(p.Key);
							list.Add(new FloatMenuOption(KEY_ASTERISK + p.Key, delegate
							{
								SetLocalGene(p.Value.Clone, true);
							}));
						}
					}
					foreach (GeneDef gd in DefDatabase<GeneDef>.AllDefs)
					{
						if (gd.HasGraphic && !gd.graphicData.drawOnEyes)
						{
							string key = gd.defName;
							if (!included.Contains(key))
							{
								included.Add(key);
								list.Add(new FloatMenuOption(KEY_SPACE + key, delegate
								{
									SetLocalGene(key);
								}));
							}
						}
					}
					if (list.Count == 0)
					{
						gene = null;
					}
					Find.WindowStack.Add(new FloatMenu(list));

				}
				tempRect = tempRect.RightHalf();
				if (Widgets.ButtonText(tempRect.LeftHalf().ContractedBy(10, 0), LABEL_BUTTON_COPY))
				{
					cacheGene = gene.Clone;
					Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
				}
				if (cacheGene != null)
				{
					if (Widgets.ButtonText(tempRect.RightHalf().ContractedBy(10, 0), LABEL_BUTTON_PASTE))
					{
						SetGeneFromCache();
						Verse.Sound.SoundStarter.PlayOneShotOnCamera(MessageTypeDefOf.PositiveEvent.sound);
					}
				}
				ls.Gap(6);
				tempRect = ls.GetRect(HEIGHT_SLIDER_TEXT);
				StylishWidgets.StylishWidgets.SetTextAndSliderRect(tempRect.LeftPart(0.485f), ref gene.size.y, ref bools[10], LABEL_GENE_SIZE_Y, 0, 200, 0, 0, 9999, 1, true);
				tempRect.RightHalf().SplitVertically(tempRect.width * 0.275f, out Rect rl, out Rect rr);
				Widgets.CheckboxLabeled(rl.ContractedBy(10, 0), LABEL_GENE_HIDE.Translate(), ref gene.hide);
				ls.Gap(-1);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref gene.size.x, ref gene.size.z, ref bools[11], ref bools[12], LABEL_GENE_SIZE_X, LABEL_GENE_SIZE_Z, 0, 200, 0, 0, 9999, 1, true);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref gene.offset.y, ref gene.offset.w, ref bools[13], ref bools[14], LABEL_GENE_OFFSET_Y, LABEL_GENE_OFFSET_W, -500, 500, 0);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref gene.offset.x, ref gene.offset.z, ref bools[15], ref bools[16], LABEL_GENE_OFFSET_X, LABEL_GENE_OFFSET_Z, -500, 500, 0);
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref gene.angle.x, ref gene.angle.y, ref bools[17], ref bools[18], LABEL_GENE_ANGLE_X, LABEL_GENE_ANGLE_Y, 0, 359, 0, 0, 359, 1, false, true);
				StylishWidgets.StylishWidgets.SetTextAndSliderRect(ls.GetRect(HEIGHT_SLIDER_TEXT).LeftPart(0.485f), ref gene.priority, ref bools[19], LABEL_APPAREL_PRIORITY, -99, 99, 0, -99, 99, 1, false, false, true);
				ls.Gap(6);
				StylishWidgets.StylishWidgets.SetButtons(
					ls,
					30,
					new List<Action> { RemoveGeneAll, RemoveGene, ResetGene, RevertGene, ApplyGene },
					new List<string> { LABEL_BUTTON_REMOVE_ALL, LABEL_BUTTON_REMOVE, LABEL_BUTTON_RESET, LABEL_BUTTON_REVERT, LABEL_BUTTON_APPLY },
					new List<bool> { true, true, false, false, false }
					);
			}
		}
		private void OpenBodyChanger(Listing_Standard ls)
		{
			if (!inGame) return;
			Rect tempRect = ls.GetRect(30f);
			if (Widgets.ButtonText(tempRect.LeftHalf(), buttonBodyChanger))
			{
				Find.WindowStack.Add(new FloatMenu(CallBodyChangerMenu()));
			}
			if (!changeBodyRaceName.NullOrEmpty())
			{
				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref changeBodySize.y, ref changeBodyOffset.y, ref bools[10], ref bools[13], LABEL_CHANGER_BODY_SIZE_Y, LABEL_CHANGER_BODY_OFFSET_Y, 0, 200, 0, 0, 9999, 1, -500, 500, 0, -9999, 9999, 10, true, false, false, false, TOOLTIP_CHANGER_ONLY_BODY, TOOLTIP_CHANGER_ONLY_BODY);

				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref changeBodySize.x, ref changeBodyOffset.x, ref bools[11], ref bools[14], LABEL_CHANGER_BODY_SIZE_X, LABEL_CHANGER_BODY_OFFSET_X, 0, 200, 0, 0, 9999, 1, -500, 500, 0, -9999, 9999, 10, true, false, false, false, TOOLTIP_CHANGER_ONLY_BODY, TOOLTIP_CHANGER_NO_INVERT);

				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref changeBodySize.z, ref changeBodyOffset.z, ref bools[12], ref bools[15], LABEL_CHANGER_BODY_SIZE_Z, LABEL_CHANGER_BODY_OFFSET_Z, 0, 200, 0, 0, 9999, 1, -500, 500, 0, -9999, 9999, 10, true, false, false, false, TOOLTIP_CHANGER_ONLY_BODY, TOOLTIP_CHANGER_ONLY_BODY);

				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref changeBodyAngle.x, ref changeBodyAngle.y, ref bools[16], ref bools[17], LABEL_CHANGER_BODY_ANGLE_X, LABEL_CHANGER_BODY_ANGLE_Y, 0, 359, 0, 0, 359, 1, false, true, TOOLTIP_CHANGER_ONLY_BODY);

				StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref changeBodyColor.x, ref changeBodyColor.y, ref bools[18], ref bools[19], LABEL_CHANGER_BODY_HUE, LABEL_CHANGER_BODY_SATURATION, 0, 255, 0, 0, 255, 1, -255, 255, 0, -255, 255, 1, false, false, true, false, TOOLTIP_CHANGER_ONLY_BODY, TOOLTIP_CHANGER_ONLY_BODY);

				StylishWidgets.StylishWidgets.SetTextAndSlider(ls, HEIGHT_SLIDER_TEXT, ref changeBodyColor.z, ref bools[20], LABEL_CHANGER_BODY_VALUE, -255, 255, 0, -255, 255, 1, false, true, false, TOOLTIP_CHANGER_ONLY_BODY);
			}
		}
		private void OpenOptionalMenu(Listing_Standard ls)
		{
			if (inGame)
			{
				ls.Gap(6f);
				Rect tempRect = ls.GetRect(30f);
				if (!isPawn)
				{
					if (Widgets.ButtonText(tempRect.LeftHalf(), selectedBodyType))
					{
						Find.WindowStack.Add(new FloatMenu(CallBodyTypeMenu()));
					}
					ls.Gap(6f);
					correction = GetCorrection(selectedBodyType);
					StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref correction.y, ref correction.z, ref bools[10], ref bools[11], LABEL_HEAD_CORRECT_Y, LABEL_HEAD_CORRECT_Z, -500, 500);
					corrections[selectedBodyType] = correction;
					ls.Gap(6f);
					drawLocCorrection = GetDrawLocCorrection(selectedBodyType);
					StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref drawLocCorrection.y, ref drawLocCorrection.z, ref bools[12], ref bools[13], LABEL_BODY_CORRECT_Y, LABEL_BODY_CORRECT_Z, -500, 500);
					drawLocCorrections[selectedBodyType] = drawLocCorrection;
				}
                else
				{
					StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref drawLoc.y, ref drawLoc.x, ref bools[10], ref bools[11], LABEL_BODY_CORRECT_PAWN_Y, LABEL_BODY_CORRECT_PAWN_Z, -500, 500);
				}
			}
			ls.Gap(18f);
			StylishWidgets.StylishWidgets.SetTextAndSliderDouble(ls, HEIGHT_SLIDER_TEXT, ref headOffsetVerticalDiff.x, ref headOffsetVerticalDiff.y, ref bools[14], ref bools[15], LABEL_HEAD_OFFSET_VERTICAL_SIDE, LABEL_HEAD_OFFSET_VERTICAL_BACK, -500, 500);
			ls.Gap(6f);
			ls.CheckboxLabeled(LABEL_OPTIONAL_IGNORE_RESTRICT.Translate(), ref ignoreRaceRestriction, TOOLTIP_IGNORE_RESTRICT.Translate() + TOOLTIP_COMMON.Translate());
			ls.Gap(6f);
			ls.CheckboxLabeled(LABEL_OPTIONAL_LEAVE_CACHE.Translate(), ref leaveOneCache, TOOLTIP_LEAVE_CACHE.Translate() + TOOLTIP_COMMON.Translate());
			ls.Gap(6f);
			ls.CheckboxLabeled(LABEL_OPTIONAL_UTILITY_ADJUST.Translate(), ref disableUtilityAdjust, TOOLTIP_UTILITY_ADJUST.Translate());

			if (!pss.AlienDef.alienRace.styleSettings[typeof(HairDef)].hasStyle)
			{
				ls.Gap(6f);
				ls.CheckboxLabeled(LABEL_HAIR_SHOW.Translate(), ref showHair, TOOLTIP_HAIR_SHOW.Translate());
			}
		}
		private List<FloatMenuOption> CallBodyTypeMenu()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<BodyTypeDef> bd = pss.AlienDef.alienRace.generalSettings.alienPartGenerator.bodyTypes;
			if (bd.Count == 0) bd = PawnStyleSet.HumanDef.alienRace.generalSettings.alienPartGenerator.bodyTypes;
			foreach (BodyTypeDef bt in bd)
			{
				list.Add(new FloatMenuOption(bt.defName, delegate
				{
					selectedBodyType = bt.defName;
				}));
			}
			return list;
		}
		private List<FloatMenuOption> CallBodyChangerMenu()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<string> included = new List<string>();
			if (isPawn)
			{
				CallBodyChangerMenuAtPawn();
			}
			else
			{
				CallBodyChangerMenuAtRace();
			}
			return list;
			void CallBodyChangerMenuAtPawn()
			{
				FloatMenuOption addDefault()
				{
					return new FloatMenuOption($"{pss.raceName} - {pss.defaultBodyTypeDefName}{LABEL_DEFAULT}", delegate
					{
						buttonBodyChanger = $"{pss.raceName} - {pss.defaultBodyTypeDefName}{LABEL_DEFAULT}";
						ResetBodyChanger();
					});
				}
				FloatMenuOption addDef(ThingDef_AlienRace def, BodyTypeDef body)
				{
					return new FloatMenuOption($"{def.defName} - {body.defName}", delegate
					{
						buttonBodyChanger = $"{def.defName} - {body.defName}";
						changeBodyRaceName = def.defName;
						changeBodyTypeName = body.defName;
					});
				}
				FloatMenuOption addCustom(string folder, BodyTypeDef body)
				{
					return new FloatMenuOption($"{folder} - {body.defName}", delegate
					{
						buttonBodyChanger = $"{folder} - {body.defName}";
						changeBodyRaceName = LABEL_STYLISH + folder;
						changeBodyTypeName = body.defName;
					});
				}

				included.Add($"{pss.raceName} - {pss.defaultBodyTypeDefName}");
				if (!changeBodyRaceName.NullOrEmpty())
				{
					if (changeBodyRaceName.Length > 7 && changeBodyRaceName.Substring(0, 7) == LABEL_STYLISH)
					{
						foreach (string folder in StylishBodyChanger.GetCustomFolderPaths())
						{
							if (folder == changeBodyRaceName.Substring(7))
							{
								foreach (BodyTypeDef b in StylishBodyChanger.ContainBodyTypes(folder))
								{
									if (b.defName == changeBodyTypeName)
									{
										included.Add($"{folder} - {b.defName}");
										list.Add(addCustom(folder, b));
									}
								}
							}
						}
					}
					else
					{
						if (defs.Where(x => x.defName == changeBodyRaceName).FirstOrDefault() is ThingDef_AlienRace def)
						{
							foreach (BodyTypeDef body in def.alienRace.generalSettings.alienPartGenerator.bodyTypes)
							{
								if (body.defName == changeBodyRaceName)
								{
									included.Add($"{def.defName} - {body.defName}");
									list.Add(addDef(def, body));
								}
							}
						}
						else
						{
							ResetBodyChanger();
						}
					}
				}
				list.Add(addDefault());

				foreach (ThingDef def in defs)
				{
					if (def is ThingDef_AlienRace d)
					{
						foreach (BodyTypeDef body in d.alienRace.generalSettings.alienPartGenerator.bodyTypes)
						{
							if (!included.Contains($"{d.defName} - {body.defName}"))
							{
								included.Add($"{d.defName} - {body.defName}");
								list.Add(addDef(d, body));
							}
						}
					}
				}
				foreach (string folder in StylishBodyChanger.GetCustomFolderPaths())
				{
					foreach (BodyTypeDef b in StylishBodyChanger.ContainBodyTypes(folder))
					{
						if (!included.Contains($"{folder} - {b.defName}"))
						{
							included.Add($"{folder} - {b.defName}");
							list.Add(addCustom(folder, b));
						}
					}
				}
			}

			void CallBodyChangerMenuAtRace()
			{
				FloatMenuOption addDefault()
				{
					return new FloatMenuOption($"{pss.key}{LABEL_DEFAULT}", delegate
					{
						buttonBodyChanger = $"{pss.key}{LABEL_DEFAULT}";
						ResetBodyChanger();
					});
				}
				FloatMenuOption addDef(ThingDef_AlienRace def)
				{
					return new FloatMenuOption(def.defName, delegate
					{
						buttonBodyChanger = def.defName;
						changeBodyRaceName = def.defName;
						changeBodyTypeName = null;
					});
				}
				FloatMenuOption addCustom(string folder)
				{
					return new FloatMenuOption(folder, delegate
					{
						buttonBodyChanger = folder;
						changeBodyRaceName = LABEL_STYLISH + folder;
						changeBodyTypeName = null;
					});
				}

				included.Add(pss.key);
				if (!changeBodyRaceName.NullOrEmpty())
				{
					if (changeBodyRaceName.Length > 7 && changeBodyRaceName.Substring(0, 7) == LABEL_STYLISH)
					{
						foreach (string folder in StylishBodyChanger.GetCustomFolderPaths())
						{
							if (folder == changeBodyRaceName.Substring(7))
							{
								included.Add(folder);
								list.Add(addCustom(folder));
							}
						}
					}
					else
					{
						ThingDef def = defs.Where(x => x.defName == changeBodyRaceName).FirstOrDefault();
						if (def != null)
						{
							included.Add(def.defName);
							list.Add(addDef(def as ThingDef_AlienRace));
						}
						else
						{
							ResetBodyChanger();
						}
					}
				}
				list.Add(addDefault());

				foreach (BodyTypeDef body in pss.AlienDef.alienRace.generalSettings.alienPartGenerator.bodyTypes)
				{
					foreach (ThingDef def in defs)
					{
						if ((def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.bodyTypes.Contains(body) ?? false)
						{
							if (!included.Contains(def.defName))
							{
								included.Add(def.defName);
								list.Add(addDef(def as ThingDef_AlienRace));
							}
						}
					}
					foreach (string folder in StylishBodyChanger.GetCustomFolderPaths())
					{
						if (!included.Contains(folder))
						{
							if (StylishBodyChanger.ContainBodyTypes(folder).Contains(body))
							{
								included.Add(folder);
								list.Add(addCustom(folder));
							}
						}
					}
				}
			}
		}

		private void SetStyleDataToLocal(bool fromCache = false) => SetStyleDataToLocal(pss, fromCache);
		private void SetStyleDataToLocalFromCache() => SetStyleDataToLocal(cachePss, true);
		private void SetStyleDataToLocal(PawnStyleSet pss, bool fromCache)
		{
			bodySize = pss.adjustBodySize * 100;
			headSize = pss.adjustHeadSize * 100;
			headOffset = pss.adjustHeadOffset * 1000;
			headOffsetVerticalDiff = pss.adjustHeadOffsetVerticalDiff * 1000;

			hairs = pss.CopyHairs();
			apparels = pss.CopyApparels();
			equipments = pss.CopyEquipments();
			genes = pss.CopyGenes();
			if (!fromCache)
			{

				bodyAddonSize = pss.adjustBodyAddonSize * 100;
				headAddonSize = pss.adjustHeadAddonSize * 100;
				bodyAddonOffset = pss.adjustBodyAddonOffset * 1000;
				headAddonOffset = pss.adjustHeadAddonOffset * 1000;

				addons = pss.Addons;

				disableUtilityAdjust = pss.disableUtilityAdjust;
				disableCache = pss.disableCache;

				changeBodyRaceName = pss.changeBodyRaceName;
				changeBodyTypeName = pss.changeBodyTypeName;
				changeBodySize = pss.adjustChangeBodySize * 100;
				changeBodyOffset = pss.adjustChangeBodyOffset * 1000;
				changeBodyAngle = pss.adjustChangeBodyAngle;
				changeBodyColor = pss.adjustChangeBodyColor;
				showHair = pss.showHair;
				if (isPawn)
				{
					PawnStyleSet pssRace = PawnStyleSet.Get(pss.raceName);
					ignoreRaceRestriction = pssRace?.ignoreRaceRestriction ?? pss.ignoreRaceRestriction;
					leaveOneCache = pssRace?.leaveOneCache ?? pss.leaveOneCache;
					borderScaleMult = pssRace?.borderScaleMult ?? pss.borderScaleMult;
					cacheSize = pssRace?.cacheSize ?? pss.cacheSize;
					cacheWidth = pssRace?.cacheWidth ?? pss.cacheWidth;
					resolution = pssRace?.resolution ?? pss.resolution;
					drawLoc = pss.adjustDrawLoc;
				}
				else
				{
					ignoreRaceRestriction = pss.ignoreRaceRestriction;
					leaveOneCache = pss.leaveOneCache;
					borderScaleMult = pss.borderScaleMult;
					cacheSize = pss.cacheSize;
					cacheWidth = pss.cacheWidth;
					resolution = pss.resolution;
					corrections = pss.Corrections;
					drawLocCorrections = pss.DrawLocCorrections;
				}
			}
		}
		private void SetStylePortraitDataToLocal()
		{
			portraitOffset = pss.adjustPortraitOffset;
			portraitRotation = pss.adjustPortraitRotation;
			portraitSize = pss.adjustPortraitSize;
			portraitAngle = pss.adjustPortraitAngle;
			portraitHeadOffsetMult = pss.adjustPortraitHeadOffsetMult;
			portraitDisableHeadgear = pss.adjustPortraitDisableHeadGear;
			portraitDisableClothes = pss.adjustPortraitDisableClothes;
		}
		private void SetStyleDataFromLocal()
		{
			pss.adjustBodySize = bodySize / 100;
			pss.adjustHeadSize = headSize / 100;
			pss.adjustHeadOffset = headOffset / 1000;
			pss.adjustHeadOffsetVerticalDiff = headOffsetVerticalDiff / 1000;

			pss.Hairs = Hairs;
			pss.Apparels = Apparels;
			pss.Equipments = Equipments;
			pss.Genes = Genes;
			pss.Addons = Addons;

			pss.disableUtilityAdjust = disableUtilityAdjust;
			pss.disableCache = disableCache;

			pss.adjustDrawLoc = drawLoc;
			pss.changeBodyRaceName = changeBodyRaceName;
			pss.changeBodyTypeName = changeBodyTypeName;
			pss.adjustChangeBodySize = changeBodySize / 100;
			pss.adjustChangeBodyOffset = changeBodyOffset / 1000;
			pss.adjustChangeBodyAngle = changeBodyAngle;
			pss.adjustChangeBodyColor = changeBodyColor;

			pss.showHair = showHair;

			if (!isPawn)
			{
				pss.ignoreRaceRestriction = ignoreRaceRestriction;
				pss.leaveOneCache = leaveOneCache;
				pss.borderScaleMult = borderScaleMult;
				pss.cacheWidth = cacheWidth;
				pss.cacheSize = cacheSize;
				pss.resolution = resolution;
				pss.corrections = corrections;
				pss.drawLocCorrections = drawLocCorrections;
			}
		}
		private void SetStylePortraitDataFromCache()
		{
			pss.adjustDrawLoc = cachePss.adjustDrawLoc;
			pss.adjustPortraitOffset = cachePss.adjustPortraitOffset;
			pss.adjustPortraitRotation = cachePss.adjustPortraitRotation;
			pss.adjustPortraitSize = cachePss.adjustPortraitSize;
			pss.adjustPortraitAngle = cachePss.adjustPortraitAngle;
			pss.adjustPortraitHeadOffsetMult = cachePss.adjustPortraitHeadOffsetMult;
			pss.adjustPortraitDisableHeadGear = cachePss.adjustPortraitDisableHeadGear;
			pss.adjustPortraitDisableClothes = cachePss.adjustPortraitDisableClothes;
		}
		private void SetStylePortraitDataFromLocal()
		{
			pss.adjustPortraitOffset = portraitOffset;
			pss.adjustPortraitRotation = portraitRotation;
			pss.adjustPortraitSize = portraitSize;
			pss.adjustPortraitAngle = portraitAngle;
			pss.adjustPortraitHeadOffsetMult = portraitHeadOffsetMult;
			pss.adjustPortraitDisableHeadGear = portraitDisableHeadgear;
			pss.adjustPortraitDisableClothes = portraitDisableClothes;
		}
		private void RevertSetting()
		{
			SetStyleDataToLocal();
			SetStylePortraitDataFromLocal();
			SetButtonRotation();
		}
		private void SetHairFromCache()
		{
			SetStylishSetFromCache(hair, cacheHair);
		}
		private void SetGeneFromCache()
		{
			SetStylishSetFromCache(gene, cacheGene);
			gene.priority = cacheGene.priority;
			gene.hide = cacheGene.hide;
		}
		private void SetApparelFromCache()
		{
			SetStylishSetFromCache(apparel, cacheApparel);
			apparel.priority = cacheApparel.priority;
			apparel.hide = cacheApparel.hide;
			apparel.northInvert = cacheApparel.northInvert;
		}
		private void SetEquipmentFromCache()
		{
			SetStylishSetFromCache(equipment, cacheEquipment);
			equipment.northInvert = cacheEquipment.northInvert;
			equipment.flipSet = new bool[4];
			if (cacheEquipment.flipSet != null)
			{
				equipment.flipSet[0] = cacheEquipment.flipSet[0];
				equipment.flipSet[1] = cacheEquipment.flipSet[1];
				equipment.flipSet[2] = cacheEquipment.flipSet[2];
				equipment.flipSet[3] = cacheEquipment.flipSet[3];
			}
			if (!equipment.key.Contains("ExtraPart"))
			{
				equipment.size.z = equipment.size.x;
				equipment.priority = 0;
				if (equipment.key != COMMON && cacheEquipment.key != COMMON && !cacheEquipment.key.Contains("ExtraPart"))
				{
					equipment.useCommonWhenAiming = cacheEquipment.useCommonWhenAiming;
				}
			}
		}
		private void SetStylishSetFromCache(StylishSet to, StylishSet from)
		{
			to.size = from.size;
			to.offset = from.offset;
			to.angle = from.angle;
		}
		private void LoadEquipment()
		{
			NewEquipments();
			SetLocalEquipmentCommon();
		}
		private void LoadHair()
		{
			if (!gameLoaded)
			{
				hair = null;
				return;
			}
			if (hairs == null) hairs = new Dictionary<string, StylishSet>();

			Pawn pawn = null;
			if (isPawn)
			{
				foreach (Pawn p in PawnsFinder.AllMaps.Where(x => x.ThingID == targetKey))
				{
					pawn = p;
				}
				if (pawn == null)
				{
					SetLocalHair("Core");
				}
				else
				{
					pss.name = pawn.Name.ToStringShort;
					string mod = pawn.story.hairDef.modContentPack.Name;
					if (Hairs.Count > 0 && Hairs.ContainsKey(mod))
					{
						SetLocalHair(Hairs[mod].Clone, true);
					}
					else
					{
						SetLocalHair(mod);
					}
				}
			}
			else
			{
				if (Hairs.Count > 0)
				{
					SetLocalHair(Hairs.First().Value.Clone, true);
				}
				else
				{
					SetLocalHair("Core");
				}
			}
		}
		private void LoadGene()
		{
			if (!gameLoaded || !ModsConfig.BiotechActive)
			{
				gene = null;
				return;
			}
			if (genes == null) genes = new Dictionary<string, StylishSet>();

			foreach (GeneDef gd in DefDatabase<GeneDef>.AllDefs)
            {
				if (gd.HasGraphic && !gd.graphicData.drawOnEyes)
                {
					SetLocalGene(gd.defName);
					return;
                }
            }
		}
		private void LoadApparel()
		{
			if (!gameLoaded)
			{
				apparel = null;
				return;
			}
			if (apparels == null) apparels = new Dictionary<string, ApparelDict>();

			Pawn pawn = null;
			if (isPawn)
			{
				foreach (Pawn p in PawnsFinder.AllMaps.Where(x => x.ThingID == targetKey))
				{
					pawn = p;
				}
				if (pawn == null)
				{
					SetLocalApparel();
				}
				else
				{
					foreach (Apparel app in pawn.apparel.WornApparel)
					{
						if (app.def.apparel.wornGraphicPath == null) continue;
						StylishSet ass = GetApparelStyleSet(app);
						if (ass != null)
						{
							SetLocalApparel(ass.Clone, true);
						}
						else
						{
							SetLocalApparel(app);
						}
						break;
					}
				}
			}
			else
			{
				if (Apparels.Count > 0 && Apparels.First().Value.Count > 0)
				{
					SetLocalApparel(Apparels.First().Value.Values[0].Clone, true);
				}
				else
				{
					foreach (Pawn p in Find.CurrentMap.mapPawns.AllPawns.Where(x => x.def.defName == targetKey))
					{
						pawn = p;
					}
					if (pawn == null)
					{
						SetLocalApparel();
					}
					else
					{
						foreach (Apparel app in pawn.apparel.WornApparel)
						{
							if (app.def.apparel.wornGraphicPath == null) continue;
							SetLocalApparel(app);
							break;
						}
					}
				}
			}
		}
		private void RevertEquipment()
		{
			if (Equipments.ContainsKey(equipKey))
			{
				SetLocalEquipment(Equipments[equipKey].Clone, true);
			}
			else
			{
				SetLocalEquipment(equipKey);
			}
		}
		private void RevertHair()
		{
			if (Hairs.ContainsKey(hairKey))
			{
				SetLocalHair(Hairs[hairKey].Clone, true);
			}
			else
			{
				SetLocalHair(hairKey);
			}
		}
		private void RevertGene()
		{
			if (Genes.ContainsKey(geneKey))
			{
				SetLocalGene(Genes[geneKey].Clone, true);
			}
			else
			{
				SetLocalGene(geneKey);
			}
		}
		private void ResetEquipment()
		{
			if (equipment == null) return;
			SetLocalEquipment(equipment.key, Equipments.ContainsKey(equipment.key));
		}
		private void ResetHair()
		{
			if (hair == null) return;
			SetLocalHair(hair.key, Hairs.ContainsKey(hair.key));
		}
		private void ResetGene()
		{
			if (gene == null) return;
			SetLocalGene(gene.key, Genes.ContainsKey(gene.key));
		}
		private void RemoveEquipment()
		{
			if (equipment == null) return;
			if (equipments.ContainsKey(equipKey))
			{
				equipments.Remove(equipKey);
			}
			if (pss.Equipments.ContainsKey(equipKey))
			{
				pss.Equipments.Remove(equipKey);
			}
			SetLocalEquipment(equipKey);
			Apply();
		}
		private void RemoveHair()
		{
			if (hair == null) return;
			if (hairs.ContainsKey(hairKey))
			{
				hairs.Remove(hairKey);
			}
			if (pss.Hairs.ContainsKey(hairKey))
			{
				pss.Hairs.Remove(hairKey);
			}
			SetLocalHair(hairKey);
			Apply();
		}
		private void RemoveGene()
		{
			if (gene == null) return;
			if (genes.ContainsKey(geneKey))
			{
				genes.Remove(geneKey);
			}
			if (pss.Genes.ContainsKey(geneKey))
			{
				pss.Genes.Remove(geneKey);
			}
			SetLocalGene(geneKey);
			Apply();
		}
		private void RemoveEquipmentAll()
		{
			equipments = null;
			pss.Equipments = null;
			NewEquipments();
			pss.NewEquipments();
			SetLocalEquipment(equipKey);
			Apply();
		}
		private void RemoveHairAll()
		{
			hairs = new Dictionary<string, StylishSet>();
			pss.Hairs = new Dictionary<string, StylishSet>();
			SetLocalHair(hairKey);
			Apply();
		}
		private void RemoveGeneAll()
		{
			genes = new Dictionary<string, StylishSet>();
			pss.Genes = new Dictionary<string, StylishSet>();
			SetLocalGene(geneKey);
			Apply();
		}
		private void ApplyEquipment()
		{
			Verse.Sound.SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOn);
			if (equipKey.NullOrEmpty()) return;
			equipment.size /= 100;
			equipment.offset /= 1000;
			if (pss.Equipments.ContainsKey(equipment.key))
			{
				pss.Equipments[equipment.key] = equipment.Clone;
			}
			else
			{
				pss.Equipments.Add(equipment.key, equipment.Clone);
			}
			equipments = pss.Equipments;
			SetLocalEquipment(equipment, equipment.key != COMMON && equipment.key != EXTRA1 && equipment.key != EXTRA2);
			Apply();
		}
		private void ApplyHair()
		{
			Verse.Sound.SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOn);
			hair.size /= 100;
			hair.offset /= 1000;
			if (pss.Hairs.ContainsKey(hair.key))
			{
				pss.Hairs[hair.key] = hair.Clone;
			}
			else
			{
				pss.Hairs.Add(hair.key, hair.Clone);
			}
			hairs = pss.Hairs;
			SetLocalHair(hair, true);
			Apply();
		}
		private void ApplyGene()
		{
			Verse.Sound.SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOn);
			gene.size /= 100;
			gene.offset /= 1000;
			if (pss.Genes.ContainsKey(gene.key))
			{
				pss.Genes[gene.key] = gene.Clone;
			}
			else
			{
				pss.Genes.Add(gene.key, gene.Clone);
			}
			genes = pss.Genes;
			SetLocalGene(gene, true);
			Apply();
		}
		private void ApplyApparel()
		{
			Verse.Sound.SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOn);
			if (EmptyApparel()) return;
			apparel.size /= 100;
			apparel.offset /= 1000;
			if (pss.Apparels == null) pss.Apparels = new Dictionary<string, ApparelDict>();
			if (!pss.Apparels.ContainsKey(apparel.key))
			{
				pss.Apparels.Add(apparel.key, new ApparelDict());
			}
			if (pss.Apparels[apparel.key].ContainsKey(apparel.layer))
			{
				pss.Apparels[apparel.key][apparel.layer] = apparel.Clone;
			}
			else
			{
				pss.Apparels[apparel.key].Add(apparel.layer, apparel.Clone);
			}
			apparels = pss.Apparels;
			SetLocalApparel(apparel, true);
			Apply();
		}
		private void RemoveApparel()
		{
			if (EmptyApparel()) return;
			RemoveApparelStyleSet(apparel.key, apparel.layer);
			pss.RemoveApparelStyleSet(apparel.key, apparel.layer);
			SetLocalApparel(apparel.key, apparel.layer, apparel.label);
			Apply();
		}
		private void RemoveApparelAll()
		{
			if (EmptyApparel()) return;
			apparels = new Dictionary<string, ApparelDict>();
			pss.Apparels = new Dictionary<string, ApparelDict>();
			SetLocalApparel(apparel.key, apparel.layer, apparel.label);
			Apply();
		}
		private void RevertApparel()
		{
			if (EmptyApparel()) return;
			SetLocalApparel(apparelTemp.Clone, ContainApparelSetting(apparel.key, apparel.layer));
		}
		private void ResetApparel()
		{
			if (EmptyApparel()) return;
			SetLocalApparel(apparel.key, apparel.layer, apparel.label, ContainApparelSetting(apparel.key, apparel.layer));
		}
		private void ApplyAddon()
		{
			Verse.Sound.SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOn);
			switch (selectedAddon)
			{
				case -2:
					pss.adjustHeadAddonSize = headAddonSize / 100;
					pss.adjustHeadAddonOffset = headAddonOffset / 1000;
					break;
				case -1:
					pss.adjustBodyAddonSize = bodyAddonSize / 100;
					pss.adjustBodyAddonOffset = bodyAddonOffset / 1000;
					break;
				default:
					addon.size /= 100;
					addon.offset /= 1000;
					if (pss.Addons.ContainsKey(selectedAddon))
					{
						pss.Addons[selectedAddon] = addon.Clone;
					}
					else
					{
						pss.Addons.Add(selectedAddon, addon.Clone);
					}
					addons = pss.Addons;
					SetLocalAddon(addon);
					break;
			}

			Apply();
		}
		private void RevertAddon()
		{
			switch (selectedAddon)
			{
				case -2:
					headAddonSize = pss.adjustHeadAddonSize * 100;
					headAddonOffset = pss.adjustHeadAddonOffset * 1000;
					break;
				case -1:
					headAddonSize = pss.adjustBodyAddonSize * 100;
					headAddonOffset = pss.adjustBodyAddonOffset * 1000;
					break;
				default:
					addon = addons[selectedAddon].Clone;
					addon.size *= 100;
					addon.offset *= 1000;
					break;
			}
		}
		private void ResetAddon()
		{
			switch (selectedAddon)
			{
				case -2:
					headAddonSize = Vector3.one * 100;
					headAddonOffset = Vector2.zero;
					break;
				case -1:
					bodyAddonSize = Vector3.one * 100;
					bodyAddonOffset = Vector2.zero;
					break;
				default:
					addon.size = Vector3.one * 100;
					addon.offset = Vector4.zero;
					addon.angle = Vector2.zero;
					break;
			}
		}
		private void ResetAddonAll()
		{
			ChangeAddon(-2);
			headAddonSize = Vector3.one * 100;
			headAddonOffset = Vector2.zero;
			bodyAddonSize = Vector3.one * 100;
			bodyAddonOffset = Vector2.zero;
			addons = null;
			InitAddon();
			pss.Addons = addons;
			Apply();
		}
		private void Rebuild()
		{
			//StylishAdjuster.RemoveMeshSet(pss.key, pss.IsPawn);
			StylishAtlasManager.TargetRaceRebuild(pss.raceName ?? pss.key);
		}
		private void ResetSetting()
		{
			bodySize = Vector3.one * 100;
			headSize = Vector3.one * 100;
			headOffset = Vector2.zero;
			headOffsetVerticalDiff = Vector2.zero;

			borderScaleMult = 1f;
			cacheSize = 6;
			resolution = 0;

			pss.adjustDrawLoc = Vector2.zero;
			changeBodySize = Vector3.one * 100;
			changeBodyOffset = Vector3.zero;
			changeBodyAngle = Vector2.zero;
			changeBodyColor = Vector3.zero;

			pss.adjustPortraitOffset = Vector2.zero;
			pss.adjustPortraitRotation = 2;
			pss.adjustPortraitSize = 100f;
			pss.adjustPortraitDisableHeadGear = false;
			pss.adjustPortraitDisableClothes = false;
			pss.adjustPortraitAngle = 0f;
			pss.adjustPortraitHeadOffsetMult = 100;

			cacheWidth = 16;
			disableUtilityAdjust = false;
			ignoreRaceRestriction = false;
			leaveOneCache = false;
			disableCache = false;

			showHair = false;
			corrections = new Dictionary<string, Vector3>();
			drawLocCorrections = new Dictionary<string, Vector3>();
			ResetBodyChanger(true);

			SetButtonRotation();
		}
		private void RemoveSetting()
		{
			StylishAdjuster.RemoveAtlasScaleRecord(pss.raceName ?? targetKey);
			StylishAdjuster.RemoveBorderScaleRecord(pss.raceName ?? targetKey);
			StylishAtlasManager.RemoveDisableCache(targetKey);

			string name = pss.name;
			string refKey = pss.raceName;
			pss.ClearApparelCache();
			pss.Remove();

			buttonTarget = buttonTarget.Replace(KEY_ASTERISK, KEY_SPACE);
			NewPawnStyleSet(targetKey, name, refKey);

			Rebuild();
		}
		private void RemoveSettingAll()
		{
			string name = pss.name;
			string refKey = pss.raceName;

			List<string> list = PawnStyleSet.Styles.Keys.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				//StylishAdjuster.RemoveMeshSet(list[i], true);
				PawnStyleSet.Remove(list[i]);
			}
			StylishAdjuster.pawnAtlasScale.Clear();
			StylishAdjuster.pawnBorderScale.Clear();

			buttonTarget = buttonTarget.Replace(KEY_ASTERISK, KEY_SPACE);
			NewPawnStyleSet(targetKey, name, refKey);

			StylishAtlasManager.ClearDisableCacheList();
			StylishAtlasManager.AddDisableCachColonist();
			StylishAtlasManager.ForceRebuild();
			//TargetRebuild();
		}
		private void ApplySetting()
		{
			Verse.Sound.SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOn);
			SetStyleDataFromLocal();
			SetStylePortraitDataToLocal();

			pss.key = targetKey;

			if (isPawn)
			{
				PawnStyleSet pssRace = PawnStyleSet.Get(pss.raceName);
				if (pssRace != null)
				{
					pssRace.ignoreRaceRestriction = ignoreRaceRestriction;
					pssRace.leaveOneCache = leaveOneCache;
					pssRace.borderScaleMult = borderScaleMult;
					pssRace.cacheSize = cacheSize;
					pssRace.resolution = resolution;
					pssRace.cacheWidth = cacheWidth;
					pssRace.Apply();
				}
			}
			Apply();
			buttonTarget = buttonTarget.Replace(KEY_SPACE, KEY_ASTERISK);
		}
		private void Apply()
		{
			pss.Apply();
			Rebuild();

			DrawPreview();
		}
		public static void RefleshAll()
		{
			StylishAdjuster.pawnAtlasScale.Clear();
			StylishAdjuster.pawnBorderScale.Clear();

			PawnStyleSet.CalculateAll();
			/*
			List<string> list = PawnStyleSet.Styles.Keys.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				StylishAdjuster.RemoveMeshSet(list[i], true);
			}
			*/

			StylishAtlasManager.ClearDisableCacheList();
			StylishAtlasManager.AddDisableCachColonist();
			StylishAtlasManager.ForceRebuild();
		}


		private void NewPawnStyleSet(string key, string name = null, string refKey = null)
		{

			if (PawnStyleSet.ContainsKey(key))
			{
				pss = PawnStyleSet.Get(key);
			}
			else
			{
				pss = new PawnStyleSet();
			}

			if (!pss.Calculated) pss.CalculateValue();

			foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.defName == (refKey ?? key)))
			{
				pss.Def = def;
				break;
			}
			inGame = pss.Def != null;
			pss.key = key;
			pss.name = name;
			pss.raceName = refKey;

			SetStyleDataToLocal();
			SetStylePortraitDataToLocal();
			SetButtonRotation();
			pss.LoadScale();
			LoadHair();
			LoadApparel();
			LoadEquipment();
			LoadGene();
			ChangeAddon(-2);
			if (isPawn)
			{
				//pss.pawn = PawnsFinder.AllMaps.Where(x => x.ThingID == key).FirstOrDefault();
				pss.pawn = PawnsFinder.AllMaps.Where(x => x.ThingID == key).FirstOrDefault();
				if (pss.defaultBodyTypeDefName.NullOrEmpty())
				{
					pss.defaultBodyTypeDefName = pss.pawn?.story?.bodyType?.defName ?? string.Empty;
				}
			}

			SetBodyTypeButton();
			//SetChangeBodyButton();

			SetPawnList();
		}
		private void SetBodyTypeButton()
		{
			selectedBodyType = pss.AlienDef?.alienRace?.generalSettings?.alienPartGenerator?.bodyTypes?.FirstOrDefault()?.defName ?? PawnStyleSet.HumanDef.alienRace.generalSettings.alienPartGenerator.bodyTypes.FirstOrDefault().defName;
		}
		private void SetChangeBodyButton()
		{
			List<string> included = new List<string>();
			if (isPawn)
			{
				if (!changeBodyRaceName.NullOrEmpty())
				{
					if (changeBodyRaceName.Length > 7 && changeBodyRaceName.Substring(0, 7) == LABEL_STYLISH)
					{
						foreach (string folder in StylishBodyChanger.GetCustomFolderPaths())
						{
							if (folder == changeBodyRaceName.Substring(7))
							{
								if (StylishBodyChanger.ContainBodyTypes(folder).Any(x => x.defName == changeBodyTypeName))
								{
									buttonBodyChanger = $"{changeBodyRaceName.Substring(7)} - {changeBodyTypeName}";
									//SetShaderButton();
									return;
								}
							}
						}
					}
					else
					{
						ThingDef def = defs.Where(x => x.defName == changeBodyRaceName).FirstOrDefault();
						if (def != null)
						{
							foreach (BodyTypeDef body in (def as ThingDef_AlienRace).alienRace.generalSettings.alienPartGenerator.bodyTypes)
							{
								if (body.defName == changeBodyTypeName)
								{
									buttonBodyChanger = $"{def.defName} - {body.defName}";
									//SetShaderButton();
									return;
								}
							}
						}
					}
					ResetBodyChanger();
				}
				buttonBodyChanger = $"{pss.raceName} - {pss.defaultBodyTypeDefName}{LABEL_DEFAULT}";
				//SetShaderButton();
			}
			else
			{
				if (!changeBodyRaceName.NullOrEmpty())
				{
					if (changeBodyRaceName.Length > 7 && changeBodyRaceName.Substring(0, 7) == LABEL_STYLISH)
					{
						foreach (string folder in StylishBodyChanger.GetCustomFolderPaths())
						{
							if (folder == changeBodyRaceName.Substring(7))
							{
								buttonBodyChanger = changeBodyRaceName.Substring(7);
								//SetShaderButton();
								return;
							}
						}
					}
					else
					{
						ThingDef def = defs.Where(x => x.defName == changeBodyRaceName).FirstOrDefault();
						if (def != null)
						{
							buttonBodyChanger = def.defName;
							//SetShaderButton();
							return;
						}
					}
					changeBodyRaceName = null;
				}
				buttonBodyChanger = $"{pss.raceName ?? pss.key}{LABEL_DEFAULT}";
				//SetShaderButton();
			}
		}
		private void ResetBodyChanger(bool resetValue = false)
		{
			changeBodyRaceName = null;
			changeBodyTypeName = null;
			//changeShaderName = null;
			pss.changer = null;
			if (resetValue)
			{
				changeBodySize = Vector3.one * 100;
				changeBodyOffset = Vector3.zero;
				changeBodyAngle = Vector2.zero;
				changeBodyColor = Vector3.zero;
			}
		}

		private void SetButtonRotation()
		{
			buttonRotate = LABEL_ROTATE + Rot[pss.adjustPortraitRotation];
		}
		private void ChangeAddon(int i)
		{
			selectedAddon = i;
			SetButtonAddon();
		}
		private void SetButtonAddon()
		{
			buttonAddons = GetAddonLabel();
		}
		private string GetAddonLabel()
		{
			switch (selectedAddon)
			{
				case -2: return ADDON_HEAD;
				case -1: return ADDON_BODY;
				default:
					if (addon == null)
					{
						InitAddon();
					}
					return GetAddonLabel(addon);
			}
		}
		private string GetAddonLabel(StylishAddon sa)
		{
			return $"{(sa.isHead ? sa.isHair ? LABEL_HAIR : LABEL_HEAD : LABEL_BODY)} {LABEL_INDEX}{sa.key}.";
		}
		private void InitAddon()
		{
			if (addons == null)
			{
				StylishAddon.SetUpAddon(ref addons, pss.AlienDef);
			}
			if (addons.ContainsKey(selectedAddon))
			{
				SetLocalAddon(addons[selectedAddon]);
			}
			else
			{
				SetLocalAddon(selectedAddon);
			}
		}
	}
}
