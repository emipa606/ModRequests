using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Verse;
using RimWorld;
using Verse.AI;
using HarmonyLib;
using UnityEngine;

namespace USESetApparelDrawOrder
{
    [StaticConstructorOnStartup]
    static class USESetApparelDrawOrder
    {
        static USESetApparelDrawOrder()
        {
            var harmony = new Harmony("use.deepscannnertargeting");

            harmony.PatchAll(Assembly.GetExecutingAssembly());

            DefsLoaded();
        }
        public static void DefsLoaded()
        {
            IEnumerable<ThingDef> things = (
                    from def in DefDatabase<ThingDef>.AllDefs
                    where def.IsApparel == true
                    select def
                ) ;

            foreach (ThingDef thingDef in things)
            {
                thingDef.comps.Add(new USEDrawOrderSetCompProperties());
            }
        }
    }

    public class USEDrawOrderSetCompProperties : CompProperties
    {
        public USEDrawOrderSetCompProperties()
        {
            this.compClass = typeof(USEDrawOrderSetComp);
        }

        public USEDrawOrderSetCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }

    public class USEDrawOrderSetComp : ThingComp
    {
        public int drawOrderEntered = 0;

        public USEDrawOrderSetCompProperties Props => (USEDrawOrderSetCompProperties)this.Props;

        public USEDrawOrderSetComp()
        {
        }

        //public override IEnumerable<Gizmo> CompGetGizmosExtra()
        //{
        //    Command_Action action = new Command_Action();
        //    action.action = delegate
        //    {
        //        drawOrderEntered--;
        //    };

        //    yield return action;
        //}

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref drawOrderEntered, "USEdrawOrderEntered");
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt))]
    public static class RederPawnAtCustomDrawOrderPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(0)]
        public static void Prefix(PawnRenderer __instance)
        {
            __instance.graphics.ClearCache();

            Pawn pawn = __instance.graphics.pawn;
            if (pawn.RaceProps.Humanlike && !__instance.graphics.apparelGraphics.NullOrEmpty())
            {
                __instance.graphics.apparelGraphics.Sort((ApparelGraphicRecord x, ApparelGraphicRecord y) => 
                x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered.CompareTo(y.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered));

                __instance.graphics.ClearCache();
            }
        }
    }

    [HarmonyPatch(typeof(Verse.Dialog_InfoCard), nameof(Verse.Dialog_InfoCard.DoWindowContents))]
    public static class InfoTabInputAdd
    {
        [HarmonyPostfix]
        [HarmonyPriority(0)]
        public static void Postfix(Verse.Dialog_InfoCard __instance, Rect inRect)
        {
            Thing thing = AccessTools.Field(typeof(Dialog_InfoCard), "thing").GetValue(__instance) as Thing;

            if (thing.def.IsApparel == true)
            {
                Rect rect = new Rect(inRect);
                rect.y = inRect.height - 45f;
                rect.x = inRect.width - 350f;
                rect.width = 300f;
                rect.height = 25f;
                USEDrawOrderSetComp comp = thing.TryGetComp<USEDrawOrderSetComp>();
                string buffer = comp.drawOrderEntered.ToString();
                //Widgets.TextFieldNumericLabeled(rect, "Draw Order", ref comp.drawOrderEntered, ref buffer, -99999, 99999);
                Widgets.IntEntry(rect, ref comp.drawOrderEntered, ref buffer);
            }
        }
    }
}