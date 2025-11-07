using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace BillDoorsFramework
{

    public class StatWorker_KeyRequirement : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (req.Def is ThingDef)
            {
                return base.ShouldShowFor(req) && (req.Def as ThingDef).HasComp(typeof(CompUsable_KeyCardRequirement));
            }
            return false;
        }

        public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
        {
            var props = (statRequest.Def as ThingDef).GetCompProperties<CompProperties_Useable_KeyCardRequirement>();

            if (props != null)
            {
                foreach (ThingDef def in props.keyCardDefs)
                {
                    yield return new Dialog_InfoCard.Hyperlink(def);
                }
            }
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var props = (req.Def as ThingDef).GetCompProperties<CompProperties_Useable_KeyCardRequirement>();

            if (props != null)
            {
                if (props.consumeCard)
                {
                    stringBuilder.AppendLine("BDsConsumeKeyCardStatExp".Translate());
                }
                stringBuilder.AppendLine("BDsRequiredKeys".Translate());
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}
