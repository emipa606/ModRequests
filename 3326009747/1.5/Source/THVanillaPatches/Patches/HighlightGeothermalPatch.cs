using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace THVanillaPatches.Patches
{
    [StaticConstructorOnStartup]
    public class HighlightGeothermalPatch(THPatchDef def) : ToggleablePatchParent(def)
    {
        private static readonly Material HighlightMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0, 255, 0, 0.25f));

        
        //PlaceWorker_OnSteamGeyser
        protected override List<PatchInfo> Patches => new List<PatchInfo>()
        {
            new PatchInfo(AccessTools.Method(typeof(PlaceWorker), nameof(PlaceWorker.DrawGhost)),
                Postfix: new HarmonyMethod(GetType(), nameof(DrawGhost)))
        };
        
        
        private static void DrawGhost(PlaceWorker __instance, ThingDef def,IntVec3 center,Rot4 rot,Color ghostCol,Thing thing = null)
        {
            if (__instance is not PlaceWorker_OnSteamGeyser) return;
            Vector3 s = new Vector3(6f, 1f, 6f);

            foreach (Thing geyser in Find.CurrentMap.listerThings.ThingsOfDef(ThingDefOf.SteamGeyser
                     ))
            {
                Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(geyser.DrawPos with { y = 14.99f }, Quaternion.identity, s), HighlightMat, 0);

                //GenDraw.Draw
                    
                //GenDraw.DrawMeshNowOrLater(MeshPool.plane10, Matrix4x4.TRS(geyser.DrawPos with { y = 14.99f }, Quaternion.identity, s), HighlightMat, false);
            }
        }
    }
}