using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace NoobertNebulous
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public static class Utils
    {
        public static RulePackDef videoNameRulePack;
        static Utils()
        {
            videoNameRulePack = new RulePackDef
            {
                defName = "NN_VideoNameRulePack"
            };
            var nouns = LanguageDatabase.activeLanguage.stringFiles.Keys.Where(x => x.Contains("Noun")).ToList();
            var adjectives = LanguageDatabase.activeLanguage.stringFiles.Keys.Where(x => x.Contains("Adjective")).ToList();
            videoNameRulePack.rulePack = new RulePack
            {
                rulesStrings = new List<string>
                {
                    "r_name-> [RandomNoun], [RandomNoun] and [RandomAdjective] [RandomNoun]"
                },
            };
            foreach (var noun in nouns)
            {
                videoNameRulePack.rulePack.rulesFiles.Add("RandomNoun->" + noun);
            }
            foreach (var adj in adjectives)
            {
                videoNameRulePack.rulePack.rulesFiles.Add("RandomAdjective->" + adj);
            }
        }

        public static TaggedString GenerateTextFromRule(RulePackDef rule, int seed = -1)
        {
            if (seed != -1)
            {
                Rand.PushState();
                Rand.Seed = seed;
            }
            string rootKeyword = "r_name";
            GrammarRequest request = default(GrammarRequest);
            request.Includes.Add(rule);
            string str = GrammarResolver.Resolve(rootKeyword, request);
            if (seed != -1)
            {
                Rand.PopState();
            }
            return str;
        }
        public static bool IsVidtuber(this Pawn pawn)
        {
            return NoobertNebulousStoryteller.Instance.vidtubers.ContainsKey(pawn);
        }

        public static VidtubeChannel GetVidtubeChannel(this Pawn pawn)
        {
            return NoobertNebulousStoryteller.Instance.vidtubers[pawn];
        }

        public static void CreateVidtubeChannel(this Pawn pawn)
        {
            var channel = new VidtubeChannel
            {
                author = pawn,
                name = "NN.DefaultChannelName".Translate(pawn.Named("PAWN"))
            };
            NoobertNebulousStoryteller.Instance.vidtubers[pawn] = channel;
        }

        public static Video CreateVideo(Pawn author, string name, List<VideoTagDef> tags = null)
        {
            if (tags is null)
            {
                tags = new List<VideoTagDef>();
            }
            return new Video { creator = author, name = name, tags = tags, creationDateTick = Find.TickManager.TicksGame };
        }
    }
}
