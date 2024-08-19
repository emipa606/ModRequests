using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace WoodIsNaturalForTrees;

public class WoodIsNaturalForTreesSettings : ModSettings
{
    public List<StuffCategoryDef> stuffMadeFrom = new()
    {
        StuffCategoryDefOf.Woody,
    };

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Collections.Look(ref stuffMadeFrom, nameof(stuffMadeFrom), LookMode.Def);

        stuffMadeFrom?.RemoveAll(x => x == null);
        if (stuffMadeFrom != null) return;

        if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
        {
            stuffMadeFrom = new List<StuffCategoryDef>
            {
                StuffCategoryDefOf.Woody,
            };
        }
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        listing.ColumnWidth = 270f;

        listing.Label("WoodIsNaturalAllowedTypes".Translate());

        foreach (var def in DefDatabase<StuffCategoryDef>.AllDefs)
        {
            var value = stuffMadeFrom.Contains(def);
            var currentValue = value;
            listing.CheckboxLabeled(def.label.CapitalizeFirst(), ref value);

            if (currentValue == value) continue;
            if (currentValue)
                stuffMadeFrom.Remove(def);
            else
                stuffMadeFrom.Add(def);
        }

        listing.End();
    }
}