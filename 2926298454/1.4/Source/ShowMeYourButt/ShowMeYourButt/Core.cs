using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ShowMeYourButt
{
    [StaticConstructorOnStartup]
    public static class Core
    {
        static Core()
        {
            new Harmony("Core.Mod").PatchAll();
            foreach (var pawn in DefDatabase<ThingDef>.AllDefs)
            {
                if (pawn.race != null && pawn.race.Humanlike)
                {
                    pawn.comps.Add(new CompProperties_Appearances());
                }
            }
        }
    }

    public class CompProperties_Appearances : CompProperties
    {
        public CompProperties_Appearances()
        {
            this.compClass = typeof(CompAppearance);
        }
    }
    public class CompAppearance : ThingComp
    {
        public bool showAppearancesButtons;
        public Pawn pawn => parent as Pawn;
        public CompAppearance()
        {
            disabledApparels = new HashSet<ThingDef>();
        }
        public HashSet<ThingDef> disabledApparels = new HashSet<ThingDef>();
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref showAppearancesButtons, "showAppearancesButtons");
            Scribe_Collections.Look(ref disabledApparels, "disabledApparels", LookMode.Def);
            if (disabledApparels is null)
            {
                disabledApparels = new HashSet<ThingDef>();
            }
        }

        public IEnumerable<Gizmo> AppearancesButtons()
        {
            if (pawn.apparel != null && pawn.IsColonist)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "ShowMeYourButt.ShowAppearancesButtons".Translate(),
                    icon = ThingDefOf.Cloth.uiIcon,
                    toggleAction = () =>
                    {
                        showAppearancesButtons = !showAppearancesButtons;
                    },
                    isActive = () => showAppearancesButtons,
                };
                if (showAppearancesButtons)
                {
                    foreach (var apparel in pawn.apparel.WornApparel)
                    {
                        if (!apparel.def.apparel.wornGraphicPath.NullOrEmpty())
                        {
                            yield return new Command_Toggle
                            {
                                defaultLabel = "ShowMeYourButt.Show".Translate() + " " + apparel.def.label,
                                icon = apparel.Graphic.ExtractInnerGraphicFor(apparel).MatAt(apparel.def.defaultPlacingRot).mainTexture as Texture2D,
                                defaultIconColor = apparel.DrawColor,
                                toggleAction = () =>
                                {
                                    if (disabledApparels.Contains(apparel.def))
                                    {
                                        disabledApparels.Remove(apparel.def);
                                    }
                                    else
                                    {
                                        disabledApparels.Add(apparel.def);
                                    }
                                    pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
                                    PortraitsCache.SetDirty(pawn);
                                    GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
                                },
                                isActive = () => !disabledApparels.Contains(apparel.def)
                            };
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.GetGizmos))]
    public static class Pawn_ApparelTracker_GetGizmos_Patch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn_ApparelTracker __instance)
        {
            foreach (var r in __result)
            {
                yield return r;
            }
            var comp = __instance.pawn.GetComp<CompAppearance>();
            if (comp != null)
            {
                foreach (var gizmo in comp.AppearancesButtons())
                {
                    yield return gizmo;
                }
            }
        }
    }
    [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveApparelGraphics))]
    public static class PawnGraphicSet_ResolveApparelGraphics_Patch
    {
        public static void Postfix(PawnGraphicSet __instance)
        {
            var pawn = __instance.pawn;
            var comp = __instance.pawn.GetComp<CompAppearance>();
            if (comp != null)
            {
                __instance.apparelGraphics.RemoveAll(x => comp.disabledApparels.Contains(x.sourceApparel.def));
            }
        }
    }
}
