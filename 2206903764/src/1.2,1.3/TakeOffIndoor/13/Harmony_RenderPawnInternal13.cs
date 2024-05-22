using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace TakeOffIndoor
{

    //[HarmonyPatch(typeof(PawnRenderer))]
    //[HarmonyPatch("RenderPawnAt")]
    //[HarmonyPatch("DrawBodyApparel")]
    //[HarmonyPatch("RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(RotDrawMode), typeof(PawnRenderFlags) })]


    //[HarmonyPatch(typeof(PawnCacheRenderer))]
    //[HarmonyPatch("RenderPawn")]


    //[HarmonyPriority(int.MaxValue)]
    //[HarmonyBefore("com.rimworld.Dalrae.Garam_RaceAddon")]
    public static class Harmony_Render
    {
        public static Pawn nowPawn = null;

        //public static void OnPostRender(Pawn ___pawn)
        //{
        //    //debug.WriteLine(___pawn.Name + ":OnPostRender");

        //}
        //public static bool ResolveApparelGraphics(Pawn ___pawn)
        //{
        //    debug.WriteLine(___pawn.Name+ ":ResolveApparelGraphics");
        //    return true;

        //}

        //public static void RendererTick(Pawn ___pawn)
        //{
        //    //debug.WriteLine(___pawn.Name + ":RendererTick");

        //}

        //public static bool RenderCache(Pawn ___pawn)
        //{
        //    //debug.WriteLine(___pawn.Name + ":RenderCache");
        //    return true;
        //}
        //public static void RenderPawnAt(PawnRenderer __instance, Pawn ___pawn, Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
        //{
        //    //debug.WriteLine(___pawn.Name + ":RenderPawnAt");
        //}

        //public static bool RenderPawnInternal(Pawn ___pawn)
        //{
        //    //debug.WriteLine(___pawn.Name + ":RenderPawnInternal");
        //    return true;
        //}


        //public static bool DrawBodyApparel(Pawn ___pawn)
        //{
        //    //debug.WriteLine(___pawn.Name + ":DrawBodyApparel");
        //    return true;
        //}

        //public static bool DrawDynamicParts(Pawn ___pawn)
        //{
        //    //debug.WriteLine(___pawn.Name + ":DrawDynamicParts");
        //    return true;
        //}

        //public static bool DrawPawnBody(Pawn ___pawn)
        //{
        //    //debug.WriteLine(___pawn.Name + ":DrawPawnBody");
        //    return true;
        //}

        public static void Patch(Harmony harmony)
        {

            MethodInfo original; HarmonyMethod prefix, postfix;

            original = AccessTools.Method(typeof(PawnRenderer), "RendererTick");
            //prefix = Util.HM(typeof(Harmony_Render), "RendererTick");
            prefix = Util.HM(typeof(Harmony_Render), "RenderTick_Prefix");
            harmony.Patch(original, prefix, null);

            original = AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal");
            //prefix = Util.HM(typeof(Harmony_Render), "RenderPawnInternal");
            prefix = Util.HM(typeof(Harmony_Render), "RenderPawnInternal_Prefix");
            harmony.Patch(original, prefix, null);

            //original = AccessTools.Method(typeof(PawnCacheRenderer), "OnPostRender");
            // prefix = Util.HM(typeof(Harmony_Render), "OnPostRender"); 
            //harmony.Patch(original, prefix, null);

            //original = AccessTools.Method(typeof(PawnRenderer), "RendererTick");
            ////prefix = Util.HM(typeof(Harmony_Render), "RendererTick");
            //prefix = Util.HM(typeof(Harmony_Render), "Prefix");
            //harmony.Patch(original, prefix, null);

            ////original = AccessTools.Method(typeof(PawnRenderer), "RenderCache");
            ////prefix = Util.HM(typeof(Harmony_Render), "RenderCache");
            ////harmony.Patch(original, prefix, null);

            ////original = AccessTools.Method(typeof(PawnRenderer), "RenderPawnAt");
            ////prefix = Util.HM(typeof(Harmony_Render), "RenderPawnAt");
            ////harmony.Patch(original, prefix, null);

            ////original = AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics");
            ////prefix = Util.HM(typeof(Harmony_Render), "ResolveApparelGraphics");
            ////harmony.Patch(original, prefix, null);


            //original = AccessTools.Method(typeof(PawnRenderer), "DrawBodyApparel");
            //prefix = Util.HM(typeof(Harmony_Render), "DrawBodyApparel");
            //harmony.Patch(original, prefix, null);

            //original = AccessTools.Method(typeof(PawnRenderer), "DrawDynamicParts");
            //prefix = Util.HM(typeof(Harmony_Render), "DrawDynamicParts");
            //harmony.Patch(original, prefix, null);

            //original = AccessTools.Method(typeof(PawnRenderer), "DrawPawnBody");
            //prefix = Util.HM(typeof(Harmony_Render), "DrawPawnBody");
            //harmony.Patch(original, prefix, null);

            original = AccessTools.PropertySetter(typeof(Pawn_DraftController), "Drafted");
            postfix = Util.HM(typeof(Harmony_Render), "NotifyPawnDraftChanged");
            harmony.Patch(original, null, postfix);
        }


        public static void RenderPawnInternal_Prefix(Pawn ___pawn, PawnRenderFlags flags)
        {
            RenderCheck(___pawn, flags.FlagSet(PawnRenderFlags.Portrait));
        }

        public static void RenderTick_Prefix(Pawn ___pawn)
        {
            RenderCheck(___pawn, false);
        }

        public static void RenderCheck(Pawn pawn,bool portrait)
        {
            if (!pawn.IsColonistPlayerControlled)
            {
                return;
            }
            TakeOffComp comp = pawn.GetComp<TakeOffComp>();

            if (comp == null)
            {
                return;
            }

            if (!comp.IsTargetPawn)
            {
                return;
            }

            if (!comp.initialized)
            {
                comp.Initialize();
            }

            bool init = comp.first;
            bool flg = init;

            if (TakeOffSettings.portraitOption)
            {
                if (portrait)
                {
                    comp.portrait = TakeOffSettings.dontApplyPortrait;
                    comp.tempHatsInvisible = TakeOffSettings.hatsOnlyOnMap;
                }
                else
                {
                    comp.portrait = false;
                    comp.tempHatsInvisible = false;
                }

                if (init && ModUtil.PawnIsGaramRace(pawn))
                {
                    comp.Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                }
                else
                {
                    comp.Pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
                }
            }
            else
            {
                comp.portrait = false;
                comp.tempHatsInvisible = false;
            }

            if (!portrait)
            {
                if (comp.ChkSettingsUpdate)
                {
                    comp.UpdateShowLayerLevel();
                    flg = true;
                }
                if (comp.ChkLayerLevelChanged)
                {
                    flg = true;
                }
                if (flg)
                {
                    RenderUtil.NotifyApparelChanged(comp);
                }
            }
        }

        public static void NotifySettingsChanged()
        {
            List<Pawn> colonists = Find.CurrentMap?.mapPawns?.FreeColonists;
            //debug.WriteLine("nsc:"+colonists?.Count);
            if (colonists != null)
            {
                foreach(Pawn p in colonists)
                {
                    RenderCheck(p, false);
                }
            }
        }

        public static void NotifyPawnDraftChanged(Pawn_DraftController __instance)
        {
            RenderCheck(__instance.pawn, false);
        }


    }
}
