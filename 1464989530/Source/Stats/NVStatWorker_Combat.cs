
using JetBrains.Annotations;

using RimWorld;
using Verse;

namespace NightVision
{

    [UsedImplicitly]
    public class NVStatWorker_Combat : NVStatWorker
    {
        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {

            Pawn pawn = req.Thing as Pawn;
            if (GlowFor.CompFor(pawn) is Comp_NightVision comp && !IsDisabledFor(pawn))
            {
                return StatReportFor_NightVision_Combat.CombatPart(pawn, comp);
            }

            return "";
        }

#if RW10
        public override string GetStatDrawEntryLabel(StatDef statDef, float value, ToStringNumberSense numberSense, StatRequest optionalReq)
        {
            return "...";
        }
#else
public override string GetStatDrawEntryLabel(StatDef statDef, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
        {
            return "...";
        }
#endif

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            return 0;
        }

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            return "";
        }

        public override bool IsDisabledFor(Thing thing)
        {
            return base.IsDisabledFor(thing) || !Settings.CombatStore.CombatFeaturesEnabled.Value;
        }

        public override bool ShouldShowFor(StatRequest req)
        {
            return base.ShouldShowFor(req) || !Settings.CombatStore.CombatFeaturesEnabled.Value;
        }

        public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {

        }
    }
}
