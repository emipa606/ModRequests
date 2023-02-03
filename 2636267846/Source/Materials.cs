using System.Linq;
using UnityEngine;
using Verse;

namespace CensoredNudity
{
    [StaticConstructorOnStartup]
    public static class Materials
    {
        static Materials()
        {
            // HugsLib doesn't load us on the main thread, so we have to do the loading using vanilla mechanisms.
            var mod = LoadedModManager.RunningMods.First(m => m.PackageId == "me.lubar.censorednudity");
            string bundleName = "windows64";
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                    if (UnityData.Is32BitBuild)
                    {
                        bundleName = "windows32";
                    }
                    break;
                case RuntimePlatform.LinuxPlayer:
                    bundleName = "linux";
                    break;
                case RuntimePlatform.OSXPlayer:
                    bundleName = "osx";
                    break;
                default:
                    Log.Error($"[CensoredNudity] Unhandled runtime platform {Application.platform}");
                    break;
            }
            var bundle = mod.assetBundles.loadedAssetBundles.FirstOrFallback(b => b.name == bundleName);
            if (bundle == null)
            {
                bundle = mod.assetBundles.loadedAssetBundles.First();
                Log.Error($"[CensoredNudity] Could not find asset bundle '{bundleName}' - using bundle '{bundle.name}' as fallback");
            }

            var shader = bundle.LoadAsset<Shader>("Assets/CensorShaderMaskedCutout.shader");

            var barHairBackMat = MaterialPool.MatFrom("CensoredNudity/BarHair_Back");
            var barFaceBackMat = MaterialPool.MatFrom("CensoredNudity/BarFace_Back");
            var barChestBackMat = MaterialPool.MatFrom("CensoredNudity/BarChest_Back");
            var barGroinBackMat = MaterialPool.MatFrom("CensoredNudity/BarGroin_Back");

            var barHairSideMat = MaterialPool.MatFrom("CensoredNudity/BarHair_Side");
            var barFaceSideMat = MaterialPool.MatFrom("CensoredNudity/BarFace_Side");
            var barChestSideMat = MaterialPool.MatFrom("CensoredNudity/BarChest_Side");
            var barGroinSideMat = MaterialPool.MatFrom("CensoredNudity/BarGroin_Side");

            var barHairFrontMat = MaterialPool.MatFrom("CensoredNudity/BarHair_Front");
            var barFaceFrontMat = MaterialPool.MatFrom("CensoredNudity/BarFace_Front");
            var barChestFrontMat = MaterialPool.MatFrom("CensoredNudity/BarChest_Front");
            var barGroinFrontMat = MaterialPool.MatFrom("CensoredNudity/BarGroin_Front");

            var mosaicHairBackMat = MaterialPool.MatFrom("CensoredNudity/MosaicHair_Back", shader);
            var mosaicFaceBackMat = MaterialPool.MatFrom("CensoredNudity/MosaicFace_Back", shader);
            var mosaicChestBackMat = MaterialPool.MatFrom("CensoredNudity/MosaicChest_Back", shader);
            var mosaicGroinBackMat = MaterialPool.MatFrom("CensoredNudity/MosaicGroin_Back", shader);

            var mosaicHairSideMat = MaterialPool.MatFrom("CensoredNudity/MosaicHair_Side", shader);
            var mosaicFaceSideMat = MaterialPool.MatFrom("CensoredNudity/MosaicFace_Side", shader);
            var mosaicChestSideMat = MaterialPool.MatFrom("CensoredNudity/MosaicChest_Side", shader);
            var mosaicGroinSideMat = MaterialPool.MatFrom("CensoredNudity/MosaicGroin_Side", shader);

            var mosaicHairFrontMat = MaterialPool.MatFrom("CensoredNudity/MosaicHair_Front", shader);
            var mosaicFaceFrontMat = MaterialPool.MatFrom("CensoredNudity/MosaicFace_Front", shader);
            var mosaicChestFrontMat = MaterialPool.MatFrom("CensoredNudity/MosaicChest_Front", shader);
            var mosaicGroinFrontMat = MaterialPool.MatFrom("CensoredNudity/MosaicGroin_Front", shader);

            HairMat = new[]
            {
                new[] { barHairBackMat, barHairSideMat, barHairFrontMat, barHairSideMat },
                new[] { mosaicHairBackMat, mosaicHairSideMat, mosaicHairFrontMat, mosaicHairSideMat }
            };
            FaceMat = new[]
            {
                new[] { barFaceBackMat, barFaceSideMat, barFaceFrontMat, barFaceSideMat },
                new[] { mosaicFaceBackMat, mosaicFaceSideMat, mosaicFaceFrontMat, mosaicFaceSideMat }
            };
            ChestMat = new[]
            {
                new[] { barChestBackMat, barChestSideMat, barChestFrontMat, barChestSideMat },
                new[] { mosaicChestBackMat, mosaicChestSideMat, mosaicChestFrontMat, mosaicChestSideMat }
            };
            GroinMat = new[]
            {
                new[] { barGroinBackMat, barGroinSideMat, barGroinFrontMat, barGroinSideMat },
                new[] { mosaicGroinBackMat, mosaicGroinSideMat, mosaicGroinFrontMat, mosaicGroinSideMat }
            };
        }

        public static Material[][] HairMat { get; private set; }
        public static Material[][] FaceMat { get; private set; }
        public static Material[][] ChestMat { get; private set; }
        public static Material[][] GroinMat { get; private set; }
    }
}
