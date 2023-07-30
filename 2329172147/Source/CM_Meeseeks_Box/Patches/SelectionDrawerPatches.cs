using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class SelectionDrawerPatches
    {
        public static Material TargetMarker = MaterialPool.MatFrom("Icons/MeeseeksTarget", ShaderDatabase.Cutout);

        [HarmonyPatch(typeof(SelectionDrawer))]
        [HarmonyPatch("DrawSelectionOverlays", MethodType.Normal)]
        static class SelectionDrawer_DrawSelectionOverlays
        {
            public static void Postfix()
            {
                if (WorldRendererUtility.WorldRenderedNow == false)
                {
                    Map map = Find.CurrentMap;

                    if (map == null)
                        return;

                    List<Pawn> selectedPawns = Find.Selector.SelectedPawns;
                    if (selectedPawns == null)
                        return;

                    CellRect currentView = Find.CameraDriver.CurrentViewRect;

                    foreach (Pawn pawn in selectedPawns)
                    {
                        CompMeeseeksMemory memory = pawn.GetComp<CompMeeseeksMemory>();
                        if (memory == null)
                            continue;

                        foreach(SavedTargetInfo target in memory.jobTargets)
                        {
                            if (currentView.Contains(target.target.Cell))
                            {
                                Matrix4x4 matrix = new Matrix4x4();
                                Vector3 position = new Vector3(target.Cell.x + 0.25f, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), target.target.Cell.z + 0.75f);
                                Vector3 scale = new Vector3(0.5f, 1.0f, 0.5f);
                                matrix.SetTRS(position, Quaternion.identity, scale);

                                Graphics.DrawMesh(MeshPool.plane10, matrix, TargetMarker, 0);
                            }
                        }
                    }
                }
            }
        }
    }
}
