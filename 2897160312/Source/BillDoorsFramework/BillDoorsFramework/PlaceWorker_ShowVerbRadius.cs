using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class PlaceWorker_ShowVerbRadius : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (VerbProperties verbProperties in ((ThingDef)checkingDef).building.turretGunDef.Verbs)
            {
                if (verbProperties.range > 0f)
                {
                    GenDraw.DrawRadiusRing(loc, verbProperties.range);
                }
                if (verbProperties.minRange > 0f)
                {
                    GenDraw.DrawRadiusRing(loc, verbProperties.minRange);
                }
            }
            return true;
        }
    }

    [StaticConstructorOnStartup]
    public class PlaceWorker_ShowVerbRadiusBySight : PlaceWorker
    {
        IntVec3 locCache;
        List<IntVec3> cellCache = new List<IntVec3>();
        List<IntVec3> badCellCache = new List<IntVec3>();
        Material redMat;
        Material greenMat;

        public PlaceWorker_ShowVerbRadiusBySight()
        {
            redMat = DebugMatsSpectrum.Mat(0, false);
            redMat.color = redMat.color.ToTransparent(0.1f);
            greenMat = DebugMatsSpectrum.Mat(50, false);
            greenMat.color = greenMat.color.ToTransparent(0.1f);
        }

        public override void DrawMouseAttachments(BuildableDef def)
        {
            string text = "BDF_ShowLosTip".Translate(KeyBindingDefOf.ShowEyedropper.MainKeyLabel);
            Vector2 mousePosition = Event.current.mousePosition;
            Vector2 vector = Text.CalcSize(text);
            Rect position = new Rect(mousePosition.x + 8f, mousePosition.y - vector.y, 32f, 32f);
            Rect rect = new Rect(position.xMax, position.y, 9999f, 100f);
            Widgets.DrawTextureRotated(new Rect(rect.x - vector.x * 0.1f, rect.y, vector.x * 1.2f, vector.y), TexUI.GrayTextBG, 0);
            Widgets.Label(rect, text);
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (KeyBindingDefOf.ShowEyedropper.IsDown)
            {
                if (locCache != loc)
                {
                    cellCache.Clear();
                    badCellCache.Clear();
                    foreach (VerbProperties verbProperties in ((ThingDef)checkingDef).building.turretGunDef.Verbs)
                    {
                        locCache = loc;
                        var cells = GenRadial.RadialCellsAround(loc, verbProperties.minRange, verbProperties.range).ToList();
                        foreach (var cell in cells)
                        {
                            if (GenSight.LineOfSight(loc, cell, map)) cellCache.Add(cell);
                            else badCellCache.Add(cell);
                        }
                    }
                }
                if (cellCache.Any())
                {
                    GenDraw.DrawFieldEdges(cellCache);
                    foreach (var cell in cellCache)
                    {
                        CellRenderer.RenderCell(cell, greenMat);
                    }
                }
                if (badCellCache.Any())
                {
                    foreach (var cell2 in badCellCache)
                    {
                        CellRenderer.RenderCell(cell2, redMat);
                    }
                }
            }
            else
            {
                foreach (VerbProperties verbProperties in ((ThingDef)checkingDef).building.turretGunDef.Verbs)
                {
                    if (verbProperties.range > 0)
                    {
                        GenDraw.DrawRadiusRing(loc, verbProperties.range);
                    }
                    if (verbProperties.minRange > 0)
                    {
                        GenDraw.DrawRadiusRing(loc, verbProperties.minRange);
                    }
                }
            }
            return true;
        }
    }
}
