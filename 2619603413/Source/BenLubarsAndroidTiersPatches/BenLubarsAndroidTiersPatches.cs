using HugsLib;
using HugsLib.Settings;
using Verse;

namespace BenLubarsAndroidTiersPatches
{
    public class BenLubarsAndroidTiersPatches : ModBase
    {
        internal static BenLubarsAndroidTiersPatches Instance;

        public BenLubarsAndroidTiersPatches()
        {
            Instance = this;
        }

        internal SettingHandle<bool> patchSpaceSuit;
        internal SettingHandle<bool> patchChronicDiseases;
        internal SettingHandle<BoolUnknown> patchBasicIdeoligion;
        internal SettingHandle<bool> patchNoJoy;

        public override void DefsLoaded()
        {
            patchSpaceSuit = Settings.GetHandle(
                "patch_spaceSuit",
                "BenLubarsAndroidTiersPatches_patch_spaceSuit_title".Translate(),
                "BenLubarsAndroidTiersPatches_patch_spaceSuit_desc".Translate(),
                true
            );
            patchSpaceSuit.NeverVisible = !IsModActive("kentington.saveourship2");

            patchChronicDiseases = Settings.GetHandle(
                "patch_chronicDiseases",
                "BenLubarsAndroidTiersPatches_patch_chronicDiseases_title".Translate(),
                "BenLubarsAndroidTiersPatches_patch_chronicDiseases_desc".Translate(),
                true
            );

            patchBasicIdeoligion = Settings.GetHandle(
                "patch_basicIdeoligion",
                "BenLubarsAndroidTiersPatches_patch_basicIdeoligion_title".Translate(),
                "BenLubarsAndroidTiersPatches_patch_basicIdeoligion_desc".Translate(),
                BoolUnknown.False,
                enumPrefix: "BenLubarsAndroidTiersPatches_patch_basicIdeoligion_"
            );
            patchBasicIdeoligion.NeverVisible = !ModLister.IdeologyInstalled;

            var legacyIdeoligion = Settings.PeekValue("patch_cantHaveIdeoligion");
            if (legacyIdeoligion != null)
            {
                patchBasicIdeoligion.Value = bool.Parse(legacyIdeoligion) ? BoolUnknown.False : BoolUnknown.Unknown;
                Settings.TryRemoveUnclaimedValue("patch_cantHaveIdeoligion");
                Settings.SaveChanges();
            }

            patchNoJoy = Settings.GetHandle(
                "patch_noJoy",
                "BenLubarsAndroidTiersPatches_patch_noJoy_title".Translate(),
                "BenLubarsAndroidTiersPatches_patch_noJoy_desc".Translate(),
                true
            );
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
