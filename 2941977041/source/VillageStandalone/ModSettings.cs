using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VillageStandalone
{
    class SleepingEffects : IExposable
    {
        public string villagestandaloneDefName;
        public string hediffDefName;
        public string label;
        public bool RemoveSoakingWet = false;
        public bool RemoveSleptOutside = false;
        public bool RemoveSleptInCold = false;
        public bool RemoveSleptInHeat = false;
        public bool RemoveSleptInBarracks = false;
        public bool RemoveSleepDisturbed = false;
        public bool RemoveToxicFallout = false;
        // public bool RemoveSunlightSensitivity_Mild = false;
        public bool ideologyVillageStandaloneAssignmentAllowed = false;
        public float fuelCapacity = 0;
        public float fuelConsumptionRate = 0;
        public bool fuelEnabled;

        public void ExposeData()
        {
        
            Scribe_Values.Look(ref villagestandaloneDefName, "villagestandaloneDefName");
            Scribe_Values.Look(ref hediffDefName, "hediffDefName");
            Scribe_Values.Look(ref RemoveSoakingWet, "RemoveSoakingWet");
            Scribe_Values.Look(ref RemoveSleptOutside, "RemoveSleptOutside", false);
            Scribe_Values.Look(ref RemoveSleptInCold, "RemoveSleptInCold", false);
            Scribe_Values.Look(ref RemoveSleptInHeat, "RemoveSleptInHeat", false);
            Scribe_Values.Look(ref RemoveSleptInBarracks, "RemoveSleptInBarracks", false);
            Scribe_Values.Look(ref RemoveSleepDisturbed, "RemoveSleepDisturbed", false);
            Scribe_Values.Look(ref RemoveToxicFallout, "RemoveToxicFallout", false);
            //Scribe_Values.Look(ref RemoveSunlightSensitivity_Mild, "RemoveSunlightSensitivity_Mild", false);
            Scribe_Values.Look(ref ideologyVillageStandaloneAssignmentAllowed, "ideologyVillageStandaloneAssignmentAllowed", false);
            Scribe_Values.Look(ref fuelCapacity, "fuelCapacity", 0);
            Scribe_Values.Look(ref fuelConsumptionRate, "fuelConsumptionRate", 0);
            Scribe_Values.Look(ref fuelEnabled, "fuelEnabled", true);
        }
    }

    class HediffSet : IExposable
    {
        public string hediffDefName;
        public string label;
        public float comfyTemperatureMin = 0;
        public float comfyTemperatureMax = 0;

        public void ExposeData()
        {
            Scribe_Values.Look(ref hediffDefName, "hediffDefName");
            Scribe_Values.Look(ref comfyTemperatureMin, "comfyTemperatureMin", 0);
            Scribe_Values.Look(ref comfyTemperatureMax, "comfyTemperatureMax", 0);
        }
    }

    class ModSettings : Verse.ModSettings
    {
        public static List<SleepingEffects> effects;
        public static List<HediffSet> hediffSets;
        public static bool xmlOverride;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref effects, "effects", LookMode.Deep);
            Scribe_Collections.Look(ref hediffSets, "hediffSets", LookMode.Deep);
            Scribe_Values.Look(ref xmlOverride, "xmlOverride", false);
        }

        private Vector2 scrollPos;

        public void DoWindowContents(Rect mainRect)
        {
            var options = new Listing_Standard();
            var viewRect = new Rect(0f, 0f, mainRect.width - 60, effects != null ? (effects.Count * 304f + hediffSets.Count * 42) : 2000f);
            Widgets.BeginScrollView(mainRect, ref this.scrollPos, viewRect);
            options.Begin(viewRect);

            Text.Font = GameFont.Medium;
            options.Label("All changes require a game restart to take effect");
            Text.Font = GameFont.Small;
            
            options.CheckboxLabeled($"Override xml values (Save your changes)", ref xmlOverride);
            options.GapLine();
            options.Gap();
            

            foreach (var effect in effects)
            {
                Text.Font = GameFont.Medium; 
                options.Label($"VillageStandalone: {effect.villagestandaloneDefName} (Hediff: {effect.label})");
                Text.Font = GameFont.Small;
                options.CheckboxLabeled($"Remove getting wet", ref effect.RemoveSoakingWet);
                options.CheckboxLabeled($"Remove slept outside", ref effect.RemoveSleptOutside);
                options.CheckboxLabeled($"Remove slept in cold", ref effect.RemoveSleptInCold);
                options.CheckboxLabeled($"Remove slept in heat", ref effect.RemoveSleptInHeat);
                options.CheckboxLabeled($"Remove slept in barracks", ref effect.RemoveSleptInBarracks);
                options.CheckboxLabeled($"Remove sleeping disturbed", ref effect.RemoveSleepDisturbed);
                //options.CheckboxLabeled($"Remove sunlight sensitivity mild", ref effect.RemoveSunlightSensitivity_Mild);
               

                if (ModsConfig.IdeologyActive) options.CheckboxLabeled($"Disable ideology villagestandalone assignment limitation", ref effect.ideologyVillageStandaloneAssignmentAllowed);
                options.CheckboxLabeled($"Fuel requirement enabled", ref effect.fuelEnabled);
                if (effect.fuelEnabled)
                {
                    var rect = options.GetRect(Text.LineHeight);
                    rect.width = options.ColumnWidth / 2;
                    Widgets.Label(rect, "Fuel capacity: ");
                    var input = Widgets.TextField(ModSettings.BRect(options.ColumnWidth - 40, rect.y, 40, Text.LineHeight), effect.fuelCapacity.ToString());
                    if (double.TryParse(input, out var inputDouble)) effect.fuelCapacity = Convert.ToInt32(inputDouble);

                    rect = options.GetRect(Text.LineHeight);
                    rect.width = options.ColumnWidth / 2;
                    Widgets.Label(rect, "Fuel consumption rate: ");
                    input = Widgets.TextField(ModSettings.BRect(options.ColumnWidth - 40, rect.y, 40, Text.LineHeight), effect.fuelConsumptionRate.ToString());
                    if (double.TryParse(input, out inputDouble)) effect.fuelConsumptionRate = Convert.ToInt32(inputDouble);
                }
                options.Gap();
            }

            options.GapLine();
            options.Gap();

            foreach (var hediff in hediffSets)
            {
                Text.Font = GameFont.Medium;
                options.Label($"Hediff: {hediff.label}");
                Text.Font = GameFont.Small;
                var rect = options.GetRect(Text.LineHeight);
                rect.width = options.ColumnWidth / 2;
                Widgets.Label(rect, "Min Compfy Temperature bonus: ");
                var input = Widgets.TextField(ModSettings.BRect(options.ColumnWidth - 40, rect.y, 40, Text.LineHeight), hediff.comfyTemperatureMin.ToString());
                if (double.TryParse(input, out var inputDouble)) hediff.comfyTemperatureMin = Convert.ToInt32(inputDouble);

                rect = options.GetRect(Text.LineHeight);
                rect.width = options.ColumnWidth / 2;
                Widgets.Label(rect, "Max Compfy Temperature bonus: ");
                input = Widgets.TextField(ModSettings.BRect(options.ColumnWidth - 40, rect.y, 40, Text.LineHeight), hediff.comfyTemperatureMax.ToString());
                if (double.TryParse(input, out inputDouble)) hediff.comfyTemperatureMax = Convert.ToInt32(inputDouble);
                options.Gap();
            }
            options.End();
            Widgets.EndScrollView();

        }

        public static Rect lastRect = default;
        public static Rect BRect(float x, float y, float width, float height)
        {
            lastRect = new Rect(x, y, width, height);
            return lastRect;
        }

        public static void InitVillageStandalones()
        {
            var villagestandalones = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.HasModExtension<VillageStandaloneModExtension>());
            if (effects != null) effects.RemoveAll(x => string.IsNullOrEmpty(x.villagestandaloneDefName));
            if (effects != null && effects?.Count == villagestandalones.Count() && ModSettings.xmlOverride) ApplyEffects(effects);
            else ReadEffects(villagestandalones);

            var hediff = DefDatabase<HediffDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("VillageStandalone_Comfy"));
            if (hediffSets != null) hediffSets.RemoveAll(x => string.IsNullOrEmpty(x.hediffDefName));
            if (hediffSets != null && hediffSets?.Count == hediff.Count() && ModSettings.xmlOverride) ApplyHediffs(hediffSets);
            else ReadHediffs(hediff);
        }

        private static void ReadHediffs(IEnumerable<HediffDef> hediffs)
        {
            hediffSets = new List<HediffSet>();
            foreach (var hediff in hediffs)
            {
                if (hediff?.defName == null) continue;

                hediffSets.Add(new HediffSet()
                {
                    hediffDefName = hediff.defName,
                    label = hediff.label,
                    comfyTemperatureMin = hediff?.stages?.FirstOrDefault()?.statOffsets?.FirstOrDefault(x => x?.stat == StatDefOf.ComfyTemperatureMin)?.value ?? 0,
                    comfyTemperatureMax = hediff?.stages?.FirstOrDefault()?.statOffsets?.FirstOrDefault(x => x?.stat == StatDefOf.ComfyTemperatureMax)?.value ?? 0,
                });
            }
        }

        private static void ApplyHediffs(List<HediffSet> hediffSets)
        {
            foreach (var hediffSet in hediffSets)
            {
                var hediff = DefDatabase<HediffDef>.GetNamed(hediffSet.hediffDefName, false);

                if (hediff != null)
                {
                    hediffSet.label = hediff.label;
                    var statTempMin = hediff?.stages?.FirstOrDefault()?.statOffsets?.FirstOrDefault(x => x?.stat == StatDefOf.ComfyTemperatureMin);
                    if (statTempMin != null) statTempMin.value = hediffSet.comfyTemperatureMin;
                    var statTempMax = hediff?.stages?.FirstOrDefault()?.statOffsets?.FirstOrDefault(x => x?.stat == StatDefOf.ComfyTemperatureMax);
                    if (statTempMax != null) statTempMax.value = hediffSet.comfyTemperatureMax;
                }
            }
        }

        private static void ReadEffects(IEnumerable<ThingDef> villagestandalones)
        {
            effects = new List<SleepingEffects>();
            foreach (var villagestandalone in villagestandalones)
            {
                if (villagestandalone?.defName == null) continue;
                var modExt = villagestandalone.GetModExtension<VillageStandaloneModExtension>();
                var refuelComp = villagestandalone.GetCompProperties<CompProperties_Refuelable>();
                var hediff = DefDatabase<HediffDef>.GetNamed(modExt?.customHediff?.defName ?? "", false);
                effects.Add(new SleepingEffects()
                {
                    villagestandaloneDefName = villagestandalone.defName,
                    label = hediff?.label ?? "",
                    hediffDefName = modExt?.customHediff?.defName ?? "",
                    RemoveSoakingWet = modExt.RemoveSoakingWet,
                    RemoveSleptInBarracks = modExt.RemoveSleptInBarracks,
                    RemoveSleptInCold = modExt.RemoveSleptInCold,
                    RemoveSleptInHeat = modExt.RemoveSleptInHeat,
                    RemoveSleptOutside = modExt.RemoveSleptOutside,
                    RemoveSleepDisturbed = modExt.RemoveSleepDisturbed,
                    //RemoveSunlightSensitivity_Mild = modExt.RemoveSunlightSensitivity_Mild,
                 
                    ideologyVillageStandaloneAssignmentAllowed = modExt.ideologyVillageStandaloneAssignmentAllowed,
                    fuelEnabled = false,
                    fuelCapacity = refuelComp?.fuelCapacity ?? 0,
                    fuelConsumptionRate = refuelComp?.fuelConsumptionRate ?? 0
                });
            }
        }

        private static void ApplyEffects(List<SleepingEffects> effects)
        {
            foreach (var effect in effects)
            {
                var villagestandalone = DefDatabase<ThingDef>.GetNamed(effect.villagestandaloneDefName, false);
                if (villagestandalone == null)
                {
                    Log.Warning($"VillageStandalone {effect.villagestandaloneDefName} was null");
                    continue;
                }

                var modExt = villagestandalone.GetModExtension<VillageStandaloneModExtension>();
                if (modExt != null)
                {
                    modExt.RemoveSoakingWet = effect.RemoveSoakingWet;
                    modExt.RemoveSleptInBarracks = effect.RemoveSleptInBarracks;
                    modExt.RemoveSleptInCold = effect.RemoveSleptInCold;
                    modExt.RemoveSleptInHeat = effect.RemoveSleptInHeat;
                    modExt.RemoveSleptOutside = effect.RemoveSleptOutside;
                    modExt.RemoveSleepDisturbed = effect.RemoveSleepDisturbed;
                    //  modExt.RemoveSunlightSensitivity_Mild = effect.RemoveSunlightSensitivity_Mild;

                    modExt.ideologyVillageStandaloneAssignmentAllowed = effect.ideologyVillageStandaloneAssignmentAllowed;
                }
                
                var refuelCompProps = villagestandalone.GetCompProperties<CompProperties_Refuelable>();
                if (!effect.fuelEnabled && refuelCompProps != null) villagestandalone.comps.Remove(refuelCompProps);
                else if (refuelCompProps != null)
                {
                    refuelCompProps.fuelCapacity = effect.fuelCapacity;
                    refuelCompProps.fuelConsumptionRate = effect.fuelConsumptionRate;
                }

                var hediff = DefDatabase<HediffDef>.GetNamed(modExt.customHediff.defName, false);
                if (hediff != null)
                {
                    effect.label = hediff.label;
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    class InitVillageStandaloneStartup { static InitVillageStandaloneStartup() => ModSettings.InitVillageStandalones(); }

}
