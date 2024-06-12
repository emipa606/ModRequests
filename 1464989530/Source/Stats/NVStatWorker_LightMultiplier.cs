// Nightvision NightVision NVStatWorker_LightMultiplier.cs
// 
// 24 10 2018
// 
// 24 10 2018

#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using JetBrains.Annotations;
using RimWorld;
using System.Reflection;
using Verse;

namespace NightVision
{
    [UsedImplicitly]
    public class NVStatWorker_LightMultiplier : NVStatWorker
    {
        public static ApparelFlags GetEffectMaskForGlow(float glow)
        {
            if (glow.GlowIsBright())
            {
                return ApparelFlags.NullifiesPS;
            }

            if (glow.GlowIsDarkness())
            {
                return ApparelFlags.GrantsNV;
            }

            return ApparelFlags.None;
        }



        public override string GetExplanationUnfinalized(
            StatRequest req,
            ToStringNumberSense numberSense
        )
        {
            if (req.Thing is Pawn pawn
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glow = GlowFor.GlowAt(pawn);
                return StatReportFor_NightVision.CompleteStatReport(Stat, GetEffectMaskForGlow(glow), comp, glow);
            }

            return string.Empty;
        }

        public override float GetValueUnfinalized(
            StatRequest req,
            bool applyPostProcess = true
        )
        {
            if (req.Thing is Pawn pawn)
            {
                if (pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    float glow = GlowFor.GlowAt(pawn);
                    return comp.FactorFromGlow(glow);
                }
            }

            return 1f;
        }

        public override bool IsDisabledFor(
            Thing thing
        )
        {
            return !(thing is Pawn);
        }

        public override bool ShouldShowFor(StatRequest req)
        {
            return req.HasThing && !IsDisabledFor(req.Thing);
        }


        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            return base.GetExplanationFinalizePart(req, numberSense, finalVal);
        }

        public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {
            base.FinalizeValue(req, ref val, applyPostProcess);
        }
    }
}