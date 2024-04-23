using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib;
using HugsLib.Settings;

using UseYourGun.Utilities;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using System.Xml;


/// <summary>
/// Thank you to RunAndGun author "roolo" on Steam for allowing me to use his code!
/// Anything below this point is his code merely slightly changed to work with my assembly!
/// </summary>



namespace UseYourGun
{
    public class Base : ModBase
    {
        public override string ModIdentifier
        {
            get { return "UseYourGun"; }
        }

        public static Base Instance { get; private set; }
        internal static SettingHandle<DictWeaponRecordHandler> weaponForbidder;
        internal static SettingHandle<String> tabsHandler;
        List<ThingDef> allWeapons;

        private static Color highlight1 = new Color(0.5f, 0, 0, 0.1f);
        String[] tabNames = { "UYG_tab1".Translate() };


        public Base()
        {
            Instance = this;
        }
        public override void DefsLoaded()
        {
            allWeapons = WeaponUtility.getAllWeapons();

            tabsHandler = Settings.GetHandle<String>("tabs", "UYG_Tabs_Title".Translate(), "", "none");
            tabsHandler.CustomDrawer = rect => { return DrawUtility.CustomDrawer_Tabs(rect, tabsHandler, tabNames); };

            weaponForbidder = Settings.GetHandle<DictWeaponRecordHandler>("weaponForbidder_new", null, "UYG_WeaponForbidder_Description".Translate(), null);
            weaponForbidder.VisibilityPredicate = delegate { return tabsHandler.Value == tabNames[0]; };

            weaponForbidder.CustomDrawer = rect => { return DrawUtility.CustomDrawer_MatchingWeapons_active(rect, weaponForbidder, allWeapons, null, "UYG_Allow".Translate(), "UYG_Forbid".Translate()); };

            DrawUtility.filterWeapons(ref weaponForbidder, allWeapons, null);

        }
        internal void ResetForbidden()
        {
            weaponForbidder.Value = null;
            DrawUtility.filterWeapons(ref weaponForbidder, allWeapons, null);
        }

    }
}
