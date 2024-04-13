using UnityEngine;
using Verse;

namespace JAHV
{
    public class JAHVMod : Mod
    {
        readonly JAHVSettings settings;

        public JAHVMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<JAHVSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(inRect);

            listingStandard.Label("JAHVmaxShipSize".Translate(settings.maxShipSizeX, settings.maxShipSizeY));
            listingStandard.Label("JAHVXSlider".Translate());
            settings.maxShipSizeX = (int)listingStandard.Slider(settings.maxShipSizeX, 0, 100);
            listingStandard.Label("JAHVYSlider".Translate());
            settings.maxShipSizeY = (int)listingStandard.Slider(settings.maxShipSizeY, 0, 100);

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "JAHVModSettings".Translate();
        }
    }
}
