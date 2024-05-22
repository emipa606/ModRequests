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

namespace TakeOffIndoor
{
    //[HarmonyPatch(typeof(PawnRenderer))]
    //[HarmonyPatch("RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
    //[HarmonyPriority(int.MaxValue)]
    //public static class Harmony_RenderPawnInternal_post
    //{
    //    [HarmonyPostfix]
    //    public static void Postfix(PawnRenderer __instance, Pawn ___pawn)
    //    {
    //        if (!___pawn.IsColonistPlayerControlled)
    //        {
    //            return;
    //        }
    //        TakeOffComp comp = ___pawn.GetComp<TakeOffComp>();
    //        bool init = comp.first;
    //        if (comp == null)
    //        {
    //            return;
    //        }
    //        if (!comp.initialized)
    //        {
    //            if (___pawn.IsColonist)
    //            {
    //                comp.Initialize();
    //                init = true;
    //            }
    //            else
    //            {
    //                return;
    //            }
    //        }
    //        if (!comp.IsTargetPawn)
    //        {
    //            return;
    //        }
    //        //if (!comp.first)
    //        //{
    //        //    if (Find.TickManager.TicksGame % 3 != comp.calcShift)
    //        //    {
    //        //        return;
    //        //    }
    //        //}
    //        bool flg = comp.ChkSettingsUpdate;
    //        if (flg)
    //        {
    //            comp.UpdateShowLayerLevel();
    //        }

    //        if (comp.ChkLayerLevelChanged || init || flg)
    //        {
    //            debug.WriteLine("rpi_rag " + ___pawn.ThingID + init.ToString() + flg.ToString() + comp.nowLayer.ToString());
    //            if (init)
    //            {
    //                __instance.graphics.ResolveAllGraphics();
    //            }
    //            else
    //            {
    //                __instance.graphics.ResolveApparelGraphics();
    //            }
    //            PortraitsCache.SetDirty(___pawn);
    //        }

