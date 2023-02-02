using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RenameGun
{
    [StaticConstructorOnStartup]
    public static class RenameGunStartup
    {
        static RenameGunStartup()
        {
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (thingDef.IsWeapon)
                {
                    if (thingDef.comps is null)
                    {
                        thingDef.comps = new List<CompProperties>();
                    }
                    thingDef.comps.Add(new CompProperties_FixedName());
                }
            }
            new Harmony("RenameGun.Mod").PatchAll();
        }
    }

    public class RenameGunMod : Mod
    {
        public static RenameGunSettings settings;
        public RenameGunMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<RenameGunSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return this.Content.Name;
        }
    }

    public class RenameGunSettings : ModSettings
    {
        public static bool allowPawnsToRenameGuns = true;
        public static bool alwaysKeepPlayerSetNames = true;
        public static float holdingPeriodInDaysForAutoRename = 3f;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref holdingPeriodInDaysForAutoRename, "holdingPeriodInDaysForAutoRename");
            Scribe_Values.Look(ref allowPawnsToRenameGuns, "allowPawnsToRenameGuns");
            Scribe_Values.Look(ref alwaysKeepPlayerSetNames, "alwaysKeepPlayerSetNames");
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled("RG.AllowColonistsToRenameGuns".Translate(), ref allowPawnsToRenameGuns);
            listingStandard.CheckboxLabeled("RG.AlwaysKeepPlayerSetNames".Translate(), ref alwaysKeepPlayerSetNames);
            listingStandard.SliderLabeled("RG.HoldingPeriodInDaysForAutoRename".Translate(), ref holdingPeriodInDaysForAutoRename, 
                "PeriodDays".Translate(holdingPeriodInDaysForAutoRename.ToStringDecimalIfSmall()), 0, 60);
            listingStandard.End();
        }
    }
}
