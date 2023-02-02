using HugsLib;
using HugsLib.Settings;
using Verse;

namespace BenLubarsVanillaExpandedPatches
{
    public class BenLubarsVanillaExpandedPatches : ModBase
    {
        public static SettingHandle<bool> forceItemProcessorStart { get; private set; }
        public static SettingHandle<bool> checkPlanterFertility { get; private set; }
        public static SettingHandle<bool> shootExtinguisherAtFires { get; private set; }
        public static SettingHandle<bool> nonViolentEquipTools { get; private set; }
        public static SettingHandle<bool> showFishingZoneCellCount { get; private set; }
        public static SettingHandle<bool> noRoyalJellyRefugees { get; private set; }
        public static SettingHandle<HomeAreaOrAlways> autoAllowMinedOres { get; private set; }
        public static SettingHandle<bool> previewIncubatedInsectoid { get; private set; }
        public static SettingHandle<HomeAreaOrAlways> autoAllowDroppedFruit { get; private set; }
        public static SettingHandle<bool> tofuIsNonMeat { get; private set; }

        public override void DefsLoaded()
        {
            forceItemProcessorStart = Settings.GetHandle(
                "forceItemProcessorStart",
                "BenLubarsVanillaExpandedPatches_forceItemProcessorStart_title".Translate(),
                "BenLubarsVanillaExpandedPatches_forceItemProcessorStart_desc".Translate(),
                true
            );

            checkPlanterFertility = Settings.GetHandle(
                "checkPlanterFertility",
                "BenLubarsVanillaExpandedPatches_checkPlanterFertility_title".Translate(),
                "BenLubarsVanillaExpandedPatches_checkPlanterFertility_desc".Translate(),
                true
            );
            checkPlanterFertility.NeverVisible = !IsModActive("VanillaExpanded.VFEFarming");

            nonViolentEquipTools = Settings.GetHandle(
                "nonViolentEquipTools",
                "BenLubarsVanillaExpandedPatches_nonViolentEquipTools_title".Translate(),
                "BenLubarsVanillaExpandedPatches_nonViolentEquipTools_desc".Translate(),
                // simple sidearms does this better already; default to disabled
                !IsModActive("PeteTimesSix.SimpleSidearms")
            );
            nonViolentEquipTools.NeverVisible = !IsModActive("VanillaExpanded.VWE");

            showFishingZoneCellCount = Settings.GetHandle(
                "showFishingZoneCellCount",
                "BenLubarsVanillaExpandedPatches_showFishingZoneCellCount_title".Translate(),
                "BenLubarsVanillaExpandedPatches_showFishingZoneCellCount_desc".Translate(),
                true
            );
            showFishingZoneCellCount.NeverVisible = !IsModActive("VanillaExpanded.VCEF");

            noRoyalJellyRefugees = Settings.GetHandle(
                "noRoyalJellyRefugees",
                "BenLubarsVanillaExpandedPatches_noRoyalJellyRefugees_title".Translate(),
                "BenLubarsVanillaExpandedPatches_noRoyalJellyRefugees_desc".Translate(),
                true
            );
            noRoyalJellyRefugees.NeverVisible = !IsModActive("OskarPotocki.VFE.Insectoid") || !ModLister.RoyaltyInstalled;

            autoAllowMinedOres = Settings.GetHandle(
                "autoAllowMinedOres",
                "BenLubarsVanillaExpandedPatches_autoAllowMinedOres_title".Translate(),
                "BenLubarsVanillaExpandedPatches_autoAllowMinedOres_desc".Translate(),
                HomeAreaOrAlways.Always,
                enumPrefix: "BenLubarsVanillaExpandedPatches_HomeAreaOrAlways_"
            );
            autoAllowMinedOres.NeverVisible = !IsModActive("OskarPotocki.VFE.Insectoid") && !IsModActive("OskarPotocki.VFE.Mechanoid");

            previewIncubatedInsectoid = Settings.GetHandle(
                "previewIncubatedInsectoid",
                "BenLubarsVanillaExpandedPatches_previewIncubatedInsectoid_title".Translate(),
                "BenLubarsVanillaExpandedPatches_previewIncubatedInsectoid_desc".Translate(),
                true
            );
            previewIncubatedInsectoid.NeverVisible = !IsModActive("OskarPotocki.VFE.Insectoid");

            autoAllowDroppedFruit = Settings.GetHandle(
                "autoAllowDroppedFruit",
                "BenLubarsVanillaExpandedPatches_autoAllowDroppedFruit_title".Translate(),
                "BenLubarsVanillaExpandedPatches_autoAllowDroppedFruit_desc".Translate(),
                HomeAreaOrAlways.HomeArea,
                enumPrefix: "BenLubarsVanillaExpandedPatches_HomeAreaOrAlways_"
            );
            autoAllowDroppedFruit.NeverVisible = !IsModActive("VanillaExpanded.VBrewECandT") || !IsModActive("VanillaExpanded.VPlantsE");

            tofuIsNonMeat = Settings.GetHandle(
                "tofuIsNonMeat",
                "BenLubarsVanillaExpandedPatches_tofuIsNonMeat_title".Translate(),
                "BenLubarsVanillaExpandedPatches_tofuIsNonMeat_desc".Translate(),
                true
            );
            tofuIsNonMeat.NeverVisible = !IsModActive("VFME.SimpleTofu");
        }

        public static bool IsModActive(string packageId)
        {
            foreach (var mod in ModLister.AllInstalledMods)
            {
                if (mod.SamePackageId(packageId))
                {
                    return mod.Active;
                }
            }

            return false;
        }
    }
}
