using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

//should be all good
namespace PropagandaAndManifestos
{
    [StaticConstructorOnStartup]
    public static class ReligiousTextPatch
    {
        private static readonly Type patchType = typeof(ReligiousTextPatch);
        static ReligiousTextPatch()
        {
            var harmony = new Harmony("rimworld.Chaoticbloom.PropagandaAndManifestos.ReligiousText");
            harmony.Patch(AccessTools.Method(typeof(Pawn_IdeoTracker), "OffsetCertainty"),
                prefix: new HarmonyMethod(patchType, nameof(Prefix)));
            harmony.Patch(AccessTools.Method(typeof(Book), "GenerateBook"),
                prefix: new HarmonyMethod(patchType, nameof(Prefix1)));
        }
        public static bool Prefix(float offset, ref Pawn ___pawn, ref float ___certaintyInt)
        {
            if (___pawn.CurJobDef == JobDefOf.Reading)
            {
                ___certaintyInt += offset;
                return false;
            }
            return true;
        }
        public static bool Prefix1(ref string ___description, ref string ___title, Pawn author, long? fixedDate, Book __instance)
        {
            int num = Rand.Range(1, 3);
            if(num==1)
            {
            }
            if (__instance.def.defName == "ReligiousText")
            {
                Log.Message("winda kork");
                ___description = author.Ideo.GetNewDescription(true).text;
                __instance.GetComp<CompIdeo>().SetIdeo(author.Ideo);
                foreach (BookOutcomeDoer doer in __instance.BookComp.Doers)
                {
                    doer.Reset();
                    doer.OnBookGenerated(author);
                    List<RulePack> topicRulePacks = doer.GetTopicRulePacks();
                    if (topicRulePacks == null)
                    {
                        continue;
                    }
                    foreach (RulePack item in topicRulePacks)
                    {
                        GrammarRequest request = default(GrammarRequest);
                        request.IncludesBare.Add(item);
                        List<(string, string)> list = new List<(string, string)>();
                        foreach (Rule rule in item.Rules)
                        {
                            if (rule.keyword.StartsWith("subject_"))
                            {
                                list.Add((rule.keyword.Substring("subject_".Length), GrammarResolver.Resolve(rule.keyword, request, null, forceLog: false, null, null, null, capitalizeFirstSentence: false)));
                            }
                        }
                    }
                }
                GrammarRequest grammarRequest = default(GrammarRequest);
                long absTicks = fixedDate ?? (GenTicks.TicksAbs - (long)(__instance.BookComp.Props.ageYearsRange.RandomInRange * 3600000f));
                grammarRequest.Rules.Add(new Rule_String("date", GenDate.DateFullStringAt(absTicks, Vector2.zero)));
                grammarRequest.Rules.Add(new Rule_String("date_season", GenDate.DateMonthYearStringAt(absTicks, Vector2.zero)));
                grammarRequest.Constants.Add("quality", ((int)__instance.GetComp<CompQuality>().Quality).ToString());
                foreach (Rule rule2 in ((author == null) ? TaleData_Pawn.GenerateRandom(humanLike: true) : TaleData_Pawn.GenerateFrom(author)).GetRules("ANYPAWN", grammarRequest.Constants))
                {
                    grammarRequest.Rules.Add(rule2);
                }
                GrammarRequest request2 = grammarRequest;
                request2.Includes.Add(__instance.BookComp.Props.nameMaker);
                ___title = GenText.CapitalizeAsTitle(GrammarResolver.Resolve("title", request2)).StripTags();
                return false;
            }
            return true;
        }
    }
    public class ReadingOutcomeDoerIdeologyModifier : BookOutcomeDoer
    {
        public int lastUpdate = 0;
        private static readonly int fifthhour = 500;
        public Ideo authorideo;
        public new BookOutcomeProperties_IdeologyModifier Props => (BookOutcomeProperties_IdeologyModifier)props;
        public override bool DoesProvidesOutcome(Pawn reader)
        {
            return true;
        }
        public override void OnReadingTick(Pawn reader, float factor)
        {
            if (reader.CurJobDef == JobDefOf.Reading)
            {
                lastUpdate++;
            }
            else
            {
                lastUpdate = 0;
            }
            if (lastUpdate >= fifthhour)
            {
                factor = GetIdeoCertaintyQuality(base.Quality);
                if (base.Book.GetComp<CompIdeo>().GetIdeo() != reader.Ideo)
                {
                    factor *= -1;
                }
                if (reader.ideo.Certainty > .10)
                    reader.ideo.OffsetCertainty(factor / 5f);
                lastUpdate = 0;
            }
        }
        public float GetIdeoCertaintyQuality(QualityCategory quality)
        {
            int qual = (int)quality;
            if (qual <= 1) { return .01f; }
            if (qual == 2) { return .02f; }
            if (qual == 3) { return .03f; }
            if (qual == 4) { return .04f; }
            if (qual >= 5) { return .05f; }
            return 0;
        }
        public override void OnBookGenerated(Pawn author = null)
        {
            base.Book.GetComp<CompIdeo>().SetIdeo(author.Ideo);
            base.Book.SetJoyFactor(BookUtility.GetNovelJoyFactorForQuality(base.Quality));
        }
        public override string GetBenefitsString(Pawn reader = null)
        {
            return string.Format(" - {0}: {1}", "Ideology Certainty change per hour", GetIdeoCertaintyQuality(base.Quality).ToStringPercent());
        }
    }
    public class BookOutcomeProperties_IdeologyModifier : BookOutcomeProperties
    {
        public override Type DoerClass => typeof(ReadingOutcomeDoerIdeologyModifier);
    }
    public class CompProperties_Ideo : CompProperties
    {
        public CompProperties_Ideo()
        {
            compClass = typeof(CompIdeo);
        }
    }
    public class CompIdeo : ThingComp
    {
        public Ideo Ideo = null;
        public void SetIdeo(Ideo ideo)
        {
            Ideo = ideo;
        }
        public Ideo GetIdeo()
        {
            return Ideo;
        }
    }


}