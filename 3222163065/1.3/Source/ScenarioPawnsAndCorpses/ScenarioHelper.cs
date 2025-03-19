using RimWorld;
using System.Linq;
using Verse;

namespace ScenarioPawnsAndCorpses
{
    /// <summary>
    /// Static helper for Scenario, allowing the plugin of additional scenpart notifications
    /// </summary>
    public class ScenarioHelper
    {
        /// <summary>
        /// Sends out a notification to applicable scenparts that the pawn is in the early generation phase
        /// </summary>
        /// <param name="pawn">The pawn being generated</param>
        /// <param name="context">The pawn's context</param>
        public static void HandleEarlyNewPawnGenerating(Pawn pawn, PawnGenerationContext context)
        {
            if (Find.Scenario != null)
            {
                float childRanChance = Verse.Rand.Range(0f, 1f);
                float adultRanChance = Verse.Rand.Range(0f, 1f);

                foreach (ScenPart part in Find.Scenario.AllParts.Where(p => p is ScenPart_ForcedChildhoodBackstory backstory && backstory.GetContext() == context))
                {
                    childRanChance -= ((ScenPart_ForcedBackstoryInterface)part).GetChance();
                    if (childRanChance <= 0)
                    {
                        ((ScenPart_ForcedBackstoryInterface)part).Notify_EarlyNewPawnGenerating(pawn, context);
                        break;
                    }
                }

                foreach (ScenPart part in Find.Scenario.AllParts.Where(p => p is ScenPart_ForcedAdultBackstory backstory && backstory.GetContext() == context))
                {
                    adultRanChance -= ((ScenPart_ForcedBackstoryInterface)part).GetChance();
                    if (adultRanChance <= 0)
                    {
                        ((ScenPart_ForcedBackstoryInterface)part).Notify_EarlyNewPawnGenerating(pawn, context);
                        break;
                    }
                }
            }
        }

    }
}
