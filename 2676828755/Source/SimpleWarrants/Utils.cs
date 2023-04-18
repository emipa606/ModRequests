using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace SimpleWarrants
{

    [DefOf]
    public static class SW_DefOf
    {
        public static IncidentDef SW_Visitors;
        public static RulePackDef SW_WantedFor;
        public static QuestScriptDef SW_Warrant_Animal;
        public static QuestScriptDef SW_Warrant_Tame;
        public static QuestScriptDef SW_Warrant_Artifact;
        public static QuestScriptDef SW_Warrant_Pawn;
    }
    public static class Utils
    {
        public static IEnumerable<ThingDef> AllArtifactDefs => DefDatabase<ThingDef>.AllDefs.Where(x => (x.tradeTags?.Contains("Artifact") ?? false)
                    || (x.thingCategories?.Contains(ThingCategoryDefOf.Artifacts) ?? false)
                    || (x.tradeTags?.Contains("ExoticMisc") ?? false));

        public static IEnumerable<PawnKindDef> AllWorthAnimalDefs => DefDatabase<PawnKindDef>.AllDefs.Where(x => x.race.race.Animal && x.race.GetStatValueAbstract(StatDefOf.MarketValue) >= 400);

        [DebugAction("General", "Populate warrants (x15)")]
        private static void PopulateWarrants()
        {
            WarrantsManager.Instance.PopulateWarrants(15);
        }

        [DebugAction("General", "Put warrant on pawn", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PutWarrantOn(Pawn p)
        {
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(PutWarrant(p)));
        }

        public static List<DebugMenuOption> PutWarrant(Pawn pawn)
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            foreach (var issuer in Find.FactionManager.AllFactions)
            {
                list.Add(new DebugMenuOption(issuer.Name, DebugMenuOptionMode.Action, delegate
                {
                    WarrantsManager.Instance.PutWarrantOn(pawn, "DEBUG", issuer);
                }));
            }
            return list;
        }

        public static Faction AnyHostileToPlayerFaction()
        {
            return Find.FactionManager.AllFactions.Where(faction => faction.def.humanlikeFaction && !faction.defeated && !faction.Hidden && !faction.IsPlayer
                                        && faction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Hostile 
                                        && Find.World.worldObjects.Settlements.Any(settlement => settlement.Faction == faction))
                                        .RandomElement();
        }

        public static TaggedString GenerateTextFromRule(RulePackDef rule, int seed = -1)
        {
            if (seed != -1)
            {
                Rand.PushState();
                Rand.Seed = seed;
            }
            string rootKeyword = rule.RulesPlusIncludes.Where(x => x.keyword == "r_logentry").RandomElement().keyword;
            GrammarRequest request = default(GrammarRequest);
            request.Includes.Add(rule);
            string str = GrammarResolver.Resolve(rootKeyword, request);
            if (seed != -1)
            {
                Rand.PopState();
            }
            return str;
        }

        public static HashSet<string> GenerateAllTextFromRule(RulePackDef rule)
        {
            HashSet<string> results = new HashSet<string>();
            for (var i = 0;i < 100; i++)
            {
                string rootKeyword = rule.FirstRuleKeyword;
                GrammarRequest request = default(GrammarRequest);
                request.Includes.Add(rule);
                string str = GrammarResolver.Resolve(rootKeyword, request);
                results.Add(str);
            }
            return results;
        }
    }
}