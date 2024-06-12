using JetBrains.Annotations;
using Verse;

namespace NightVision
{
    public class CompProperties_NightVisionApparel : CompProperties
    {
        public ApparelVisionSetting AppVisionSetting;
        public bool GrantsNightVision = false;
        public bool NullifiesPhotosensitivity = false;

        [UsedImplicitly]
        public CompProperties_NightVisionApparel() => compClass = typeof(Comp_NightVisionApparel);
    }
}