    //        return;
    //    }
    //}

    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
    [HarmonyPriority(int.MaxValue)]
    [HarmonyBefore("com.rimworld.Dalrae.Garam_RaceAddon")]
    //[HarmonyAfter("com.rimworld.Dalrae.Garam_RaceAddon")]
    public static class Harmony_Render
    {
        public static void Patch(Harmony harmony)
        {
            
        }
        public static Pawn nowPawn = null;
        [HarmonyPrefix]
        public static void Prefix(PawnRenderer __instance, Pawn ___pawn, bool portrait)
        {
            nowPawn = ___pawn;
            if (!___pawn.IsColonistPlayerControlled)
            {
                return;
            }
            TakeOffComp comp = ___pawn.GetComp<TakeOffComp>();
            if (comp == null)
            {
                return;
            }
            bool init = comp.first;
            bool flg = init;

            if (!comp.IsTargetPawn)
            {
                return;
            }

            if (!comp.initialized)
            {
                comp.Initialize();
                init = true;
                flg = true;
                //debug.WriteLine("rpi it:" + comp.Pawn.Name.ToStringShort);
            }

            /* legacy mode beginning*/
            if (TakeOffSettings.ResolveLegacy)
            {
                bool flgl = comp.ChkSettingsUpdate;
                if (flgl)
                {
                    comp.UpdateShowLayerLevel();
                }

                if (comp.ChkLayerLevelChanged || init || flgl)
                {
                    debug.WriteLine("ResolveApparelGraphics");
                    __instance.graphics.ResolveApparelGraphics();
                }

                return;
            }
            else if (TakeOffSettings.ResolveLegacy2)
            {
                bool flgl = comp.ChkSettingsUpdate;
                if (flgl)
                {
                    comp.UpdateShowLayerLevel();
                }

                if (comp.ChkLayerLevelChanged || init || flgl)
                {
                    debug.WriteLine("ResolveApparelGraphics");

                    if ((init && ModUtil.PawnIsGaramRace(___pawn)))
                    {
                        __instance.graphics.ResolveAllGraphics();
                    }
                    else
                    {
                        __instance.graphics.ResolveApparelGraphics();
                    }
                    PortraitsCache.SetDirty(___pawn);
                }

                return;
            }
            else if (TakeOffSettings.ResolveLegacy3)
            {

                if (portrait && TakeOffSettings.portraitOption)
                {
                    comp.portrait = TakeOffSettings.dontApplyPortrait;
                    comp.tempHatsInvisible = TakeOffSettings.hatsOnlyOnMap;
                    ResolveGraphicsW(__instance, init);
                    comp.tempExecute = true;
                }
                else
                {
                    if (comp.ChkSettingsUpdate)
                    {
                        comp.UpdateShowLayerLevel();
                        if (!comp.tempExecute && TakeOffSettings.portraitOption)
                        {
                            PortraitsCache.SetDirty(___pawn);
                            return;
                        }
                        else
                        {
                            flg = true;
                        }
                    }

                    if (comp.ChkLayerLevelChanged)
                    {
                        if (!comp.tempExecute && !TakeOffSettings.dontApplyPortrait && TakeOffSettings.hatsOnlyOnMap) //帽子マップのみオンならポートレイト描画(ポートレイト更新なしの場合不要)
                        {
                            PortraitsCache.SetDirty(___pawn);
                            return;
                        }
                        flg = true;
                    }
                    if (comp.tempExecute)
                    {
                        flg = true;
                        comp.tempExecute = false;
                        comp.portrait = false;
                        comp.tempHatsInvisible = false;
                    }

                    if (flg)
                    {
                        ResolveGraphicsW(__instance, init);

                        if (!TakeOffSettings.dontApplyPortrait && !TakeOffSettings.hatsOnlyOnMap)
                        {
                            PortraitsCache.SetDirty(___pawn);
                        }
                    }
                }

                return;
            }
            /* legacy mode ending */

            /* test mode beginning */
            else if (TakeOffSettings.ResolveTest)
            {
                if (portrait && TakeOffSettings.portraitOption)
                {
                    comp.portrait = TakeOffSettings.dontApplyPortrait;
                    comp.tempHatsInvisible = TakeOffSettings.hatsOnlyOnMap;
                    ResolveGraphicsW(__instance, false);
                    comp.tempExecute = true;
                }
                else
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
                    bool setDirty = !portrait;
                    if (comp.tempExecute)
                    {
                        flg = true;
                        comp.tempExecute = false;
                        comp.portrait = false;
                        comp.tempHatsInvisible = false;
                        setDirty = false;
                    }

                    if (flg)
                    {
                        ResolveGraphicsW(__instance, false);

                        if (setDirty)
                        {
                            PortraitsCache.SetDirty(___pawn);
                        }
                    }
                }

                return;
            }
            /* test mode ending */

            if (portrait && TakeOffSettings.portraitOption)
            {
                comp.portrait = TakeOffSettings.dontApplyPortrait;
                comp.tempHatsInvisible = TakeOffSettings.hatsOnlyOnMap;
                ResolveGraphicsW(__instance, false);
                comp.tempExecute = true;
            }
            else
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
                bool setDirty = !portrait;
                if (comp.tempExecute)
                {
                    flg = true;
                    comp.tempExecute = false;
                    comp.portrait = false;
                    comp.tempHatsInvisible = false;
                    setDirty = false;
                }

                if (flg)
                {
                    if(init && ModUtil.PawnIsGaramRace(___pawn))
                    {
                        ResolveGraphicsW(__instance, init);
                        PortraitsCache.SetDirty(___pawn);
                    }
                    else
                    {
                        ResolveGraphicsW(__instance, false);

                        if (setDirty)
                        {
                            PortraitsCache.SetDirty(___pawn);
                        }
                    }
                }
            }

            return;
        }

        static void ResolveGraphicsW(PawnRenderer render, bool init)
        {
            if (TakeOffSettings.ResolveAlwaysAll || (init && !TakeOffSettings.ResolveAlwaysApparel))
            {
                render.graphics.ResolveAllGraphics();
            }
            else
            {
                render.graphics.ResolveApparelGraphics();
            }
        }

        public static void NotifySettingsChanged() //1.3用
        {
        }
    }
}
