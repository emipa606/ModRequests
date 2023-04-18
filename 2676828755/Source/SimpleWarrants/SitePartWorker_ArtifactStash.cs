using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace SimpleWarrants
{
	public class SitePartWorker_ArtifactStash : SitePartWorker
	{
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
		{
			base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
			part.things = new ThingOwner<Thing>(part, oneStackOnly: true);
			var artifact = slate.Get<Thing>("artifact");
			part.things.TryAdd(artifact, false);
		}
    }

	public class GenStep_ArtifactStash : GenStep_ItemStash
    {
        public override void Generate(Map map, GenStepParams parms)
        {
			var artifact = parms.sitePart.things[0];
			var warrant = WarrantsManager.Instance.acceptedWarrants.First(x => x.thing == artifact);
			base.Generate(map, parms);
		}
    }
}