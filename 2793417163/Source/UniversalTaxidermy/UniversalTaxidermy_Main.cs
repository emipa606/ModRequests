using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using EstupendoBase;
using HarmonyLib;

namespace UniversalTaxidermy
{
	[DefOf]
	public static class DefOfs
	{
		public static ThingDef				TaxidermyMount;
		public static SpecialThingFilterDef SpecialThingFilterDef_TaxidermyMount;
		public static ThingCategoryDef      ThingCategory_TaxidermyMount;
	}

	internal class UniversalTaxidermyMod : Mod
	{
		private static List<SpecialThingFilterDef> TaxidermyFilters = new List<SpecialThingFilterDef>();
		public static Harmony HarmonyInstance;

		public UniversalTaxidermyMod(ModContentPack content) : base(content)
		{
			HarmonyInstance = new Harmony("elmonoestupendo.universaltaxidermy");
			HarmonyInstance.PatchAll();
			Log.Message("Loaded Universal Taxidermy");
		}

		public static void DefsLoaded()
		{
			AddSpecialFilterDefs();
		}

		private static void AddSpecialFilterDefs()
		{
			foreach( PawnKindDef pawn_kind in DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef pkd) => !pkd.race.race.Humanlike).OrderBy((PawnKindDef pkd) => pkd.label) )
			{
				AddSpecialFilterDef(pawn_kind.label.AsTitle(), pawn_kind);
			}

			AddSpecialFilterDef("Colonist", PawnKindDefOf.Colonist);
			AddSpecialFilterDef("NonColonist", PawnKindDefOf.Beggar);

			DefOfs.ThingCategory_TaxidermyMount.childSpecialFilters.Remove(DefOfs.SpecialThingFilterDef_TaxidermyMount);
		}

		private static void AddSpecialFilterDef(string new_name, PawnKindDef pawn_kind)
		{
			SpecialThingFilterDef   Baseline            = DefOfs.SpecialThingFilterDef_TaxidermyMount;
			ThingCategoryDef		TaxidermyCategory	= DefOfs.ThingCategory_TaxidermyMount;
			SpecialThingFilterDef	newDef				= new SpecialThingFilterDef();

			if( DefDatabase<SpecialThingFilterDef>.AllDefs.Any((SpecialThingFilterDef stfd) => stfd.label == Baseline.label.Replace("ANIMAL", new_name)) ) return;

			newDef.label = Baseline.label.Replace("ANIMAL", new_name);
			newDef.description = Baseline.description.Replace("ANIMAL", new_name);
			newDef.defName = Baseline.defName + pawn_kind.label.ToUpper().Replace(" ", "");
			newDef.workerClass = Baseline.workerClass;
			newDef.parentCategory = Baseline.parentCategory;
			newDef.saveKey = Baseline.saveKey + pawn_kind.label.ToUpper().Replace(" ", "");
			( (SpecialThingFilterWorker_TaxidermyMount)newDef.Worker ).PawnKind = pawn_kind;

			TaxidermyFilters.Add(newDef);
			TaxidermyCategory.childSpecialFilters.Add(newDef);
			DefDatabase<SpecialThingFilterDef>.Add(newDef);
		}
	}

	[StaticConstructorOnStartup]
	internal static class StaticInitializer
	{
		static StaticInitializer()
		{
			LongEventHandler.QueueLongEvent(new System.Action(UniversalTaxidermyMod.DefsLoaded), "Loading Taxidermy Defs", true, null);
		}
	}

	public class ThingDef_TaxidermyMount : ThingDef
	{
	}


}
