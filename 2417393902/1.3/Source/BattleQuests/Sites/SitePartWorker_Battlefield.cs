using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;

namespace BattleQuests
{
	public class SitePartWorker_Battlefield : SitePartWorker
	{
        public override bool FactionCanOwn(Faction faction)
        {
            return base.FactionCanOwn(faction) && faction.HostileTo(Faction.OfPlayer) && faction.def.pawnGroupMakers.Where(x => x.kindDef == PawnGroupKindDefOf.Combat && x.options.Count > 0).Any();
        }
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            var comp = Current.Game.GetComponent<QuestManager>();
            comp.threatLevels[part.site] = slate.Get<float>("challengeRating", 1);
            comp.allyFactions[part.site] = slate.Get<Pawn>("asker").Faction;
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
        }
    }
}
