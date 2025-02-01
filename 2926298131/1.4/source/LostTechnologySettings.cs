using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace LostTechnology
{
    public class LostTechnologySettings : ModSettings
    {
        public static bool endeavouringSystemEnabled = true;
        public static float threatMultiplier = 1f;
        public static Dictionary<TechLevel, IntRange> spawnIntervalsByTechLevels;
        public static Dictionary<TechLevel, IntRange> distanceRangeByTechLevels;
        public static float priceMultiply = 1f;
        public static float costMultiply = 1f;
        public static float appearanceProbability = 0f;
        public static bool UnlockNeolithic = false;
        public static bool UnlockMedieval = false;
        public static bool UnlockIndustrial = false;
        public static bool UnlockSpacer = false;
        public static bool UnlockUltra = false;
        public static float minThreatScale;
        public static float maxThreatScale;
        public static bool disallowNPCfactionFromSellingTechprints;
        public static IntRange minMaxDaysForTPSiteDuration = new IntRange(1, 60);
        public static int maxTPSiteCountToSpawn = 3;

        public static bool testMode;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref priceMultiply, "priceMultiply", 1f);
            Scribe_Values.Look(ref costMultiply, "costMultiply", 1f);
            Scribe_Values.Look(ref appearanceProbability, "appearanceProbability", 0f);
            Scribe_Values.Look(ref UnlockNeolithic, "UnlockNeolithic");
            Scribe_Values.Look(ref UnlockMedieval, "UnlockMedieval");
            Scribe_Values.Look(ref UnlockIndustrial, "UnlockIndustrial");
            Scribe_Values.Look(ref UnlockSpacer, "UnlockSpacer");
            Scribe_Values.Look(ref UnlockUltra, "UnlockUltra");
            Scribe_Values.Look(ref endeavouringSystemEnabled, "endeavouringSystemEnabled", true);
            Scribe_Values.Look(ref threatMultiplier, "threatMultiplier", 1f);
            Scribe_Values.Look(ref maxTPSiteCountToSpawn, "maxTPSiteCountToSpawn", 3);
            Scribe_Values.Look(ref disallowNPCfactionFromSellingTechprints, "disallowNPCfactionFromSellingTechprints", false);
            Scribe_Collections.Look(ref spawnIntervalsByTechLevels, "spawnIntervalsByTechLevels", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref distanceRangeByTechLevels, "distanceRangeByTechLevels", LookMode.Value, LookMode.Value);
            int timeIntervalMin = minMaxDaysForTPSiteDuration.min;
            int timeIntervalMax = minMaxDaysForTPSiteDuration.max;
            Scribe_Values.Look(ref timeIntervalMin, nameof(timeIntervalMin), 1);
            Scribe_Values.Look(ref timeIntervalMax, nameof(timeIntervalMax), 60);
            minMaxDaysForTPSiteDuration = new IntRange(timeIntervalMin, timeIntervalMax);
        }

        [TweakValue("0000", -100, 100)] public static float testY = 0;
        private Vector2 scrollPosition;
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(rect.x, rect.y, rect.width - 30f, 965);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect2);
            if (listingStandard.ButtonText("Reset".Translate()))
            {
                threatMultiplier = 1f;
                spawnIntervalsByTechLevels = distanceRangeByTechLevels = null;
                endeavouringSystemEnabled = true;
                Startup.Reset();
            }
            listingStandard.CheckboxLabeled("LTA.EndeavouringSystemEnabled".Translate(), ref endeavouringSystemEnabled);
            listingStandard.Label("LTA.ThreatMultiplier".Translate((threatMultiplier * 100) + "%"));
            threatMultiplier = (float)(Math.Round(listingStandard.Slider(threatMultiplier, 0.01f, 30f) / 0.05, MidpointRounding.AwayFromZero) * 0.05); ;
            listingStandard.GapLine();
            foreach (TechLevel techLevel in Enum.GetValues(typeof(TechLevel)))
            {
                if (spawnIntervalsByTechLevels.TryGetValue(techLevel, out IntRange spawnInterval))
                {
                    listingStandard.Label("LTA.SpawnIntervalFor".Translate(techLevel.ToStringHuman(), $"{spawnInterval.min.ToStringTicksToPeriodVerbose(false)} - {spawnInterval.max.ToStringTicksToPeriodVerbose(false)}"));
                    listingStandard.IntRange(ref spawnInterval, 60, GenDate.TicksPerYear);
                    spawnIntervalsByTechLevels[techLevel] = spawnInterval;
                }
            }
            listingStandard.GapLine();
            foreach (TechLevel techLevel in Enum.GetValues(typeof(TechLevel)))
            {
                if (distanceRangeByTechLevels.TryGetValue(techLevel, out IntRange distanceRange))
                {
                    listingStandard.Label("LTA.DistanceRange".Translate(techLevel.ToStringHuman(), $"{distanceRange.min} - {distanceRange.max}"));
                    listingStandard.IntRange(ref distanceRange, 0, 1000);
                    distanceRangeByTechLevels[techLevel] = distanceRange;
                }
            }
            listingStandard.CheckboxLabeled("LTA.DisallowNPCFactionsFromSellingTechprints".Translate(), ref disallowNPCfactionFromSellingTechprints);
            listingStandard.Label("LTA.AdjustTPSiteDurationTimeInterval".Translate());
            listingStandard.Label($"{minMaxDaysForTPSiteDuration.min} days - {minMaxDaysForTPSiteDuration.max} days");
            listingStandard.IntRange(ref minMaxDaysForTPSiteDuration, 1,  60);
            maxTPSiteCountToSpawn = (int)listingStandard.SliderLabeled("LTA.MaxTPSiteCountToSpawn".Translate(maxTPSiteCountToSpawn), maxTPSiteCountToSpawn, 1, 10);

            //priceMultiplySetting = Settings.GetHandle<float>("losttech_priceMultiply", "TechPrint Price Multiply", "The base price of tech print is multiplied by this value.\ndefault = 1.0\nneed restart game", 1f);
            costMultiply = listingStandard.SliderLabeled("TechPrint Research Cost Multiply", costMultiply, 0, 10, 0.5f, "The base cost of research is multiplied by this value.\ndefault = 0.35\nneed restart game");
            //appearanceProbabilitySetting = Settings.GetHandle<float>("losttech_appearanceProbability", "TechPrint Appearance Probability", "This affects the overall probability of appearance.\nDue to the limitations of the vanilla system, even if this value is high, the probability of appearance does not rise above a certain level.\ndefault = 1\nneed restart game", 1f);

            // use techprint by techlevel
            listingStandard.CheckboxLabeled("Unlock Neolithic level research", ref UnlockNeolithic);
            listingStandard.CheckboxLabeled("Unlock Medieval level research", ref UnlockMedieval);
            listingStandard.CheckboxLabeled("Unlock Industrial level research", ref UnlockIndustrial);
            listingStandard.CheckboxLabeled("Unlock Spacer level research", ref UnlockSpacer);
            listingStandard.CheckboxLabeled("Unlock Ultra level research", ref UnlockUltra);
            minThreatScale = listingStandard.SliderLabeled("Techprint Site Min Threat", minThreatScale, 0, 1000, 0.5f, "(# NEED NEW GAME) Threat size for the lowest level techprint outpost");
            maxThreatScale = listingStandard.SliderLabeled("Techprint Site Max Threat", maxThreatScale, 0, 10000, 0.5f, "(# NEED NEW GAME) Threat size for the highest level techprint outpost");
            listingStandard.CheckboxLabeled("# Test Mode", ref testMode);

            listingStandard.End();
            SettingsChanged();
            Widgets.EndScrollView();
        }

        public static void SettingsChanged()
        {
            //priceMultiplySetting.Value = Mathf.Clamp(priceMultiplySetting.Value, 0.01f, 100f);
            //priceMultiply = priceMultiplySetting.Value;
            costMultiply = Mathf.Clamp(costMultiply, 0f, 100f);
            //appearanceProbabilitySetting.Value = Mathf.Clamp(appearanceProbabilitySetting.Value, 0f, 1000f);
            //appearanceProbability = appearanceProbabilitySetting.Value;
            minThreatScale = Mathf.Clamp(minThreatScale, 1f, 1000000f);
            maxThreatScale = Mathf.Clamp(minThreatScale, 1f, 1000000f);
            MarketValueLootThreatPoints = new SimpleCurve();
            MarketValueLootThreatPoints.Add(new CurvePoint(200f, minThreatScale));
            MarketValueLootThreatPoints.Add(new CurvePoint(12000f, 4f * maxThreatScale));
        }

        public static float priceFactor => 0.5f * priceMultiply;
        static float f;

        static int firstGenDist = 7;
        static int retryNum = 200;
        static ResearchProjectDef r;
        static List<ResearchProjectDef> ar_research = new List<ResearchProjectDef>();

        [HarmonyPatch(typeof(TickManager), nameof(TickManager.DoSingleTick))]
        public static class TickManager_DoSingleTick_Patch
        {
            public static void Postfix()
            {
                if (Find.TickManager.TicksGame == 2)
                {
                    regenerate();
                }
                else if (Find.TickManager.TicksGame % 60000 == 0)
                {
                    if (!Find.Storyteller?.incidentQueue?.queuedIncidents?.Any(x => x?.firingInc?.def == LTA_DefOf.LTA_NewTechprintSite) ?? false)
                    {
                        Core.AddTechprintIncident();
                    }
                }
            }
        }

        static public void removeAllTechprintSite()
        {
            foreach (Site s in Current.Game.World.worldObjects.Sites.FindAll(a => a.parts.Find(b => b.def == LostTechnology.Def.SitePartDefOf.Techprint) != null))
            {
                SitePart sp = s.parts.Find(a => a.def == LostTechnology.Def.SitePartDefOf.Techprint);

                s.parts.Remove(sp);
                sp.def.Worker.PostDestroy(sp);
                sp.PostDestroy();

                s.Destroy();

            }
        }

        static public void regenerate()
        {
            if (LostTechnologySettings.endeavouringSystemEnabled)
            {
                Core.MakeTechprintSites();
                return;
            }
            removeAllTechprintSite();
            firstGenDist = 7;
            retryNum = 200;
            ar_research = new List<ResearchProjectDef>();
            ar_research.AddRange(DefDatabase<ResearchProjectDef>.AllDefs);
            ar_research = ar_research.OrderBy(a => a.techLevel).ThenBy(a => a.researchViewX).ToList();
            ar_research.RemoveAll(a => a.TechprintCount <= 0);
            for (int i = 0; i < ar_research.Count; i++)
            {
                r = ar_research[i];
                if (r.Techprint != null)
                {
                    while (retryNum >= 0)
                    {
                        if (makeTechPrintSite(r.Techprint, firstGenDist, r.techLevel))
                        {
                            firstGenDist++;
                            break;
                        }
                        else
                        {
                            if (firstGenDist > 7) firstGenDist--;
                            retryNum--;
                        }

                    }

                }
            }
        }



        static int tile;


        static bool makeTechPrintSite(ThingDef td, int dist, TechLevel tlv)
        {
            List<Thing> ar_t = new List<Thing>();
            ar_t.Add(ThingMaker.MakeThing(td));
            float threat = MarketValueLootThreatPoints.Evaluate(GenThing.GetMarketValue(ar_t));

            List<SitePartDef> parts = new List<SitePartDef>();
            parts.Add(LostTechnology.Def.SitePartDefOf.Techprint);
            parts.Add(LostTechnology.Def.SitePartDefOf.ItemStash);

            if (tlv <= TechLevel.Medieval && Rand.Chance(0.15f))
            {
                tlv = TechLevel.Undefined;
                parts.Add(SitePartDefOf.Manhunters);
            }
            else
            {
                switch (Rand.Range(0, 6))
                {
                    default:
                        parts.Add(SitePartDefOf.Outpost);
                        break;
                    case 1:
                        parts.Add(SitePartDefOf.Turrets);
                        break;
                    case 2:
                        parts.Add(SitePartDefOf.BanditCamp);
                        break;
                    case 3:
                        parts.Add(SitePartDefOf.SleepingMechanoids);
                        break;
                    case 4:
                        parts.Add(LostTechnology.Def.SitePartDefOf.MechClusterForceNoConditionCauser);
                        break;
                }
            }




            tile = 0;
            if (ar_t.Count > 0 && TileFinder.TryFindNewSiteTile(out tile, dist - 5, dist + 5))
            {
                Site site = SiteMaker.TryMakeSite(parts, tile, true, a => 
                (int)a.def.techLevel >= Mathf.Min((int)tlv, (int)TechLevel.Industrial) && a.def.humanlikeFaction, true, threat);
                if (site == null)
                {
                    Messages.Message("Could not find any valid faction for this site.", MessageTypeDefOf.RejectInput, false);
                }
                else
                {
                    Find.WorldObjects.Add(site);
                    site.customLabel = td.label;
                    site.parts[1].things = new ThingOwner<Thing>(site.parts[1], false);
                    site.parts[1].things.TryAdd(ar_t[0]);
                    var data = dataUtility.GetData(site);
                    data.durationInTicks = (int)(Find.TickManager.TicksGame + new FloatRange(LostTechnologySettings.minMaxDaysForTPSiteDuration.min,
                        LostTechnologySettings.minMaxDaysForTPSiteDuration.max).RandomInRange * GenDate.TicksPerDay);
                    data.ar_td = new List<ThingDef>
                    {
                        td
                    };
                    return true;
                }

            }
            return false;
        }


        public static float getThreat(ThingDef td)
        {
            //return getThreat(td.BaseMarketValue);
            return MarketValueLootThreatPoints.Evaluate(td.BaseMarketValue);
        }

        public static SimpleCurve MarketValueLootThreatPoints = new SimpleCurve
        {
            new CurvePoint(200f, minThreatScale),
            new CurvePoint(12000f, 4f * maxThreatScale)
        };
    }
}