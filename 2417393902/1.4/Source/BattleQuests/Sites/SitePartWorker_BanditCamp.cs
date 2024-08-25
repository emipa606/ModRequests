using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace BattleQuests;

public class SitePartWorker_BanditCamp : SitePartWorker
{
    public override bool FactionCanOwn(Faction faction)
    {
        return base.FactionCanOwn(faction) && faction.HostileTo(Faction.OfPlayer) &&
               faction.def.pawnGroupMakers.Any(x => x.kindDef == PawnGroupKindDefOf.Combat && x.options.Count > 0);
    }

    public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
    {
        var arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
        lookTargets =
            map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.RaceProps.Humanlike && x.HostileTo(Faction.OfPlayer));
        return arrivedLetterPart;
    }

    public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules,
        Dictionary<string, string> outExtraDescriptionConstants)
    {
        base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
        var comp = Current.Game.GetComponent<QuestManager>();
        comp.threatLevels[part.site] = slate.Get<float>("challengeRating", 1);
        comp.allyFactions[part.site] = slate.Get<Pawn>("asker").Faction;
        var enemiesCount = GetEnemiesCount(part.site, part.parms);
        outExtraDescriptionRules.Add(new Rule_String("enemiesCount", enemiesCount.ToString()));
        outExtraDescriptionRules.Add(new Rule_String("enemiesLabel", GetEnemiesLabel(part.site, enemiesCount)));
    }

    public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
    {
        return $"{base.GetPostProcessedThreatLabel(site, sitePart)}: " +
               "KnownSiteThreatEnemyCountAppend".Translate(GetEnemiesCount(site, sitePart.parms),
                   "Enemies".Translate());
    }

    public override SitePartParams GenerateDefaultParams(float myThreatPoints, int tile, Faction faction)
    {
        var sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
        sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints,
            faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement));
        return sitePartParams;
    }

    protected int GetEnemiesCount(Site site, SitePartParams parms)
    {
        return PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms
        {
            tile = site.Tile,
            faction = site.Faction,
            groupKind = PawnGroupKindDefOf.Settlement,
            points = parms.threatPoints,
            inhabitants = true,
            seed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms)
        }).Count();
    }

    protected string GetEnemiesLabel(Site site, int enemiesCount)
    {
        if (site.Faction == null)
        {
            return enemiesCount == 1 ? "Enemy".Translate() : "Enemies".Translate();
        }

        return enemiesCount != 1 ? site.Faction.def.pawnsPlural : site.Faction.def.pawnSingular;
    }
}