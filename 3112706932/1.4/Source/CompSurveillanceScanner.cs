using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using VFED;

namespace ImperialFunctionality
{
    public class CompProperties_ScannerSurveillance : CompProperties_Scanner
    {
        public CompProperties_ScannerSurveillance()
        {
            compClass = typeof(CompSurveillanceScanner);
        }
    }

    public class CompSurveillanceScanner : CompScanner
    {
        public float raidPointsFound;
        public new CompProperties_ScannerSurveillance Props => props as CompProperties_ScannerSurveillance;
        private CompAffectedByFacilities compAffectedByFacilities;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compAffectedByFacilities = parent.GetComp<CompAffectedByFacilities>();
        }
        [HarmonyPatch(typeof(CompScanner), "CanUseNow", MethodType.Getter)]
        public static class CompScanner_CanUseNow_Patch
        {
            public static bool Prefix(CompScanner __instance, ref bool __result)
            {
                if (__instance is CompSurveillanceScanner scanner)
                {
                    __result = scanner.CanUseNowOverride;
                    return false;
                }
                return true;
            }
        }

        public List<Thing> WorkingDisplayBanks
        {
            get
            {
                var displayBanks = compAffectedByFacilities.LinkedFacilitiesListForReading.Where(x => x.TryGetComp<CompPowerTrader>().PowerOn).ToList();
                var pillars = this.parent.Map.listerBuildings
                    .AllBuildingsColonistOfDef(IF_DefOf.VFED_SurveillancePillar).Where(x => x.TryGetComp<CompPowerTrader>().PowerOn).ToList();
                return displayBanks.Take(pillars.Count).ToList();
            }
        }

        public bool CanUseNowOverride
        {
            get
            {
                if (!parent.Spawned)
                {
                    return false;
                }
                if (powerComp != null && !powerComp.PowerOn)
                {
                    return false;
                }
                if (forbiddable != null && forbiddable.Forbidden)
                {
                    return false;
                }
                if (WorkingDisplayBanks.Any() is false)
                {
                    return false;
                }
                return parent.Faction == Faction.OfPlayer;
            }
        }

        public override void DoFind(Pawn worker)
        {
            var points = WorkingDisplayBanks.Count * 100f;
            raidPointsFound += points;
            Find.LetterStack.ReceiveLetter("IF.LetterLabelFoundRaidersNearby".Translate(), "IF.LetterLabelFoundRaidersNearbyDesc".Translate(worker.Named("FINDER"), points), LetterDefOf.PositiveEvent);
        }

        public override string CompInspectStringExtra()
        {
            return "IF.ActiveDisplayBanks".Translate(WorkingDisplayBanks.Count);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref raidPointsFound, "raidPointsFound");
        }
    }
}
