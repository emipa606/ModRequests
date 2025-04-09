using Verse;
using HugsLib;
using HugsLib.Settings;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace PneumaticTubes
{
    /// <summary>
    /// Rules to add mod settings that determine what pipe networks can be created
    /// </summary>
    public class ModSettings : ModBase
	{
		/// <summary>
		/// The mod category in the mod setting file
		/// </summary>
		public override string ModIdentifier => PipeStuff.ModIdentifier;

		/// <summary>
		/// The mod settings for each of the categories
		/// </summary>
		private static Dictionary<ThingCategoryDef, SettingHandle<bool>> CategoryList = new Dictionary<ThingCategoryDef, SettingHandle<bool>>();

		/// <summary>
		/// The mod settings for each of the thingdefs
		/// </summary>
		private static Dictionary<ThingDef, SettingHandle<bool>> ThingList = new Dictionary<ThingDef, SettingHandle<bool>>();

		private static ModSettings Instance;

		private static IEnumerable<ThingCategoryDef> RealizedAllowedThingCategories;

		/// <summary>
		/// The enabled categories as per the mod settings
		/// </summary>
		public static IEnumerable<ThingCategoryDef> GetAllowedThingCategories()
        {
			if (Instance == null)
			{
				Instance = new ModSettings();
				Instance.Initialize();

				RealizedAllowedThingCategories = CategoryList.Where(e => e.Value).Select(e => e.Key).OrderBy(t => t.childCategories.Count);
			}
			return RealizedAllowedThingCategories;
		}

		public static bool IsAllowedThing(ThingDef t)
		{
			///////
			/// New
			///////
			///


			return t.thingCategories != null
				&& GetAllowedThingCategories().SharesElementWith(t.thingCategories)
				&& ThingList.ContainsKey(t)
				&& ThingList[t].Value
				&& t.thingCategories.Intersect(GetAllowedThingCategories()).Count() == t.thingCategories.Count();

			///////
			/// end New
			///////

			//return t.thingCategories != null && GetAllowedThingCategories().SharesElementWith(t.thingCategories) && t.thingCategories.Intersect(GetAllowedThingCategories()).Count() == t.thingCategories.Intersect(PotentialAllowedThingCategories).Count();
		}

		/// <summary>
		/// Default Categories to allow pipe networks to be created from
		/// </summary>
		private static List<ThingCategoryDef> DefaultAllowedThingCategories = new List<ThingCategoryDef>()
		{
			ThingCategoryDefOf.StoneBlocks,
			ThingCategoryDefOf.ResourcesRaw,
			ThingCategoryDefOf.Leathers,
			ThingCategoryDefOf.Wools,
			ThingCategoryDefOf.Textiles,
		};

		/// <summary>
		/// Default thing def names to not allow pipe networks to be created from
		/// </summary>
		private static List<string> DefaultDisallowedThingNames = new List<string>()
		{
			ThingDefOf.Chemfuel.defName,
			"VCHE_Deepchem"
		};

		/// <summary>
		/// The list of categories to allow enabling via the mod settings. Explicitly set rather than pulled from def dictionary.
		/// </summary>
		//private static List<ThingCategoryDef> PotentialAllowedThingCategories = new List<ThingCategoryDef>()
		//{
		//	ThingCategoryDefOf.Drugs,
		//	ThingCategoryDefOf.MeatRaw,
		//	ThingCategoryDefOf.Medicine,
		//	ThingCategoryDefOf.PlantFoodRaw,
		//	ThingCategoryDefOf.PlantMatter,
		//	ThingCategoryDefOf.StoneBlocks,
		//	ThingCategoryDefOf.ResourcesRaw,
		//	ThingCategoryDefOf.Leathers,
		//	ThingCategoryDefOf.Wools,
		//	ThingCategoryDefOf.Textiles,
		//	ThingCategoryDefOf.Chunks,
		//	ThingCategoryDefOf.Manufactured,
		//};

		public static Dictionary<ThingCategoryDef, HashSet<ThingDef>> dictionary = new Dictionary<ThingCategoryDef, HashSet<ThingDef>>();

		/// <summary>
		/// Load in the pipe network settings
		/// </summary>
		public override void Initialize()
        {
			Log.Warning($"PNEUMATIC TUBES: HugsLib Initialize() begins");


			///////
			/// New
			///////
			//Dictionary<ThingCategoryDef, HashSet<ThingDef>> dictionary = new Dictionary<ThingCategoryDef, HashSet<ThingDef>>();
			HashSet<ThingDef> addedThings = new HashSet<ThingDef>();

			IEnumerable<ThingDef> potentialThings = DefDatabase<ThingDef>.AllDefs.Where(t => PipeStuff.IsCandidateForPipeNetwork(t));

			Log.Warning($"PNEUMATIC TUBES: {potentialThings.Count()} potential things");

			foreach (ThingDef thingDef in potentialThings)
			{
				if (addedThings.Contains(thingDef))
                {
					continue;
                }

				var allCategories = thingDef.thingCategories.OrderBy(t => t.childCategories.Count);

				if (allCategories.Any() == false)
                {
					Log.Warning($"PNEUMATIC TUBES: out of interest, thingdef with label '{thingDef.label}' and defname '{thingDef.defName}' was the bad guy: doesn't have any thingcategories");
					continue;
                }					

				ThingCategoryDef category = allCategories.First();

				if (!dictionary.ContainsKey(category))
                {
					dictionary.Add(category, new HashSet<ThingDef>());
				}

				dictionary[category].Add(thingDef);
				addedThings.Add(thingDef);
			}

			Log.Warning($"PNEUMATIC TUBES: {dictionary.Keys.Count()} potential categories");

			foreach (ThingCategoryDef categoryDef in dictionary.Keys.OrderBy(c => ModSettings.GetExpandedCategoryName(c)))
			{
				if (!ModSettings.CategoryList.ContainsKey(categoryDef))
				{
					ModSettings.CategoryList[categoryDef] = Settings.GetHandle<bool>(
						$"Category_{categoryDef.defName}",
						"* " + "StandardizedExtrusions.EnableCategory_Title".Translate(ModSettings.GetExpandedCategoryName(categoryDef)),
						"StandardizedExtrusions.EnableCategory_Description".Translate(),
						ModSettings.DefaultAllowedThingCategories.Contains(categoryDef));

					ModSettings.CategoryList[categoryDef].CustomDrawerHeight = 100;

					foreach (ThingDef thingDef in dictionary[categoryDef].OrderBy(t => t.label))
					{
						if (!ModSettings.ThingList.ContainsKey(thingDef))
						{
							ModSettings.ThingList[thingDef] = Settings.GetHandle<bool>(
								$"Category_{thingDef.defName}",
								"    - " + "StandardizedExtrusions.EnableThing_Title".Translate(thingDef.label),
								"StandardizedExtrusions.EnableThing_Description".Translate(),
								!ModSettings.DefaultDisallowedThingNames.Contains(thingDef.defName));
						}

						ModSettings.ThingList[thingDef].CustomDrawerHeight = 50;
					}
				}
			}

			///////
			/// end New
			///////

			//foreach (ThingCategoryDef categoryDef in ModSettings.PotentialAllowedThingCategories)
			//{
			//	if (!ModSettings.CategoryList.ContainsKey(categoryDef))
			//	{
			//		ModSettings.CategoryList[categoryDef] = Settings.GetHandle<bool>(
			//			$"Category_{categoryDef.defName}",
			//			"StandardizedExtrusions.EnableCategory_Title".Translate(categoryDef.label),
			//			"StandardizedExtrusions.EnableCategory_Description".Translate(),
			//			ModSettings.DefaultAllowedThingCategories.Contains(categoryDef));
			//	}
			//}
		}

		public static string GetExpandedCategoryName(ThingCategoryDef c)
		{
			if (c == null)
			{
				return string.Empty;
			}

			if (c.parent == null || c.parent == ThingCategoryDefOf.Root)
            {
				return c.label;
            }

			return $"{ModSettings.GetExpandedCategoryName(c.parent)} > {c.label}";
		}

        public override void DefsLoaded()
        {
            base.DefsLoaded();
			PipeStuff.AdjustVatCapacity();

			foreach (ThingDef thing in DefDatabase<ThingDef>.AllDefs.Where(t => t.defName.StartsWith(PipeStuff.ModIdentifier)))
            {
				PipeStuff.FinalizeDef(thing, typeof(ThingDef));
            }

		}
    }
}
