using RimWorld;
using Verse;

namespace BillDoorsLootsAndShelves
{
    public class StatPart_DisplayShelf : StatPart
    {
        public bool nullifyNegativeStat;

        public float statFromWealthMult;

        float GetStat(Building_Locker locker)
        {
            float f = locker.tempStorage[0].GetStatValue(parentStat);
            if (f < 0 && nullifyNegativeStat) { f = 0; }
            return f;
        }

        float GetStatFromWealth(Building_Locker locker)
        {
            float f = locker.tempStorage[0].GetStatValue(RimWorld.StatDefOf.MarketValue);
            if (statFromWealthMult > 0) { f *= statFromWealthMult * locker.tempStorage[0].stackCount; }
            return f;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing is Building_Locker locker && locker.isDisplay && locker.tempStorage.Any)
            {
                return "BDsStatPart_DisplayShelf".Translate(locker.tempStorage[0].LabelNoParenthesis, GetStat(locker).ToString(), GetStatFromWealth(locker).ToString());
            }
            return "";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing is Building_Locker locker && locker.isDisplay && locker.tempStorage.Any)
            {
                val += GetStat(locker) + GetStatFromWealth(locker);
            }
        }
    }
}
