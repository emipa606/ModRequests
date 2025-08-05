using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BDsPlasmaWeapon
{
    public class Setting : ModSettings
    {
        public bool OverchargeDontReturnCasing = true;

        public bool OverchargeDamageWeapon = true;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref OverchargeDontReturnCasing, "OverchargeDontReturnCasing", defaultValue: true);
            Scribe_Values.Look(ref OverchargeDamageWeapon, "OverchargeDamageWeapon", defaultValue: true);
        }
    }

    public class BDPMod : Mod
    {
        public static Setting settings;

        public static bool OverchargeDontReturnCasing => settings.OverchargeDontReturnCasing;
        public static bool OverchargeDamageWeapon => settings.OverchargeDamageWeapon;

        public BDPMod(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<Setting>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (inRect.width - 17f) / 2f;
            listing_Standard.Begin(inRect);
            Text.Font = GameFont.Small;
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("BDP_OverchargeDontReturnCasing_Title".Translate(), ref settings.OverchargeDontReturnCasing, "BDP_OverchargeDontReturnCasing_Desc".Translate());
            listing_Standard.CheckboxLabeled("BDP_OverchargeDamageWeapon_Title".Translate(), ref settings.OverchargeDamageWeapon, "BDP_OverchargeDamageWeapon_Desc".Translate());
            listing_Standard.End();
        }

        public override string SettingsCategory()
        {
            return "BDP_Setting".Translate();
        }
    }
}
