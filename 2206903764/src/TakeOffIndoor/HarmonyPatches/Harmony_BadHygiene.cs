using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using Verse.AI;
using DubsBadHygiene;

namespace TakeOffIndoor
{
    public static class Harmony_BadHygiene_H_RenderPawn
    {
        public static void Patch(Harmony harmony)
        {

            //MethodInfo original; HarmonyMethod postfix;

            //original = AccessTools.Method(ModUtil.BadHygiene_HRP, "Prefix");
            //postfix = Util.HM(typeof(Harmony_BadHygiene_H_RenderPawn), nameof(Harmony_BadHygiene_H_RenderPawn.Postfix));
            //harmony.Patch(original, null, postfix);


            //original = AccessTools.Method(ModUtil.BadHygiene_JDUS, "WatchTickAction");
            //postfix = Util.HM(typeof(Harmony_BadHygiene_H_RenderPawn), nameof(Harmony_BadHygiene_H_RenderPawn.WatchTickAction));
            //harmony.Patch(original, postfix);

            //original = AccessTools.Method(ModUtil.BadHygiene_JDGW, "swimticker");
            //postfix = Util.HM(typeof(Harmony_BadHygiene_H_RenderPawn), nameof(Harmony_BadHygiene_H_RenderPawn.swimticker));
            //harmony.Patch(original, postfix);



            //Log.Message("DubsBadHygiene Patched.");
            //debug.WriteLine("DubsBadHygiene Patched.");
        }
        //public static void Postfix(Pawn pawn,ref bool renderHeadgear, ref bool renderClothes, bool portrait)
        //{
        //    debug.WriteLine("BHRP1:" + pawn?.Name?.ToStringShort+" pr:"+ portrait.ToString() + " rc:" + renderClothes.ToString());
        //    if (portrait) return;

        //    TakeOffComp comp = pawn.GetComp<TakeOffComp>();
        //    if (comp == null) return;

        //    comp.renderClothes = renderClothes;
        //    comp.renderHeadgear = renderHeadgear;
        //}

        public static bool IsWashing(Pawn pawn) //結局Harmonyではない
        {
            if (ModUtil.ExistBadHygiene)
            {
                return _IsWashing(pawn);
            }
            return false;
        }

        static bool _IsWashing(Pawn pawn)
        {
            if (pawn.CurJob != null)
            {
                WashingJobDef washingJobDef = pawn.CurJob.def as WashingJobDef;
                if (washingJobDef != null)
                {
                    if (!pawn.health.hediffSet.HasHediff(DubDef.Washing, false))
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        //public static void swimticker(JobDriver __instance)
        //{
        //    debug.WriteLine("st:" + __instance.GetActor()?.Name?.ToStringShort);
        //}

        //public static void WatchTickAction(JobDriver __instance)
        //{
        //    debug.WriteLine("wta:" + __instance.GetActor()?.Name?.ToStringShort);
        //}

    }
}
