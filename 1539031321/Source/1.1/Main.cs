using System;
using Verse;
using HarmonyLib;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace StatueOfColonist {
    [StaticConstructorOnStartup]
    class Main {
        static Main() {
            var harmony = new Harmony("com.tammybee.rimstatue");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            StatueOfColonistPref.LoadPref();
        }
    }

    [HarmonyPatch(typeof(Blueprint))]
    [HarmonyPatch("Draw")]
    class Blueprint_Draw_Patch {
        private static Color BlueprintColor = new Color(0.5f, 0.5f, 1f, 0.35f);

        static void Postfix(Blueprint __instance) {
            Blueprint_Install bp = __instance as Blueprint_Install;
            if (bp != null) {
                Building_StatueOfColonist statue = bp.MiniToInstallOrBuildingToReinstall.GetInnerIfMinified() as Building_StatueOfColonist;
                if (statue != null) {
                    if (!statue.IsValid) {
                        Pawn p = __instance.Map.mapPawns.FreeColonistsAndPrisoners.RandomElement();
                        statue.ResolveGraphics(p);
                    }

                    Vector3 pos = __instance.DrawPos + new Vector3(statue.Def.offsetX, 0f, statue.Def.offsetZ);
                    statue.Render(pos, Quaternion.identity, true, __instance.Rotation, __instance.Rotation, RotDrawMode.Fresh, false, false, statue.Def.scale, Building_StatueOfColonist.RenderMode.Blueprint);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Designator_Install))]
    [HarmonyPatch("DrawGhost")]
    class Designator_Install_DrawGhost_Patch {
        static bool Prefix(Designator_Install __instance, Color ghostCol) {
            Traverse trv = Traverse.Create(__instance);
            Thing thing = trv.Property("ThingToInstall").GetValue<Thing>();

            Building_StatueOfColonist statue = thing as Building_StatueOfColonist;
            if (statue != null) {
                if (!statue.IsValid) {
                    Pawn p = statue.Map.mapPawns.FreeColonists.RandomElement();
                    statue.ResolveGraphics(p);
                }

                Rot4 rot = trv.Field("placingRot").GetValue<Rot4>();
                Vector3 loc = GenThing.TrueCenter(UI.MouseCell(), rot, statue.def.Size, Altitudes.AltitudeFor(AltitudeLayer.Blueprint));
                Vector3 pos = loc + new Vector3(statue.Def.offsetX, 0f, statue.Def.offsetZ);

                bool canDesignate = __instance.CanDesignateCell(UI.MouseCell()).Accepted;
                Building_StatueOfColonist.RenderMode renderMode = Building_StatueOfColonist.RenderMode.CanDesignateGhost;
                if (!canDesignate) {
                    renderMode = Building_StatueOfColonist.RenderMode.CanNotDesignateGhost;
                }
                statue.Render(pos, Quaternion.identity, true, rot, rot, RotDrawMode.Fresh, false, false, statue.Def.scale,renderMode);

                Graphic baseGraphic = thing.Graphic.ExtractInnerGraphicFor(thing);
                IntVec3 locBase = loc.ToIntVec3();
                if (statue.Def.size == new IntVec2(2, 2)) {
                    locBase = locBase + new IntVec3(-1, 0, -1);
                }
                GhostDrawer.DrawGhostThing(locBase, Rot4.North, (ThingDef)__instance.PlacingDef, baseGraphic, ghostCol, AltitudeLayer.Blueprint);
                return false;
            }

            return true;
        }
    }
    /*
    [HarmonyPatch(typeof(StockGenerator_BuyArt))]
    [HarmonyPatch("HandlesThingDef")]
    class StockGenerator_BuyArt_HandlesThingDef_Patch {
        static void Postfix(ThingDef thingDef,ref bool __result) {
            if (thingDef.thingClass == typeof(Building_StatueOfColonist)) {
                __result = true;
            }
        }
    }
    */


    [HarmonyPatch(typeof(Graphics))]
    [HarmonyPatch("Internal_DrawMesh")]
    public class Graphics_Internal_DrawMesh_Patch {
        public static float scale = 1f;
        
        public static void Prefix(ref Matrix4x4 matrix) {
            matrix.m00 *= scale;
            matrix.m22 *= scale;
        }

        public static void Reset() {
            scale = 1f;
        }
    }
}
