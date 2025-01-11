using HarmonyLib;
using System.Linq;
using Verse;

namespace HumanResources
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        public static Harmony instance = null;

        public static bool
            PrisonLabor = false,
            VFEM = false,
            RunSpecialCases = false,
            SemiRandom = false,
            VisibleBooksCategory = false;

        public static ResearchTreeVersion TreeVer = ResearchTreeVersion.Tree;

        public static Harmony Instance
        {
            get
            {
                if (instance == null)
                    instance = new Harmony("JPT.HumanResources");
                return instance;
            }
        }

        public static string ResearchPalNamespaceRoot
        {
            get
            {
                switch (TreeVer)
                {
                    case ResearchTreeVersion.Tree:
                        return "Fluffy.ResearchTree";
                    case ResearchTreeVersion.Pal:
                        return "ResearchPal";
                    default: return string.Empty;
                }
            }
        }

        static HarmonyPatches()
        {
            //Harmony.DEBUG = true;
            Instance.PatchAll();

            //ResearchTree/ResearchPal integration
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("fluffy.researchtree")))
            {
                Log.Message("[HumanResources] Deriving from ResearchTree.");
                ResearchTree_Patches.Execute(Instance, "FluffyResearchTree");
                TreeVer = ResearchTreeVersion.Tree;
            }
            else if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("Neigh.ResearchPalRepackage")))
            {
                Log.Message("[HumanResources] Deriving from ResearchPal - 1.4 Repackage.");
                ResearchTree_Patches.Execute(Instance, "ResearchPal", ResearchTreeVersion.Pal);
                TreeVer = ResearchTreeVersion.Pal;
            }
            else
            {
                Log.Error("[HumanResources] Could not find ResearchTree. Human Resources will not work!");
            }

            //Go Explore! integration
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("Albion.GoExplore")))
            {
                Log.Message("[HumanResources] Go Explore detected! Integrating...");
                GoExplore_Patches.Execute(Instance);
            }

            //Material Filter patch
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("KamiKatze.MaterialFilter")))
            {
                Log.Message("[HumanResources] Material Filter detected! Adapting...");
                MaterialFilter_Patch.Execute(Instance);
            }

            //Recipe icons patch
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("automatic.recipeicons")))
            {
                Log.Message("[HumanResources] Recipe Icons detected! Adapting...");
                RecipeIcons_Patch.Execute(Instance);
            }

            //Simple Sidearms integration
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("PeteTimesSix.SimpleSidearms")))
            {
                Log.Message("[HumanResources] Simple Sidearms detected! Integrating...");
                SimpleSidearms_Patches.Execute(Instance);
            }

            //Prison Labor integration
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("avius.prisonlabor")))
            {
                Log.Message("[HumanResources] Prison Labor detected! Integrating...");
                PrisonLabor = true;
            }

            //Dual Wield integration
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("Roolo.DualWield")))
            {
                Log.Message("[HumanResources] Dual Wield detected! Integrating...");
                DualWield_Patch.Execute(Instance);
            }

            //Hospitality integration
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("orion.hospitality")))
            {
                Log.Message("[HumanResources] Hospitality detected! Integrating...");
                Hospitality_Patches.Execute(Instance);
            }

            //VFEM integration
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("OskarPotocki.VFE.Mechanoid")))
            {
                Log.Message("[HumanResources] Vanilla Factions Expanded - Mechanoids detected! Integrating...");
                VFEM_Patches.Execute(Instance);
                VFEM = true;
            }

            //Pick Up and Haul integration
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("Mehni.PickUpAndHaul")))
            {
                Log.Message("[HumanResources] Pick Up And Haul detected! Adapting...");
                PUAH_Patch.Execute(Instance);
            }

            //Semi-Random integration
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("CaptainMuscles.SemiRandomResearch")))
            {
                Log.Message("[HumanResources] Semi-Random Research detected! Integrating...");
                SemiRandom_Patch.Execute(Instance);
                SemiRandom = true;
            }

            //Fluffy Breakdowns integration
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("Fluffy.FluffyBreakdowns")))
            {
                Log.Message("[HumanResources] Fluffy Breakdowns detected! Integrating...");
                FluffyBreakdowns_Patches.Execute(Instance);
            }

            //Research Data
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("kongkim.ResearchData")))
            {
                Log.Message("[HumanResources] Research Data detected! Integrating...");
                var func1 = AccessTools.Method("ResearchData.WorkGiver_Researcher_JobOnThing_Patch:Prefix");
                Instance.Patch(AccessTools.Method("WorkGiver_ResearchTech:JobOnThing"), prefix: new HarmonyMethod(func1));
                var func2 = AccessTools.Method("ResearchData.WorkGiver_Researcher_HasJobOnThing_Patch:Prefix");
                Instance.Patch(AccessTools.Method("WorkGiver_ResearchTech:HasJobOnThing"), prefix: new HarmonyMethod(func2));
            }

            //Provisions for specific research projects
            if (LoadedModManager.RunningModsListForReading.Any(x =>
            x.PackageIdPlayerFacing.StartsWith("loconeko.roadsoftherim") ||
            x.PackageIdPlayerFacing.StartsWith("Mlie.RoadsOfTheRim") ||
            x.PackageIdPlayerFacing.StartsWith("fluffy.backuppower") ||
            x.PackageIdPlayerFacing.StartsWith("Fluffy.FluffyBreakdowns") ||
            x.PackageIdPlayerFacing.StartsWith("Ogliss.AdMech.Armoury") ||
            x.PackageIdPlayerFacing.StartsWith("VanillaExpanded.VFEArt")))
            {
                RunSpecialCases = true;
            }

        }

        public static bool CheckKnownWeapons(Pawn pawn, Thing thing)
        {
            return CheckKnownWeapons(pawn, thing.def);
        }

        public static bool CheckKnownWeapons(Pawn pawn, ThingWithComps thing)
        {
            return CheckKnownWeapons(pawn, thing.def);
        }

        public static bool CheckKnownWeapons(Pawn pawn, ThingDef def)
        {
            var knownWeapons = pawn.TryGetComp<CompKnowledge>()?.knownWeapons;
            return !knownWeapons.EnumerableNullOrEmpty() && knownWeapons.Contains(def);
        }

        public static void InitNewGame_Prefix()
        {
            Find.FactionManager.OfPlayer.def.startingResearchTags.Clear();
            Log.Message("[HumanResources] Starting a new game, player faction has been stripped of all starting research.");
        }
    }
}