﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Alerts;

internal class Alert_RogueAI_DrainingMood : Alert
{
    public Alert_RogueAI_DrainingMood()
    {
        defaultLabel = "WTH_Alert_RogueAI_DrainingMood_Label".Translate();
        defaultPriority = AlertPriority.High;
    }

    private IEnumerable<Building_RogueAI> RogueAIs
    {
        get
        {
            foreach (var map in Find.Maps)
            {
                foreach (var rAI in map.listerBuildings.AllBuildingsColonistOfDef(WTH_DefOf.WTH_RogueAI)
                             .Cast<Building_RogueAI>())
                {
                    if (rAI.CurrentlyDrainingMood)
                    {
                        yield return rAI;
                    }
                }
            }
        }
    }

    public override TaggedString GetExplanation()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine();
        return string.Format("WTH_Alert_RogueAI_DrainingMood_Description".Translate(), stringBuilder);
    }

    public override AlertReport GetReport()
    {
        return AlertReport.CulpritIs(RogueAIs.FirstOrDefault());
    }
}