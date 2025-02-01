using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Grammar;
using RimWorld;

namespace LostTechnology
{
    public class SitePartWorker_Outpost : SitePartWorker
    {
        public static readonly SimpleCurve ThreatPointsLootMarketValue = new SimpleCurve
        {
            new CurvePoint(100f, 200f),
            new CurvePoint(250f, 450f),
            new CurvePoint(800f, 1000f),
            new CurvePoint(10000f, 2000f)
        };

        public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
        {
            string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
            lookTargets = new LookTargets(map.Parent);
            return arrivedLetterPart;
        }

        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
            outExtraDescriptionRules.Add(new Rule_String("itemStashContents", GenLabel.ThingsLabel(part.site.parts.Find(a => a.def == LostTechnology.Def.SitePartDefOf.ItemStash).things)));
        }

        public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
        {
            return base.GetPostProcessedThreatLabel(site, sitePart) + ": " + string.Join(", ", from t in site.parts.Find(a => a.def == LostTechnology.Def.SitePartDefOf.ItemStash).things where t.def.thingCategories.Contains(ThingCategoryDefOf.Techprints) select t.Label);
        }

        public override SitePartParams GenerateDefaultParams(float myThreatPoints, int tile, Faction faction)
        {
            SitePartParams sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
            if (faction.def.pawnGroupMakers.Any(gr => gr.kindDef == PawnGroupKindDefOf.Settlement))
            {
                Log.Message("settlement1");
                sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement));
            }
            else
            {
                Log.Message("combat1");
                sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
            }
            sitePartParams.lootMarketValue = ThreatPointsLootMarketValue.Evaluate(sitePartParams.threatPoints);
            return sitePartParams;
        }

    }
}
