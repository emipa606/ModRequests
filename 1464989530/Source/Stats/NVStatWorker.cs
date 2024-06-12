using RimWorld;
using System.Reflection;
using Verse;

namespace NightVision
{
    public class NVStatWorker : StatWorker
    {
        public float Glow;

        public ApparelFlags StatEffectMask;

        public float DefaultStatValue;

        public string Acronym;

        public StatDef Stat => stat;



        public override string GetExplanationUnfinalized(
            StatRequest req,
            ToStringNumberSense numberSense
        )
        {
            if (req.Thing is Pawn pawn
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                return StatReportFor_NightVision.CompleteStatReport(Stat, StatEffectMask, comp, Glow);
            }

            return string.Empty;
        }

#if RW10
        public override string GetStatDrawEntryLabel(StatDef pStat, float value, ToStringNumberSense numberSense, StatRequest optionalReq)
        {
            return $"x{GetValueUnfinalized(optionalReq).ToStringPercent()}";
        }
#else
        public override string GetStatDrawEntryLabel(StatDef pStat, float value, ToStringNumberSense numberSense, StatRequest optionalReq,
            bool finalized = true)
        {
            return $"x{GetValueUnfinalized(optionalReq).ToStringPercent()}";
        }
#endif


        public override float GetValueUnfinalized(
            StatRequest req,
            bool applyPostProcess = true
        )
        {
            if (req.Thing is Pawn pawn
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                return comp.FactorFromGlow(Glow);
            }

            return DefaultStatValue;
        }

        public override bool IsDisabledFor(
            Thing thing
        )
        {
            return !(thing is Pawn pawn) || pawn.TryGetComp<Comp_NightVision>() == null;
        }

        public override bool ShouldShowFor(StatRequest req)
        {
            return req.HasThing && !IsDisabledFor(req.Thing);
        }

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            return "";

            //StringBuilder stringBuilder = new StringBuilder();
            //if (this.stat.parts != null)
            //{
            //    for (int index = 0; index < this.stat.parts.Count; ++index)
            //    {
            //        string str = this.stat.parts[index].ExplanationPart(req);
            //        if (!str.NullOrEmpty())
            //        {
            //            stringBuilder.AppendLine(str);
            //            stringBuilder.AppendLine();
            //        }
            //    }
            //}
            //if (this.stat.postProcessCurve != null)
            //{
            //    float num1 = this.GetValue(req, false);
            //    float num2 = this.GetValue(req, true);
            //    if (!Mathf.Approximately(num1, num2))
            //    {
            //        string str1 = this.ValueToString(num1, false, ToStringNumberSense.Absolute);
            //        string str2 = this.stat.ValueToString(num2, numberSense);
            //        stringBuilder.AppendLine("StatsReport_PostProcessed".Translate() + ": " + str1 + " => " + str2);
            //        stringBuilder.AppendLine();
            //    }
            //}
            //float statFactor = Find.Scenario.GetStatFactor(this.stat);
            //if ((double) statFactor != 1.0)
            //{
            //    stringBuilder.AppendLine("StatsReport_ScenarioFactor".Translate() + ": " + statFactor.ToStringPercent());
            //    stringBuilder.AppendLine();
            //}
            //stringBuilder.Append("StatsReport_FinalValue".Translate() + ": " + this.stat.ValueToString(finalVal, this.stat.toStringNumberSense));
            //return stringBuilder.ToString();
        }
    }
}