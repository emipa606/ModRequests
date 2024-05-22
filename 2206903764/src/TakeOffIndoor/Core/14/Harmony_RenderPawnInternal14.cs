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

            //original = AccessTools.Method(typeof(PawnRenderer), "RendererTick");
            ////prefix = Util.HM(typeof(Harmony_Render), "RendererTick");
            //prefix = Util.HM(typeof(Harmony_Render), "RenderTick_Prefix");
            //harmony.Patch(original, prefix, null);

            original = AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal");
            //prefix = Util.HM(typeof(Harmony_Render), "RenderPawnInternal");
            prefix = Util.HM(typeof(Harmony_Render), nameof(Harmony_Render.RenderPawnInternal_Prefix));
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
            postfix = Util.HM(typeof(Harmony_Render), nameof(Harmony_Render.NotifyPawnDraftChanged));
            harmony.Patch(original, null, postfix);

            //original = AccessTools.Method(typeof(PawnCacheRenderer), nameof(PawnCacheRenderer.RenderPawn));
            //prefix = Util.HM(typeof(Harmony_Render), nameof(Harmony_Render.RenderPawn_Prefix));
            //harmony.Patch(original, prefix);


            //original = AccessTools.Method(typeof(PawnRenderer), "DrawHeadHair");
            //prefix = Util.HM(typeof(Harmony_Render), nameof(Harmony_Render.DrawHeadHair_Prefix_test));
            //harmony.Patch(original, prefix);
            //debug.WriteLine("DHH Patch");
        }


        public static void RenderPawnInternal_Prefix(Pawn ___pawn, PawnRenderFlags flags)
        {
            RenderCheck(___pawn, flags.FlagSet(PawnRenderFlags.Portrait));
        }

        //public static void RenderTick_Prefix(Pawn ___pawn)
        //{
        //    RenderCheck(___pawn, false);
        //}



        //static FieldInfo fiPawn = AccessTools.Field(typeof(PawnCacheRenderer), "pawn");
        //static FieldInfo fiPortrait = AccessTools.Field(typeof(PawnCacheRenderer), "portrait");
        //public static bool GetPawnChacheField(this Pawn pawn)
        //{

        //    Pawn cachePawn=(Pawn)fiPawn.GetValue(Find.PawnCacheRenderer);

        //    if (pawn == cachePawn)
        //    {
        //        debug.WriteLine("GPCF_T:"+fiPortrait.GetValue(Find.PawnCacheRenderer).ToString());
        //        return (bool)fiPortrait.GetValue(Find.PawnCacheRenderer);
        //    }

        //    debug.WriteLine("GPCF_F");
        //    return false;
        //}

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
                    //if (pawn.GetPawnChacheField())
                {
                    comp.portrait = TakeOffSettings.dontApplyPortrait;
                    comp.tempHatsInvisible = TakeOffSettings.hatsOnlyOnMap;

                }
                else
                {
                    comp.portrait = false;
                    comp.tempHatsInvisible = false;
                }
                if (init && ModUtil.PawnIsGaramRace(pawn)) //ここが原因でportraitオンの場合のみdubs bad hygieneと競合 → portrait=trueの時のみに変更→逆に通常時が正常表示されない→そもそもrenderpawnのportraitフラグが想定通りの動作をしてない疑惑 dubsがどうやって正常に動いてるのか分からない
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


        //renderHeadgear設定してるなら取得出来るはずだが、何かに潰されててそもそも通らないっぽい
        //public static void RenderPawn_Prefix(Pawn pawn, ref bool renderHeadgear, ref bool renderClothes, bool portrait)
        //{
        //    //if (pawn?.Name?.ToStringShort == "王立ハート領領")
        //    //    debug.WriteLine("RPP1:" + pawn?.Name?.ToStringShort + " pr:" + portrait.ToString() + " rc:" + renderClothes.ToString());
        //    if (portrait) return;

        //    TakeOffComp comp = pawn.GetComp<TakeOffComp>();
        //    if (comp == null) return;

        //    comp.renderClothes = renderClothes;
        //    comp.renderHeadgear = renderHeadgear;

        //    //debug.WriteLine("RPP2:" + pawn?.Name?.ToStringShort);
        //}

        //public static bool DrawHeadHair_Prefix_test() //Hats Display等と被る
        //{
        //    debug.WriteLine("DHH");
        //    return false;
        //}
    }
}
