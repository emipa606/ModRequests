using HarmonyLib;
using LostTechnology;
using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using System.Linq;

namespace LostTechnology
{
    [StaticConstructorOnStartup]
    public static class Core
    {
        public static ResearchProjectDef GetFirstTechPrintResearchProject(HashSet<ResearchProjectDef> defsToExclude = null)
        {
            var defs = DefDatabase<ResearchProjectDef>.AllDefs.Where(x => x.Techprint != null && !x.IsFinished 
            && x.PrerequisitesCompleted && (defsToExclude is null || defsToExclude.Contains(x) is false));
            if (defs.Any())
            {
                var curTechLevel = TechLevel.Animal;
                while (curTechLevel < TechLevel.Archotech)
                {
                    var curDefs = defs.Where(x => x.techLevel == curTechLevel);
                    if (curDefs.Any())
                    {
                        return curDefs.RandomElement();
                    }
                    else
                    {
                        curTechLevel = (TechLevel)((int)curTechLevel + 1);
                    }
                }
                return defs.RandomElement();
            }
            return null;
        }

        public static HashSet<ResearchProjectDef> GetPlayerPossesedTechprints()
        {
            var techprints = new HashSet<ResearchProjectDef>();
            foreach (var map in Find.Maps.Where(x => x.IsPlayerHome))
            {
                foreach (var thing in map.listerThings.AllThings)
                {
                    var comp = thing.TryGetComp<CompTechprint>();
                    if (comp != null)
                    {
                        techprints.Add(comp.Props.project);
                    }
                }
            }

            foreach (var caravan in Find.WorldObjects.Caravans)
            {
                if (caravan.IsPlayerControlled)
                {
                    foreach (var thing in caravan.AllThings)
                    {
                        var comp = thing.TryGetComp<CompTechprint>();
                        if (comp != null)
                        {
                            techprints.Add(comp.Props.project);
                        }
                    }
                }
            }
            return techprints;
        }

        public static void AddTechprintIncident()
        {
            var project = DefDatabase<ResearchProjectDef>.AllDefs.FirstOrDefault(x => x.IsFinished is false && x.PrerequisitesCompleted);
            var incidentParms = StorytellerUtility.DefaultParmsNow(LTA_DefOf.LTA_NewTechprintSite.category, Find.World);
            incidentParms.target = Find.World;
            Find.Storyteller.incidentQueue.Add(LTA_DefOf.LTA_NewTechprintSite, Find.TickManager.TicksGame
                + LostTechnologySettings.spawnIntervalsByTechLevels[project.techLevel].RandomInRange, incidentParms);
        }
        public static List<Site> MakeTechprintSites()
        {
            var list = new List<Site>();
            var existingSpots = Find.WorldObjects.AllWorldObjects.OfType<Site>()
                .Where(x => x.parts?.Any(y => y.def == LTA_DefOf.LTA_BanditCampWithTechPrint) ?? false).ToList();
            var existingResearchProjects = existingSpots.SelectMany(x => dataUtility.GetData(x).ar_td)
                .Select(x => x.GetCompProperties<CompProperties_Techprint>().project).Concat(GetPlayerPossesedTechprints()).ToHashSet();

            var countToSpawn = LostTechnologySettings.maxTPSiteCountToSpawn - existingSpots.Count;
            for (var i = 0; i < countToSpawn; i++)
            {
                var project = GetFirstTechPrintResearchProject(existingResearchProjects);
                if (project != null)
                {
                    existingResearchProjects.Add(project);
                    var site = MakeTechPrintSite(project.Techprint, project.techLevel);
                    list.Add(site);
                }
            }
            return list;
        }
        public static Site MakeTechPrintSite(ThingDef td, TechLevel tlv)
        {
            var techPrint = ThingMaker.MakeThing(td);
            float threat = StorytellerUtility.DefaultThreatPointsNow(Find.World) * LostTechnologySettings.threatMultiplier;
            List<SitePartDef> parts = new List<SitePartDef>
            {
                LTA_DefOf.LTA_BanditCampWithTechPrint
            };
            var distRamge = LostTechnologySettings.distanceRangeByTechLevels[tlv];
            if (TileFinder.TryFindNewSiteTile(out int tile, distRamge.min, distRamge.max))
            {
                var faction = GetFactionFor(tlv);
                Site site = SiteMaker.MakeSite(parts, tile, faction, true, threat);
                if (site == null)
                {
                    Messages.Message("Could not find any valid faction for this site.", MessageTypeDefOf.RejectInput, false);
                }
                else
                {
                    Find.WorldObjects.Add(site);
                    site.customLabel = td.label;
                    site.parts[0].things = new ThingOwner<Thing>(site.parts[0], false);
                    site.parts[0].things.TryAdd(techPrint);
                    var data = dataUtility.GetData(site);
                    data.durationInTicks = (int)(Find.TickManager.TicksGame + new FloatRange(LostTechnologySettings.minMaxDaysForTPSiteDuration.min,
                        LostTechnologySettings.minMaxDaysForTPSiteDuration.max).RandomInRange * GenDate.TicksPerDay);
                    data.ar_td = new List<ThingDef>
                    {
                        td
                    };
                    return site;
                }
            }
            return null;
        }

        private static Faction GetFactionFor(TechLevel techLevel)
        {
            var allHostileFactions = Find.FactionManager.AllFactions.Where(x => x.HostileTo(Faction.OfPlayer) 
                && x.def.humanlikeFaction && x.def.pawnGroupMakers != null && 
                x.def.pawnGroupMakers.Any(group =>(group.kindDef==PawnGroupKindDefOf.Settlement||group.kindDef==PawnGroupKindDefOf.Settlement_RangedOnly))).ToList();
            switch (techLevel)
            {
                case TechLevel.Animal:
                case TechLevel.Neolithic:
                case TechLevel.Medieval:
                    return allHostileFactions.Where(x => x.def.techLevel <= TechLevel.Medieval).RandomElement();
                case TechLevel.Industrial:
                case TechLevel.Spacer:
                    return allHostileFactions
                        .Where(x => x.def.techLevel > TechLevel.Medieval && x.def.techLevel <= TechLevel.Spacer)
                        //.Concat(Faction.OfInsects)
                        //.Concat(Faction.OfMechanoids)
                        .Distinct().RandomElement();
                case TechLevel.Ultra:
                    return allHostileFactions
                        .Where(x => x.def.techLevel >= TechLevel.Spacer)
                        //.Concat(Faction.OfMechanoids)
                        .Distinct().RandomElement();
            }
            return Find.FactionManager.AllFactions.Where(x => x.HostileTo(Faction.OfPlayer)
                && x.def.humanlikeFaction && x.def.pawnGroupMakers != null &&
                x.def.pawnGroupMakers.Any(group => group.kindDef == PawnGroupKindDefOf.Combat || (group.kindDef == PawnGroupKindDefOf.Settlement || group.kindDef == PawnGroupKindDefOf.Settlement_RangedOnly))).ToList().RandomElement();
        }
    }

    [HarmonyPatch(typeof(IncidentWorker), "CanFireNow")]
    public static class CanFireNowPatch
    {
        public static void Prefix(IncidentParms parms)
        {
            if (parms.target is null)
            {
                parms.target = Find.World;
            }
        }
    }
}
