using System;
using Verse;
using Harmony;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace StatueOfAnimal {
    [StaticConstructorOnStartup]
    class Main {
        static Main() {
            var harmony = HarmonyInstance.Create("com.tammybee.statueofanimal");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            StatueOfAnimalPref.LoadPref();
        }
    }

    [HarmonyPatch(typeof(Blueprint))]
    [HarmonyPatch("Draw")]
    class Blueprint_Draw_Patch {
        private static Color BlueprintColor = new Color(0.5f, 0.5f, 1f, 0.35f);

        static void Postfix(Blueprint __instance) {
            Blueprint_Install bp = __instance as Blueprint_Install;
            if (bp != null) {
                Building_StatueOfAnimal statue = bp.MiniToInstallOrBuildingToReinstall.GetInnerIfMinified() as Building_StatueOfAnimal;
                if (statue != null) {
                    if (!statue.IsValid) {
                        statue.Data = new StatueOfAnimalData(statue.DrawColor);
                        statue.ResolveGraphics();
                    }
                    //Log.Message("Blueprint.Draw");

                    statue.DrawBase(__instance.DrawPos, Building_StatueOfAnimal.RenderMode.Blueprint);
                    Vector3 pos = __instance.DrawPos + new Vector3(statue.Def.offsetX, 0f, statue.Def.offsetZ);
                    statue.Render(pos, __instance.Rotation, false, statue.Def.scale, statue.Data.packed, Building_StatueOfAnimal.RenderMode.Blueprint);
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

            Building_StatueOfAnimal statue = thing as Building_StatueOfAnimal;
            if (statue != null) {
                if (!statue.IsValid) {
                    statue.Data = new StatueOfAnimalData(statue.DrawColor);
                    statue.ResolveGraphics();
                }
                //Log.Message("DrawGhost");

                Rot4 rot = trv.Field("placingRot").GetValue<Rot4>();
                Vector3 loc = GenThing.TrueCenter(UI.MouseCell(), rot, statue.def.Size, Altitudes.AltitudeFor(AltitudeLayer.Blueprint));
                Vector3 pos = loc + new Vector3(statue.Def.offsetX, 0f, statue.Def.offsetZ);


                bool canDesignate = __instance.CanDesignateCell(UI.MouseCell()).Accepted;
                Building_StatueOfAnimal.RenderMode renderMode = Building_StatueOfAnimal.RenderMode.CanDesignateGhost;
                if (!canDesignate) {
                    renderMode = Building_StatueOfAnimal.RenderMode.CanNotDesignateGhost;
                }
                statue.DrawBase(loc, renderMode);
                statue.Render(pos, rot, false, statue.Def.scale, statue.Data.packed, renderMode);

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

    [HarmonyPatch(typeof(StockGenerator_BuyArt))]
    [HarmonyPatch("HandlesThingDef")]
    class StockGenerator_BuyArt_HandlesThingDef_Patch {
        static void Postfix(ThingDef thingDef,ref bool __result) {
            if (thingDef.thingClass == typeof(Building_StatueOfAnimal)) {
                __result = true;
            }
        }
    }


    [HarmonyPatch(typeof(Graphics))]
    [HarmonyPatch("DrawMeshImpl")]
    public class Graphics_DrawMeshImpl_Patch {
        public static float scaleX = 1f;
        public static float scaleY = 1f;

        public static void Prefix(ref Matrix4x4 matrix) {
            matrix.m00 *= scaleX;
            matrix.m22 *= scaleY;
        }

        public static void Reset() {
            scaleX = 1f;
            scaleY = 1f;
        }
    }
}
