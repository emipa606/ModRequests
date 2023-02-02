using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Verse;
using Verse.Sound;

namespace MoreIdeoColors
{
    /*[HarmonyPatch]*/
    /*[HarmonyPatch("DrawStylingItemType")]*/
    class DrawStylingItemTypePatch
    {
        /*static MethodBase TargetMethod()
        {
            return typeof(Dialog_StylingStation)
                .GetMethod("DrawStylingItemType",
                           BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(typeof(HairDef));
        }*/

        static bool Prefix(ref Rect rect, ref Vector2 scrollPosition, Action<Rect, StyleItemDef> drawAction, Action<StyleItemDef> selectAction, Func<StyleItemDef, bool> hasStyleItem, Func<StyleItemDef, bool> hadStyleItem, Func<StyleItemDef, bool> extraValidator = null, bool doColors = false)
        {
            if (doColors)
            {
                float bottomMargin = 90f;
                rect.yMax -= bottomMargin;
            }
            return true;
        }

            /*static bool Prefix(Dialog_StylingStation __instance, ref List<StyleItemDef> ___tmpStyleItems, ref float ___viewRectHeight, ref Vector2 ___hairScrollPosition, Pawn ___pawn, bool ___devEditMode, Rect rect, ref Vector2 scrollPosition, Action<Rect, HairDef> drawAction, Action<HairDef> selectAction, Func<StyleItemDef, bool> hasStyleItem, Func<StyleItemDef, bool> hadStyleItem, Func<StyleItemDef, bool> extraValidator = null, bool doColors = false)
            {
                if (!doColors) {
                    Log.Message("Nocolor");
                    return true;
                }
                float bottomMargin = 90f;
                rect.yMax -= bottomMargin;
                Rect viewRect = new Rect(rect.x, rect.y, rect.width - 16f, ___viewRectHeight);
                int num = Mathf.FloorToInt(viewRect.width / 60f) - 1;
                Log.Message(___viewRectHeight.ToString());
                float num2 = (viewRect.width - (float)num * 60f - (float)(num - 1) * 10f) / 2f;
                int num3 = 0;
                int num4 = 0;
                int num5 = 0;
               *//* PropertyInfo tmpStyleItemsProp = typeof(Dialog_StylingStation).GetProperty("tmpStyleItems",
                    BindingFlags.NonPublic | BindingFlags.Static);

                List<StyleItemDef> tmpStyleItems = (List<StyleItemDef>)tmpStyleItemsProp.GetValue(null);*//*
                ___tmpStyleItems.Clear();
                ___tmpStyleItems.AddRange(from x in DefDatabase<HairDef>.AllDefs
                                       where (___devEditMode || PawnStyleItemChooser.WantsToUseStyle(___pawn, x, null) || hadStyleItem(x)) && (extraValidator == null || extraValidator(x))
                                       select x);
                ___tmpStyleItems.SortBy((StyleItemDef x) => -PawnStyleItemChooser.StyleItemChoiceLikelihoodFor(x, ___pawn));
                if (___tmpStyleItems.NullOrEmpty<StyleItemDef>())
                {
                    Widgets.NoneLabelCenteredVertically(rect, "(" + "NoneUsableForPawn".Translate(___pawn.Named("PAWN")) + ")");
                }
                else
                {
                    Widgets.BeginScrollView(rect, ref ___hairScrollPosition, viewRect, true);
                    foreach (StyleItemDef styleItemDef in ___tmpStyleItems)
                    {
                        if (num5 >= num - 1)
                        {
                            num5 = 0;
                            num4++;
                        }
                        else if (num3 > 0)
                        {
                            num5++;
                        }
                        Rect rect2 = new Rect(rect.x + num2 + (float)num5 * 60f + (float)num5 * 10f, rect.y + (float)num4 * 60f + (float)num4 * 10f, 60f, 60f);
                        Widgets.DrawHighlight(rect2);
                        if (Mouse.IsOver(rect2))
                        {
                            Widgets.DrawHighlight(rect2);
                            TooltipHandler.TipRegion(rect2, styleItemDef.LabelCap);
                        }
                        if (drawAction != null)
                        {
                            drawAction(rect2, styleItemDef as HairDef);
                        }
                        if (hasStyleItem(styleItemDef))
                        {
                            Widgets.DrawBox(rect2, 2, null);
                        }
                        if (Widgets.ButtonInvisible(rect2, true))
                        {
                            if (selectAction != null)
                            {
                                selectAction(styleItemDef as HairDef);
                            }
                            SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                            ___pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
                        }
                        num3++;
                    }
                    if (Event.current.type == EventType.Layout)
                    {
                        ___viewRectHeight = (float)(num4 + 1) * 60f + (float)num4 * 10f + 10f;
                    }
                    Widgets.EndScrollView();
                }
                if (doColors)
                {
                    MethodInfo DrawHairColorsMethod = __instance.GetType().GetMethod("DrawHairColors",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                    DrawHairColorsMethod.Invoke(__instance, new object[] { new Rect(rect.x, rect.yMax + 10f, rect.width, 110f + bottomMargin) });
                    *//*DrawHairColors(new Rect(rect.x, rect.yMax + 10f, rect.width, 200f));*//*
                }
                return false;
            }*/
        }
}
