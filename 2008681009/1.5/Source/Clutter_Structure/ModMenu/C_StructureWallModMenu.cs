using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;

namespace Clutter_StructureWall
{
    [StaticConstructorOnStartup]
    public class C_StructureWallModMenu : Mod
    {

        public static C_StructureWallModUI settings;

        public C_StructureWallModMenu(ModContentPack content) : base(content)
        {
            C_StructureWallModMenu.settings = base.GetSettings<C_StructureWallModUI>();

            //Log.Message("Clutter Furniture :: initialized");
        }

        public override string SettingsCategory()
        {
            return "Clutter Structure Walls";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            C_StructureWallModMenu.settings.DoWindowContents(inRect);
        }
    }
}
