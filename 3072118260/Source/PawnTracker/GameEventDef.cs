using Verse;
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using static PawnTrackerMain.PawnTrackerUtility;
using System.Linq;
using static PawnTrackerMain.DocumentedEventDefOf;
using System.Data.SqlTypes;
using UnityEngine;
using Verse.Noise;
using System.Text;

namespace PawnTrackerMain {
    public class GameEventDef : ThingDef
    {
        public bool canEnd = true;
        public string resolutionString = "that lasted";

        public override void PostLoad()
        {
            base.PostLoad();
        }
    }
}