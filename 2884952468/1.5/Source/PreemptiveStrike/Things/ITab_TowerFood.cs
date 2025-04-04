﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace PreemptiveStrike.Things
{
    class ITab_TowerFood : ITab
    {
        private Vector2 scrollPosition = default(Vector2);

        private static readonly Vector2 WinSize = new Vector2(300f, 480f);

        private ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();
        private IStoreSettingsParent SelStoreSettingsParent
        {
            get
            {
                return ((ThingWithComps)SelObject).GetComp<CompTowerFoodRefuelable>();
            }
        }

        public override bool IsVisible
        {
            get
            {
                if (SelStoreSettingsParent == null)
                    return false;
                return SelStoreSettingsParent.StorageTabVisible;
            }
        }

        public ITab_TowerFood()
        {
            size = WinSize;
            labelKey = "PES_RefuelTab";
        }

        public override void FillTab()
        {
            IStoreSettingsParent selStoreSettingsParent = SelStoreSettingsParent;
            if (selStoreSettingsParent == null) return;
            StorageSettings storeSettings = selStoreSettingsParent.GetStoreSettings();
            Rect rect = GenUI.ContractedBy(new Rect(0f, 0f, WinSize.x, WinSize.y), 10f);
            GUI.BeginGroup(rect);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            //Widgets.Label(new Rect(rect)
            //{
            //    height = 32f
            //}, "RefuelTitle");
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            ThingFilter thingFilter = null;
            if (selStoreSettingsParent.GetParentStoreSettings() != null)
            {
                thingFilter = selStoreSettingsParent.GetParentStoreSettings().filter;
            }
            Rect rect2 = new Rect(0f, 40f, rect.width, rect.height - 40f);

            ThingFilterUI.DoThingFilterConfigWindow(rect2, thingFilterState, storeSettings.filter, thingFilter, 8, null, null, false, false, false, null, null);  //Replaced ref scrollPosition with null; second variable
            GUI.EndGroup();
        }
    }
}
