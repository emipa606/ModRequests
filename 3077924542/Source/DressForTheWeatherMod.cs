using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DressForTheWeather
{
    public class DressForTheWeatherMod : Mod
    {
	    private static List<SpecialThingFilterDef>? specialThingFilterDefs;

	    private static List<SpecialThingFilterDef> SpecialThingFilterDefs {
		    get {
				if(specialThingFilterDefs is not null) return specialThingFilterDefs;
				specialThingFilterDefs = new List<SpecialThingFilterDef>() {
					SpecialThingFilterDefOf.AllowDeadmansApparel,
					SpecialThingFilterDefOf.AllowNonDeadmansApparel,
					SpecialThingFilterDefOf.AllowFresh,
					SpecialThingFilterDef.Named("AllowSmeltableApparel"),
					SpecialThingFilterDef.Named("AllowNonSmeltableApparel"),
					SpecialThingFilterDef.Named("AllowBurnableApparel"),
					SpecialThingFilterDef.Named("AllowNonBurnableApparel"),
					SpecialThingFilterDef.Named("AllowBiocodedApparel"),
					SpecialThingFilterDef.Named("AllowNonBiocodedApparel")
				};
				return specialThingFilterDefs;
            }
	    }

        private static ThingFilter? apparelGlobalFilter;

	    private static ThingFilter ApparelGlobalFilter {
		    get {
			    if (apparelGlobalFilter is not null) return apparelGlobalFilter;

			    apparelGlobalFilter = new ThingFilter();
			    apparelGlobalFilter.SetAllow(ThingCategoryDefOf.Apparel, true);
			    apparelGlobalFilter.allowedHitPointsConfigurable = false;
			    apparelGlobalFilter.allowedQualitiesConfigurable = false;
			    apparelGlobalFilter.DisplayRootCategory = new TreeNode_ThingCategory(ThingCategoryDefOf.Apparel);
			    return apparelGlobalFilter;
		    }
	    }

        private static DressForTheWeatherSettings? settings;
        private static DressForTheWeatherSettings Settings => settings ??= LoadedModManager.GetMod<DressForTheWeatherMod>().GetSettings<DressForTheWeatherSettings>();

        private readonly ThingFilterUI.UIState apparelThingFilterState = new() ;
		private readonly ThingFilterUI.UIState replaceableThingFilterState = new();

        public DressForTheWeatherMod(ModContentPack content) : base(content)
        {

            Harmony harmony = new("DanielWedemeyer.DressForTheWeather");
            harmony.PatchAll();

        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            ApparelGlobalFilter.allowedQualitiesConfigurable = false;
            Widgets.Label(inRect with { width = inRect.width / 2, height = 24 }, "DressForTheWeather_Raiders".Translate());
            Widgets.Checkbox(new Vector2(inRect.x + inRect.width - 24, inRect.y), ref Settings.RaidersComePrepared);

            Rect rect4 = new Rect(0.0f, inRect.y + 40f, inRect.width / 2, inRect.height - 40f);
            
            Widgets.BeginGroup(rect4);
			Widgets.Label(new Rect(0.0f, 0.0f, rect4.width, 24f), "DressForTheWeather_ApparelFilter".Translate());
			
			ThingFilterUI.DoThingFilterConfigWindow(new Rect(0, 40, 300f, (float)(rect4.height - 45.0 - 10.0 - 40f)),
	            apparelThingFilterState, 
	            Settings.ApparelFilter, 
	            ApparelGlobalFilter, 
	            16, 
	            forceHideHitPointsConfig: true,
	            forceHiddenFilters: 
	            SpecialThingFilterDefs);
            Widgets.EndGroup();

			Rect rect5 = new Rect(inRect.width / 2, inRect.y + 40f, inRect.width / 2, inRect.height - 40f);

			Widgets.BeginGroup(rect5);
			Widgets.Label(new Rect(0.0f, 0.0f, rect5.width, 24f), "DressForTheWeather_ReplaceableFilter".Translate());
			ThingFilterUI.DoThingFilterConfigWindow(new Rect(0, 40, 300f, (float)(rect5.height - 45.0 - 10.0 - 40f)),
				apparelThingFilterState,
				Settings.ReplaceableFilter,
				ApparelGlobalFilter,
				16,
				forceHideHitPointsConfig: true,
				forceHiddenFilters: SpecialThingFilterDefs);
			Widgets.EndGroup();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "Dress For The Weather";
    }

    [StaticConstructorOnStartup]
    public class DressForTheWeatherSettings : ModSettings {
	    public bool RaidersComePrepared = false;
	    private ThingFilter? apparelFilter;
        private ThingFilter? replaceableFilter;


	    public ThingFilter ApparelFilter {
		    get {
			    if (apparelFilter is not null) return apparelFilter;

				apparelFilter = new ThingFilter();
				apparelFilter.SetAllow(ThingCategoryDefOf.Apparel, true);
				apparelFilter.SetAllow(ThingCategoryDefOf.ApparelArmor, false);
				apparelFilter.SetAllow(ThingCategoryDefOf.ArmorHeadgear, false);
                apparelFilter.SetAllow(DefDatabase<ThingCategoryDef>.GetNamed("ApparelUtility"), false);
				return apparelFilter;
			
		    }
		    private set => apparelFilter = value;
	    }

	    public ThingFilter ReplaceableFilter {
		    get {
				if (replaceableFilter is not null) return replaceableFilter;

				replaceableFilter = new ThingFilter();
				replaceableFilter.SetAllow(ThingCategoryDefOf.Apparel, true);
                apparelFilter.SetAllow(DefDatabase<ThingCategoryDef>.GetNamed("ApparelUtility"), false);
				replaceableFilter.SetAllow(ThingCategoryDefOf.ApparelArmor, false);
				replaceableFilter.SetAllow(ThingCategoryDefOf.ArmorHeadgear, false);
				replaceableFilter.SetAllow(DefDatabase<ThingDef>.GetNamed("Apparel_AdvancedHelmet"), true);
				replaceableFilter.SetAllow(DefDatabase<ThingDef>.GetNamed("Apparel_SimpleHelmet"), true);
				replaceableFilter.SetAllow(DefDatabase<ThingDef>.GetNamed("Apparel_FlakVest"), true);
				replaceableFilter.SetAllow(DefDatabase<ThingDef>.GetNamed("Apparel_FlakPants"), true);
				replaceableFilter.SetAllow(DefDatabase<ThingDef>.GetNamed("Apparel_FlakJacket"), true);
				//gas mask
				replaceableFilter.SetAllow(DefDatabase<ThingDef>.GetNamed("Apparel_GasMask"), false);
				return replaceableFilter;

			}
			private set => replaceableFilter = value;
		}

        public override void ExposeData()
        {
            Scribe_Values.Look(ref RaidersComePrepared, nameof(RaidersComePrepared), false);
            ThingFilter filter = ApparelFilter;
            Scribe_Deep.Look(ref filter, nameof(ApparelFilter));
            ApparelFilter = filter;

			ThingFilter filter2 = ReplaceableFilter;
			Scribe_Deep.Look(ref filter2, nameof(ReplaceableFilter));
			ReplaceableFilter = filter2;

            base.ExposeData();
        }
    }
}